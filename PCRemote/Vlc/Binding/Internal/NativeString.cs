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
using System.Runtime.InteropServices;

namespace Caprica.VlcSharp.Binding.Internal {

    /**
     * Encapsulation of access to native strings.
     * <p>
     * This class takes care of freeing the memory that was natively allocated for the string.
     * <p>
     * FIXME for all I know right now, some or all of this can be replaced by MarshalAs annotations on the native binding
     *       but this works just fine for now
     */
    public class NativeString {

        /**
         * Create a new string pointer for a string.
         * <p>
         * The returned pointer is suitable for passing to native code.
         * <p>
         * When no longer needed, the pointer should be explicitly released.
         * 
         * @param str string
         * @return string pointer
         */
        public static IntPtr StringPointer(string str) {
            if(str != null) {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                Marshal.WriteByte(ptr, bytes.Length, 0);
                return ptr;
            }
            else {
                return IntPtr.Zero;
            }
        }

        /**
         * Free a previously allocated string pointer.
         * 
         * @param ptr previously allocated string pointer, may be <code>null</code>
         */
        public static void Release(IntPtr ptr) {
            if(ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /**
         * Get a string from a native string pointer.
         * <p>
         * The native string pointer is <em>not</em> freed.
         * 
         * @param pointer pointer to native string, may be <code>null</code>
         * @return string, or <code>null</code> if the pointer was <code>null</code>
         */
        public static string String(IntPtr pointer) {
            if(pointer != IntPtr.Zero) {
                return Marshal.PtrToStringAnsi(pointer); // FIXME why ANSI? why not Uni?
            }
            else {
                return null;
            }
        }

        /**
         * Get a string from a native string pointer, freeing the native string pointer when done.
         * <p>
         * If the native string pointer is not freed then a native memory leak will occur.
         * 
         * @param pointer pointer to native string, may be <code>null</code>
         * @return string, or <code>null</code> if the pointer was <code>null</code>
         */
        public static string GetNativeString(IntPtr pointer) {
            if(pointer != IntPtr.Zero) {
                string result =  Marshal.PtrToStringAnsi(pointer); // FIXME why ANSI? why not Uni?
                try{
					Release(pointer);
				}
				catch{
				
				}
                return result;
            }
            else {
                return null;
            }
        }
    }
}
