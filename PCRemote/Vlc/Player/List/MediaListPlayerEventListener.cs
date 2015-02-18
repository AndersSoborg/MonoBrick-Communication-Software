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
     * Specification for a component that is interested in receiving event notifications from the media
     * list player.
     */
    public interface MediaListPlayerEventListener {
    
        // === Events relating to the media list player =============================
    
        /**
         * Place-holder, do not use.
         * 
         * <strong>Warning: the native media list player event manager reports that it does not support
         * this event.</strong>
         * 
         * @param mediaListPlayer media list player that raised the event
         */
        void Played(MediaListPlayer mediaListPlayer);
    
        /**
         * The media list player started playing the next item in the list.
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param item next item instance
         * @param itemMrl MRL of the next item
         */
        void NextItem(MediaListPlayer mediaListPlayer, IntPtr item, string itemMrl);
    
        /**
         * Place-holder, do not use.
         * 
         * <strong>Warning: the native media list player event manager reports that it does <em>not</em>
         * support this event.</strong>
         * 
         * @param mediaListPlayer media list player that raised the event
         */
        void Stopped(MediaListPlayer mediaListPlayer);
    
        // === Events relating to the current media =================================
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param metaType meta data type
         */
        void MediaMetaChanged(MediaListPlayer mediaListPlayer, int metaType);
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param subItem media sub-item instance handle
         */
        void MediaSubItemAdded(MediaListPlayer mediaListPlayer, IntPtr subItem);
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param newDuration
         */
        void MediaDurationChanged(MediaListPlayer mediaListPlayer, long newDuration);
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param newStatus
         */
        void MediaParsedChanged(MediaListPlayer mediaListPlayer, int newStatus);
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         */
        void MediaFreed(MediaListPlayer mediaListPlayer);
    
        /**
         * 
         * 
         * @param mediaListPlayer media list player that raised the event
         * @param newState
         */
        void MediaStateChanged(MediaListPlayer mediaListPlayer, int newState);
    }
}
