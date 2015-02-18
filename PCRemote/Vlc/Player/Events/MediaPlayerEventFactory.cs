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

// FIXME implement event mask

namespace Caprica.VlcSharp.Player.Events {

    /**
     * A factory that creates a media playerstringinstance for a native media player event or a
     * semantic event.
     * <p>
     * A "semantic" event is one that has no directly associated native event, but is instead a higher
     * level event (like "sub-item finished", there is no such native event but it can be inferred and
     * is a useful event).
     */
    public class MediaPlayerEventFactory {
    
        /**
         * Media player to create events for.
         */
        private readonly MediaPlayer mediaPlayer;
    
        /**
         * Create a media player event factory.
         * 
         * @param mediaPlayer media player to create events for
         */
        public MediaPlayerEventFactory(MediaPlayer mediaPlayer) {
            this.mediaPlayer = mediaPlayer;
        }
    
        /**
         * Create a new media player event for a given native event.
         * 
         * @param event native event
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the native event type is not enabled or otherwise could not be handled
         */
        public MediaPlayerEvent CreateEvent(libvlc_event_t evt, int eventMask) {
            // Create an event suitable for the native event type...
            MediaPlayerEvent result = null;
            switch(evt.type) {
    
            // === Events relating to the media player ==============================
    
                case libvlc_event_e.libvlc_MediaPlayerMediaChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_CHANGED)) {
                        result = new MediaPlayerMediaChangedEvent(mediaPlayer, evt.u.md, mediaPlayer.Mrl(evt.u.md));
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerNothingSpecial:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_CHANGED)) {
                        result = new MediaPlayerNothingSpecialEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerOpening:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.OPENING)) {
                        result = new MediaPlayerOpeningEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerBuffering:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.BUFFERING)) {
                        result = new MediaPlayerBufferingEvent(mediaPlayer, evt.u.new_cache);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerPlaying:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.PLAYING)) {
                        result = new MediaPlayerPlayingEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerPaused:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.PAUSED)) {
                        result = new MediaPlayerPausedEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerStopped:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.STOPPED)) {
                        result = new MediaPlayerStoppedEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerForward:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.FORWARD)) {
                        result = new MediaPlayerForwardEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerBackward:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.BACKWARD)) {
                        result = new MediaPlayerBackwardEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerEndReached:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.FINISHED)) {
                        result = new MediaPlayerEndReachedEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerEncounteredError:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.ERROR)) {
                        result = new MediaPlayerEncounteredErrorEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerTimeChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.TIME_CHANGED)) {
                        result = new MediaPlayerTimeChangedEvent(mediaPlayer, evt.u.new_time);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerPositionChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.POSITION_CHANGED)) {
                        result = new MediaPlayerPositionChangedEvent(mediaPlayer, evt.u.new_position);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerSeekableChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.SEEKABLE_CHANGED)) {
                        result = new MediaPlayerSeekableChangedEvent(mediaPlayer, evt.u.new_seekable);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerPausableChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.PAUSABLE_CHANGED)) {
                        result = new MediaPlayerPausableChangedEvent(mediaPlayer, evt.u.new_pausable);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerTitleChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.TITLE_CHANGED)) {
                        result = new MediaPlayerTitleChangedEvent(mediaPlayer, evt.u.new_title);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerSnapshotTaken:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.SNAPSHOT_TAKEN)) {
                        result = new MediaPlayerSnapshotTakenEvent(mediaPlayer, NativeString.String(evt.u.psz_filename));
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerLengthChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.LENGTH_CHANGED)) {
                        result = new MediaPlayerLengthChangedEvent(mediaPlayer, evt.u.new_length);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaPlayerVout:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.VIDEO_OUTPUT)) {
                        result = new MediaPlayerVoutEvent(mediaPlayer, evt.u.new_count);
    //                }
                    break;
    
                // === Events relating to the current media =============================
    
                case libvlc_event_e.libvlc_MediaMetaChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_META_CHANGED)) {
                        result = new MediaMetaChangedEvent(mediaPlayer, evt.u.meta_type);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaSubItemAdded:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_SUB_ITEM_ADDED)) {
                        result = new MediaSubItemAddedEvent(mediaPlayer, evt.u.new_child);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaDurationChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_DURATION_CHANGED)) {
                        result = new MediaDurationChangedEvent(mediaPlayer, evt.u.new_duration);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaParsedChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_PARSED_CHANGED)) {
                        result = new MediaParsedChangedEvent(mediaPlayer, evt.u.new_status);
    //                }
                    break;

                case libvlc_event_e.libvlc_MediaFreed:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_FREED)) {
                        result = new MediaFreedEvent(mediaPlayer);
    //                }
                    break;
    
                case libvlc_event_e.libvlc_MediaStateChanged:
    //                if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.MEDIA_STATE_CHANGED)) {
                        result = new MediaStateChangedEvent(mediaPlayer, evt.u.new_state);
    //                }
                    break;
            }
            return result;
        }
    
        /**
         * Create a new semantic event for new media.
         * 
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the event type is not enabled
         */
        public MediaPlayerEvent CreateMediaNewEvent(int eventMask) {
    //        if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.NEW_MEDIA)) {
                return new MediaNewEvent(mediaPlayer);
    //        }
    //        return null;
        }
    
        /**
         * Create a new semantic event for a played sub-item.
         * 
         * @param subItemIndex index of the sub-item that was played
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the event type is not enabled
         */
        public MediaPlayerEvent CreateMediaSubItemPlayedEvent(int subItemIndex, int eventMask) {
    //        if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.SUB_ITEM_PLAYED)) {
                return new MediaSubItemPlayedEvent(mediaPlayer, subItemIndex);
    //        }
    //        return null;
        }
    
        /**
         * Create a new semantic event for a finished sub-item.
         * 
         * @param subItemIndex index of the sub-item that finished playing
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the event type is not enabled
         */
        public MediaPlayerEvent CreateMediaSubItemFinishedEvent(int subItemIndex, int eventMask) {
    //        if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.SUB_ITEM_FINISHED)) {
                return new MediaSubItemFinishedEvent(mediaPlayer, subItemIndex);
    //        }
    //        return null;
        }
    
        /**
         * Create a new semantic event for the end of the sub-items being reached.
         * 
         * @param eventMask bit mask of enabled events (i.e. events to send notifications for)
         * @return media player event, or <code>null</code> if the event type is not enabled
         */
        public MediaPlayerEvent RreateMediaEndOfSubItemsEvent(int eventMask) {
    //        if(MediaPlayerEventType.set(eventMask, MediaPlayerEventType.END_OF_SUB_ITEMS)) {
                return new MediaEndOfSubItemsEvent(mediaPlayer);
    //        }
    //        return null;
        }
    }
}