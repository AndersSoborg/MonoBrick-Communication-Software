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
using System.Text.RegularExpressions;

namespace Caprica.VlcSharp.Version {

    /**
     * Encapsulation of version information and related behaviours.
     * <p>
     * This may be useful to implement version-specific features.
     */
    public class VersionNumber : IComparable {
 
        /**
         * Raw version information.
         */
        private readonly string version;

        /**
         * Major version number.
         */
        private readonly int major;

        /**
         * Minor version number.
         */
        private readonly int minor;

        /**
         * Revision number.
         */
        private readonly int revision;

        /**
         * Extra.
         */
        private readonly string extra;

        /**
         * Create a new version.
         *
         * @param version version string
         */
        public VersionNumber(string version) {
            this.version = version;
            string[] parts = Regex.Split(version, "[.-]|\\s");
            this.major = Convert.ToInt32(parts[0]);
            this.minor = Convert.ToInt32(parts[1]);
            this.revision = Convert.ToInt32(parts[2]);
            if(parts.Length > 3) {
                this.extra = parts[3];
            }
            else {
                this.extra = null;
            }
        }

        /**
         * Get the original version string.
         *
         * @return version
         */
        public string GetVersion() {
            return version;
        }

        /**
         * Get the major version.
         *
         * @return major version number
         */
        public int GetMajor() {
            return major;
        }

        /**
         * Get the minor version.
         *
         * @return minor version number
         */
        public int GetMinor() {
            return minor;
        }

        /**
         * Get the revision.
         *
         * @return revision number
         */
        public int GetRevision() {
            return revision;
        }

        /**
         * Get the extra.
         *
         * @return extra
         */
        public string GetExtra() {
            return extra;
        }

        /**
         * Test whether or not this version is at least the required version.
         *
         * @param required required version
         * @return <code>true</code> if this version is at least (equal to or
         *         greater than) the required version
         */
        public bool AtLeast(VersionNumber required) {
            return CompareTo(required) >= 0;
        }

        public int CompareTo(object o) {
            if(o == null) {
                return 1;
            }
            VersionNumber other = o as VersionNumber;
            if(other != null) {
                if(major == other.major) {
                    if(minor == other.minor) {
                        if(revision == other.revision) {
                            return 0;
                        }
                        else {
                            return revision - other.revision;
                        }
                    }
                    else {
                        return minor - other.minor;
                    }
                }
                else {
                    return major - other.major;
                }
            }
            else {
                throw new ArgumentException("Other object is not a VersionNumber");
            }
        }

        public override string ToString() {
            return version;
        }
    }
}
