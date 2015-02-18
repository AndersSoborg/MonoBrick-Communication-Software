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

using Caprica.VlcSharp.Binding.Internal;

namespace Caprica.VlcSharp.Player.List.Events {

    /**
     * A factory that creates a media player event instance for a native media list player event.
     */
    public class MediaListPlayerEventFactory {
    
        /**
         * Media player to create events for.
         */
        private readonly MediaListPlayer mediaListPlayer;
    
        /**
         * Create a media player event factory.
         * 
         * @param mediaPlayer media list player to create events for
         */
        public MediaListPlayerEventFactory(MediaListPlayer mediaPlayer) {
            this.mediaListPlayer = mediaPlayer;
        }
    
        /**
         * Create a new media player event for a given native event.
         * 
         * @param event native event
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the native event type is not enabled or otherwise could not be handled
         */
        public MediaListPlayerEvent newMediaListPlayerEvent(libvlc_event_t evt, int eventMask) {
            // Create an event suitable for the native event type...
            MediaListPlayerEvent result = null;
            switch(evt.type) {
    
            // === Events relating to the media list player =========================
    
                case libvlc_event_e.libvlc_MediaListPlayerNextItemSet:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_CHANGED)) {
                        IntPtr media = evt.u.item;
                        result = new MediaListPlayerNextItemSetEvent(mediaListPlayer, media, mediaListPlayer.Mrl(media));
    //                }
                    break;
    
                // === Events relating to the current media =============================
    
                case libvlc_event_e.libvlc_MediaMetaChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_META_CHANGED)) {
                        result = new MediaListMediaMetaChangedEvent(mediaListPlayer, evt.u.meta_type);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaSubItemAdded:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_SUB_ITEM_ADDED)) {
                        result = new MediaListMediaSubItemAddedEvent(mediaListPlayer, evt.u.new_child);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaDurationChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_DURATION_CHANGED)) {
                        result = new MediaListMediaDurationChangedEvent(mediaListPlayer, evt.u.new_duration);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaParsedChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_PARSED_CHANGED)) {
                        result = new MediaListMediaParsedChangedEvent(mediaListPlayer, evt.u.new_status);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaFreed:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_FREED)) {
                        result = new MediaListMediaFreedEvent(mediaListPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaStateChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_STATE_CHANGED)) {
                        result = new MediaListMediaStateChangedEvent(mediaListPlayer, evt.u.new_state);
    //                }
                    break;
            }
            return result;
        }
    }
}
