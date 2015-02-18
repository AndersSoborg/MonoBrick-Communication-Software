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
     * Base track info.
     */
    public abstract class TrackInfo {

        /**
         * Codec (fourcc).
         */
        private readonly int codec;

        /**
         * Codec name.
         */
        private readonly string codecName;

        /**
         * Track id.
         */
        private readonly int id;

        /**
         * Profile.
         */
        private readonly int profile;

        /**
         * Level.
         */
        private readonly int level;

        /**
         * Create a new track info.
         *
         * @param codec codec
         * @param id track id
         * @param profile profile
         * @param level level
         */
        protected TrackInfo(int codec, int id, int profile, int level) {
            this.codec = codec;
            this.codecName = codec != 0 ? System.Text.ASCIIEncoding.ASCII.GetString(new byte[] {(byte)codec, (byte)(codec >> 8), (byte)(codec >> 16), (byte)(codec >> 24)}).Trim() : null;
            this.id = id;
            this.profile = profile;
            this.level = level;
        }

        /**
         * Get the codec (fourcc).
         *
         * @return codec
         */
        public int Codec() {
            return codec;
        }

        /**
         * Get the codec name.
         *
         * @return codec name
         */
        public String CodecName() {
            return codecName;
        }

        /**
         * Get the track id.
         *
         * @return track id
         */
        public int Id() {
            return id;
        }

        /**
         * Get the profile.
         *
         * @return profile
         */
        public int Profile() {
            return profile;
        }

        /**
         * Get the level.
         *
         * @return level
         */
        public int Level() {
            return level;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("TrackInfo").Append('['); // FIXME get class name
            sb.Append("codec=").Append(codec).Append(',');
            sb.Append("codecName=").Append(codecName).Append(',');
            sb.Append("id=").Append(id).Append(',');
            sb.Append("profile=").Append(profile).Append(',');
            sb.Append("level=").Append(level).Append(']');
            return sb.ToString();
        }
    }
}
