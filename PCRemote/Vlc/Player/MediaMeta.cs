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

namespace Caprica.VlcSharp.Player {
 
    /**
     * Specification for local media meta data.
     * <p>
     * It is possible that any particular meta data value may be <code>null</code>.
     * <p>
     * When invoking setter methods to change media meta data then that change is <em>not</em> applied
     * to the media file. It is necessary to call {@link #save()} to commit the changes to the media
     * file.
     * <p>
     * Media meta data instances should be explicitly cleaned up by using {@link #release()}, otherwise
     * a native memory leak may occur.
     * <p>
     * The media may contain meta data over and above that exposed here - this interface provides
     * access to the meta data that vlc can provide.
     * <p>
     * It is <em>not</em> possible to re-read (or re-parse) the media meta data after it has already
     * been parsed - this means that when invoking a setter method on a media meta instance it is not
     * possible to undo that and restore the old value without obtaining a new media instance.
     * <p>
     * Not all media types can be parsed - parsing such media may cause fatal errors or application
     * hangs.
     */
    public interface MediaMeta {

        /**
         * Parse the media to load meta data.
         * <p>
         * If the media is already parsed this will have no effect. If the media is not already parsed
         * then it will be parsed synchronously.
         */
        void Parse();

        /**
         * Get the title meta data.
         *
         * @return title
         */
        string GetTitle();
 
        /**
         * Set the title meta data.
         *
         * @param title title
         */
        void SetTitle(string title);

        /**
         * Get the artist meta data.
         *
         * @return artits
         */
        string GetArtist();

        /**
         * Set the artist meta data.
         *
         * @param artist artist
         */
        void SetArtist(string artist);
 
        /**
         * Get the genre meta data.
         *
         * @return genre
         */
        string GetGenre();

        /**
         * Set the genre meta data.
         *
         * @param genre genre
         */
        void SetGenre(string genre);
 
        /**
      * Get the copyright meta data.
      * 
      * @return copyright
      */
        string GetCopyright();
 
        /**
         * Set the copyright meta data.
         *
         * @param copyright copyright
         */
        void SetCopyright(string copyright);
 
        /**
         * Get the album meta data.
         *
         * @return album
         */
        string GetAlbum();
 
        /**
         * Set the album meta data.
         *
         * @param album album
         */
        void SetAlbum(string album);
 
        /**
         * Get the track number meta data.
         *
         * @return track number
         */
        string GetTrackNumber();
 
        /**
         * Set the track number meta data.
         *
         * @param trackNumber track number
         */
        void SetTrackNumber(string trackNumber);
 
        /**
         * Get the description meta data.
         *
         * @return description
         */
        string GetDescription();

        /**
         * Set the description meta data.
         *
         * @param description description
         */
        void SetDescription(string description);
 
        /**
         * Get the rating meta data.
         *
         * @return rating
         */
        string GetRating();
 
        /**
         * Set the rating meta data.
         *
         * @param rating rating
         */
        void SetRating(string rating);
 
        /**
         * Get the date meta data.
         *
         * @return date
         */
        string GetDate();
 
        /**
         * Set the date meta data.
         *
         * @param date date
         */
        void SetDate(string date);
 
        /**
         * Get the setting meta data.
         *
         * @return setting
         */
        string GetSetting();

        /**
         * Set the setting meta data.
         *
         * @param setting setting
         */
        void SetSetting(string setting);
 
        /**
         * Get the URL meta data.
         *
         * @return url
         */
        string GetUrl();
 
        /**
         * Set the URL meta data.
         *
         * @param url url
         */
        void SetUrl(string url);
 
        /**
         * Get the language meta data.
         *
         * @return language
         */
        string GetLanguage();

        /**
         * Set the language meta data.
         *
         * @param language language
         */
        void SetLanguage(string language);
 
        /**
         * Get the now playing meta data.
         *
         * @return now playing
         */
        string GetNowPlaying();

        /**
         * Set the now playing meta data.
         *
         * @param nowPlaying now playing
         */
        void SetNowPlaying(string nowPlaying);

        /**
         * Get the publisher meta data.
         *
         * @return publisher
         */
        string GetPublisher();
 
        /**
         * Set the publisher meta data.
         *
         * @param publisher publisher
         */
        void SetPublisher(string publisher);

        /**
         * Get the encoded by meta data.
         *
         * @return encoded by
         */
        string GetEncodedBy();
 
        /**
         * Set the encoded by meta data.
         *
         * @param encodedBy encoded by
         */
        void SetEncodedBy(string encodedBy);

        /**
         * Get the artwork URL meta data.
         * <p>
         * <strong>Invoking this method may trigger an HTTP request to download the artwork.</strong>
         *
         * @return artwork URL
         */
        string GetArtworkUrl();
 
        /**
         * Set the artwork URL meta data.
         *
         * @param artworkUrl
         */
        void SetArtworkUrl(string artworkUrl);

        /**
         * Get the track id meta data.
         *
         * @return track id
         */
        string GetTrackId();
 
        /**
         * Set the track id meta data.
         *
         * @param trackId track id
         */
        void SetTrackId(string trackId);

        /**
         * Load the artwork associated with this media.
         * <p>
         * <strong>Invoking this method may trigger an HTTP request to download the artwork.</strong>
         *
         * @return artwork image, or <code>null</code> if no artwork available
         */
        Bitmap GetArtwork();
  
        /**
         * Write the meta data to the media.
         */
        void Save();

        /**
         * Release the resources associated with this meta data instance.
         * <p>
         * If {@link #release()} is not invoked before this instance is discarded, a native memory leak
         * may occur.
         */
        void Release();
    }
}
