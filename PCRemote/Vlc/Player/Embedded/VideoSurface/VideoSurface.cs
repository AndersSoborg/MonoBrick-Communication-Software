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

namespace Caprica.VlcSharp.Player.Embedded.VideoSurface {
 
    /**
     * Encapsulation of a video surface.
     */
    public abstract class VideoSurface {

        /**
         * Operating System specific video surface adapter implementation.
         */
        protected readonly VideoSurfaceAdapter videoSurfaceAdapter;

        /**
         * Create a new video surface wrapper.
         *
         * @param videoSurfaceAdapter video surface adapter implementation
         */
        public VideoSurface(VideoSurfaceAdapter videoSurfaceAdapter) {
            this.videoSurfaceAdapter = videoSurfaceAdapter;
        }

        /**
         * Attach the video surface to a media player.
         *
         * @param mediaPlayer media player instance
         */
        public abstract void Attach(DefaultMediaPlayer mediaPlayer);
    }
}
