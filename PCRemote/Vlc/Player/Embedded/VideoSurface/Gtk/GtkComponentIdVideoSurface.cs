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
using System.Runtime.InteropServices;

using Gtk;

namespace Caprica.VlcSharp.Player.Embedded.VideoSurface.Gtk {

    /**
     * Implementation of a video surface that renders video into a GTK widget.
     * <p>
     * This implementation uses the GTK native library to identify the unique
     * window identifier of the widget.
     */
    public class GtkComponentIdVideoSurface : ComponentIdVideoSurface {

        /**
         * Create a new video surface.
         * 
         * @param widget widget into which the video will be rendered
         * @param videoSurfaceAdapter video surface adapter
         */
        public GtkComponentIdVideoSurface(Widget widget, VideoSurfaceAdapter videoSurfaceAdapter) : base(Wid(widget), videoSurfaceAdapter) {
        }

        /**
         * Get a native window identifier for a widget.
         * 
         * @param widget widget
         * @return widget id, or zero if the id could not be determined
         */
        private static Int32 Wid(Widget widget) {
			/*IntPtr windowPtr =  gtk_widget_get_window(widget.Handle);
			if(windowPtr != IntPtr.Zero) {
                IntPtr xidPtr = gdk_x11_drawable_get_xid(windowPtr);
				if(xidPtr != IntPtr.Zero) {
                    return xidPtr.ToInt32();
                }
			}
            return 0;*/
			IntPtr windowPtr = gdk_win32_drawable_get_handle(widget.GdkWindow.Handle);
			return windowPtr.ToInt32();

        }

        /**
         * Get the native window handle for a widget handle.
         *
         * @param widget widget handle
         * @return native window handle
         */
        [DllImport("gtk-x11-2.0")]
        private static extern IntPtr gtk_widget_get_window(IntPtr widget);

        /**
         * Get the native drawable window identifier for a native window handle.
         *
         * @param drawable native window handle
         * @return native drawable identifier
         */
        [DllImport("gdk-x11-2.0")]
        private static extern IntPtr gdk_x11_drawable_get_xid(IntPtr drawable);

		[DllImport ("libgdk-win32-2.0-0.dll")]
		private static extern IntPtr gdk_win32_drawable_get_handle (IntPtr drawable);
    }
}
