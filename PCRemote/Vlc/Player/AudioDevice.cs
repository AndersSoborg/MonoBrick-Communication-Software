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
     * Description of an audio output device.
     */
    public class AudioDevice {

        /**
         * Device identifier.
         */
        private readonly string deviceId;

        /**
         * Long name.
         */
        private readonly string longName;

        /**
         * Create an audio device.
         *
         * @param deviceId device identifier
         * @param longName long name
         */
        public AudioDevice(string deviceId, string longName) {
            this.deviceId = deviceId;
            this.longName = longName;
        }

        /**
         * Get the device identifier.
         *
         * @return device identifier
         */
        public string GetDeviceId() {
            return deviceId;
        }

        /**
         * Get the long name.
         *
         * @return long name
         */
        public string GetLongName() {
            return longName;
        }
 
        public override string ToString() {
            StringBuilder sb = new StringBuilder(60);
            sb.Append("AudioDevice").Append('[');
            sb.Append("deviceId=").Append(deviceId).Append(',');
            sb.Append("longName=").Append(longName).Append(']');
            return sb.ToString();
        }
    }
}
