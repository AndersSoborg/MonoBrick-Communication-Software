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
using System.Text;

namespace Caprica.VlcSharp.Player {
 
    /**
     * Description of a track, e.g. a video or audio track.
     */
    public class TrackDescription {

        /**
         * Identifier.
         */
        private readonly int id;

        /**
         * Description.
         */
        private readonly string description;
 
        /**
         * Create a track description.
         *
         * @param id track identifier
         * @param description track description
         */
        public TrackDescription(int id, string description) {
            this.id = id;
            this.description = description;
        }
 
        /**
         * Get the track identifier
         *
         * @return identifier
         */
        public int Id() {
            return id;
        }
 
        /**
         * Get the track description
         *
         * @return description
         */
        public string Description() {
            return description;
        }
 
        public override string ToString() {
            StringBuilder sb = new StringBuilder(60);
            sb.Append("TrackDescription").Append('[');
            sb.Append("id=").Append(id).Append(',');
            sb.Append("description=").Append(description).Append(']');
            return sb.ToString();
        }
    }
}
