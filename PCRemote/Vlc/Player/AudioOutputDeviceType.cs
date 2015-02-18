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

namespace Caprica.VlcSharp.Player {

    /**
     * Enumeration of audio output device types.
     */
    public enum AudioOutputDeviceType {
 
        AUDIO_ERROR  = -1,
        AUDIO_MONO   = 1, 
        AUDIO_STEREO = 2,
        AUDIO_2F2R   = 4, 
        AUDIO_3F2R   = 5, 
        AUDIO_5_1    = 6, 
        AUDIO_6_1    = 7, 
        AUDIO_7_1    = 8, 
        AUDIO_SPDIF  = 10
    }
}
