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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Caprica.VlcSharp.MediaList {

    /**
     * Encapsulation of an item in a {@link MediaList}.
     * <p>
     * An item comprises a name, a media resource locator (MRL) that may be passed
     * to a media player instance to play it, and possibly a sub-item list.
     * <p>
     * The sub-item list should be an empty list rather than <code>null</code> if
     * there are no items.
     */
    public class MediaListItem {
    
        /**
         * Name/description of the item.
         */
        private readonly string name;
        
        /**
         * MRL of the item.
         */
        private readonly string mrl;
        
        /**
         * List of sub-items.
         */
        private readonly List<MediaListItem> subItems;
        
        /**
         * Create a media list item.
         * 
         * @param name name/description
         * @param mrl MRL
         * @param subItems
         */
        public MediaListItem(string name, string mrl, List<MediaListItem> subItems) {
            this.name = name;
            this.mrl = mrl;
            this.subItems = subItems;
        }
        
        /**
         * Get the name/description of this item.
         * 
         * @return name/description
         */
        public string Name() {
            return name;
        }
        
        /**
         * Get the MRL of this item.
         * 
         * @return MRL
         */
        public string Mrl() {
            return mrl;
        }
    
        /**
         * Get the sub-item list.
         * 
         * @return sub-items
         */
        public ReadOnlyCollection<MediaListItem> SubItems() {
            return new ReadOnlyCollection<MediaListItem>(subItems);
        }
    
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("MediaListItem").Append('[');
            sb.Append("name=").Append(name).Append(',');
            sb.Append("mrl=").Append(mrl).Append(',');
            sb.Append("subItems=").Append(subItems).Append(']');
            return sb.ToString();
        }
    }
}
