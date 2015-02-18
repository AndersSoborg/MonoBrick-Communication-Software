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

using Caprica.VlcSharp.Binding.Internal;
using Caprica.VlcSharp.Player;
using Caprica.VlcSharp.Player.List;

namespace Caprica.VlcSharp.Log {

    /**
     * Encapsulation of an audio media list player.
     * <p>
     * This component extends the {@link AudioMediaPlayerComponent} to incorporate a
     * {@link MediaListPlayer} and an associated {@link MediaList}.
     */
    public class AudioMediaListPlayerComponent : AudioMediaPlayerComponent, MediaListPlayerEventListener {
    
        /**
         * Media list player.
         */
        private readonly MediaListPlayer mediaListPlayer;
    
        /**
         * Media list.
         */
        private readonly MediaList.MediaList mediaList;
    
        /**
         * Construct a media list player component.
         */
        public AudioMediaListPlayerComponent() {
            // Create the native resources
            MediaPlayerFactory mediaPlayerFactory = GetMediaPlayerFactory();
            mediaListPlayer = mediaPlayerFactory.NewMediaListPlayer();
            mediaList = mediaPlayerFactory.NewMediaList();
            mediaListPlayer.SetMediaList(mediaList);
            mediaListPlayer.SetMediaPlayer(GetMediaPlayer());
            // Register listeners
            mediaListPlayer.AddMediaListPlayerEventListener(this);
            // Sub-class initialisation
            OnAfterConstruct();
        }
    
    	// FIXME final methods?
    	
        /**
         * Get the embedded media list player reference.
         * <p>
         * An application uses this handle to control the media player, add listeners and so on.
         * 
         * @return media list player
         */
        public MediaListPlayer GetMediaListPlayer() {
            return mediaListPlayer;
        }
    
        /**
         * Get the embedded media list reference.
         * 
         * @return media list
         */
        public MediaList.MediaList GetMediaList() {
            return mediaList;
        }
    
        protected new virtual void OnBeforeRelease() {
            OnBeforeReleaseComponent();
            mediaListPlayer.Release();
            mediaList.Release();
        }

        /**
         * Template method invoked before the media list player is released.
         */
        protected virtual void OnBeforeReleaseComponent() {
        }
    
        // === MediaListPlayerEventListener =========================================
    
        public virtual void Played(MediaListPlayer mediaListPlayer) {
        }
    
        public virtual void NextItem(MediaListPlayer mediaListPlayer, IntPtr item, string itemMrl) {
        }
    
        public virtual void Stopped(MediaListPlayer mediaListPlayer) {
        }
    
        public virtual void MediaMetaChanged(MediaListPlayer mediaListPlayer, int metaType) {
        }
    
        public virtual void MediaSubItemAdded(MediaListPlayer mediaListPlayer, IntPtr subItem) {
        }
    
        public virtual void MediaDurationChanged(MediaListPlayer mediaListPlayer, long newDuration) {
        }
    
        public virtual void MediaParsedChanged(MediaListPlayer mediaListPlayer, int newStatus) {
        }
    
        public virtual void MediaFreed(MediaListPlayer mediaListPlayer) {
        }
    
        public virtual void MediaStateChanged(MediaListPlayer mediaListPlayer, int newState) {
        }
    }
}
