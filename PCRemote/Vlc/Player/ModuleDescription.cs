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
     * Description of a module (e.g. audio/video filter).
     */
    public class ModuleDescription {

        /**
         * Name.
         */
        private readonly String name;

        /**
         * Short name.
         */
        private readonly String shortName;

        /**
         * Long name.
         */
        private readonly String longName;

        /**
         * Help text.
         */
        private readonly String help;

        /**
         * Create a new module description
         *
         * @param name name
         * @param shortName short name
         * @param longName long name
         * @param help help text
         */
        public ModuleDescription(string name, string shortName, string longName, string help) {
            this.name = name;
            this.shortName = shortName;
            this.longName = longName;
            this.help = help;
        }

        /**
         * Get the module name.
         *
         * @return name
         */
        public string Name() {
            return name;
        }

        /**
         * Get the module short name.
         *
         * @return short name
         */
        public string ShortName() {
            return shortName;
        }

        /**
         * Get the module long name.
         *
         * @return long name
         */
        public string LongName() {
            return longName;
        }
 
        /**
         * Get the module help text.
         *
         * @return help text
         */
        public string Help() {
            return help;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(200);
            sb.Append("ModuleDescription").Append('[');
            sb.Append("name=").Append(name).Append(',');
            sb.Append("shortName=").Append(shortName).Append(',');
            sb.Append("longName=").Append(longName).Append(',');
            sb.Append("help=").Append(help).Append(']');
            return sb.ToString();
        }
    }
}
