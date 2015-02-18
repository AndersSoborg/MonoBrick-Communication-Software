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
using Caprica.VlcSharp.MediaList.Events;
using Caprica.VlcSharp.Util.Concurrent;

namespace Caprica.VlcSharp.MediaList {
    
    /**
     * A media list (i.e. a play-list).
     * <p>
     * To do anything more advanced than the functionality provided by this class, the underlying native
     * media list instance is accessible via {@link #MediaListInstance}.
     */
    public class MediaList {

        /**
         * Collection of media player event listeners.
         */
        private readonly List<MediaListEventListener> eventListenerList = new List<MediaListEventListener>();

        /**
         * Factory to create media player events from native events.
         */
        private readonly MediaListEventFactory eventFactory;

        /**
         * Background thread to event notifications.
         * <p>
         * The single-threaded nature of this executor service ensures that events are delivered to
         * listeners in a thread-safe manner and in their proper sequence.
         */
        private readonly SingleThreadExecutor listenersService = new SingleThreadExecutor();

        /**
         * Native library instance.
         */
        private IntPtr instance;

        /**
         * Play-list instance.
         */
        private IntPtr mediaListInstance;

        /**
         * Event manager instance.
         */
        private IntPtr mediaListEventManager;
        
        /**
         * Call-back to handle native media player events.
         */
        private MediaListCallbackDelegate callback;

        /**
         * Set to true when the media list has been released.
         */
//        private final AtomicBoolean released = new AtomicBoolean();

        /**
         * Standard media options to be applied to each media item that is played.
         */
        private string[] standardMediaOptions;

        /**
         * Create a new media list.
         * 
         * @param libvlc native interface
         * @param instance native library instance
         */
        public MediaList(IntPtr instance) : this(instance, IntPtr.Zero) {
        }

        /**
         * Create a media list for a given native media list instance.
         * 
         * @param libvlc native interface
         * @param instance native library instance
         * @param mediaListInstance media list instance
         */
        public MediaList(IntPtr instance, IntPtr mediaListInstance) {
            this.eventFactory = new MediaListEventFactory(this);
            this.instance = instance;
            CreateInstance(mediaListInstance);
        }

        /**
         * Add a component to be notified of media list events.
         * 
         * @param listener component to add
         */
        public void AddMediaListEventListener(MediaListEventListener listener) {
            Logger.Debug("AddMediaListEventListener(listener={})", listener);
            eventListenerList.Add(listener);
        }

        /**
         * Remove a component previously added so that it no longer receives media
         * list events.
         * 
         * @param listener component to remove
         */
        public void RemoveListEventListener(MediaListEventListener listener) {
            Logger.Debug("RemoveMediaListEventListener(listener={})", listener);
            eventListenerList.Remove(listener);
        }

        /**
         * Set standard media options for all media items subsequently played.
         * <p>
         * This will <strong>not</strong> affect any currently playing media item.
         * 
         * @param standardMediaOptions options to apply to all subsequently played media items
         */
        public void SetStandardMediaOptions(params string[] standardMediaOptions) {
            Logger.Debug("SetStandardMediaOptions(standardMediaOptions={})", standardMediaOptions);
            this.standardMediaOptions = standardMediaOptions;
        }

        /**
         * Add a media item, with options, to the play-list.
         * 
         * @param mrl media resource locator
         * @param mediaOptions zero or more media item options
         */
        public void AddMedia(string mrl, params string[] mediaOptions) {
            Logger.Debug("AddMedia(mrl={},mediaOptions={})", mrl, mediaOptions);
            try {
                LockList();
                // Create a new native media descriptor
                IntPtr mediaDescriptor = newMediaDescriptor(mrl, mediaOptions);
                // Insert the media descriptor into the media list
                LibVlc.libvlc_media_list_add_media(mediaListInstance, mediaDescriptor);
                // Release the native reference
                ReleaseMediaDescriptor(mediaDescriptor);
            }
            finally {
                UnlockList();
            }
        }

        /**
         * Insert a media item, with options, to the play-list.
         * 
         * @param index position at which to insert the media item (counting from zero)
         * @param mrl media resource locator
         * @param mediaOptions zero or more media item options
         */
        public void InsertMedia(int index, string mrl, params string[] mediaOptions) {
            Logger.Debug("InsertMedia(index={},mrl={},mediaOptions={})", index, mrl, mediaOptions);
            try {
                LockList();
                // Create a new native media descriptor
                IntPtr mediaDescriptor = newMediaDescriptor(mrl, mediaOptions);
                // Insert the media descriptor into the media list
                LibVlc.libvlc_media_list_insert_media(mediaListInstance, mediaDescriptor, index);
                // Release the native reference
                ReleaseMediaDescriptor(mediaDescriptor);
            }
            finally {
                UnlockList();
            }
        }

        /**
         * Remove a media item from the play-list.
         * 
         * @param index item to remove (counting from zero)
         */
        public void RemoveMedia(int index) {
            Logger.Debug("RemoveMedia(index={})", index);
            try {
                LockList();
                IntPtr oldMediaInstance = LibVlc.libvlc_media_list_item_at_index(mediaListInstance, index);
                if(oldMediaInstance != IntPtr.Zero) {
                    // Remove the media descriptor from the media list
                    LibVlc.libvlc_media_list_remove_index(mediaListInstance, index);
                    // Release the native media instance
                    LibVlc.libvlc_media_release(oldMediaInstance);
                }
            }
            finally {
                UnlockList();
            }
        }

        /**
         * Clear the list.
         */
        public void Clear() {
            Logger.Debug("Clear()");
            try {
                LockList();
                // Traverse the list from the end back to the start...
                for(int i = LibVlc.libvlc_media_list_count(mediaListInstance)-1; i >= 0; i--) {
                    LibVlc.libvlc_media_list_remove_index(mediaListInstance, i);
                }
            }
            finally {
                UnlockList();
            }
        }

        /**
         * Get the number of items currently in the list.
         * 
         * @return item count
         */
        public int Size() {
            Logger.Debug("Size()");
            try {
                LockList();
                int size = LibVlc.libvlc_media_list_count(mediaListInstance);
                return size;
            }
            finally {
                UnlockList();
            }
        }

        /**
         * Test if the play-list is read-only.
         * 
         * @return <code>true</code> if the play-list is currently read-only, otherwise <code>false</code>
         */
        public bool IsReadOnly() {
            Logger.Debug("IsReadOnly()");
            return LibVlc.libvlc_media_list_is_readonly(mediaListInstance) == 0;
        }

        /**
         * Get the list of items.
         * 
         * @return list of items
         */
        // FIXME readonly collection?
        public List<MediaListItem> Items() {
            Logger.Debug("Items()");
            List<MediaListItem> result = new List<MediaListItem>();
            try {
                LockList();
                for(int i = 0; i < LibVlc.libvlc_media_list_count(mediaListInstance); i++) { 
                    IntPtr mediaInstance = LibVlc.libvlc_media_list_item_at_index(mediaListInstance, i);
                    result.Add(NewMediaListItem(mediaInstance));
                }
            }
            finally {
                UnlockList();
            }
            return result;
        }

        /**
         * Create a new media list item for a give native media instance.
         * 
         * @param mediaInstance native media instance
         * @return media list item
         */
        private MediaListItem NewMediaListItem(IntPtr mediaInstance) {
            string name = NativeString.GetNativeString(LibVlc.libvlc_media_get_meta(mediaInstance, (int)libvlc_meta_e.libvlc_meta_Title));
            string mrl = NativeString.GetNativeString(LibVlc.libvlc_media_get_mrl(mediaInstance));
            List<MediaListItem> subItems;
            IntPtr subItemList = LibVlc.libvlc_media_subitems(mediaInstance);
            if(subItemList != IntPtr.Zero) {
                try {
                    LibVlc.libvlc_media_list_lock(subItemList);
                    subItems = new List<MediaListItem>();
                    for(int i = 0; i < LibVlc.libvlc_media_list_count(subItemList); i++) {
                        IntPtr subItemInstance = LibVlc.libvlc_media_list_item_at_index(subItemList, i);
                        subItems.Add(NewMediaListItem(subItemInstance));
                        LibVlc.libvlc_media_release(subItemInstance);
                    }
                }
                finally {
                    LibVlc.libvlc_media_list_unlock(subItemList);
                }
                LibVlc.libvlc_media_list_release(subItemList);
            }
            else {
                subItems = new List<MediaListItem>(0);
            }
            return new MediaListItem(name, mrl, subItems);
        }

        /**
         * Clean up media list resources.
         */
        public void Release() {
            Logger.Debug("Release()");
//            if(released.compareAndSet(false, true)) {
                DestroyInstance();
//            }
        }

        /**
         * Create and initialise a new media list instance.
         */
        private void CreateInstance(IntPtr mediaListInstance) {
            Logger.Debug("CreateInstance()");
            if(mediaListInstance == IntPtr.Zero) {
                mediaListInstance = LibVlc.libvlc_media_list_new(instance);
            }

            this.mediaListInstance = mediaListInstance;
            Logger.Debug("mediaListInstance={}", mediaListInstance);
            
            mediaListEventManager = LibVlc.libvlc_media_list_event_manager(mediaListInstance);
            Logger.Debug("mediaListEventManager={}", mediaListEventManager);
            
            RegisterEventListener();
        }

        /**
         * Clean up and free the media list instance.
         */
        private void DestroyInstance() {
            Logger.Debug("DestroyInstance()");
            
            DeregisterEventListener();
            
            if(mediaListInstance != IntPtr.Zero) {
                LibVlc.libvlc_media_list_release(mediaListInstance);
            }

            Logger.Debug("Shut down listeners...");
            listenersService.Shutdown();
            Logger.Debug("Listeners shut down.");
        }

        /**
         * Register a call-back to receive native media player events.
         */
        private void RegisterEventListener() {
            Logger.Debug("RegisterEventListener()");
            callback = new MediaListCallbackDelegate(HandleEvent);
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                if(value >= (int)libvlc_event_e.libvlc_MediaListItemAdded && value <= (int)libvlc_event_e.libvlc_MediaListWillDeleteItem) {
                    Logger.Debug("event={}", value);
                    int result = LibVlc.libvlc_event_attach(mediaListEventManager, value, callbackPtr, IntPtr.Zero);
                    Logger.Debug("result={}", result);
                }
            }
        }

        /**
         * De-register the call-back used to receive native media player events.
         */
        private void DeregisterEventListener() {
            Logger.Debug("DeregisterEventListener()");
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
            foreach(int value in Enum.GetValues(typeof(libvlc_event_e))) {
                if(value >= (int)libvlc_event_e.libvlc_MediaListItemAdded && value <= (int)libvlc_event_e.libvlc_MediaListWillDeleteItem) {
                    Logger.Debug("event={}", value);
                    LibVlc.libvlc_event_detach(mediaListEventManager, value, callbackPtr, IntPtr.Zero);
                }
            }
        }

        /**
         * Raise an event.
         * 
         * @param mediaListEvent event to raise, may be <code>null</code>
         */
        private void RaiseEvent(MediaListEvent mediaListEvent) {
            Logger.Trace("raiseEvent(mediaListEvent={}", mediaListEvent);
            if(mediaListEvent != null) {
                listenersService.Submit(new NotifyEventListenersRunnable(this, mediaListEvent));
            }
        }

        /**
         * Acquire the media list lock.
         */
        private void LockList() {
            Logger.Debug("LockList()");
            LibVlc.libvlc_media_list_lock(mediaListInstance);
        }

        /**
         * Release the media list lock.
         */
        private void UnlockList() {
            Logger.Debug("UnlockList()");
            LibVlc.libvlc_media_list_unlock(mediaListInstance);
        }

        /**
         * Create a new native media instance.
         * 
         * @param media media resource locator
         * @param mediaOptions zero or more media options
         * @return native media instance
         */
        private IntPtr newMediaDescriptor(string media, params string[] mediaOptions) {
            Logger.Debug("newMediaDescriptor(media={},mediaOptions={})", media, mediaOptions);
            IntPtr mediaPointer = NativeString.StringPointer(media);
            IntPtr mediaInstance = IntPtr.Zero;
            if(mediaPointer != IntPtr.Zero) {
                try {
                    mediaInstance = LibVlc.libvlc_media_new_path(instance, mediaPointer);
                    Logger.Debug("mediaDescriptor={}", mediaInstance);
                    if (mediaListInstance != IntPtr.Zero) {
                        // Set the standard media options (if any)...
                        AddMediaOptions(mediaInstance, standardMediaOptions); // FIXME handle return false?
                        // Set the particular media options (if any)...
                        AddMediaOptions(mediaInstance, mediaOptions); // FIXME handle return false?
                    }
                }
                finally {
                    NativeString.Release(mediaPointer);
                }
            }
            return mediaInstance;
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

        /**
         * Release a native media instance.
         * 
         * @param mediaDescripor native media instance
         */
        private void ReleaseMediaDescriptor(IntPtr mediaDescriptor) {
            Logger.Debug("ReleaseMediaDescriptor(mediaDescriptor={})", mediaDescriptor);
            LibVlc.libvlc_media_release(mediaDescriptor);
        }

        /**
         * Get the native media list instance handle.
         * 
         * @return native media list handle
         */
        public IntPtr MediaListInstance() {
            return mediaListInstance;
        }
        
        /**
         * A call-back to handle events from the native media list.
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
                RaiseEvent(eventFactory.CreateEvent(e));
            }
        }

        /**
         * Native delegate declaration for the media list event callback.
         * 
         * @param evt event
         * @param data user data (unused)
         */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MediaListCallbackDelegate(IntPtr evt, IntPtr userData);

        /**
         * A runnable task used to fire event notifications.
         * <p>
         * Care must be taken not to re-enter the native library during an event notification so the
         * notifications are off-loaded to a separate thread.
         * <p>
         * These events therefore do <em>not</em> run on the Event Dispatch Thread.
         */
        // FIXME protection level?
        public class NotifyEventListenersRunnable : Runnable {

            /**
             * Enclosing media list instance.
             */
            private readonly MediaList mediaList;

            /**
             * Event to notify.
             */
            private readonly MediaListEvent mediaListEvent;

            /**
             * Create a runnable.
             * 
             * @param mediaList enclosing media list instance
             * @param mediaPlayerEvent event to notify
             */
            public NotifyEventListenersRunnable(MediaList mediaList, MediaListEvent mediaListEvent) {
                this.mediaList = mediaList;
                this.mediaListEvent = mediaListEvent;
            }

            public void Run() {
                Logger.Trace("Run()");
                for(int i = mediaList.eventListenerList.Count - 1; i >= 0; i -- ) {
                    MediaListEventListener listener = mediaList.eventListenerList[i];
                    try {
                        mediaListEvent.Notify(listener);
                    }
                    catch(Exception t) {
                        Logger.Warn("Event listener {} threw an exception", t, listener);
                        // Continue with the next listener...
                    }
                }
                Logger.Trace("runnable exits");
            }
        }
    }
}
