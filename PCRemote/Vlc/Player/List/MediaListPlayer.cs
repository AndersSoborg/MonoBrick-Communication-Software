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

using Caprica.VlcSharp.MediaList;

namespace Caprica.VlcSharp.Player.List {
    
    /**
     * Specification for a media list player component.
     * <p>
     * A media list player can be used with an embedded media player (without this a native video window
     * will be opened when video is played). For example:
     * 
     * <pre>
     * MediaPlayerFactory mediaPlayerFactory = new MediaPlayerFactory();
     * 
     * Canvas canvas = new Canvas();
     * canvas.setBackground(Color.black);
     * CanvasVideoSurface videoSurface = mediaPlayerFactory.newVideoSurface(canvas);
     * 
     * EmbeddedMediaPlayer mediaPlayer = mediaPlayerFactory.newEmbeddedMediaPlayer();
     * mediaPlayer.setVideoSurface(videoSurface);
     * 
     * MediaListPlayer mediaListPlayer = mediaPlayerFactory.newMediaListPlayer();
     * 
     * // Important: associate the media player with the media list player
     * mediaListPlayer.setMediaPlayer(mediaPlayer);
     * 
     * MediaList mediaList = mediaPlayerFactory.newMediaList();
     * mediaList.addMedia(&quot;/movies/1.mp4&quot;);
     * mediaList.addMedia(&quot;/movies/2.mp4&quot;);
     * mediaList.addMedia(&quot;/movies/3.mp4&quot;);
     * 
     * mediaListPlayer.setMediaList(mediaList);
     * mediaListPlayer.setMode(MediaListPlayerMode.LOOP);
     *
     * mediaListPlayer.play();
     * </pre>
     */
    public interface MediaListPlayer {
    
        /**
         * Add a component to be notified of media list player events.
         * 
         * @param listener component to notify
         */
        void AddMediaListPlayerEventListener(MediaListPlayerEventListener listener);
    
        /**
         * Remove a component that was previously interested in notifications of media list player
         * events.
         * 
         * @param listener component to stop notifying
         */
        void RemoveMediaListPlayerEventListener(MediaListPlayerEventListener listener);
    
        /**
         * Restrict the set of media list player events that generate event notifications to listeners.
         * <p>
         * If a set of events is not explicitly enabled, then it is expected that <strong>all</strong>
         * events be enabled.
         * <p>
         * See {@link MediaListPlayerEventType}.
         * 
         * @param eventMask bit mask of events to enable
         */
        void EnableEvents(int eventMask);
    
        /**
         * Associate an actual media player with the media list player.
         * 
         * @param mediaPlayer media player
         */
        void SetMediaPlayer(MediaPlayer mediaPlayer);
    
        /**
         * Set the media list (i.e. the "play" list).
         * 
         * @param mediaList media list
         */
        void SetMediaList(MediaList.MediaList mediaList);
    
        /**
         * Get the media list.
         * 
         * @return media list
         */
        MediaList.MediaList GetMediaList();
    
        /**
         * Play the media list.
         */
        void Play();
    
        /**
         * Pause the media list.
         */
        void Pause();
    
        /**
         * Stop the media list.
         */
        void Stop();

        /**
         * Play a particular item on the media list.
         * <p>
         * <strong>There is a bug in vlc that prevents proper operation of this method,
         * and may cause a fatal JVM failure. This is resolved in vlc 2.0.2 and later.</strong>
         * 
         * @param itemIndex index of the item to play
         * @return <code>true</code> if the item could be played, otherwise <code>false</code>
         */
        bool PlayItem(int itemIndex);
    
        /**
         * Play the next item in the media list.
         */
        void PlayNext();
    
        /**
         * Play the previous item in the media list.
         */
        void PlayPrevious();
    
        /**
         * Determine whether or not the media list is playing.
         * 
         * @return <code>true</code> if playing, otherwise <code>false</code>
         */
        bool IsPlaying();
    
        /**
         * Set the media list play mode.
         * <p>
         * Note that if you set the play mode to REPEAT before you have played any media then play-back
         * will never start.
         *
         * @param mode mode
         */
        void SetMode(MediaListPlayerMode mode);
    
        /**
         * Get the media resource locator for a media instance.
         * <p>
         * The native media instance may be an automatically/scripted added sub-item.
         * 
         * @param mediaInstance native media instance
         * @return URL-encoded media resource locator
         */
        string Mrl(IntPtr mediaInstance);
    
        /**
         * Get the user data associated with the media player.
         * 
         * @return user data
         */
        Object UserData();
    
        /**
         * Set user data to associate with the media player.
         * 
         * @param userData user data
         */
        void UserData(Object userData);
    
        /**
         * Release the media list player resources.
         */
        void Release();
    }
}
