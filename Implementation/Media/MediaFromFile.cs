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
using System.Runtime.InteropServices;
using Declarations;
using Declarations.Media;
using LibVlcWrapper;
using Implementation.Exceptions;
using System.Collections.Generic;

namespace Implementation.Media
{
    internal class MediaFromFile : BasicMedia, IMediaFromFile
    {
        public MediaFromFile(IntPtr hMediaLib)
            : base(hMediaLib)
        {

        }

        public override string Input
        {
            get
            {
                return m_path;
            }
            set
            {
                m_path = value;
                m_hMedia = LibVlcMethods.libvlc_media_new_path(m_hMediaLib, m_path.ToUtf8());
            }
        }

        public string GetMetaData(MetaDataType dataType)
        {
            IntPtr pData = LibVlcMethods.libvlc_media_get_meta(m_hMedia, (libvlc_meta_t)dataType);
            return Marshal.PtrToStringAnsi(pData);
        }

        public void SetMetaData(MetaDataType dataType, string argument)
        {
            LibVlcMethods.libvlc_media_set_meta(m_hMedia, (libvlc_meta_t)dataType, argument.ToUtf8());
        }

        public void SaveMetaData()
        {
            LibVlcMethods.libvlc_media_save_meta(m_hMedia);
        }

        public long Duration
        {
            get
            {
                return LibVlcMethods.libvlc_media_get_duration(m_hMedia);
            }
        }

        [Obsolete]
        public MediaTrackInfo[] TracksInfo
        {
            get
            {
                IntPtr pTr = IntPtr.Zero;
                int num = LibVlcMethods.libvlc_media_get_tracks_info(m_hMedia, out pTr);

                if (num == 0 || pTr == IntPtr.Zero)
                {
                    throw new LibVlcException("Failed to get media tracks");
                }

                int size = Marshal.SizeOf(typeof(libvlc_media_track_info_t));
                libvlc_media_track_info_t[] tracks = new libvlc_media_track_info_t[num];
                for (int i = 0; i < num; i++)
                {
                    tracks[i] = (libvlc_media_track_info_t)Marshal.PtrToStructure(pTr, typeof(libvlc_media_track_info_t));
                    pTr = new IntPtr(pTr.ToInt64() + size);
                }

                MediaTrackInfo[] mtis = new MediaTrackInfo[num];
                for (int i = 0; i < num; i++)
                {
                    mtis[i] = tracks[i].ToMediaInfo();
                }

                return mtis;
            }
        }
    }
}
