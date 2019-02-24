//    nVLC
//    
//    Author:  Roman Ginzburg
//
//    nVLC is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    nVLC is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//     
// ========================================================================

using Declarations;
using Declarations.Enums;
using Declarations.Media;
using Declarations.Players;
using Declarations.Structures;
using Implementation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace nVLC_Demo_MemoryInputOutput
{
    public partial class Form1 : Form
    {
        IMediaPlayerFactory m_factory;
        IVideoPlayer m_sourcePlayer;
        IVideoPlayer m_renderPlayer;
        ICompositeMemoryInputMedia m_inputMedia;
        FrameData data = new FrameData() { DTS = -1 };
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        BitmapFormat m_videoFormat;
        long m_firstFrameTS;
        FrameData audioData = new FrameData() { DTS = -1 };
        SoundFormat m_audioFormat;
        TinyAudioProcessor m_audioDsp;
        long m_firstAudioPts = 0;
        const int VIDEO_ID = 1;
        const int AUDIO_ID = 2;
        const int AUDIO_BUFFERS = 150;

        public Form1()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!m_renderPlayer.IsPlaying)
                return;

            String text = string.Format("Video FPS:{0}, audio FPS:{1}, pending video frames:{2}, pending audio frames:{3}",
                m_sourcePlayer.CustomRendererEx.ActualFrameRate,
                m_sourcePlayer.CustomAudioRenderer.ActualFrameRate,
                m_inputMedia.GetNumberOfPendingFrames(VIDEO_ID), m_inputMedia.GetNumberOfPendingFrames(AUDIO_ID));
            this.Text = text;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                m_factory = new MediaPlayerFactory(true);
                m_sourcePlayer = m_factory.CreatePlayer<IVideoPlayer>();
                m_sourcePlayer.Mute = false;
                m_sourcePlayer.Volume = 100;

                m_renderPlayer = m_factory.CreatePlayer<IVideoPlayer>();
                m_renderPlayer.WindowHandle = panel1.Handle;

                m_inputMedia = m_factory.CreateMedia<ICompositeMemoryInputMedia>(MediaStrings.IMEM);
                SetupVideoSourceOutput(m_sourcePlayer.CustomRendererEx);
                SetupAudioSourceOutput(m_sourcePlayer.CustomAudioRenderer);

                Predicate<LogMessage> filter = p => p.Level == LogLevel.Warning || p.Level == LogLevel.Error;
                m_factory.SubscribeToLogMessages(log => Console.WriteLine(log.Message), filter);

                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                m_sourcePlayer.Stop();
                m_renderPlayer.Stop();
                m_inputMedia.Dispose();
                m_renderPlayer.Dispose();
                m_sourcePlayer.Dispose();
            });

            base.OnClosing(e);
        }

        private void SetupVideoSourceOutput(IMemoryRendererEx iMemoryRenderer)
        {
            iMemoryRenderer.SetFormatSetupCallback(OnSetupCallback);
            iMemoryRenderer.SetExceptionHandler(OnErrorCallback);
            iMemoryRenderer.SetCallback(OnNewFrameCallback);
        }

        private void SetupAudioSourceOutput(IAudioRenderer audioRenderer)
        {
            audioRenderer.SetExceptionHandler(OnErrorCallback);
            audioRenderer.SetFormatCallback(OnAudioSetup);
            audioRenderer.SetCallbacks(null, OnNewSound);
        }

        private void OnNewSound(Sound newSound)
        {
            if (m_firstAudioPts == 0)
            {
                m_firstAudioPts = newSound.Pts;
            }

            if (m_audioDsp == null)
            {
                m_audioDsp = new TinyAudioProcessor((int)newSound.SamplesSize * sizeof(float));
            }

            audioData.Data = m_audioFormat.Channels == 6 ? m_audioDsp.DownMixAndConvertToFloat32(newSound) :
                    m_audioDsp.ConvertToFloat32(newSound);
           
            audioData.DataSize = (int)(newSound.SamplesSize / m_audioFormat.Channels * sizeof(float));
            audioData.PTS = newSound.Pts - m_firstAudioPts;

            m_inputMedia.AddFrame(AUDIO_ID, audioData);

            if (!m_renderPlayer.IsPlaying)
            {
                m_renderPlayer.Play();
            }
        }

        private SoundFormat OnAudioSetup(SoundFormat arg)
        {
            m_audioFormat = arg;
            var streamInfo = StreamInfo.FromSoundFormat(arg);
            streamInfo.ID = AUDIO_ID;
            m_inputMedia.AddOrUpdateStream(streamInfo, AUDIO_BUFFERS);

            // This sample supports only stereo or 5.1 audio, to use other audio channel layouts change TinyAudioProcessor
            arg.UseCustomAudioRendering = (arg.Channels == 2 || arg.Channels == 6); 
            return arg;
        }

        private BitmapFormat OnSetupCallback(BitmapFormat format)
        {
            m_videoFormat = new BitmapFormat(format.Width, format.Height, ChromaType.RV24);
            SetupInput(m_videoFormat);

            return m_videoFormat;
        }

        private void OnErrorCallback(Exception error)
        {
            Console.WriteLine(error.Message);
        }

        private unsafe void OnNewFrameCallback(PlanarFrame frame)
        {
            if (m_firstFrameTS == 0)
                m_firstFrameTS = m_factory.Clock;

            data.Data = frame.Planes[0];
            data.DataSize = frame.Lenghts[0];
            data.PTS = m_factory.Clock - m_firstFrameTS;

            m_inputMedia.AddFrame(VIDEO_ID, data);

            if (/*m_inputMedia.PendingFramesCount == 10 &&*/ !m_renderPlayer.IsPlaying)
            {
                m_renderPlayer.Open(m_inputMedia);
                m_renderPlayer.Mute = false;
                m_renderPlayer.Volume = 100;
                m_renderPlayer.Play();
            }
        }

        private void SetupInput(BitmapFormat format)
        {
            var streamInfo = StreamInfo.FromBitmapFormat(format);
            streamInfo.ID = VIDEO_ID;
            streamInfo.FPS = (int)Math.Floor(m_sourcePlayer.FPS != 0 ? m_sourcePlayer.FPS : 24);
            m_inputMedia.AddOrUpdateStream(streamInfo, streamInfo.FPS);
            
            m_inputMedia.SetExceptionHandler(OnErrorCallback);
            
        }

        private void OpenSourceMedia(string path)
        {
            IMediaFromFile media = m_factory.CreateMedia<IMediaFromFile>(path);
            m_sourcePlayer.Open(media);
            m_sourcePlayer.Play();
            
            timer.Start();
        }
            
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                OpenSourceMedia(ofd.FileName);
            }
        }
    }
}
