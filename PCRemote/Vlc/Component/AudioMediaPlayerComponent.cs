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

namespace Caprica.VlcSharp.Log {

    /**
     * Encapsulation of an audio player.
     * <p>
     * Most implementation details, like creating a factory, are encapsulated.
     * <p>
     * The default implementation will work out-of-the-box, but there are various template methods
     * available to sub-classes to tailor the behaviour of the component.
     * <p>
     * This class implements the most the most common use-case for an audio player and is intended to
     * enable a developer to get quickly started with the vlcj framework. More advanced applications are
     * free to directly use the {@link MediaPlayerFactory}, if required, as has always been the case.
     * <p>
     * This component also adds implements the various media player listener interfaces, consequently an
     * implementation sub-class can simply override those listener methods to handle events.
     * <p>
     * Applications can get a handle to the underlying media player object by invoking
     * {@link #getMediaPlayer()}.
     * <p>
     * To use, simply create an instance of this class.
     * <p>
     * In this minimal example, only two lines of code are required to create an audio player and play
     * media:
     * 
     * <pre>
     * mediaPlayerComponent = new AudioMediaPlayerComponent(); // &lt;--- 1
     * mediaPlayerComponent.getMediaPlayer().playMedia(mrl); // &lt;--- 2
     * </pre>
     * 
     * This is not quite as useful as the {@link EmbeddedMediaPlayerComponent} as audio players are
     * generally quite simple to create anyway.
     * <p>
     * An audio player may still have a user interface, but it will not have an associated video
     * surface.
     */
    public class AudioMediaPlayerComponent : MediaPlayerEventAdapter {
    
        /**
         * Default factory initialisation arguments.
         * <p>
         * Sub-classes may totally disregard these arguments and provide their own.
         * <p>
         * A sub-class has access to these default arguments so new ones could be merged with these if
         * required.
         */
        protected static readonly string[] DEFAULT_FACTORY_ARGUMENTS = {
            "--no-plugins-cache", 
            "--quiet", 
            "--quiet-synchro", 
            "--intf", 
            "dummy"
        };
    
        /**
         * Media player factory.
         */
        private readonly MediaPlayerFactory mediaPlayerFactory;
    
        /**
         * Media player.
         */
        private readonly MediaPlayer mediaPlayer;
    
        /**
         * Construct a media player component.
         */
        public AudioMediaPlayerComponent() {
            mediaPlayerFactory = OnGetMediaPlayerFactory();
            mediaPlayer = mediaPlayerFactory.NewHeadlessMediaPlayer();
            // Sub-class initialisation
            OnAfterConstruct();
            // Register listeners
            mediaPlayer.AddMediaPlayerEventListener(this);
        }
    
        // FIXME final methods?

        /**
         * Get the media player factory reference.
         *
         * @return media player factory
         */
        public MediaPlayerFactory GetMediaPlayerFactory() {
            return mediaPlayerFactory;
        }

        /**
         * Get the embedded media player reference.
         * <p>
         * An application uses this handle to control the media player, add listeners and so on.
         * 
         * @return media player
         */
        public MediaPlayer GetMediaPlayer() {
            return mediaPlayer;
        }
    
        /**
         * Release the media player component and the associated native media player resources.
         */
        public void Release() {
            OnBeforeRelease();
            mediaPlayer.Release();
            mediaPlayerFactory.Release();
            OnAfterRelease();
        }
    
        /**
         * Template method to create a media player factory.
         * <p>
         * The default implementation will invoke the {@link #onGetMediaPlayerFactoryArgs()} template
         * method.
         * <p>
         * When this component is released via {@link #release()} the factory instance returned by this
         * method will also be released.
         * 
         * @return media player factory
         */
        protected MediaPlayerFactory OnGetMediaPlayerFactory() {
            return new MediaPlayerFactory(OnGetMediaPlayerFactoryArgs());
        }
    
        /**
         * Template method to obtain the initialisation arguments used to create the media player
         * factory instance.
         * <p>
         * If a sub-class overrides the {@link #onGetMediaPlayerFactory()} template method there is no
         * guarantee that {@link #onGetMediaPlayerFactoryArgs()} will be called.
         * 
         * @return media player factory initialisation arguments
         */
        protected String[] OnGetMediaPlayerFactoryArgs() {
            return DEFAULT_FACTORY_ARGUMENTS;
        }
    
        /**
         * Template method invoked at the end of the media player constructor.
         */
        protected void OnAfterConstruct() {
        }
        
        /**
         * Template method invoked immediately prior to releasing the media player and media player
         * factory instances.
         */
        protected void OnBeforeRelease() {
        }
    
        /**
         * Template method invoked immediately after releasing the media player and media player factory
         * instances.
         */
        protected void OnAfterRelease() {
        }
    }
}
