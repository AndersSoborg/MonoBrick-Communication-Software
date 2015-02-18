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

using System.Diagnostics;
using System.IO;
using System.Text;

namespace Caprica.VlcSharp {
 
    /**
     * A very simple lightweight log system.
     * <p>
     * The log level can be changed by invoking {@link #setLevel(Level)}.
     * <p>
     * The log level can be configured at run-time by specifying a system property on the command-line,
     * for example:
     *
     * <pre>
     *   -Dvlcj.log=INFO
     * </pre>
     *
     * The log levels are defined in {@link Level}.
     */
    public class Logger {

        /**
         * Place-holder identifier.
         */
        private static readonly String PLACE_HOLDER = "{}";

        /**
         *
         */
        private static readonly Logger INSTANCE = new Logger();

        /**
         *
         */
        private readonly TextWriter sout = Console.Out;

        /**
         *
         */
        private readonly TextWriter err = Console.Error;

        /**
         *
         */
        private Level threshold = Level.NONE;

        /**
         *
         */
        public enum Level {
            NONE,
            FATAL,
            ERROR,
            WARN,
            INFO,
            DEBUG,
            TRACE
        }

        private Logger() {
            // FIXME set initial threshold based on system property?
        }

        public static void SetLevel(Level threshold) {
            INSTANCE.threshold = threshold;
        }

        public static Level GetLevel() {
            return INSTANCE.threshold;
        }

        public static void Trace(string msg, params object[] args) {
            if(Level.TRACE.CompareTo(INSTANCE.threshold) <= 0) {
                Log("TRACE", msg, null, args);
            }
        }

        public static void Trace(string msg, Exception t, params object[] args) {
            if(Level.TRACE.CompareTo(INSTANCE.threshold) <= 0) {
                Log("TRACE", msg, t, args);
            }
        }

        public static void Debug(string msg, params object[] args) {
            if(Level.DEBUG.CompareTo(INSTANCE.threshold) <= 0) {
                Log("DEBUG", msg, null, args);
            }
        }

        public static void Debug(string msg, Exception t, params object[] args) {
            if(Level.DEBUG.CompareTo(INSTANCE.threshold) <= 0) {
                Log("DEBUG", msg, t, args);
            }
        }

        public static void Info(string msg, params object[] args) {
            if(Level.INFO.CompareTo(INSTANCE.threshold) <= 0) {
                Log("INFO", msg, null, args);
            }
        }

        public static void Info(string msg, Exception t, params object[] args) {
            if(Level.INFO.CompareTo(INSTANCE.threshold) <= 0) {
                Log("INFO", msg, t, args);
            }
        }

        public static void Warn(string msg, params object[] args) {
            if(Level.WARN.CompareTo(INSTANCE.threshold) <= 0) {
                Log("WARN", msg, null, args);
            }
        }

        public static void Warn(string msg, Exception t, params object[] args) {
            if(Level.WARN.CompareTo(INSTANCE.threshold) <= 0) {
                Log("WARN", msg, t, args);
            }
        }

        public static void Error(string msg, params object[] args) {
            if(Level.ERROR.CompareTo(INSTANCE.threshold) <= 0) {
                Log("ERROR", msg, null, args);
            }
        }

        public static void Error(string msg, Exception t, params object[] args) {
            if(Level.ERROR.CompareTo(INSTANCE.threshold) <= 0) {
                Log("ERROR", msg, t, args);
            }
        }

        public static void Fatal(string msg, params object[] args) {
            if(Level.FATAL.CompareTo(INSTANCE.threshold) <= 0) {
                Log("FATAL", msg, null, args);
            }
        }

        public static void Fatal(string msg, Exception t, params object[] args) {
            if(Level.FATAL.CompareTo(INSTANCE.threshold) <= 0) {
                Log("FATAL", msg, t, args);
            }
        }

        private static void Log(string level, string msg, Exception t, params object[] args) {
            TextWriter sout = INSTANCE.sout;
            string fileName;
            string lineNumber;
            try {
                StackFrame el = GetLine();
                fileName = el.GetFileName();
                fileName = fileName.Substring(fileName.LastIndexOf('/') + 1); // FIXME portable?
                lineNumber = el.GetFileLineNumber().ToString();
            }
            catch(Exception) {
                fileName = "?";
                lineNumber = "?";
            }
            string location = String.Format("({0}:{1})", fileName, lineNumber);
            sout.WriteLine(String.Format("vlcsharp: {0,-46} | {1,-5} | {2}", location, level, format(msg, args)));
            sout.Flush();
            if(t != null) {
                TextWriter err = INSTANCE.err;
                err.WriteLine(String.Format("vlcsharp: {0,-46} | {1,-5} | {2}", location, level, t.Message));
                err.Flush();
            }
        }

        private static StackFrame GetLine() {
            StackTrace t = new StackTrace(true);
            return t.GetFrame(3); // Take care!
        }

        /**
         * Format a string, such as "something{0}, another{1}", replacing the tokens with argument
         * values.
         *
         * @param msg message, including token place-holders
         * @param args values to substitute
         * @return formatted string
         */
        public static string format(string msg, params object[] args) {
            if(args == null || args.Length == 0 || msg == null) {
                return msg;
            }
            else {
                StringBuilder sb = new StringBuilder(msg.Length + args.Length * 10);
                for(int current = 0, argIndex = 0; current < msg.Length;) {
                    int token = msg.IndexOf(PLACE_HOLDER, current);
                    if(token > -1) {
                        sb.Append(msg.Substring(current, token - current));
                        sb.Append(args[argIndex ++]);
                        current = token + PLACE_HOLDER.Length;
                    }
                    else {
                        sb.Append(msg.Substring(current));
                        break;
                    }
                }
                return sb.ToString();
            }
        }
    }
}
