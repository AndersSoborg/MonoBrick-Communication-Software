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
     * Audio track info.
     */
    public class AudioTrackInfo : TrackInfo {

        /**
         * Number of audio channels.
         */
        private readonly int channels;

        /**
         * Rate.
         */
        private readonly int rate;

        /**
         * Create a new audio track info
         *
         * @param codec audio codec
         * @param id track id
         * @param profile profile
         * @param level level
         * @param channels number of channels
         * @param rate rate
         */
        public AudioTrackInfo(int codec, int id, int profile, int level, int channels, int rate) : base(codec, id, profile, level) {
            this.channels = channels;
            this.rate = rate;
        }
 
        /**
         * Get the number of channels.
         *
         * @return channel count
         */
        public int Channels() {
            return channels;
        }
 
        /**
         * Get the rate.
         *
         * @return rate
         */
        public int Rate() {
            return rate;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder(200);
            sb.Append(base.ToString()).Append('[');
            sb.Append("channels=").Append(channels).Append(',');
            sb.Append("rate=").Append(rate).Append(']');
            return sb.ToString();
        }
    }
}

