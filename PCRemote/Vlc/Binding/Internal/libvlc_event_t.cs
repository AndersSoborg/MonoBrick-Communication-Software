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

    /**
     * 
     * 
     * Use default sequential layout to accommodate 64bit and 32bit architectures,
     * and embed another class to represent the union.
     */
    [StructLayout(LayoutKind.Sequential)]
    public class libvlc_event_t {

        public libvlc_event_e type;
        public IntPtr pObj;
        public libvlc_event_u u;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class libvlc_event_u {

        // media_meta_changed
        [FieldOffset(0)]
        public int meta_type;
        
        // media_subitem_added
        [FieldOffset(0)]
        public IntPtr new_child;
        
        // media_duration_changed
        [FieldOffset(0)]
        public long new_duration;
        
        // media_parsed_changed
        [FieldOffset(0)]
        public int new_status;
        
        // media_freed
        [FieldOffset(0)]
        public IntPtr md;
        
        // media_state_changed
        [FieldOffset(0)]
        public int new_state;
        
        // media_player_buffering
        [FieldOffset(0)]
        public float new_cache;
        
        // media_player_position_changed
        [FieldOffset(0)]
        public float new_position;
        
        // media_player_time_changed
        [FieldOffset(0)]
        public int new_time;
        
        // media_title_changed
        [FieldOffset(0)]
        public int new_title;
        
        // media_seekable_changed
        [FieldOffset(0)]
        public int new_seekable;
        
        // media_pausable_changed
        [FieldOffset(0)]
        public int new_pausable;
        
        // media_player_vout
        [FieldOffset(0)]
        public int new_count;
        
        // media_player_snapshot_taken
        [FieldOffset(0)]
        public IntPtr psz_filename;
        
        // media_player_length_changed
        [FieldOffset(0)]
        public long new_length;
        
        // media_list_item_added
        // media_list_item_deleted
        // media_list_will_add_item
        // media_list_will_delete_item

        // FIXME hmmmmm....

        // media_list_player_next_item_set
        [FieldOffset(0)]
        public IntPtr item;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class libvlc_media_list_event_t {

        public IntPtr item;
        public int index;
    }
}
