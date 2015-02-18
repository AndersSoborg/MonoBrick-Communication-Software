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

namespace Caprica.VlcSharp.MediaList {

    /**
     * Default implementation of the media list event listener.
     * <p>
     * Simply override the methods you're interested in.
     * <p>
     * Events are likely <em>not</em> raised on the Swing Event Dispatch thread so if updating user
     * interface components in response to these events care must be taken to use
     * {@link SwingUtilities#invokeLater(Runnable)}.
     */
    public class MediaListEventAdapter : MediaListEventListener {
    
        public void MediaListWillAddItem(MediaList mediaList, IntPtr mediaInstance, int index) {
        }
    
        public void MediaListItemAdded(MediaList mediaList, IntPtr mediaInstance, int index) {
        }
    
        public void MediaListWillDeleteItem(MediaList mediaList, IntPtr mediaInstance, int index) {
        }
    
        public void MediaListItemDeleted(MediaList mediaList, IntPtr mediaInstance, int index) {
        }
    }
}
