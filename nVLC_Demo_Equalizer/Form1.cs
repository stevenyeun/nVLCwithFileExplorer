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
using Declarations.Media;
using Declarations.Players;
using Implementation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace nVLC_Demo_Equalizer
{
    public partial class Form1 : Form
    {
        IMediaPlayerFactory m_factory;
        IVideoPlayer m_player;
        IMediaFromFile m_media;
        Equalizer m_equalizer;
        Dictionary<int, Preset> m_presets;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_factory = new MediaPlayerFactory(new string[] { "--audio-visual=goom" }, true);
            m_player = m_factory.CreatePlayer<IVideoPlayer>();
            m_player.WindowHandle = panel1.Handle;

            m_presets = Equalizer.Presets.ToDictionary(key => key.Index);
            comboBox1.DataSource = m_presets.Values.ToList();
            comboBox1.DisplayMember = "Name";
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            m_player.SetEqualizer(null);
            SafeDispose(m_equalizer);
            SafeDisposePlayer(m_player);
            SafeDispose(m_media);
            SafeDispose(m_factory);
            
            base.OnClosing(e);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (m_player != null)
                    m_player.StopAsync();
                SafeDispose(m_media);
                m_media = m_factory.CreateMedia<IMediaFromFile>(ofd.FileName);
                m_player.Open(m_media);
                m_player.Play();
            }      
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SafeDispose(m_equalizer);
            m_equalizer = new Equalizer(m_presets[comboBox1.SelectedIndex]);

            listView1.Items.Clear();
            foreach (var band in m_equalizer.Bands)
            {
                listView1.Items.Add(new ListViewItem(new string[] { band.Index.ToString(), band.Frequency.ToString(),
                    band.Amplitude.ToString() }));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_equalizer != null)
                m_player.SetEqualizer(m_equalizer);
        }

        private static void SafeDispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        private static void SafeDisposePlayer(IVideoPlayer player)
        {          
            if (player != null)
            {
                player.Stop();
                player.WindowHandle = IntPtr.Zero;
                player.Dispose();
                player = null;
            }
        }
    }
}
