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
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Caprica.VlcSharp.Binding;
using Caprica.VlcSharp.Binding.Internal;
using Caprica.VlcSharp.Player.Embedded;
using Caprica.VlcSharp.Player.List.Events;
using Caprica.VlcSharp.Util.Concurrent;

namespace Caprica.VlcSharp.Player.List {
    
    /**
     * Implementation of a media list player.
     */
    public class DefaultMediaListPlayer : AbstractMediaPlayer, MediaListPlayer {
    
        /**
         * Collection of event listeners.
         */
        private readonly List<MediaListPlayerEventListener> eventListenerList = new List<MediaListPlayerEventListener>();
    
        /**
         * Factory to create media player events from native events.
         */
        private readonly MediaListPlayerEventFactory eventFactory;
    
        /**
         * Background service to notify event listeners.
         */
        private readonly SingleThreadExecutor listenersService = new SingleThreadExecutor();
    
        /**
         * Native media player instance.
         */
        private IntPtr mediaListPlayerInstance;
    
        /**
         * Native event manager instance.
         */
        private IntPtr mediaListPlayerEventManager;
    
        /**
         * Native event call-back.
         */
        private VlcVideoPlayerCallbackDelegate callback;
    
        /**
         * Associated native media player instance.
         * <p>
         * This may be <code>null</code>.
         */
        private MediaPlayer mediaPlayer;
    
        /**
         * Mask of the native events that will cause notifications to be sent to listeners.
         */
    //    private int eventMask = MediaListPlayerEventType.ALL.value();
    
        /**
         * Media list.
         */
        private MediaList.MediaList mediaList;
    
        /**
         * Opaque reference to user/application-specific data associated with this media player.
         */
        private Object userData;
    
        /**
         * Set to true when the player has been released.
         */
    //    private final AtomicBoolean released = new AtomicBoolean();
    
        /**
         * Create a new media list player.
         * 
         * @param libvlc native library interface
         * @param instance libvlc instance
         */
        public DefaultMediaListPlayer(IntPtr instance) : base(instance) {
            Logger.Debug("DefaultMediaListPlayer(instance={})", instance);
            this.eventFactory = new MediaListPlayerEventFactory(this);
            CreateInstance();
        }
    
        public void AddMediaListPlayerEventListener(MediaListPlayerEventListener listener) {
            Logger.Debug("AddMediaPlayerEventListener(listener={})", listener);
            eventListenerList.Add(listener);
        }
    
        public void RemoveMediaListPlayerEventListener(MediaListPlayerEventListener listener) {
            Logger.Debug("RemoveMediaPlayerEventListener(listener={})", listener);
            eventListenerList.Remove(listener);
        }
    
        public void EnableEvents(int eventMask) {
            Logger.Debug("EnableEvents(eventMask={})", eventMask);
    //        this.eventMask = eventMask;
        }
    
        public void SetMediaPlayer(MediaPlayer mediaPlayer) {
            Logger.Debug("SetMediaPlayer(mediaPlayer={})", mediaPlayer);
            this.mediaPlayer = mediaPlayer;
            LibVlc.libvlc_media_list_player_set_media_player(mediaListPlayerInstance, mediaPlayer.MediaPlayerInstance());
        }
    
        public void SetMediaList(MediaList.MediaList mediaList) {
            Logger.Debug("SetMediaList(mediaList={})", mediaList);
//            LibVlc.libvlc_media_list_player_set_media_list(mediaListPlayerInstance, mediaList.MediaListInstance());
            this.mediaList = mediaList;
        }
    
        public MediaList.MediaList GetMediaList() {
            Logger.Debug("GetMediaList()");
            return mediaList;
        }
    
        public void Play() {
            Logger.Debug("Play()");
            // If there is an associated media player then make sure the video surface
            // is attached
            if(mediaPlayer is EmbeddedMediaPlayer) {
                ((EmbeddedMediaPlayer)mediaPlayer).AttachVideoSurface();
            }
            LibVlc.libvlc_media_list_player_play(mediaListPlayerInstance);
        }
    
        public void Pause() {
            Logger.Debug("Pause()");
            LibVlc.libvlc_media_list_player_pause(mediaListPlayerInstance);
        }
    
        public void Stop() {
            Logger.Debug("Stop()");
            LibVlc.libvlc_media_list_player_stop(mediaListPlayerInstance);
        }
    
        public bool PlayItem(int itemIndex) {
            Logger.Debug("PlayItem(itemIndex={})", itemIndex);
            return LibVlc.libvlc_media_list_player_play_item_at_index(mediaListPlayerInstance, itemIndex) == 0;
        }
    
        public void PlayNext() {
            Logger.Debug("PlayNext()");
            LibVlc.libvlc_media_list_player_next(mediaListPlayerInstance);
        }
    
        public void PlayPrevious() {
            Logger.Debug("PlayPrevious()");
            LibVlc.libvlc_media_list_player_previous(mediaListPlayerInstance);
        }
    
        public bool IsPlaying() {
            Logger.Debug("IsPlaying()");
            return LibVlc.libvlc_media_list_player_is_playing(mediaListPlayerInstance) != 0;
        }
    
        public void SetMode(MediaListPlayerMode mode) {
            Logger.Debug("SetMode(mode={})", mode);
            libvlc_playback_mode_e playbackMode;
            switch(mode) {
                case MediaListPlayerMode.DEFAULT:
                    playbackMode = libvlc_playback_mode_e.libvlc_playback_mode_default;
                    break;
    
                case MediaListPlayerMode.LOOP:
                    playbackMode = libvlc_playback_mode_e.libvlc_playback_mode_loop;
                    break;
    
                case MediaListPlayerMode.REPEAT:
                    playbackMode = libvlc_playback_mode_e.libvlc_playback_mode_repeat;
                    break;
    
                default:
                    throw new ArgumentException("Invalid mode " + mode);
            }
            LibVlc.libvlc_media_list_player_set_playback_mode(mediaListPlayerInstance, (int)playbackMode);
        }
    
        public string Mrl(IntPtr mediaInstance) {
            Logger.Debug("Mrl(mediaInstance={})", mediaInstance);
            return NativeString.GetNativeString(LibVlc.libvlc_media_get_mrl(mediaInstance));
        }
    
        public Object UserData() {
            Logger.Debug("UserData()");
            return userData;
        }
    
        public void UserData(Object userData) {
            Logger.Debug("UserData(userData={})", userData);
            this.userData = userData;
        }
    
        public void Release() {
            Logger.Debug("Release()");
    //        if(released.compareAndSet(false, true)) {
                DestroyInstance();
    //        }
        }
    
        /**
         * 
         */
        private void CreateInstance() {
            Logger.Debug("CreateInstance()");
    
            mediaListPlayerInstance = LibVlc.libvlc_media_list_player_new(instance);
    
            mediaListPlayerEventManager = LibVlc.libvlc_media_list_player_event_manager(mediaListPlayerInstance);
            Logger.Debug("mediaListPlayerEventManager={}", mediaListPlayerEventManager);
    
            RegisterEventListener();
    
    //        AddMediaListPlayerEventListener(new NextItemHandler());
        }
    
        /**
         * 
         */
        private void DestroyInstance() {
            Logger.Debug("DestroyInstance()");
    
            Logger.Debug("Detach events...");
            DeregisterEventListener();
            Logger.Debug("Events detached.");
    
            eventListenerList.Clear();
            if(mediaListPlayerInstance != IntPtr.Zero) {
                Logger.Debug("Release media list player...");
                LibVlc.libvlc_media_list_player_release(mediaListPlayerInstance);
                Logger.Debug("Media list player released");
            }

            Logger.Debug("Shut down listeners...");
            listenersService.Shutdown();
            Logger.Debug("Listeners shut down.");
        }
    
        /**
         * 
         */
        private void RegisterEventListener() {
            Logger.Debug("RegisterEventListener()");
            callback = new VlcVideoPlayerCallbackDelegate(HandleEvent);
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                // The native event manager reports that it does not support
                // libvlc_MediaListPlayerPlayed or libvlc_MediaListPlayerStopped
                if(value >= (int)libvlc_event_e.libvlc_MediaListPlayerNextItemSet && value <= (int)libvlc_event_e.libvlc_MediaListPlayerNextItemSet) {
                    Logger.Debug("event={}", (libvlc_event_e)value);
                    int result = LibVlc.libvlc_event_attach(mediaListPlayerEventManager, value, callbackPtr, IntPtr.Zero);
                    Logger.Debug("result={}", result);
                }
            }
        }

        /**
         *
         */
        private void DeregisterEventListener() {
            Logger.Debug("DeregisterEventListener()");
            if(callback != null) {
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
                foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                    // The native event manager reports that it does not support
                    // libvlc_MediaListPlayerPlayed or libvlc_MediaListPlayerStopped
                    if(value >= (int)libvlc_event_e.libvlc_MediaListPlayerNextItemSet && value <= (int)libvlc_event_e.libvlc_MediaListPlayerNextItemSet) {
                        Logger.Debug("event={}", (libvlc_event_e)value);
                        LibVlc.libvlc_event_detach(mediaListPlayerEventManager, value, callbackPtr, IntPtr.Zero);
                    }
                }
                callback = null;
            }
        }
    
        private void HandleEvent(IntPtr evt, IntPtr data) {
            if(eventListenerList.Count > 0) {
                libvlc_event_t e = (libvlc_event_t)Marshal.PtrToStructure(evt, typeof(libvlc_event_t));
                // Create a new media player event for the native event
    //                MediaListPlayerEvent mediaListPlayerEvent = eventFactory.newMediaListPlayerEvent(e, eventMask);
                MediaListPlayerEvent mediaListPlayerEvent = eventFactory.newMediaListPlayerEvent(e, 0);
                Logger.Trace("mediaListPlayerEvent={}", mediaListPlayerEvent);
                if(mediaListPlayerEvent != null) {
                    listenersService.Submit(new NotifyListenersRunnable(this, mediaListPlayerEvent));
                }
            }
        } // FIXME refactor part to separate raiseEvent like main player?
        
        // FIXME best place for this? proper decl for this, what exactly is needed?
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VlcVideoPlayerCallbackDelegate(IntPtr evt, IntPtr userData);

        /**
         * FIXME visibility
         */
        public class NotifyListenersRunnable : Runnable {
    
            /**
             *
             */
            private readonly DefaultMediaListPlayer mediaListPlayer;
            
            /**
             * 
             */
            private readonly MediaListPlayerEvent mediaListPlayerEvent;
    
            /**
             * 
             * 
             * @param mediaListPlayer
             * @param mediaListPlayerEvent
             */
            public NotifyListenersRunnable(DefaultMediaListPlayer mediaListPlayer, MediaListPlayerEvent mediaListPlayerEvent) {
                this.mediaListPlayer = mediaListPlayer;
                this.mediaListPlayerEvent = mediaListPlayerEvent;
            }
    
            public void Run() {
                Logger.Trace("run()");
                for(int i = mediaListPlayer.eventListenerList.Count - 1; i >= 0; i -- ) {
                    MediaListPlayerEventListener listener = mediaListPlayer.eventListenerList[i];
                    try {
                        mediaListPlayerEvent.Notify(listener);
                    }
                    catch(Exception t) {
                        Logger.Warn("Event listener {} threw an exception", t, listener);
                        // Continue with the next listener...
                    }
                }
                Logger.Trace("runnable exits");
            }
        }

        /**
         * A call-back to handle events from the native media player.
         * <p>
         * There are some important implementation details for this callback:
         * <ul>
         * <li>First, the event notifications are off-loaded to a different thread so as to prevent
         * application code re-entering libvlc in an event call-back which may lead to a deadlock in the
         * native code;</li>
         * <li>Second, the native event union structure refers to natively allocated memory which will
         * not be in the scope of the thread used to actually dispatch the event notifications.</li>
         * </ul>
         * Without copying the fields at this point from the native union structure, the native memory
         * referred to by the native event is likely to get deallocated and overwritten by the time the
         * notification thread runs. This would lead to unreliable data being sent with the
         * notification, or even a fatal JVM crash.
         */
    /*    private class VlcVideoPlayerCallback implements libvlc_callback_t {
    
            public void callback(libvlc_event_t event, Pointer userData) {
                Logger.trace("callback(event={},userData={})", event, userData);
                if(!eventListenerList.isEmpty()) {
                    // Create a new media player event for the native event
                    MediaListPlayerEvent mediaListPlayerEvent = eventFactory.newMediaListPlayerEvent(event, eventMask);
                    Logger.trace("mediaListPlayerEvent={}", mediaListPlayerEvent);
                    if(mediaListPlayerEvent != null) {
                        listenersService.submit(new NotifyListenersRunnable(mediaListPlayerEvent));
                    }
                }
            }
        }
     */
    /*    protected void finalize() throws Throwable {
            Logger.Debug("finalize()");
            Logger.Debug("Media list player has been garbage collected");
        }*/
    }
}
