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
     * Video track info.
     */
    public class VideoTrackInfo : TrackInfo {

        /**
         * Video width.
         */
        private readonly int width;

        /**
         * Video height.
         */
        private readonly int height;

        /**
         * Create a new video track info.
         *
         * @param codec video codec
         * @param id track id
         * @param profile profile
         * @param level level
         * @param width width
         * @param height height
         */
        public VideoTrackInfo(int codec, int id, int profile, int level, int width, int height) : base(codec, id, profile, level) {
            this.width = width;
            this.height = height;
        }

        /**
         * Get the video width.
         *
         * @return width
         */
        public int Width() {
            return width;
        }

        /**
         * Get the video height.
         *
         * @return height
         */
        public int Height() {
            return height;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder(200);
            sb.Append(base.ToString()).Append('[');
            sb.Append("width=").Append(width).Append(',');
            sb.Append("height=").Append(height).Append(']');
            return sb.ToString();
        }
    }
}

