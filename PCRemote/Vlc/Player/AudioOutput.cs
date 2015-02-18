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
using System.Text;

namespace Caprica.VlcSharp.Player {

    /**
     * Description of an audio output.
     * <p>
     * An audio output has zero or more associated audio devices. Each device has a unique identifier
     * than can be used to select the required output device for a media player.
     */
    public class AudioOutput {

        /**
         * Name.
         */
        private readonly string name;

        /**
         * Description.
         */
        private readonly string description;
 
        /**
         * Long name.
         */
        private readonly List<AudioDevice> devices;
 
        /**
         * Create an audio output.
         *
         * @param name name
         * @param description description
         * @param devices collection of audio devices for this output
         */
        public AudioOutput(string name, string description, List<AudioDevice> devices) {
            this.name = name;
            this.description = description;
            this.devices = devices;
        }
 
        /**
         * Get the name.
         *
         * @return name
         */
        public string GetName() {
            return name;
        }
 
        /**
         * Get the description.
         *
         * @return description
         */
        public string GetDescription() {
            return description;
        }
 
        /**
         * Get the collection of audio devices for this output.
         *
         * @return audio devices
         */
        public List<AudioDevice> GetDevices() {
            return devices;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder(60);
            sb.Append("AudioOutput").Append('[');
            sb.Append("name=").Append(name).Append(',');
            sb.Append("description=").Append(description).Append(',');
            sb.Append("devices=").Append(devices).Append(']');
            return sb.ToString();
        }
    }
}
