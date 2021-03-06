﻿//    nVLC
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibVlcWrapper;
using Implementation.Exceptions;
using Implementation.Events;
using Declarations.Events;
using System.Runtime.InteropServices;
using Declarations.VLM;

namespace Implementation.VLM
{
    internal class VideoLanManager : DisposableBase, IEventProvider, IVideoLanManager
    {
        private IntPtr m_hMediaLib;

        private VlmEventManager m_Eventbroker;

        public IVlmEventManager Events
        {
            get
            {
                return m_Eventbroker;
            }
        }

        public VideoLanManager(IntPtr p_hMediaLib)
        {
            m_hMediaLib = p_hMediaLib;

            m_Eventbroker = new VlmEventManager(this);
        }

        public IntPtr EventManagerHandle
        {
            get
            {
                return LibVlcMethods.libvlc_vlm_get_event_manager(m_hMediaLib);
            }
        }

        protected override void Dispose(bool disposing)
        {
            m_Eventbroker.Dispose();
            LibVlcMethods.libvlc_vlm_release(m_hMediaLib);
        }

        public void AddBroadcast(string name, string input, string output, IEnumerable<string> options, bool bEnabled, bool bLoop)
        {
            int optionsNumber = 0;
            string[] optionsArray = null;

            if (options != null)
            {
                optionsNumber = options.Count();
                optionsArray = options.ToArray();
            }

            if (LibVlcMethods.libvlc_vlm_add_broadcast(m_hMediaLib, name.ToUtf8(), input.ToUtf8(), output.ToUtf8(), optionsNumber, optionsArray, bEnabled == true ? 1 : 0, bLoop == true ? 1 : 0) != 0)
            {
                throw new LibVlcException("Failed to add broadcast");
            }
        }

        public void AddVod(string name, string input, IEnumerable<string> options, bool bEnabled, string mux)
        {
            int optionsNumber = 0;
            string[] optionsArray = null;

            if (options != null)
            {
                optionsNumber = options.Count();
                optionsArray = options.ToArray();
            }

            if (LibVlcMethods.libvlc_vlm_add_vod(m_hMediaLib, name.ToUtf8(), input.ToUtf8(), optionsNumber, optionsArray, bEnabled == true ? 1 : 0, mux.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to add VOD");
            }
        }

        public void DeleteMedia(string name)
        {
            if (LibVlcMethods.libvlc_vlm_del_media(m_hMediaLib, name.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to delete media");
            }
        }

        public void SetEnabled(string name, bool bEnabled)
        {
            if (LibVlcMethods.libvlc_vlm_set_enabled(m_hMediaLib, name.ToUtf8(), bEnabled == true ? 1 : 0) != 0)
            {
                throw new LibVlcException("Failed to enable/disbale");
            }
        }

        public void SetInput(string name, string input)
        {
            if (LibVlcMethods.libvlc_vlm_set_input(m_hMediaLib, name.ToUtf8(), input.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to set input");
            }
        }

        public void SetOutput(string name, string output)
        {
            if (LibVlcMethods.libvlc_vlm_set_output(m_hMediaLib, name.ToUtf8(), output.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to set output");
            }
        }

        public void AddInput(string name, string input)
        {
            if (LibVlcMethods.libvlc_vlm_add_input(m_hMediaLib, name.ToUtf8(), input.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to add input");
            }
        }

        public void SetLoop(string name, bool bLoop)
        {
            if (LibVlcMethods.libvlc_vlm_set_loop(m_hMediaLib, name.ToUtf8(), bLoop == true ? 1 : 0) != 0)
            {
                throw new LibVlcException("Failed to set loop");
            }
        }

        public void SetMux(string name, string mux)
        {
            if (LibVlcMethods.libvlc_vlm_set_mux(m_hMediaLib, name.ToUtf8(), mux.ToUtf8()) != 0)
            {
                throw new LibVlcException("Failed to set mux");
            }
        }

        public void ChangeMedia(string name, string input, string output, IEnumerable<string> options, bool bEnabled, bool bLoop)
        {
            int optionsNumber = 0;
            string[] optionsArray = null;

            if (options != null)
            {
                optionsNumber = options.Count();
                optionsArray = options.ToArray();
            }

            if (LibVlcMethods.libvlc_vlm_change_media(m_hMediaLib, name.ToUtf8(), input.ToUtf8(), output.ToUtf8(), optionsNumber, optionsArray, bEnabled == true ? 1 : 0, bLoop == true ? 1 : 0) != 0)
            {
                throw new LibVlcException("Failed to change media");
            }
        }

        public void Play(string name)
        {
            LibVlcMethods.libvlc_vlm_play_media(m_hMediaLib, name.ToUtf8());
        }

        public void Stop(string name)
        {
            LibVlcMethods.libvlc_vlm_stop_media(m_hMediaLib, name.ToUtf8());
        }

        public void Pause(string name)
        {
            LibVlcMethods.libvlc_vlm_pause_media(m_hMediaLib, name.ToUtf8());
        }

        public void Seek(string name, float percentage)
        {
            LibVlcMethods.libvlc_vlm_seek_media(m_hMediaLib, name.ToUtf8(), percentage);
        }

        public float GetMediaPosition(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_position(m_hMediaLib, name.ToUtf8(), 0);
        }

        public int GetMediaTime(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_time(m_hMediaLib, name.ToUtf8(), 0);
        }

        public int GetMediaLength(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_length(m_hMediaLib, name.ToUtf8(), 0);
        }

        public int GetMediaRate(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_rate(m_hMediaLib, name.ToUtf8(), 0);
        }

        public int GetMediaTitle(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_title(m_hMediaLib, name.ToUtf8(), 0);
        }

        public int GetMediaChapter(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_chapter(m_hMediaLib, name.ToUtf8(), 0);
        }

        public bool IsMediaSeekable(string name)
        {
            return LibVlcMethods.libvlc_vlm_get_media_instance_seekable(m_hMediaLib, name.ToUtf8(), 0) == 1;
        }
    }
}
