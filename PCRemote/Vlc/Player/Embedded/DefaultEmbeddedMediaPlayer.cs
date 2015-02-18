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
     * Implementation of a media player that renders video to an embedded component.
     */
    public class DefaultEmbeddedMediaPlayer : DefaultMediaPlayer, EmbeddedMediaPlayer {
        
        /**
         * Full-screen strategy implementation, may be <code>null</code>.
         */
        private readonly FullScreenStrategy fullScreenStrategy;
        
        /**
         * Component to render the video to.
         */
        private ComponentIdVideoSurface videoSurface;
        
        /**
         * Create a new media player.
         * <p>
         * Full-screen will not be supported.
         *
         * @param instance libvlc instance
         */
        public DefaultEmbeddedMediaPlayer(IntPtr instance) : this(instance, null) {
        }

        /**
         * Create a new media player.
         *
         * @param instance libvlc instance
         * @param fullScreenStrategy
         */
        public DefaultEmbeddedMediaPlayer(IntPtr instance, FullScreenStrategy fullScreenStrategy) : base(instance) {
            this.fullScreenStrategy = fullScreenStrategy;
        }

        public void SetVideoSurface(ComponentIdVideoSurface videoSurface) {
            Logger.Debug("SetVideoSurface(videoSurface={})", videoSurface);
            // Keep a hard reference to the video surface component
            this.videoSurface = videoSurface;
            // The video surface is not actually attached to the media player until the
            // media is played
        }
    
        public void AttachVideoSurface() {
            Logger.Debug("AttachVideoSurface()");
            videoSurface.Attach(this);
        }

        public void ToggleFullScreen() {
            Logger.Debug("ToggleFullScreen()");
            if(fullScreenStrategy != null) {
                SetFullScreen(!fullScreenStrategy.IsFullScreenMode());
            }
        }

        public void SetFullScreen(bool fullScreen) {
            Logger.Debug("SetFullScreen(fullScreen={})", fullScreen);
            if(fullScreenStrategy != null) {
                if(fullScreen) {
                    fullScreenStrategy.EnterFullScreenMode();
                }
                else {
                    fullScreenStrategy.ExitFullScreenMode();
                }
            }
        }
    
        public bool IsFullScreen() {
            Logger.Debug("IsFullScreen()");
            if(fullScreenStrategy != null) {
                return fullScreenStrategy.IsFullScreenMode();
            }
            else {
                return false;
            }
        }
    
        public Bitmap GetVideoSurfaceContents() {
            Logger.Debug("GetVideoSurfaceContents()");
            // FIXME
            return null;
        }
    
        public void SetEnableMouseInputHandling(bool enable) {
            Logger.Debug("SetEnableMouseInputHandling(enable={})", enable);
            LibVlc.libvlc_video_set_mouse_input(MediaPlayerInstance(), enable ? 1 : 0);
        }
    
        public void SetEnableKeyInputHandling(bool enable) {
            Logger.Debug("SetEnableKeyInputHandling(enable={})", enable);
            LibVlc.libvlc_video_set_key_input(MediaPlayerInstance(), enable ? 1 : 0);
        }
    
        override protected void OnBeforePlay() {
            Logger.Debug("OnBeforePlay()");
            AttachVideoSurface();
        }
    }
}
