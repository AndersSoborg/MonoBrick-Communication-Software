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

    public enum libvlc_video_adjust_option_e {
    
        libvlc_adjust_Enable     = 0,
        libvlc_adjust_Contrast   = 1,  // Float
        libvlc_adjust_Brightness = 2,  // Float
        libvlc_adjust_Hue        = 3,  // Integer
        libvlc_adjust_Saturation = 4,  // Float
        libvlc_adjust_Gamma      = 5   // Float
    }
}

