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
using System.Text;

using Caprica.VlcSharp.Binding;
using Caprica.VlcSharp.Binding.Internal;

namespace Caprica.VlcSharp.Player {

    /**
     * Representation of all available media meta data.
     * <p>
     * This implementation retains a reference to the supplied native media instance, and releases
     * this native reference in {@link #release()}.
     * <p>
     * Invoking {@link #getArtworkUrl()}, {@link #getArtwork()} or {@link #tostring()} may cause an
     * HTTP request to be made to download artwork.
     */
    public class DefaultMediaMeta : MediaMeta {
    
        /**
         * Set to true when the player has been released.
         */
    //	    private final AtomicBoolean released = new AtomicBoolean();
    
        /**
         * Associated media instance.
         * <p>
         * May be <code>null</code>.
         */
        private readonly IntPtr media;
    
        /**
         * Artwork.
         * <p>
         * Lazily loaded.
         */
        private Bitmap artwork = null;
      
        /**
         * Create media meta.
         * 
         * @param media media instance
         */
        public DefaultMediaMeta(IntPtr media) {
            this.media = media;
            // Keep a native reference
            LibVlc.libvlc_media_retain(media);
        }
        
        public void Parse() {
            Logger.Debug("parse()");
            LibVlc.libvlc_media_parse(media);
        }
    
        public string GetTitle() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Title);
        }
    
        public void SetTitle(string title) {
            SetMeta(libvlc_meta_e.libvlc_meta_Title, title);
        }
    
        public string GetArtist() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Artist);
        }
    
        public void SetArtist(string artist) {
            SetMeta(libvlc_meta_e.libvlc_meta_Artist, artist);
        }
    
        public string GetGenre() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Genre);
        }
    
        public void SetGenre(string genre) {
            SetMeta(libvlc_meta_e.libvlc_meta_Genre, genre);
        }
    
        public string GetCopyright() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Copyright);
        }
    
        public void SetCopyright(string copyright) {
            SetMeta(libvlc_meta_e.libvlc_meta_Copyright, copyright);
        }
    
        public string GetAlbum() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Album);
        }
    
        public void SetAlbum(string album) {
            SetMeta(libvlc_meta_e.libvlc_meta_Album, album);
        }
    
        public string GetTrackNumber() {
            return GetMeta(libvlc_meta_e.libvlc_meta_TrackNumber);
        }
    
        public void SetTrackNumber(string trackNumber) {
            SetMeta(libvlc_meta_e.libvlc_meta_TrackNumber, trackNumber);
        }
    
        public string GetDescription() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Description);
        }
    
        public void SetDescription(string description) {
            SetMeta(libvlc_meta_e.libvlc_meta_Description, description);
        }
    
        public string GetRating() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Rating);
        }
    
        public void SetRating(string rating) {
            SetMeta(libvlc_meta_e.libvlc_meta_Rating, rating);
        }
    
        public string GetDate() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Date);
        }
    
        public void SetDate(string date) {
            SetMeta(libvlc_meta_e.libvlc_meta_Date, date);
        }
    
        public string GetSetting() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Setting);
        }
    
        public void SetSetting(string setting) {
            SetMeta(libvlc_meta_e.libvlc_meta_Setting, setting);
        }
    
        public string GetUrl() {
            return GetMeta(libvlc_meta_e.libvlc_meta_URL);
        }
    
        public void SetUrl(string url) {
            SetMeta(libvlc_meta_e.libvlc_meta_URL, url);
        }
    
        public string GetLanguage() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Language);
        }
    
        public void SetLanguage(string language) {
            SetMeta(libvlc_meta_e.libvlc_meta_Language, language);
        }
    
        public string GetNowPlaying() {
            return GetMeta(libvlc_meta_e.libvlc_meta_NowPlaying);
        }
    
        public void SetNowPlaying(string nowPlaying) {
            SetMeta(libvlc_meta_e.libvlc_meta_NowPlaying, nowPlaying);
        }
    
        public string GetPublisher() {
            return GetMeta(libvlc_meta_e.libvlc_meta_Publisher);
        }
    
        public void SetPublisher(string publisher) {
            SetMeta(libvlc_meta_e.libvlc_meta_Publisher, publisher);
        }
    
        public string GetEncodedBy() {
            return GetMeta(libvlc_meta_e.libvlc_meta_EncodedBy);
        }
    
        public void SetEncodedBy(string encodedBy) {
            SetMeta(libvlc_meta_e.libvlc_meta_EncodedBy, encodedBy);
        }
    
        public string GetArtworkUrl() {
            return GetMeta(libvlc_meta_e.libvlc_meta_ArtworkURL);
        }
    
        public void SetArtworkUrl(string artworkUrl) {
            SetMeta(libvlc_meta_e.libvlc_meta_ArtworkURL, artworkUrl);
        }
    
        public string GetTrackId() {
            return GetMeta(libvlc_meta_e.libvlc_meta_TrackID);
        }
    
        public void SetTrackId(string trackId) {
            SetMeta(libvlc_meta_e.libvlc_meta_TrackID, trackId);
        }
    
        public Bitmap GetArtwork() {
            Logger.Debug("GetArtwork()");
            if(artwork == null) {
                string artworkUrl = GetArtworkUrl();
                if(artworkUrl != null && artworkUrl.Length > 0) {
    /* FIXME                try {
                        URL url = new URL(artworkUrl);
                        Logger.Debug("url={}", url);
                        artwork = ImageIO.read(url);
                    }
                    catch(Exception e) {
                        throw new RuntimeException("Failed to load artwork", e);
                    }*/
                }
            }
            return artwork;
        }
    
        public void Save() {
            Logger.Debug("save()");
            LibVlc.libvlc_media_save_meta(media);
        }
    
        public void Release() {
            Logger.Debug("release()");
    //	        if(released.compareAndSet(false, true)) {
                LibVlc.libvlc_media_release(media);
    //	        }
        }
    
    /*	    protected void finalize() throws Throwable {
            Logger.Debug("finalize()");
            Logger.Debug("Meta data has been garbage collected");
            // FIXME should this invoke release()?
        }*/
    
        /**
         * Get a local meta data value for a media instance.
         * 
         * @param metaType type of meta data
         * @return meta data value
         */
        private string GetMeta(libvlc_meta_e metaType) {
            Logger.Trace("GetMeta(metaType={},media={})", metaType, media);
            return NativeString.GetNativeString(LibVlc.libvlc_media_get_meta(media, (int)metaType));
        }
    
        /**
         * Set a local meta data value for a media instance.
         * <p>
         * Setting meta does not affect the underlying media file until {@link #save()} is called.
         * 
         * @param metaType type of meta data
         * @param media media instance
         * @param value meta data value
         */
        private void SetMeta(libvlc_meta_e metaType, string metaValue) {
            Logger.Trace("SetMeta(metaType={},media={},value={})", metaType, media, metaValue);
            IntPtr metaValuePtr = NativeString.StringPointer(metaValue);
            try {
                LibVlc.libvlc_media_set_meta(media, (int)metaType, metaValuePtr);
            }
            finally {
                NativeString.Release(metaValuePtr);
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(200);
            sb.Append("DefaultMediaMeta").Append('[');
            sb.Append("title=").Append(GetTitle()).Append(',');
            sb.Append("artist=").Append(GetArtist()).Append(',');
            sb.Append("genre=").Append(GetGenre()).Append(',');
            sb.Append("copyright=").Append(GetCopyright()).Append(',');
            sb.Append("album=").Append(GetAlbum()).Append(',');
            sb.Append("trackNumber=").Append(GetTrackNumber()).Append(',');
            sb.Append("description=").Append(GetDescription()).Append(',');
            sb.Append("rating=").Append(GetRating()).Append(',');
            sb.Append("date=").Append(GetDate()).Append(',');
            sb.Append("setting=").Append(GetSetting()).Append(',');
            sb.Append("url=").Append(GetUrl()).Append(',');
            sb.Append("language=").Append(GetLanguage()).Append(',');
            sb.Append("nowPlaying=").Append(GetNowPlaying()).Append(',');
            sb.Append("publisher=").Append(GetPublisher()).Append(',');
            sb.Append("encodedBy=").Append(GetEncodedBy()).Append(',');
            sb.Append("artworkUrl=").Append(GetArtworkUrl()).Append(',');
            sb.Append("trackId=").Append(GetTrackId()).Append(']');
            return sb.ToString();
        }
    }
}
