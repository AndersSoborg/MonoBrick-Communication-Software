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
using System.Reflection;

using Caprica.VlcSharp.Version;

namespace Caprica.VlcSharp {

    /**
    * Application information banner.
    */
    public class Info {

        /**
         * Application name.
         */
        private static readonly string APP_MSG =
            "       _          _                      " + "\n" +
            "__   _| | ___ ___| |__   __ _ _ __ _ __  " + "\n" +
            "\\ \\ / / |/ __/ __| '_ \\ / _` | '__| '_ \\ " + "\n" +
            " \\ V /| | (__\\__ \\ | | | (_| | |  | |_) |" + "\n" +
            "  \\_/ |_|\\___|___/_| |_|\\__,_|_|  | .__/ 0.1.0" + "\n" +
            "                                  |_|    www.capricasoftware.co.uk" + "\n";
    
        /**
         * Application license (GPL3) summary text.
         */
        private static readonly string LICENSE_MSG =
            "VLCSHARP is free software: you can redistribute it and/or modify"     + "\n" +
            "it under the terms of the GNU General Public License as published by" + "\n" +
            "the Free Software Foundation, either version 3 of the License, or"    + "\n" +
            "(at your option) any later version."                                  + "\n" +
            ""                                                                     + "\n" +
            "VLCSHARP is distributed in the hope that it will be useful,"          + "\n" +
            "but WITHOUT ANY WARRANTY; without even the implied warranty of"       + "\n" +
            "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the"        + "\n" +
            "GNU General Public License for more details."                         + "\n" +
            ""                                                                     + "\n" +
            "You should have received a copy of the GNU General Public License"    + "\n" +
            "along with VLCSHARP.  If not, see <http://www.gnu.org/licenses/>."    + "\n" +
            ""                                                                     + "\n" +
            "Copyright 2012 Caprica Software Limited."                             + "\n";
          
        /**
         * Singleton holder.
         */
        private static class InfoHolder {
        
            /**
             * Singleton instance.
             */
            public static readonly Info INSTANCE = new Info();
        }
        
        /**
         * Get application information.
         * 
         * @return singleton instance
         */
        public static Info getInstance() {
            return InfoHolder.INSTANCE;
        }
        
        /**
         * vlcsharp version.
         */
        private VersionNumber version;
        
        /**
         * Private constructor.
         */
        private Info() {
            Console.Error.WriteLine(APP_MSG);
            Console.Error.WriteLine(LICENSE_MSG);
            Console.Error.Flush();
            Assembly assembly = Assembly.GetExecutingAssembly();
            version = new VersionNumber(assembly.GetName().Version.ToString());
            Logger.Info("vlcsharp: {}", version != null ? version.ToString() : "<version not available>");
            //Logger.Info("runtime: {} {}", assembly.ImageRuntimeVersion, Environment.Is64BitProcess ? "64-bit" : "32-bit");
            //Logger.Info("os: {} {}", Environment.OSVersion.ToString(), Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");
        }
        
        /**
         * Get the vlcsharp version.
         *
         * @return version
         */
        public VersionNumber GetVersion() {
            return version;
        }
    }
}
