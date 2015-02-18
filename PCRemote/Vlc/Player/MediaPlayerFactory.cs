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
using Caprica.VlcSharp.MediaList;
using Caprica.VlcSharp.Player;
using Caprica.VlcSharp.Player.Embedded;
using Caprica.VlcSharp.Player.Headless;
using Caprica.VlcSharp.Player.List;
using Caprica.VlcSharp.Log;

namespace Caprica.VlcSharp.Player {
    
    /**
    * Factory for media player instances.
    * <p>
    * The factory initialises a single libvlc instance and uses that to create media player instances.
    * <p>
    * If required, you can create multiple factory instances each with their own libvlc options.
    * <p>
    * You should release the factory when your application terminates to properly clean up native
    * resources.
    * <p>
    * The factory also provides access to the native libvlc Logger and other resources such as the list
    * of audio outputs, and the list of available audio and video filters.
    * <p>
    * Usage:
    * 
    * <pre>
    *   // Set some options for libvlc
    *   String[] libvlcArgs = {...add options here...};
    * 
    *   // Create a factory instance (once), you can keep a reference to this
    *   MediaPlayerFactory mediaPlayerFactory = new MediaPlayerFactory(libvlcArgs);
    *   
    *   // Create a full-screen strategy
    *   FullScreenStrategy fullScreenStrategy = new DefaultFullScreenStrategy(mainFrame);
    *   
    *   // Create a media player instance
    *   EmbeddedMediaPlayer mediaPlayer = mediaPlayerFactory.newEmbeddedMediaPlayer(fullScreenStrategy);
    * 
    *   // Do some interesting things with the media player, like setting a video surface...
    *   
    *   ...
    *   
    *   // Release the media player
    *   mediaPlayer.release();
    *   
    *   // Release the factory
    *   factory.release();
    * </pre>
    * 
    * You <em>must</em> make sure you keep a hard reference to the media player (and possibly other)
    * objects created by this factory. If you allow a media player object to go out of scope, then
    * unpredictable behaviour will occur (such as events no longer seeming to fire) even though the
    * video playback continues (since that happens via native code). You may also likely suffer fatal
    * JVM crashes.
    */
    public class MediaPlayerFactory {
    
        /**
         * Native library instance.
         */
        private IntPtr instance;
        
        /**
         * True when the factory has been released.
         */
        //private bool released;
        
        /**
         * Create a new media player factory.
         * <p>
         * If you want to enable logging or synchronisation of the native library interface you must use
         * {@link #MediaPlayerFactory(LibVlc)} and {@link LibVlcFactory}.
         * <p>
         * This factory constructor will enforce a minimum required native library version check - if a
         * suitable native library version is not found a RuntimeException will be thrown.
         * <p>
         * If you do not want to enforce this version check, use one of the other constructors that
         * accepts a LibVlc instance that you obtain from the {@link LibVlcFactory}.
         */
        public MediaPlayerFactory() : this(new string[0]) {
        }
        
        /**
         * Create a new media player factory.
         * <p>
         * If you want to enable logging or synchronisation of the native library interface you must use
         * {@link #MediaPlayerFactory(LibVlc)} and {@link LibVlcFactory}.
         * <p>
         * This factory constructor will enforce a minimum required native library version check - if a
         * suitable native library version is not found, a RuntimeException will be thrown.
         * <p>
         * If you do not want to enforce this version check, use one of the other constructors that
         * accepts a LibVlc instance that you obtain from the {@link LibVlcFactory}.
         * <p>
         * Most initialisation arguments may be gleaned by invoking <code>"vlc -H"</code>.
         * 
         * @param libvlcArgs initialisation arguments to pass to libvlc
         */
        public MediaPlayerFactory(string[] args) {
            this.instance = LibVlc.libvlc_new(args.Length, args);
        }
        
        /**
         * Release the native resources associated with this factory.
         */
        public void Release() {
            Logger.Debug("Release()");
            if(instance != IntPtr.Zero) {
                LibVlc.libvlc_release(instance);
            }
        }
        
        // === Factory Configuration ================================================
        
        /**
         * Set the application name.
         * 
         * @param userAgent application name
         */
        public void SetUserAgent(string userAgent) {
            Logger.Debug("SetUserAgent(userAgent={})", userAgent);
            SetUserAgent(userAgent, null);
        }
        
        /**
         * Set the application name.
         *
         * @param userAgent application name
         * @param httpUserAgent application name for HTTP
         */
        public void SetUserAgent(string userAgent, string httpUserAgent) {
            Logger.Debug("SetUserAgent(userAgent={},httpUserAgent)", userAgent, httpUserAgent);
            IntPtr userAgentPtr = IntPtr.Zero;
            IntPtr httpUserAgentPtr = IntPtr.Zero;
            try {
                userAgentPtr = NativeString.StringPointer(userAgent);
                if(httpUserAgent != null) {
                    httpUserAgentPtr = NativeString.StringPointer(httpUserAgent);
                }
                LibVlc.libvlc_set_user_agent(instance, userAgentPtr, httpUserAgentPtr);
            }
            finally {
                NativeString.Release(httpUserAgentPtr);
                NativeString.Release(userAgentPtr);
            }
        }
        
        /**
         * Get the available audio outputs.
         * <p>
         * Each audio output has zero or more audio devices, each device having it's own unique
         * identifier that can be used on a media player to set the select the required output device.
         * 
         * @return collection of audio outputs
         */
        public List<AudioOutput> GetAudioOutputs() {
            Logger.Debug("GetAudioOutputs()");
            List<AudioOutput> result = new List<AudioOutput>();
            IntPtr audioOutputsPtr = LibVlc.libvlc_audio_output_list_get(instance);
            IntPtr audioOutputPtr = audioOutputsPtr;
            while(audioOutputPtr != IntPtr.Zero) {
                libvlc_audio_output_t audioOutput = (libvlc_audio_output_t)Marshal.PtrToStructure(audioOutputPtr, typeof(libvlc_audio_output_t));
                string name = NativeString.String(audioOutput.psz_name);
                result.Add(new AudioOutput(name, NativeString.String(audioOutput.psz_description), GetAudioOutputDevices(name)));
                audioOutputPtr = audioOutput.p_next;
            }
            LibVlc.libvlc_audio_output_list_release(audioOutputsPtr);
            return result;
        }
        
        /**
         * Get the devices associated with an audio output.
         *
         * @param outputName output
         * @return collection of audio output devices
         */
        private List<AudioDevice> GetAudioOutputDevices(string outputName) {
            Logger.Debug("GetAudioOutputDevices(outputName={})", outputName);
            IntPtr outputNamePtr = NativeString.StringPointer(outputName);
            int deviceCount = LibVlc.libvlc_audio_output_device_count(instance, outputNamePtr);
            Logger.Debug("deviceCount={}", deviceCount);
            List<AudioDevice> result = new List<AudioDevice>(deviceCount);
            for(int i = 0; i < deviceCount; i ++ ) {
                string deviceId = NativeString.GetNativeString(LibVlc.libvlc_audio_output_device_id(instance, outputNamePtr, i));
                string longName = NativeString.GetNativeString(LibVlc.libvlc_audio_output_device_longname(instance, outputNamePtr, i));
                result.Add(new AudioDevice(deviceId, longName));
            }
            return result;
        }
        
        /**
         * Get the available audio filters.
         * 
         * @return collection of audio filter descriptions
         * 
         * @since libvlc 2.0.0
         */
        public List<ModuleDescription> GetAudioFilters() {
            Logger.Debug("GetAudioFilters()");
            IntPtr moduleDescriptions = LibVlc.libvlc_audio_filter_list_get(instance);
            List<ModuleDescription> result = GetModuleDescriptions(moduleDescriptions);
            LibVlc.libvlc_module_description_list_release(moduleDescriptions);
            return result;
        }
        
        /**
         * Get the available video filters.
         * 
         * @return collection of video filter descriptions
         * 
         * @since libvlc 2.0.0
         */
        public List<ModuleDescription> GetVideoFilters() {
            Logger.Debug("GetVideoFilters()");
            IntPtr moduleDescriptions = LibVlc.libvlc_video_filter_list_get(instance);
            List<ModuleDescription> result = GetModuleDescriptions(moduleDescriptions);
            LibVlc.libvlc_module_description_list_release(moduleDescriptions);
            return result;
        }
        
        /**
         * Convert a collection of native module description structures.
         * 
         * @param moduleDescriptions module descriptions
         * @return collection of module descriptions
         */
        private List<ModuleDescription> GetModuleDescriptions(IntPtr moduleDescriptions) {
            List<ModuleDescription> result = new List<ModuleDescription>();
            IntPtr moduleDescriptionPointer = moduleDescriptions;
            while(moduleDescriptionPointer != IntPtr.Zero) {
                libvlc_module_description_t moduleDescription = (libvlc_module_description_t)Marshal.PtrToStructure(moduleDescriptionPointer, typeof(libvlc_module_description_t));
                result.Add(new ModuleDescription(NativeString.String(moduleDescription.psz_name), NativeString.String(moduleDescription.psz_shortname), NativeString.String(moduleDescription.psz_longname), NativeString.String(moduleDescription.psz_help)));
                moduleDescriptionPointer = moduleDescription.p_next;
            }
            return result;
        }
        
        // === Media Player =========================================================
        
        public EmbeddedMediaPlayer NewEmbeddedMediaPlayer() {
            Logger.Debug("NewEmbeddedMediaPlayer()");
            return NewEmbeddedMediaPlayer(null);
        }
        
        public EmbeddedMediaPlayer NewEmbeddedMediaPlayer(FullScreenStrategy fullScreenStrategy) {
            Logger.Debug("NewEmbeddedMediaPlayer(fullScreenStrategy={})", fullScreenStrategy);
            return new DefaultEmbeddedMediaPlayer(instance, fullScreenStrategy);
        }
        
        public HeadlessMediaPlayer NewHeadlessMediaPlayer() {
            Logger.Debug("NewHeadlessMediaPlayer()");
            return new DefaultHeadlessMediaPlayer(instance);
        }
        
        public MediaListPlayer NewMediaListPlayer() {
            Logger.Debug("NewMediaListPlayer()");
            return new DefaultMediaListPlayer(instance);
        }
        
        // === Video Surface ========================================================
        
        // FIXME how exactly to do this...        
        
        // === Media List ===========================================================
        
        public MediaList.MediaList NewMediaList() {
            Logger.Debug("NewMediaList()");
            return new MediaList.MediaList(instance);
        }
        
        // === Meta Data ============================================================
        
        public MediaMeta GetMediaMeta(string mediaPath, bool parse) {
            return null;
        }
        
        // === Log ==================================================================
        
        /*public NativeLog NewLog() {
            Logger.Debug("NewLog()");
            return new NativeLog();
        }*/
        
        // === Clock ================================================================
        
        public long Clock() {
            return LibVlc.libvlc_clock();
        }
        
        // === Build Information ====================================================
        
        public string Version() {
            return NativeString.String(LibVlc.libvlc_get_version());
        }

        public String Compiler() {
            return NativeString.String(LibVlc.libvlc_get_compiler());
        }

        public String ChangeSet() {
            return NativeString.String(LibVlc.libvlc_get_changeset());
        }
    }
}
