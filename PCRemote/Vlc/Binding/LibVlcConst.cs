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

namespace Caprica.VlcSharp.Binding {

    /**
     * Various constants defined by LibVLC, useful for example to set ranges for slider components.
     */
    public static class LibVlcConst {
    
        const int MIN_VOLUME = 0;
        const int MAX_VOLUME = 200;
        
        const float MIN_CONTRAST = 0.0f;
        const float MAX_CONTRAST = 2.0f;
          
        const float MIN_BRIGHTNESS = 0.0f;
        const float MAX_BRIGHTNESS = 2.0f;
          
        const int MIN_HUE = 0;
        const int MAX_HUE = 360;
          
        const float MIN_SATURATION = 0.0f;
        const float MAX_SATURATION = 3.0f;
          
        const float MIN_GAMMA = 0.01f;
        const float MAX_GAMMA = 10.0f;
          
        const float MIN_GAIN = -20.0f;
        const float MAX_GAIN = 20.0f;
    }
}
