/*
 * This file is part of VLCSHARP.
 *
 * VLCSHARP is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VLCSHARP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VLCSHARP.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Copyright 2012 Caprica Software Limited.
 */

using System;

namespace Caprica.VlcSharp.Binding.Internal {

    public enum libvlc_video_marquee_option_e {
    
        libvlc_marquee_Enable   = 0,
        libvlc_marquee_Text     = 1,        /** string argument */
        libvlc_marquee_Color    = 2,
        libvlc_marquee_Opacity  = 3,
        libvlc_marquee_Position = 4,
        libvlc_marquee_Refresh  = 5,
        libvlc_marquee_Size     = 6,
        libvlc_marquee_Timeout  = 7,
        libvlc_marquee_X        = 8,
        libvlc_marquee_Y        = 9
    }
}

