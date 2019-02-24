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

using Declarations;
using LibVlcWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Implementation
{
    internal unsafe class ViewPoint : DisposableBase, IViewPoint, INativePointer 
    {
        private readonly libvlc_video_viewpoint_t* m_pViewPoint;
        public ViewPoint(float yaw, float pitch, float roll, float fieldOfView)
        {
            m_pViewPoint = LibVlcMethods.libvlc_video_new_viewpoint();
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            FieldOfView = fieldOfView;
        }
     
        public IntPtr Pointer
        {
            get
            {
                return new IntPtr(m_pViewPoint);
            }
        }

        public float Yaw
        {
            get
            {
                return m_pViewPoint->f_yaw;
            }
            set
            {
                m_pViewPoint->f_yaw = value;
            }
        }
            
        public float Pitch
        {
            get
            {
                return m_pViewPoint->f_pitch;
            }
            set
            {
                m_pViewPoint->f_pitch = value;
            }
        }
        public float Roll
        {
            get
            {
                return m_pViewPoint->f_roll;
            }
            set
            {
                m_pViewPoint->f_roll = value;
            }
        }
        public float FieldOfView
        {
            get
            {
                return m_pViewPoint->f_field_of_view;
            }
            set
            {
                m_pViewPoint->f_field_of_view = value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            LibVlcMethods.libvlc_free(new IntPtr(m_pViewPoint));
        }
    }
}
