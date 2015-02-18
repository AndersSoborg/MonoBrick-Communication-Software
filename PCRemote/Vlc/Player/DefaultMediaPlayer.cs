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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

using Caprica.VlcSharp.Binding;
using Caprica.VlcSharp.Binding.Internal;
using Caprica.VlcSharp.Player.Events;
using Caprica.VlcSharp.Util.Concurrent;

namespace Caprica.VlcSharp.Player {

    /**
     * Media player implementation.
     */
    public class DefaultMediaPlayer : AbstractMediaPlayer, MediaPlayer {

        /**
         * Collection of media player event listeners.
         */
        private readonly List<MediaPlayerEventListener> eventListenerList = new List<MediaPlayerEventListener>();
        
        /**
         * Factory to create media player events from native events.
         */
        private readonly MediaPlayerEventFactory eventFactory;
        
        /**
         * Background thread to send event notifications to listeners.
         * <p>
         * The single-threaded nature of this executor service ensures that events are delivered to
         * listeners in a thread-safe manner and in their proper sequence.
         */
        private readonly SingleThreadExecutor listenersService = new SingleThreadExecutor();
        
        /**
         * Native media player instance.
         */
        private IntPtr mediaPlayerInstance;
        
        /**
         * Native media player event manager.
         */
        private IntPtr mediaPlayerEventManager;
        
        /**
         * Call-back to handle native media player events.
         */
        private VlcVideoPlayerCallbackDelegate callback;
        
        /**
         * Native media instance for current media (if there is one).
         */
        private IntPtr mediaInstance;
        
        /**
         * Standard options to be applied to all played media.
         */
        private string[] standardMediaOptions;
        
        /**
         * Native media statistics for the current media.
         */
        private libvlc_media_stats_t libvlcMediaStats;
        
        /**
         * Flag whether or not to automatically replay media after the media has finished playing.
         */
        private bool repeat;
        
        /**
         * Flag whether or not to automatically play media sub-items if there are any.
         */
        private bool playSubItems;
        
        /**
         * Index of the current sub-item, or -1.
         */
        private int subItemIndex;
        
        /**
         * Optional name of the directory to save video snapshots to.
         * <p>
         * If this is not set then snapshots will be saved to the user home directory.
         */
        private string snapshotDirectoryName;
        
        /**
         * Opaque reference to user/application-specific data associated with this media player.
         */
        private Object userData;
        
        /**
         * Set to true when the player has been released.
         */
        //    private readonly AtomicBoolean released = new AtomicBoolean();
        
        /**
         * Create a new media player.
         * 
         * @param instance libvlc instance
         */
        public DefaultMediaPlayer(IntPtr instance) : base(instance) {
            this.eventFactory = new MediaPlayerEventFactory(this);
            CreateInstance();
        }
        
        public void AddMediaPlayerEventListener(MediaPlayerEventListener listener) {
            Logger.Debug("AddMediaPlayerEventListener(listener={})", listener);
            eventListenerList.Add(listener);
        }
        
        public void RemoveMediaPlayerEventListener(MediaPlayerEventListener listener) {
            Logger.Debug("RemoveMediaPlayerEventListener(listener={})", listener);
            eventListenerList.Remove(listener);
        }

        public void EnableEvents(int eventMask) {
            Logger.Debug("EnableEvents(eventMask={})", eventMask);
        //        this.eventMask = eventMask;
        }
        
        // === Media Controls =======================================================
        
        public void SetStandardMediaOptions(params string[] options) {
            Logger.Debug("SetStandardMediaOptions(options={})", options);
            this.standardMediaOptions = options;
        }
        
        public bool PlayMedia(string mrl, params string[] mediaOptions) {
            Logger.Debug("PlayMedia(mrl={},mediaOptions={})", mrl, mediaOptions);
            // First 'prepare' the media...
            if(PrepareMedia(mrl, mediaOptions)) {
                // ...then play it
                Play();
                return true;
            }
            else {
                return false;
            }

        }
        
        public bool PrepareMedia(string mrl, params string[] mediaOptions) {
            Logger.Debug("PrepareMedia(mrl={},mediaOptions={})", mrl, mediaOptions);
            return SetMedia(mrl, mediaOptions);
        }
        
        public bool StartMedia(string mrl, params string[] mediaOptions) {
            Logger.Debug("StartMedia(mrl={},mediaOptions={})", mrl, mediaOptions);
            // First 'prepare' the media...
            PrepareMedia(mrl, mediaOptions);
            // ...then play it and wait for it to start (or error)
        //        return new MediaPlayerLatch(this).play();
            return true; // FIXME
        }

        public void ParseMedia() {
            Logger.Debug("ParseMedia()");
            if(mediaInstance != IntPtr.Zero) {
                LibVlc.libvlc_media_parse(mediaInstance);
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }
        
        public void RequestParseMedia() {
            Logger.Debug("RequestParseMedia()");
            if(mediaInstance != IntPtr.Zero) {
                LibVlc.libvlc_media_parse_async(mediaInstance);
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }
        
        public bool IsMediaParsed() {
            Logger.Debug("IsMediaParsed()");
            if(mediaInstance != IntPtr.Zero) {
                return 0 != LibVlc.libvlc_media_is_parsed(mediaInstance);
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }

        public MediaMeta GetMediaMeta() {
            Logger.Debug("GetMediaMeta()");
            return GetMediaMeta(mediaInstance);
        }
        
        public MediaMeta GetMediaMeta(IntPtr media) {
            Logger.Debug("GetMediaMeta(media={})", media);
            if(media != IntPtr.Zero) {
                return new DefaultMediaMeta(media);
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }
        
        public List<MediaMeta> GetSubItemMediaMeta() {
        /*        return handleSubItems(new SubItemsHandler<List<MediaMeta>>() {
                public List<MediaMeta> subItems(int count, libvlc_media_list_t subItems) {
                    List<MediaMeta> result = new ArrayList<MediaMeta>(count);
                    for(libvlc_media_t subItem : new LibVlcMediaListIterator(libvlc, subItems)) {
                        result.add(getMediaMeta(subItem));
                    }
                    return result;
                }
            });*/
            return null;
        }

        public void AddMediaOptions(params string[] mediaOptions) {
            Logger.Debug("AddMediaOptions(mediaOptions={})", mediaOptions);
            if(mediaInstance != IntPtr.Zero) {
                AddMediaOptions(mediaInstance, mediaOptions);
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }
        
        public void SetRepeat(bool repeat) {
            Logger.Debug("SetRepeat(repeat={})", repeat);
            this.repeat = repeat;
        }

        public bool GetRepeat() {
            Logger.Debug("GetRepeat()");
            return repeat;
        }
        
        // === Sub-Item Controls ====================================================
        
        public void SetPlaySubItems(bool playSubItems) {
            Logger.Debug("SetPlaySubItems(playSubItems={})", playSubItems);
            this.playSubItems = playSubItems;
        }
        
        public int SubItemCount() {
            Logger.Debug("SubItemCount()");
        /*        return handleSubItems(new SubItemsHandler<Integer>() {
                public Integer subItems(int count, libvlc_media_list_t subItems) {
                    return count;
                }
            });*/
            return -1;
        }
        
        public int SubItemIndex() {
            Logger.Debug("SubItemIndex()");
            return subItemIndex;
        }
        
        public List<string> SubItems() {
            Logger.Debug("SubItems()");
        /*        return handleSubItems(new SubItemsHandler<List<String>>() {
                public List<String> subItems(int count, libvlc_media_list_t subItems) {
                    List<String> result = new ArrayList<String>(count);
                    for(libvlc_media_t subItem : new LibVlcMediaListIterator(libvlc, subItems)) {
                        result.add(NativeString.getNativeString(libvlc, LibVlc.libvlc_media_get_mrl(subItem)));
                    }
                    return result;
                }
            });*/
            return null;
        }
        
        public List<IntPtr> SubItemsMedia() {
            Logger.Debug("SubItemsMedia()");
        /*        return handleSubItems(new SubItemsHandler<List<libvlc_media_t>>() {
                
                public List<libvlc_media_t> subItems(int count, libvlc_media_list_t subItems) {
                    List<libvlc_media_t> result = new ArrayList<libvlc_media_t>(count);
                    for(libvlc_media_t subItem : new LibVlcMediaListIterator(libvlc, subItems)) {
                        result.add(subItem);
                    }
                    return result;
                }
            });*/
            return null;
        }
        
        public bool PlayNextSubItem(params string[] mediaOptions) {
            Logger.Debug("PlayNextSubItem(mediaOptions={})", mediaOptions);
            return PlaySubItem(subItemIndex + 1, mediaOptions);
        }
        
        public bool PlaySubItem(int index, params string[] mediaOptions) {
//        Logger.Debug("PlaySubItem(index={},mediaOptions={})", index, Arrays.toString(mediaOptions));
        /*        return handleSubItems(new SubItemsHandler<bool>() {
                public bool subItems(int count, libvlc_media_list_t subItems) {
                    if(subItems != null) {
                        Logger.Debug("Handling media sub-item...");
                        // Advance the current sub-item (initially it will be -1)...
                        Logger.Debug("count={}", count);
                        subItemIndex = index;
                        Logger.Debug("subItemIndex={}", subItemIndex);
                        // If the last sub-item already been played...
                        if(subItemIndex >= count) {
                            Logger.Debug("End of sub-items reached");
                            if(!repeat) {
                                Logger.Debug("Do not repeat sub-items");
                                subItemIndex = -1;
                                Logger.Debug("Raising events for end of sub-items");
                                void Menu(eventFactory.createMediaEndOfSubItemsEvent(eventMask));
                            }
                            else {
                                Logger.Debug("Repeating sub-items");
                                subItemIndex = 0;
                            }
                        }
                        if(subItemIndex != -1) {
                            // Get the required sub item from the list
                            libvlc_media_t subItem = LibVlc.libvlc_media_list_item_at_index(subItems, subItemIndex);
                            // If there is an item to play...
                            if(subItem != null) {
                                // Set the sub-item as the new media for the media player
                                LibVlc.libvlc_media_player_set_media(mediaPlayerInstance, subItem);
                                // Set any standard media options
                                if(standardMediaOptions != null) {
                                    for(String standardMediaOption : standardMediaOptions) {
                                        Logger.Debug("standardMediaOption={}", standardMediaOption);
                                        LibVlc.libvlc_media_add_option(subItem, standardMediaOption);
                                    }
                                }
                                // Set any media options
                                if(mediaOptions != null) {
                                    for(String mediaOption : mediaOptions) {
                                        Logger.Debug("mediaOption={}", mediaOption);
                                        LibVlc.libvlc_media_add_option(subItem, mediaOption);
                                    }
                                }
                                // Play the media
                                LibVlc.libvlc_media_player_play(mediaPlayerInstance);
                                // Release the sub-item
                                LibVlc.libvlc_media_release(subItem);
                                // Raise a semantic event to announce the sub-item was played
                                Logger.Debug("Raising played event for sub-item {}", subItemIndex);
                                raiseEvent(eventFactory.createMediaSubItemPlayedEvent(subItemIndex, eventMask));
                                // A sub-item was played
                                return true;
                            }
                        }
                    }
                    // A sub-item was not played
                    return false;
                }
            });*/
            return false;
        }
        
        // === Status Controls ======================================================
        
        public bool IsPlayable() {
            Logger.Trace("IsPlayable()");
            return LibVlc.libvlc_media_player_will_play(mediaPlayerInstance) == 1;
        }
        
        public bool IsPlaying() {
            Logger.Trace("IsPlaying()");
            return LibVlc.libvlc_media_player_is_playing(mediaPlayerInstance) == 1;
        }
        
        public bool IsSeekable() {
            Logger.Trace("IsSeekable()");
            return LibVlc.libvlc_media_player_is_seekable(mediaPlayerInstance) == 1;
        }
        
        public bool CanPause() {
            Logger.Trace("CanPause()");
            return LibVlc.libvlc_media_player_can_pause(mediaPlayerInstance) == 1;
        }
        
        public long GetLength() {
            Logger.Trace("GetLength()");
            return LibVlc.libvlc_media_player_get_length(mediaPlayerInstance);
        }
        
        public long GetTime() {
            Logger.Trace("GetTime()");
            return LibVlc.libvlc_media_player_get_time(mediaPlayerInstance);
        }
        
        public float GetPosition() {
            Logger.Trace("GetPosition()");
            return LibVlc.libvlc_media_player_get_position(mediaPlayerInstance);
        }

        public float GetFps() {
            Logger.Trace("GetFps()");
            return LibVlc.libvlc_media_player_get_fps(mediaPlayerInstance);
        }
        
        public float GetRate() {
            Logger.Trace("GetRate()");
            return LibVlc.libvlc_media_player_get_rate(mediaPlayerInstance);
        }
        
        public int GetVideoOutputs() {
            Logger.Trace("GetVideoOutputs()");
            return LibVlc.libvlc_media_player_has_vout(mediaPlayerInstance);
        }
        
        public Size GetVideoDimension() {
            Logger.Debug("GetVideoDimension()");
            if(GetVideoOutputs() > 0) {
                uint x = 0;
                uint y = 0;
                int result = LibVlc.libvlc_video_get_size(mediaPlayerInstance, 0, out x, out y);
                if(result == 0) {
                    return new Size((int)x, (int)y);
                }
                else {
                    Logger.Warn("Video size is not available");
                }
            }
            else {
                Logger.Warn("Can't get video dimension if no video output has been started");
            }
            return new Size(0, 0); // FIXME would prefer null
        }
        
        public MediaDetails GetMediaDetails() {
            Logger.Debug("GetMediaDetails()");
            // The media must be playing to get this meta data...
            if(IsPlaying()) {
                MediaDetails mediaDetails = new MediaDetails();
                mediaDetails.SetTitleCount(GetTitleCount());
                mediaDetails.SetVideoTrackCount(GetVideoTrackCount());
                mediaDetails.SetAudioTrackCount(GetAudioTrackCount());
                mediaDetails.SetSpuCount(GetSpuCount());
                mediaDetails.SetTitleDescriptions(GetTitleDescriptions());
                mediaDetails.SetVideoDescriptions(GetVideoDescriptions());
                mediaDetails.SetAudioDescriptions(GetAudioDescriptions());
                mediaDetails.SetSpuDescriptions(GetSpuDescriptions());
                mediaDetails.SetChapterDescriptions(GetAllChapterDescriptions());
                return mediaDetails;
            }
            else {
                Logger.Warn("Can't get media meta data if media is not playing");
                return null;
            }
        }
        
        public string GetAspectRatio() {
            Logger.Debug("GetAspectRatio()");
            return NativeString.GetNativeString(LibVlc.libvlc_video_get_aspect_ratio(mediaPlayerInstance));
        }
        
        public float GetScale() {
            Logger.Debug("GetScale()");
            return LibVlc.libvlc_video_get_scale(mediaPlayerInstance);
        }
        
        public string GetCropGeometry() {
            Logger.Debug("GetCropGeometry()");
            return NativeString.GetNativeString(LibVlc.libvlc_video_get_crop_geometry(mediaPlayerInstance));
        }
        
        public libvlc_media_stats_t GetMediaStatistics() {
            Logger.Trace("GetMediaStatistics()");
            return GetMediaStatistics(mediaInstance);
        }
        
        public libvlc_media_stats_t GetMediaStatistics(IntPtr media) {
            Logger.Trace("GetMediaStatistics(media={})", media);
            // Must first check that the media is playing otherwise a fatal JVM crash
            // will occur - potentially this could still cause a fatal crash if the
            // media item supplied is not the one actually playing right now
            if(IsPlaying()) {
                if(media != IntPtr.Zero) {
                    IntPtr statsPointer = Marshal.AllocHGlobal(Marshal.SizeOf(libvlcMediaStats));
                    try {
                        Marshal.StructureToPtr(libvlcMediaStats, statsPointer, false);
                        LibVlc.libvlc_media_get_stats(media, statsPointer);
                    }
                    finally {
                        Marshal.FreeHGlobal(statsPointer);
                    }
                }
            }
            return libvlcMediaStats;
        }
        
        // FIXME do not return the native structure, should be a Java enum
        public libvlc_state_e GetMediaState() {
            Logger.Debug("GetMediaState()");
            libvlc_state_e state = 0; // FIXME ?
            if(mediaInstance != IntPtr.Zero) {
                state = (libvlc_state_e)LibVlc.libvlc_media_get_state(mediaInstance);
            }
            return state;
        }
        
        // FIXME do not return the native structure, should be a Java enum
        public libvlc_state_e GetMediaPlayerState() {
            Logger.Debug("GetMediaPlayerState()");
            return (libvlc_state_e)LibVlc.libvlc_media_player_get_state(mediaPlayerInstance);
        }
        
        // === Title/Track Controls =================================================
        
        public int GetTitleCount() {
            Logger.Debug("GetTitleCount()");
            return LibVlc.libvlc_media_player_get_title_count(mediaPlayerInstance);
        }
        
        public int GetTitle() {
            Logger.Debug("GetTitle()");
            return LibVlc.libvlc_media_player_get_title(mediaPlayerInstance);
        }
        
        public void SetTitle(int title) {
            Logger.Debug("SetTitle(title={})", title);
            LibVlc.libvlc_media_player_set_title(mediaPlayerInstance, title);
        }
        
        public int GetVideoTrackCount() {
            Logger.Debug("getVideoTrackCount()");
            return LibVlc.libvlc_video_get_track_count(mediaPlayerInstance);
        }
        
        public int GetVideoTrack() {
            Logger.Debug("getVideoTrack()");
            return LibVlc.libvlc_video_get_track(mediaPlayerInstance);
        }
        
        public void SetVideoTrack(int track) {
            Logger.Debug("SetVideoTrack(track={})", track);
            LibVlc.libvlc_video_set_track(mediaPlayerInstance, track);
        }
        
        public int GetAudioTrackCount() {
            Logger.Debug("getVideoTrackCount()");
            return LibVlc.libvlc_audio_get_track_count(mediaPlayerInstance);
        }
        
        public int GetAudioTrack() {
            Logger.Debug("getAudioTrack()");
            return LibVlc.libvlc_audio_get_track(mediaPlayerInstance);
        }
        
        public void SetAudioTrack(int track) {
            Logger.Debug("SetAudioTrack(track={})", track);
            LibVlc.libvlc_audio_set_track(mediaPlayerInstance, track);
        }
        
        // === Basic Playback Controls ==============================================
        
        public void Play() {
            Logger.Debug("Play()");
            OnBeforePlay();
            LibVlc.libvlc_media_player_play(mediaPlayerInstance);
        }

        public bool Start() {
            Logger.Debug("Start()");
        //        return new MediaPlayerLatch(this).play();
            return false;
        }

        public void Stop() {
            Logger.Debug("Stop()");
            LibVlc.libvlc_media_player_stop(mediaPlayerInstance);
        }

        public void SetPause(bool pause) {
            Logger.Debug("SetPause(pause={})", pause);
            LibVlc.libvlc_media_player_set_pause(mediaPlayerInstance, pause ? 1 : 0);
        }

        public void Pause() {
            Logger.Debug("Pause()");
            LibVlc.libvlc_media_player_pause(mediaPlayerInstance);
        }
        
        public void NextFrame() {
            Logger.Debug("NextFrame()");
            LibVlc.libvlc_media_player_next_frame(mediaPlayerInstance);
        }
        
        public void Skip(long delta) {
            Logger.Debug("Skip(delta={})", delta);
            long current = GetTime();
            if(current != -1) {
                SetTime(current + delta);
            }
        }
        
        public void SkipPosition(float delta) {
            Logger.Debug("SkipPosition(delta={})", delta);
            float current = GetPosition();
            if(current != -1) {
                SetPosition(current + delta);
            }
        }
        
        public void SetTime(long time) {
            Logger.Debug("SetTime(time={})", time);
            LibVlc.libvlc_media_player_set_time(mediaPlayerInstance, time);
        }
        
        public void SetPosition(float position) {
            Logger.Debug("SetPosition(position={})", position);
            LibVlc.libvlc_media_player_set_position(mediaPlayerInstance, position);
        }
        
        public int SetRate(float rate) {
            Logger.Debug("SetRate(rate={})", rate);
            return LibVlc.libvlc_media_player_set_rate(mediaPlayerInstance, rate);
        }
        
        public void SetAspectRatio(string aspectRatio) {
            IntPtr aspectRatioPtr = NativeString.StringPointer(aspectRatio);
            if(aspectRatioPtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_aspect_ratio(mediaPlayerInstance, aspectRatioPtr);
                }
                finally {
                    NativeString.Release(aspectRatioPtr);
                }
            }
        }
        
        public void SetScale(float factor) {
            Logger.Debug("SetScale(factor={})", factor);
            LibVlc.libvlc_video_set_scale(mediaPlayerInstance, factor);
        }
        
        public void SetCropGeometry(string cropGeometry) {
            Logger.Debug("SetCropGeometry(cropGeometry={})", cropGeometry);
            IntPtr cropGeometryPtr = NativeString.StringPointer(cropGeometry);
            if(cropGeometryPtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_crop_geometry(mediaPlayerInstance, cropGeometryPtr);
                }
                finally {
                    NativeString.Release(cropGeometryPtr);
                }
            }
        }

        // === Audio Controls =======================================================
        
        public bool SetAudioOutput(string output) {
            Logger.Debug("SetAudioOutput(output={})", output);
            IntPtr outputPtr = NativeString.StringPointer(output);
            if(outputPtr != IntPtr.Zero) {
                try {
                    return 0 == LibVlc.libvlc_audio_output_set(mediaPlayerInstance, outputPtr);
                }
                finally {
                    NativeString.Release(outputPtr);
                }
            }
            else {
                return false;
            }
        }
        
        public void SetAudioOutputDevice(string output, string outputDeviceId) {
            Logger.Debug("SetAudioOutputDevice(output={},outputDeviceId={})", output, outputDeviceId);
            IntPtr outputPtr = IntPtr.Zero;
            IntPtr outputDeviceIdPtr = IntPtr.Zero;
            try {
                outputPtr = NativeString.StringPointer(output);
                if(outputPtr != IntPtr.Zero)  {
                    outputDeviceIdPtr = NativeString.StringPointer(outputDeviceId);
                    if(outputDeviceIdPtr != IntPtr.Zero) {
                        LibVlc.libvlc_audio_output_device_set(mediaPlayerInstance, outputPtr, outputDeviceIdPtr);
                    }
                }
            }
            finally {
                NativeString.Release(outputPtr);
                NativeString.Release(outputDeviceIdPtr);
            }
        }
        
        public void SetAudioOutputDeviceType(AudioOutputDeviceType deviceType) {
            Logger.Debug("SetAudioOutputDeviceType(deviceType={})");
            LibVlc.libvlc_audio_output_set_device_type(mediaPlayerInstance, (int)deviceType);
        }
        
        public AudioOutputDeviceType GetAudioOutputDeviceType() {
            Logger.Debug("GetAudioOutputDeviceType()");
            return (AudioOutputDeviceType)LibVlc.libvlc_audio_output_get_device_type(mediaPlayerInstance);
        }
        
        public void Mute() {
            Logger.Debug("Mute()");
            LibVlc.libvlc_audio_toggle_mute(mediaPlayerInstance);
        }
        
        public void Mute(bool mute) {
            Logger.Debug("Mute(mute={})", mute);
            LibVlc.libvlc_audio_set_mute(mediaPlayerInstance, mute ? 1 : 0);
        }
        
        public bool IsMute() {
            Logger.Debug("IsMute()");
            return LibVlc.libvlc_audio_get_mute(mediaPlayerInstance) != 0;
        }
        
        public int GetVolume() {
            Logger.Debug("GetVolume()");
            return LibVlc.libvlc_audio_get_volume(mediaPlayerInstance);
        }
        
        public void SetVolume(int volume) {
            Logger.Debug("SetVolume(volume={})", volume);
            LibVlc.libvlc_audio_set_volume(mediaPlayerInstance, volume);
        }
        
        public int GetAudioChannel() {
            Logger.Debug("GetAudioChannel()");
            return LibVlc.libvlc_audio_get_channel(mediaPlayerInstance);
        }
        
        public void SetAudioChannel(int channel) {
            Logger.Debug("SetAudioChannel(channel={})", channel);
            LibVlc.libvlc_audio_set_channel(mediaPlayerInstance, channel);
        }
        
        public long GetAudioDelay() {
            Logger.Debug("GetAudioDelay()");
            return LibVlc.libvlc_audio_get_delay(mediaPlayerInstance);
        }
        
        public void SetAudioDelay(long delay) {
            Logger.Debug("SetAudioDelay(delay={})", delay);
            LibVlc.libvlc_audio_set_delay(mediaPlayerInstance, delay);
        }
        
        // === Chapter Controls =====================================================
        
        public int GetChapterCount() {
            Logger.Debug("GetChapterCount()");
            return LibVlc.libvlc_media_player_get_chapter_count(mediaPlayerInstance);
        }
        
        public int GetChapter() {
            Logger.Debug("GetChapter()");
            return LibVlc.libvlc_media_player_get_chapter(mediaPlayerInstance);
        }
        
        public void SetChapter(int chapterNumber) {
            Logger.Debug("SetChapter(chapterNumber={})", chapterNumber);
            LibVlc.libvlc_media_player_set_chapter(mediaPlayerInstance, chapterNumber);
        }
        
        public void NextChapter() {
            Logger.Debug("NextChapter()");
            LibVlc.libvlc_media_player_next_chapter(mediaPlayerInstance);
        }
        
        public void PreviousChapter() {
            Logger.Debug("PreviousChapter()");
            LibVlc.libvlc_media_player_previous_chapter(mediaPlayerInstance);
        }
        
        // === DVD Menu Navigation Controls =========================================
        
        public void MenuActivate() {
            LibVlc.libvlc_media_player_navigate(mediaPlayerInstance, (int)libvlc_navigate_mode_e.libvlc_navigate_activate);
        }
        
        public void MenuUp() {
            LibVlc.libvlc_media_player_navigate(mediaPlayerInstance, (int)libvlc_navigate_mode_e.libvlc_navigate_up);
        }
        
        public void MenuDown() {
            LibVlc.libvlc_media_player_navigate(mediaPlayerInstance, (int)libvlc_navigate_mode_e.libvlc_navigate_down);
        }
        
        public void MenuLeft() {
            LibVlc.libvlc_media_player_navigate(mediaPlayerInstance, (int)libvlc_navigate_mode_e.libvlc_navigate_left);
        }
        
        public void MenuRight() {
            LibVlc.libvlc_media_player_navigate(mediaPlayerInstance, (int)libvlc_navigate_mode_e.libvlc_navigate_right);
        }
        
        // === Sub-Picture/Sub-Title Controls =======================================
        
        public int GetSpuCount() {
            Logger.Debug("GetSpuCount()");
            return LibVlc.libvlc_video_get_spu_count(mediaPlayerInstance);
        }
        
        public int GetSpu() {
            Logger.Debug("GetSpu()");
            return LibVlc.libvlc_video_get_spu(mediaPlayerInstance);
        }
        
        public void SetSpu(int spu) {
            Logger.Debug("SetSpu(spu={})", spu);
            int spuCount = GetSpuCount();
            Logger.Debug("spuCount={}", spuCount);
            if(spuCount != 0 && spu <= spuCount) {
                LibVlc.libvlc_video_set_spu(mediaPlayerInstance, spu);
            }
            else {
                Logger.Debug("Ignored out of range spu number {} because spu count is {}", spu, spuCount);
            }
        }
        
        public void CycleSpu() {
            Logger.Debug("CycleSpu()");
            int spu = GetSpu();
            int spuCount = GetSpuCount();
            if(spu >= spuCount) {
                spu = 0;
            }
            else {
                spu ++ ;
            }
            SetSpu(spu);
        }
        
        public long GetSpuDelay() {
            Logger.Debug("GetSpuDelay()");
            return LibVlc.libvlc_video_get_spu_delay(mediaPlayerInstance);
        }
        
        public void SetSpuDelay(long delay) {
            Logger.Debug("SetSpuDelay(delay={})", delay);
            LibVlc.libvlc_video_set_spu_delay(mediaPlayerInstance, delay);
        }
        
        public void SetSubTitleFile(string subTitleFileName) {
            Logger.Debug("SetSubTitleFile(subTitleFileName={})", subTitleFileName);
            IntPtr subTitleFileNamePtr = NativeString.StringPointer(subTitleFileName);
            if(subTitleFileNamePtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_subtitle_file(mediaPlayerInstance, subTitleFileNamePtr);
                }
                finally {
                    NativeString.Release(subTitleFileNamePtr);
                }
            }
        }
        
        public void SetSubTitleFile(FileStream subTitleFile) {
            Logger.Debug("SetSubTitleFile(subTitleFile={})", subTitleFile);
        //        setSubTitleFile(subTitleFile.getAbsolutePath());
        }
        
        // === Teletext Controls ====================================================
        
        public int GetTeletextPage() {
            Logger.Debug("GetTeletextPage()");
            return LibVlc.libvlc_video_get_teletext(mediaPlayerInstance);
        }
        
        public void SetTeletextPage(int pageNumber) {
            Logger.Debug("SetTeletextPage(pageNumber={})", pageNumber);
            LibVlc.libvlc_video_set_teletext(mediaPlayerInstance, pageNumber);
        }
        
        public void ToggleTeletext() {
            Logger.Debug("ToggleTeletext()");
            LibVlc.libvlc_toggle_teletext(mediaPlayerInstance);
        }
        
        // === Description Controls =================================================

        public List<TrackDescription> GetTitleDescriptions() {
            Logger.Debug("GetTitleDescriptions()");
            IntPtr trackDescriptionsPointer = LibVlc.libvlc_video_get_title_description(mediaPlayerInstance);
            return GetTrackDescriptions(trackDescriptionsPointer);
        }
        
        public List<TrackDescription> GetVideoDescriptions() {
            Logger.Debug("GetVideoDescriptions()");
            IntPtr trackDescriptionsPointer = LibVlc.libvlc_video_get_track_description(mediaPlayerInstance);
            return GetTrackDescriptions(trackDescriptionsPointer);
        }
        
        public List<TrackDescription> GetAudioDescriptions() {
            Logger.Debug("GetAudioDescriptions()");
            IntPtr trackDescriptionsPointer = LibVlc.libvlc_audio_get_track_description(mediaPlayerInstance);
            return GetTrackDescriptions(trackDescriptionsPointer);
        }
        
        public List<TrackDescription> GetSpuDescriptions() {
            Logger.Debug("GetSpuDescriptions()");
            IntPtr trackDescriptionsPointer = LibVlc.libvlc_video_get_spu_description(mediaPlayerInstance);
            return GetTrackDescriptions(trackDescriptionsPointer);
        }
        
        public List<string> GetChapterDescriptions(int title) {
            Logger.Debug("GetChapterDescriptions(title={})", title);
            List<string> trackDescriptionList = new List<string>();
            IntPtr trackDescriptionsPointer = LibVlc.libvlc_video_get_chapter_description(mediaPlayerInstance, title);
            IntPtr trackDescriptionPointer = trackDescriptionsPointer;
            while(trackDescriptionPointer != IntPtr.Zero) {
                libvlc_track_description_t trackDescription = (libvlc_track_description_t)Marshal.PtrToStructure(trackDescriptionPointer, typeof(libvlc_track_description_t));
                trackDescriptionList.Add(NativeString.String(trackDescription.psz_name));
                trackDescriptionPointer = trackDescription.p_next;
            }
            if(trackDescriptionsPointer != IntPtr.Zero) {
                LibVlc.libvlc_track_description_list_release(trackDescriptionsPointer);
            }
            return trackDescriptionList;
        }
        
        public List<string> GetChapterDescriptions() {
            Logger.Debug("GetChapterDescriptions()");
            return GetChapterDescriptions(GetTitle());
        }
        
        public List<List<string>> GetAllChapterDescriptions() {
            Logger.Debug("GetAllChapterDescriptions()");
            int titleCount = GetTitleCount();
            List<List<string>> result = new List<List<string>>(titleCount);
            for(int i = 0; i < titleCount; i ++ ) {
                result.Add(GetChapterDescriptions(i));
            }
            return result;
        }
        
        public List<TrackInfo> GetTrackInfo() {
            Logger.Debug("GetTrackInfo()");
            return GetTrackInfo(mediaInstance);
        }
        
        public List<TrackInfo> GetTrackInfo(IntPtr media) {
            Logger.Debug("GetTrackInfo(media={})", media);
            if(media != IntPtr.Zero) {
                IntPtr tracksPointer = new IntPtr();
                int numberOfTracks = LibVlc.libvlc_media_get_tracks_info(media, out tracksPointer);
                Logger.Debug("numberOfTracks={}", numberOfTracks);
                List<TrackInfo> result = new List<TrackInfo>(numberOfTracks);
                int size = Marshal.SizeOf(typeof(libvlc_media_track_info_t));
                // FIXME!!!!
                size = 28;
                if(numberOfTracks > 0) { // REDUNDANT
                    for(int i = 0; i < numberOfTracks; i++) {
                        IntPtr offsetPointer = new IntPtr(tracksPointer.ToInt64() + (i*size));
                        libvlc_media_track_info_t trackInfo = (libvlc_media_track_info_t)Marshal.PtrToStructure(offsetPointer, typeof(libvlc_media_track_info_t));
                        switch(trackInfo.i_type) {
                            case (int)libvlc_track_type_e.libvlc_track_unknown:
                                result.Add(new UnknownTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level));
                                break;
        
                            case (int)libvlc_track_type_e.libvlc_track_video:
// FIXME                            result.Add(new VideoTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level, trackInfo.u.video.i_width, trackInfo.u.video.i_height));
                                result.Add(new VideoTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level, 0, 0));
                                break;
        
                            case (int)libvlc_track_type_e.libvlc_track_audio:
// FIXME                            result.Add(new AudioTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level, trackInfo.u.audio.i_channels, trackInfo.u.audio.i_rate));
                                result.Add(new AudioTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level, 0, 0));
                                break;
        
                            case (int)libvlc_track_type_e.libvlc_track_text:
                                result.Add(new SpuTrackInfo(trackInfo.i_codec, trackInfo.i_id, trackInfo.i_profile, trackInfo.i_level));
                                break;
                        }
                    }
                }
                LibVlc.libvlc_free(tracksPointer);
                return result;
            }
            else {
                return null;
            }
        }
        
        public List<List<TrackInfo>> GetSubItemTrackInfo() {
            Logger.Debug("GetSubItemTrackInfo()");
        /*        return handleSubItems(new SubItemsHandler<List<List<TrackInfo>>>() {
                @Override
                public List<List<TrackInfo>> subItems(int count, libvlc_media_list_t subItems) {
                    List<List<TrackInfo>> result = new ArrayList<List<TrackInfo>>(count);
                    for(libvlc_media_t subItem : new LibVlcMediaListIterator(libvlc, subItems)) {
                        result.add(getTrackInfo(subItem));
                    }
                    return result;
                }
            });*/
            return null;
        }

        /**
         * Get track descriptions.
         * 
         * @param trackDescriptions native track descriptions, this pointer will be freed by this method
         * @return collection of track descriptions
         */
        private List<TrackDescription> GetTrackDescriptions(IntPtr trackDescriptionsPointer) {
            Logger.Debug("GetTrackDescriptions()");
            List<TrackDescription> trackDescriptionList = new List<TrackDescription>();
            IntPtr trackDescriptionPointer = trackDescriptionsPointer;
            while(trackDescriptionPointer != IntPtr.Zero) {
                libvlc_track_description_t trackDescription = (libvlc_track_description_t)Marshal.PtrToStructure(trackDescriptionPointer, typeof(libvlc_track_description_t));
                trackDescriptionList.Add(new TrackDescription(trackDescription.i_id, NativeString.String(trackDescription.psz_name)));
                trackDescriptionPointer = trackDescription.p_next;
            }
            if(trackDescriptionsPointer != IntPtr.Zero) {
                LibVlc.libvlc_track_description_list_release(trackDescriptionsPointer);
            }
            return trackDescriptionList;
        }

        // === Snapshot Controls ====================================================
        
        public void SetSnapshotDirectory(string snapshotDirectoryName) {
            Logger.Debug("SetSnapshotDirectory(snapshotDirectoryName={})", snapshotDirectoryName);
            this.snapshotDirectoryName = snapshotDirectoryName;
        }
        
        public bool SaveSnapshot() {
            Logger.Debug("SaveSnapshot()");
            return SaveSnapshot(0, 0);
        }
        
        public bool SaveSnapshot(int width, int height) {
            Logger.Debug("SaveSnapshot(width={},height={})", width, height);
        //        File snapshotDirectory = new File(snapshotDirectoryName == null ? System.getProperty("user.home") : snapshotDirectoryName);
        //        File snapshotFile = new File(snapshotDirectory, "vlcj-snapshot-" + System.currentTimeMillis() + ".png");
        //        return saveSnapshot(snapshotFile, width, height);
            return false;
        }
        
        public bool SaveSnapshot(FileStream file) {
            Logger.Debug("SaveSnapshot(file={})", file);
            return SaveSnapshot(file, 0, 0);
        }
        
        public bool SaveSnapshot(FileStream file, int width, int height) {
            Logger.Debug("saveSnapshot(file={},width={},height={})", file, width, height);
        /*        File snapshotDirectory = file.getParentFile();
            if(snapshotDirectory == null) {
                snapshotDirectory = new File(".");
                Logger.debug("No directory specified for snapshot, snapshot will be saved to {}", snapshotDirectory.getAbsolutePath());
            }
            if(!snapshotDirectory.exists()) {
                snapshotDirectory.mkdirs();
            }
            if(snapshotDirectory.exists()) {
                boolean snapshotTaken = libvlc.libvlc_video_take_snapshot(mediaPlayerInstance, 0, file.getAbsolutePath(), width, height) == 0;
                Logger.debug("snapshotTaken={}", snapshotTaken);
                return snapshotTaken;
            }
            else {
                throw new RuntimeException("Directory does not exist and could not be created for '" + file.getAbsolutePath() + "'");
            }*/
            return false;
        }
        
        public Bitmap GetSnapshot() {
            Logger.Debug("GetSnapshot()");
            return GetSnapshot(0, 0);
        }
        
        public Bitmap GetSnapshot(int width, int height) {
            Logger.Debug("GetSnapshot(width={},height={})", width, height);
        /*        File file = null;
            try {
                file = File.createTempFile("vlcj-snapshot-", ".png");
                Logger.debug("file={}", file.getAbsolutePath());
                if(saveSnapshot(file, width, height)) {
                    BufferedImage snapshotImage = ImageIO.read(file);
                    return snapshotImage;
                }
                else {
                    return null;
                }
            }
            catch(IOException e) {
                throw new RuntimeException("Failed to get snapshot image", e);
            }
            finally {
                if(file != null) {
                    boolean deleted = file.delete();
                    Logger.debug("deleted={}", deleted);
                }
            }*/
            return null;
        }
        
        // === Logo Controls ========================================================
        
        public void EnableLogo(bool enable) {
            Logger.Debug("EnableLogo(enable={})", enable);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_enable, enable ? 1 : 0);
        }
        
        public void SetLogoOpacity(int opacity) {
            Logger.Debug("SetLogoOpacity(opacity={})", opacity);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_opacity, opacity);
        }
        
        public void SetLogoOpacity(float opacity) {
            Logger.Debug("SetLogoOpacity(opacity={})", opacity);
            int opacityValue = (int)Math.Round(opacity * 255.0f);
            Logger.Debug("opacityValue={}", opacityValue);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_opacity, opacityValue);
        }
        
        public void SetLogoLocation(int x, int y) {
            Logger.Debug("SetLogoLocation(x={},y={})", x, y);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_x, x);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_y, y);
        }
        
        public void SetLogoPosition(libvlc_logo_position_e position) {
            Logger.Debug("SetLogoPosition(position={})", position);
            LibVlc.libvlc_video_set_logo_int(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_position, (int)position);
        }
        
        public void SetLogoFile(string logoFile) {
            Logger.Debug("SetLogoFile(logoFile={})", logoFile);
            IntPtr logoFilePtr = NativeString.StringPointer(logoFile);
            if(logoFilePtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_logo_string(mediaPlayerInstance, (int)libvlc_video_logo_option_e.libvlc_logo_file, logoFilePtr);
                }
                finally {
                    NativeString.Release(logoFilePtr);
                }
            }
        }
        
        public void SetLogoImage(Bitmap logoImage) {
            Logger.Debug("SetLogoImage(logoImage={})", logoImage);
        /*        File file = null;
            try {
                // Create a temporary file for the logo...
                file = File.createTempFile("vlcj-logo-", ".png");
                ImageIO.write(logoImage, "png", file);
                // ...then set the logo as normal
                setLogoFile(file.getAbsolutePath());
            }
            catch(IOException e) {
                throw new RuntimeException("Failed to set logo image", e);
            }
            finally {
                if(file != null) {
                    boolean deleted = file.delete();
                    Logger.Debug("deleted={}", deleted);
                }
            }*/
        }
        
        // === Marquee Controls =====================================================
        
        public void EnableMarquee(bool enable) {
            Logger.Debug("EnableMarquee(enable={})", enable);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Enable, enable ? 1 : 0);
        }
        
        public void SetMarqueeText(string text) {
            Logger.Debug("SetMarqueeText(text={})", text);
            IntPtr textPtr = NativeString.StringPointer(text);
            if(textPtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_marquee_string(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Text, textPtr);
                }
                finally {
                    NativeString.Release(textPtr);
                }
            }
        }
        
        public void SetMarqueeColour(Color colour) {
            Logger.Debug("SetMarqueeColour(colour={})", colour);
            SetMarqueeColour(colour.ToArgb() & 0x00ffffff);
        }
        
        public void SetMarqueeColour(int colour) {
            Logger.Debug("SetMarqueeColour(colour={})", colour);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Color, colour);
        }
        
        public void SetMarqueeOpacity(int opacity) {
            Logger.Debug("SetMarqueeOpacity(opacity={})", opacity);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Opacity, opacity);
        }
        
        public void SetMarqueeOpacity(float opacity) {
            Logger.Debug("SetMarqueeOpacity(opacity={})", opacity);
            int opacityValue = (int)Math.Round(opacity * 255.0f);
            Logger.Debug("opacityValue={}", opacityValue);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Opacity, opacityValue);
        }
        
        public void SetMarqueeSize(int size) {
            Logger.Debug("SetMarqueeSize(size={})", size);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Size, size);
        }
        
        public void SetMarqueeTimeout(int timeout) {
            Logger.Debug("SetMarqueeTimeout(timeout={})", timeout);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Timeout, timeout);
        }
        
        public void SetMarqueeLocation(int x, int y) {
            Logger.Debug("SetMarqueeLocation(x={},y={})", x, y);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_X, x);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Y, y);
        }
        
        public void SetMarqueePosition(libvlc_marquee_position_e position) {
            Logger.Debug("SetMarqueePosition(position={})", position);
            LibVlc.libvlc_video_set_marquee_int(mediaPlayerInstance, (int)libvlc_video_marquee_option_e.libvlc_marquee_Position, (int)position);
        }
        
        // === Filter Controls ======================================================
        
        public void SetDeinterlace(DeinterlaceMode deinterlaceMode) {
            Logger.Debug("SetDeinterlace(deinterlaceMode={})", deinterlaceMode);
            IntPtr deinterlaceModePtr = NativeString.StringPointer(deinterlaceMode.GetDescription());
            if(deinterlaceModePtr != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_video_set_deinterlace(mediaPlayerInstance, deinterlaceModePtr);
                }
                finally {
                    NativeString.Release(deinterlaceModePtr);
                }
            }
        }
        
        // === Video Adjustment Controls ============================================
        
        public void SetAdjustVideo(bool adjustVideo) {
            Logger.Debug("SetAdjustVideo(adjustVideo={})", adjustVideo);
            LibVlc.libvlc_video_set_adjust_int(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Enable, adjustVideo ? 1 : 0);
        }
        
        public bool IsAdjustVideo() {
            Logger.Debug("IsAdjustVideo()");
            return LibVlc.libvlc_video_get_adjust_int(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Enable) == 1;
        }
        
        public float GetContrast() {
            Logger.Debug("GetContrast()");
            return LibVlc.libvlc_video_get_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Contrast);
        }
        
        public void SetContrast(float contrast) {
            Logger.Debug("SetContrast(contrast={})", contrast);
            LibVlc.libvlc_video_set_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Contrast, contrast);
        }
        
        public float GetBrightness() {
            Logger.Debug("GetBrightness()");
            return LibVlc.libvlc_video_get_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Brightness);
        }
        
        public void SetBrightness(float brightness) {
            Logger.Debug("SetBrightness(brightness={})", brightness);
            LibVlc.libvlc_video_set_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Brightness, brightness);
        }
        
        public int GetHue() {
            Logger.Debug("GetHue()");
            return LibVlc.libvlc_video_get_adjust_int(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Hue);
        }
        
        public void SetHue(int hue) {
            Logger.Debug("SetHue(hue={})", hue);
            LibVlc.libvlc_video_set_adjust_int(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Hue, hue);
        }
        
        public float GetSaturation() {
            Logger.Debug("GetSaturation()");
            return LibVlc.libvlc_video_get_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Saturation);
        }
        
        public void SetSaturation(float saturation) {
            Logger.Debug("SetSaturation(saturation={})", saturation);
            LibVlc.libvlc_video_set_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Saturation, saturation);
        }
        
        public float GetGamma() {
            Logger.Debug("GetGamma()");
            return LibVlc.libvlc_video_get_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Gamma);
        }
        
        public void SetGamma(float gamma) {
            Logger.Debug("SetGamma(gamma={})", gamma);
            LibVlc.libvlc_video_set_adjust_float(mediaPlayerInstance, (int)libvlc_video_adjust_option_e.libvlc_adjust_Gamma, gamma);
        }
        
        // === Implementation =======================================================
        
        public string Mrl() {
            Logger.Debug("Mrl()");
            if(mediaInstance != IntPtr.Zero) {
                return NativeString.GetNativeString(LibVlc.libvlc_media_get_mrl(mediaInstance));
            }
            else {
                throw new InvalidOperationException("No media");
            }
        }
        
        public string Mrl(IntPtr mediaInstance) {
            Logger.Debug("mrl(mediaInstance={})", mediaInstance);
            return NativeString.GetNativeString(LibVlc.libvlc_media_get_mrl(mediaInstance));
        }
        
        public object GetUserData() {
            Logger.Debug("GetUserData()");
            return userData;
        }
        
        public void SetUserData(object userData) {
            Logger.Debug("SetUserData(userData={})", userData);
            this.userData = userData;
        }
        
        public void Release() {
            Logger.Debug("Release()");
        //          if(released.compareAndSet(false, true)) {
                DestroyInstance();
                OnAfterRelease();
        //          }
        }
        
        public IntPtr MediaPlayerInstance() {
            return mediaPlayerInstance;
        }
        
        virtual protected void OnBeforePlay() {
        }
        
        virtual protected void OnAfterRelease() {
        }
        
        private void CreateInstance() {
            Logger.Debug("CreateInstance()");
            
            mediaPlayerInstance = LibVlc.libvlc_media_player_new(instance);
            Logger.Debug("MediaPlayerInstance={}", mediaPlayerInstance);
            
            mediaPlayerEventManager = LibVlc.libvlc_media_player_event_manager(mediaPlayerInstance);
            Logger.Debug("MediaPlayerEventManager={}", mediaPlayerEventManager);
            
            RegisterEventListener();

            // The order these handlers execute in is important for proper operation
            eventListenerList.Add(new NewMediaEventHandler(this));
            eventListenerList.Add(new RepeatPlayEventHandler(this));
            eventListenerList.Add(new SubItemEventHandler(this));
        }
        
        private void DestroyInstance() {
            Logger.Debug("DestroyInstance()");
        
            Logger.Debug("Detach media events...");
            DeregisterMediaEventListener();
            Logger.Debug("Media events detached.");
        
            if(mediaInstance != IntPtr.Zero) {
                Logger.Debug("Release media...");
                LibVlc.libvlc_media_release(mediaInstance);
                Logger.Debug("Media released.");
            }
        
            Logger.Debug("Detach media player events...");
            DeregisterEventListener();
            Logger.Debug("Media player events detached.");
        
            eventListenerList.Clear();
        
            if(mediaPlayerInstance != IntPtr.Zero) {
                Logger.Debug("Release media player...");
                LibVlc.libvlc_media_player_release(mediaPlayerInstance);
                Logger.Debug("Media player released.");
            }
        
            Logger.Debug("Shut down listeners...");
            listenersService.Shutdown();
            Logger.Debug("Listeners shut down.");
        }
        
        private void RegisterEventListener() {
            Logger.Debug("RegisterEventListener()");
            callback = new VlcVideoPlayerCallbackDelegate(HandleEvent);
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                if(value >= (int)libvlc_event_e.libvlc_MediaPlayerMediaChanged && value <= (int)libvlc_event_e.libvlc_MediaPlayerVout) {
                    Logger.Debug("event={}", (libvlc_event_e)value);
                    int result = LibVlc.libvlc_event_attach(mediaPlayerEventManager, value, callbackPtr, IntPtr.Zero);
                    Logger.Debug("result={}", result);
                }
            }
        }
        
        private void DeregisterEventListener() {
            Logger.Debug("DeregisterEventListener()");
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                if(value >= (int)libvlc_event_e.libvlc_MediaPlayerMediaChanged && value <= (int)libvlc_event_e.libvlc_MediaPlayerVout) {
                    Logger.Debug("event={}", (libvlc_event_e)value);
                    LibVlc.libvlc_event_detach(mediaPlayerEventManager, value, callbackPtr, IntPtr.Zero);
                }
            }
        }
        
        /**
         * Register a call-back to receive media native events.
         */
        private void RegisterMediaEventListener() {
            Logger.Debug("RegisterMediaEventListener()");
            // If there is a media, register a new listener...
            if(mediaInstance != IntPtr.Zero) {
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
                IntPtr mediaEventManager = LibVlc.libvlc_media_event_manager(mediaInstance);
                foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                    if(value >= (int)libvlc_event_e.libvlc_MediaMetaChanged && value <= (int)libvlc_event_e.libvlc_MediaStateChanged) {
                        Logger.Debug("event={}", (libvlc_event_e)value);
                        int result = LibVlc.libvlc_event_attach(mediaEventManager, value, callbackPtr, IntPtr.Zero);
                        Logger.Debug("result={}", result);
                    }
                }
            }
        }
        
        /**
         * De-register the call-back used to receive native media events.
         */
        private void DeregisterMediaEventListener() {
            Logger.Debug("DeregisterMediaEventListener()");
            // If there is a media, deregister the listener...
            if(mediaInstance != IntPtr.Zero) {
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
                IntPtr mediaEventManager = LibVlc.libvlc_media_event_manager(mediaInstance);
                foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                    if(value >= (int)libvlc_event_e.libvlc_MediaMetaChanged && value <= (int)libvlc_event_e.libvlc_MediaStateChanged) {
                        Logger.Debug("event={}", (libvlc_event_e)value);
                        LibVlc.libvlc_event_detach(mediaEventManager, value, callbackPtr, IntPtr.Zero);
                    }
                }
            }
        }
        
        /**
         * Raise an event.
         * 
         * @param mediaPlayerEvent event to raise, may be <code>null</code>
         */
        private void RaiseEvent(MediaPlayerEvent mediaPlayerEvent) {
            if(mediaPlayerEvent != null) {
                listenersService.Submit(new NotifyEventListenersRunnable(this, mediaPlayerEvent));
            }
        }

        /**
         * Set new media for the native media player.
         * <p>
         * This method cleans up the previous media if there was one before associating new media with
         * the media player.
         * 
         * @param media media
         * @param mediaOptions zero or more media options
         */
        private bool SetMedia(string media, params string[] mediaOptions) {
            Logger.Debug("SetMedia(media={},mediaOptions={})", media, mediaOptions);
            // If there is a current media, clean it up
            if(mediaInstance != IntPtr.Zero) {
                // Release the media event listener
                DeregisterMediaEventListener();
                // Release the native resource
                LibVlc.libvlc_media_release(mediaInstance);
                mediaInstance = IntPtr.Zero;
            }
            // Reset sub-items
            subItemIndex = -1;
            // Create new media...
            IntPtr mediaPointer = NativeString.StringPointer(media);
            if(mediaPointer != IntPtr.Zero) {
                try {
                    mediaInstance = LibVlc.libvlc_media_new_path(instance, mediaPointer);
                }
                finally {
                    NativeString.Release(mediaPointer);
                }
                if(mediaInstance != IntPtr.Zero) {
                    // Set the standard media options (if any)...
                    AddMediaOptions(mediaInstance, standardMediaOptions); // FIXME handle return false?
                    // Set the particular media options (if any)...
                    AddMediaOptions(mediaInstance, mediaOptions); // FIXME handle return false?
                    // Attach a listener to the new media
                    RegisterMediaEventListener();
                    // Set the new media on the media player
                    LibVlc.libvlc_media_player_set_media(mediaPlayerInstance, mediaInstance);
                }
                else {
                    Logger.Error("Failed to create native media resource for '{}'", media);
                }
                // Prepare a new statistics object to re-use for the new media item
                libvlcMediaStats = new libvlc_media_stats_t();
                return mediaInstance != IntPtr.Zero;
            }
            else {
                return false;
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
         * 
         * @param evt event
         * @param data user data (unused)
         */
        private void HandleEvent(IntPtr evt, IntPtr data) {
            if(eventListenerList.Count > 0) {
                libvlc_event_t e = (libvlc_event_t)Marshal.PtrToStructure(evt, typeof(libvlc_event_t));
                // Create a new media player event for the native event
//                RaiseEvent(eventFactory.CreateEvent(evt, eventMask));
                RaiseEvent(eventFactory.CreateEvent(e, 0));
            }
        }
        
        /**
         * Native delegate declaration for the media player event callback.
         * 
         * @param evt event
         * @param data user data (unused)
         */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void VlcVideoPlayerCallbackDelegate(IntPtr evt, IntPtr userData);

        /**
         * A runnable task used to fire event notifications.
         * <p>
         * Care must be taken not to re-enter the native library during an event notification so the
         * notifications are off-loaded to a separate thread.
         * <p>
         * These events therefore do <em>not</em> run on the Event Dispatch Thread.
         */
        public class NotifyEventListenersRunnable : Runnable {
        
            /**
             * Enclsoing media player instance.
             */
            private readonly DefaultMediaPlayer mediaPlayer;

            /**
             * Event to notify.
             */
            private readonly MediaPlayerEvent mediaPlayerEvent;
        
            /**
             * Create a runnable.
             *
             * @param mediaPlayer enclosing media player instance
             * @param mediaPlayerEvent event to notify
             */
            public NotifyEventListenersRunnable(DefaultMediaPlayer mediaPlayer, MediaPlayerEvent mediaPlayerEvent) {
                this.mediaPlayer = mediaPlayer;
                this.mediaPlayerEvent = mediaPlayerEvent;
            }
        
            public void Run() {
                Logger.Trace("Run()");
                for(int i = mediaPlayer.eventListenerList.Count - 1; i >= 0; i -- ) {
                    MediaPlayerEventListener listener = mediaPlayer.eventListenerList[i];
                    try {
                        mediaPlayerEvent.Notify(listener);
                    }
                    catch(Exception t) {
                        Logger.Warn("Event listener {} threw an exception", t, listener);
                        // Continue with the next listener...
                    }
                }
                Logger.Trace("runnable exits");
            }
        }

        // FIXME protection level???
        // FIXME some non ideal stuff here with the enclosing mediaPlayer and the mediaPlayer on the event

        /**
         * Event listener implementation that handles a new item being played.
         * <p>
         * This is not for sub-items.
         */
        private class NewMediaEventHandler : MediaPlayerEventAdapter {

            private readonly DefaultMediaPlayer mediaPlayer;

            public NewMediaEventHandler(DefaultMediaPlayer mediaPlayer) {
                this.mediaPlayer = mediaPlayer;
            }

            public override void MediaChanged(MediaPlayer mediaPlayer, IntPtr media, String mrl) {
                Logger.Debug("MediaChanged(mediaPlayer={},media={},mrl={})", mediaPlayer, media, mrl);
                // If this is not a sub-item...
                if(mediaPlayer.SubItemIndex() == -1) {
                    // Raise a semantic event to announce the media was changed
                    Logger.Debug("Raising event for new media");
//                    RaiseEvent(eventFactory.CreateMediaNewEvent(eventMask));
                    this.mediaPlayer.RaiseEvent(this.mediaPlayer.eventFactory.CreateMediaNewEvent(0));
                }
            }
        }

        /**
         * Event listener implementation that handles auto-repeat.
         */
        private class RepeatPlayEventHandler : MediaPlayerEventAdapter {

            private readonly DefaultMediaPlayer mediaPlayer;

            public RepeatPlayEventHandler(DefaultMediaPlayer mediaPlayer) {
                this.mediaPlayer = mediaPlayer;
            }

            public override void Finished(MediaPlayer mediaPlayer) {
                Logger.Debug("Finished(mediaPlayer={})", mediaPlayer);
                if(this.mediaPlayer.repeat && this.mediaPlayer.mediaInstance != IntPtr.Zero) {
                    int subItemCount = mediaPlayer.SubItemCount();
                    Logger.Debug("subitemCount={}", subItemCount);
                    if(subItemCount == 0) {
                        string mrl = NativeString.GetNativeString(LibVlc.libvlc_media_get_mrl(this.mediaPlayer.mediaInstance));
                        Logger.Debug("auto repeat mrl={}", mrl);
                        // It is not sufficient to simply call play(), the MRL must explicitly
                        // be played again - this is the reason why the repeat play might not
                        // be seamless
                        mediaPlayer.PlayMedia(mrl);
                    }
                    else {
                        Logger.Debug("Sub-items handling repeat");
                    }
                }
                else {
                    Logger.Debug("No repeat");
                }
            }
        }

        /**
         * Event listener implementation that handles media sub-items.
         * <p>
         * Some media types when you 'play' them do not actually play any media and instead sub-items
         * are created and attached to the current media descriptor.
         * <p>
         * This event listener responds to the media player "finished" event by getting the current
         * media from the player and automatically playing the first sub-item (if there is one).
         * <p>
         * If there is more than one sub-item, then they will simply be played in order, and repeated
         * depending on the value of the "repeat" property.
         */
        internal class SubItemEventHandler : MediaPlayerEventAdapter {

            private readonly DefaultMediaPlayer mediaPlayer;

            internal SubItemEventHandler(DefaultMediaPlayer mediaPlayer) {
                this.mediaPlayer = mediaPlayer;
            }

            public override void Finished(MediaPlayer mediaPlayer) {
                Logger.Debug("Finished(mediaPlayer={})", mediaPlayer);
                // If a sub-item being played...
                if(this.mediaPlayer.subItemIndex != -1) {
                    // Raise a semantic event to announce the sub-item was finished
                    Logger.Debug("Raising finished event for sub-item {}", this.mediaPlayer.subItemIndex);
//                    RaiseEvent(eventFactory.CreateMediaSubItemFinishedEvent(subItemIndex, eventMask));
                    this.mediaPlayer.RaiseEvent(this.mediaPlayer.eventFactory.CreateMediaSubItemFinishedEvent(this.mediaPlayer.subItemIndex, 0));
                }
                // If set to automatically play sub-items...
                if(this.mediaPlayer.playSubItems) {
                    // ...play the next sub-item
                    this.mediaPlayer.PlayNextSubItem();
                }
            }
        }

        /**
         * Specification for a component that handles media list sub-items.
         * 
         * @param <T> desired result type
         */
        private interface SubItemsHandler<T> {

            /**
             * Handle sub-items.
             * 
             * @param count number of sub-items in the list, will always be zero or greater
             * @param subItems sub-item list, may be <code>null</code>
             * @return result of processing the sub-items
             */
            T subItems(int count, IntPtr subItems);
        }

        /**
         * Add native media options.
         * 
         * @param mediaInstance native media instance
         * @param options options to add
         * @return <code>true</code> if the options were added; <code>false</code> if they were not
         */
        private bool AddMediaOptions(IntPtr mediaInstance, params string[] options) {
            Logger.Debug("AddMediaOptions(options={})", options);
            if(options != null) {
                IntPtr optionPtr = IntPtr.Zero;
                foreach(string option in options) {
                    Logger.Debug("option={}", option);
                    optionPtr = NativeString.StringPointer(option);
                    if(optionPtr != IntPtr.Zero) {
                        try {
                            LibVlc.libvlc_media_add_option(mediaInstance, optionPtr);
                        }
                        finally {
                            NativeString.Release(optionPtr);
                        }
                    }
                    else {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
