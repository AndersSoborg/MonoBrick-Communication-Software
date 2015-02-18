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

namespace Caprica.VlcSharp.Player.List {

    /**
     * Default implementation of the media player event listener.
     * <p>
     * Simply override the methods you're interested in.
     */
    public class MediaListPlayerEventAdapter : MediaListPlayerEventListener {
    
        // === Events relating to the media player ==================================
    
        public void Played(MediaListPlayer mediaListPlayer) {
        }
    
        public void NextItem(MediaListPlayer mediaListPlayer, IntPtr item, string itemMrl) {
        }
    
        public void Stopped(MediaListPlayer mediaListPlayer) {
        }
    
        // === Events relating to the current media =================================
    
        public void MediaMetaChanged(MediaListPlayer mediaListPlayer, int metaType) {
        }
    
        public void MediaSubItemAdded(MediaListPlayer mediaListPlayer, IntPtr subItem) {
        }
    
        public void MediaDurationChanged(MediaListPlayer mediaListPlayer, long newDuration) {
        }
    
        public void MediaParsedChanged(MediaListPlayer mediaListPlayer, int newStatus) {
        }
    
        public void MediaFreed(MediaListPlayer mediaListPlayer) {
        }
    
        public void MediaStateChanged(MediaListPlayer mediaListPlayer, int newState) {
        }
    }
}
