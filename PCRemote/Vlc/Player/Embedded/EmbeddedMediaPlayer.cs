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
using System.Drawing;

using Caprica.VlcSharp.Binding;
using Caprica.VlcSharp.Player.Embedded;
using Caprica.VlcSharp.Player.Embedded.VideoSurface;

namespace Caprica.VlcSharp.Player.Embedded {
    
    /**
     * Specification for a media player component that is intended to be embedded in a user-interface
     * component.
     */
    public interface EmbeddedMediaPlayer : MediaPlayer {

        /**
         * Set the component used to render video.
         * <p>
         * Setting the video surface on the native component is actually deferred so the component used
         * as the video surface need <em>not</em> be visible and fully realised before calling this
         * method.
         * <p>
         * The video surface will not be associated with the native media player until the media is
         * played.
         * <p>
         * It is possible to change the video surface after it has been set, but the change will not
         * take effect until the media is played.
         * 
         * @param videoSurface component to render video to
         */
        void SetVideoSurface(ComponentIdVideoSurface videoSurface);
    
        /**
         * Ensure that the video surface has been associated with the native media player.
         * <p>
         * Ordinarily when setting the video surface the actual association of the video surface with
         * the native media player is deferred until the first time media is played.
         * <p>
         * This deferring behaviour is usually a good thing because when setting a video surface
         * component on the native media player the video surface component must be a displayable
         * component and this is often not the case during the construction and initialisation of the
         * application.
         * <p>
         * Most applications will not need to call this method.
         * <p>
         * However, in special circumstances such as associating an embedded media player with a media
         * list player, media is played through the media list rather than the media player itself so
         * the deferred association of the video surface would never happen.
         * <p>
         * Calling this method ensures that the video surface is properly associated with the native
         * media player and consequently the video surface component must be visible when this method is
         * called.
         */
        void AttachVideoSurface();
    
        /**
         * Toggle whether the video display is in full-screen or not.
         * <p>
         * Setting the display into or out of full-screen mode is delegate to the
         * {@link FullScreenStrategy} that was used when the media player was created.
         */
        void ToggleFullScreen();
    
        /**
         * Set full-screen mode.
         * <p>
         * Setting the display into or out of full-screen mode is delegate to the
         * {@link FullScreenStrategy} that was used when the media player was created.
         * 
         * @param fullScreen true for full-screen, otherwise false
         */
        void SetFullScreen(bool fullScreen);
    
        /**
         * Check whether the full-screen mode is currently active or not.
         * <p>
         * Testing whether or not the display is in full-screen mode is delegate to the
         * {@link FullScreenStrategy} that was used when the media player was created.
         * 
         * @return true if full-screen is active, otherwise false
         */
        bool IsFullScreen();

        /**
         * Set whether or not to enable native media player mouse input handling.
         * 
         * @param enable <code>true</code> to enable, <code>false</code> to disable
         */
        void SetEnableMouseInputHandling(bool enable);
    
        /**
         * Set whether or not to enable native media player keyboard input handling.
         * 
         * @param enable <code>true</code> to enable, <code>false</code> to disable
         */
        void SetEnableKeyInputHandling(bool enable);
    }
}
