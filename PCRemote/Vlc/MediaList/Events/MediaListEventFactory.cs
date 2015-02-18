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

namespace Caprica.VlcSharp.MediaList.Events {

    /**
     * A factory that creates a media list event instance for a native media list event.
     */
    public class MediaListEventFactory {
    
        /**
         * Media list to which the event relates.
         */
        private readonly MediaList mediaList;
        
        /**
         * Create a new factory.
         * 
         * @param mediaList media list to create events for
         */
        public MediaListEventFactory(MediaList mediaList) {
            this.mediaList = mediaList;
        }
    
        /**
         * Create an event.
         * 
         * @param event native event
         * @return media list event, or <code>null</code> if the native event type is not enabled or otherwise could not be handled
         */
        public MediaListEvent CreateEvent(libvlc_event_t evt) {
            // Create an event suitable for the native event type...
            MediaListEvent result = null;
            switch(evt.type) {
                case libvlc_event_e.libvlc_MediaListWillAddItem:
                    //result = new MediaListWillAddItemEvent(mediaList, evt.u.media_list_event.item, evt.u.media_list_event.index); // FIXME
                    break;
                    
                case libvlc_event_e.libvlc_MediaListItemAdded:
                    //result = new MediaListItemAddedEvent(mediaList, evt.u.media_list_event.item, evt.u.media_list_event.index);
                    break;
    
                case libvlc_event_e.libvlc_MediaListWillDeleteItem:
                    //result = new MediaListWillDeleteItemEvent(mediaList, evt.u.media_list_event.item, evt.u.media_list_event.index);
                    break;
                    
                case libvlc_event_e.libvlc_MediaListItemDeleted:
                    //result = new MediaListItemDeletedEvent(mediaList, evt.u.media_list_event.item, evt.u.media_list_event.index);
                    break;
            }
            return result;
        }
    }
}
