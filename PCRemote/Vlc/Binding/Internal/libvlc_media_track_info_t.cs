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
using System.Runtime.InteropServices;

// FIXME for now I will not consider 32bit vs 64bit architectures

namespace Caprica.VlcSharp.Binding.Internal {

    [StructLayout(LayoutKind.Sequential)]
    public class libvlc_media_track_info_t {

        /* Codec fourcc */
        public int i_codec;
        public int i_id;
        public int i_type;

        /* Codec specific */
        public int i_profile;
        public int i_level;

        // FIXME track info union?
    }

    [StructLayout(LayoutKind.Explicit)]
    public class libvlc_media_track_info_u {

        [FieldOffset(0)]
        public int i_channels;
        [FieldOffset(4)]
        public int i_rate;

        [FieldOffset(0)]
        public int i_height;
        [FieldOffset(4)]
        public int i_width;

        // FIXME hmmmmmm.....
    }
}
