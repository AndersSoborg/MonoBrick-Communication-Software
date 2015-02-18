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
using System.ComponentModel;
using System.Reflection;

namespace Caprica.VlcSharp.Player {

    public static class Extensions {        
     
        public static string GetDescription(this DeinterlaceMode mode) {
            FieldInfo fi = mode.GetType().GetField(mode.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if(attributes != null && attributes.Length > 0) {
                return attributes[0].Description;
            }
            else {
                return mode.ToString();      
            }        
        }
    }
 
    /**
     * Enumeration of deinterlace modes.
     * <p>
     * These are defined in "modules/video_filter/deinterlace.c".
     */
    public enum DeinterlaceMode {
     
        [Description("discard")]
        DISCARD,
 
        [Description("blend")]
        BLEND,
 
        [Description("mean")]
        MEAN,
 
        [Description("bob")]
        BOB,
 
        [Description("linear")]
        LINEAR,
 
        [Description("x")]
        X,

        [Description("yadif")]
        YADIF,

        [Description("yadif2x")]
        YADIF2X,

        [Description("phosphor")]
        PHOSPHOR,
     
        [Description("ivtc")]
        IVTC
    }
}
