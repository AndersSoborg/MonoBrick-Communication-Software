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
using System.Text;

namespace Caprica.VlcSharp.Player {
 
    /**
     * Media meta data.
     */
    public class MediaDetails {
 
        /**
         * Number of titles.
         */
        private int titleCount;

        /**
         * Number of video tracks.
         */
        private int videoTrackCount;

        /**
         * Number of audio tracks.
         */
        private int audioTrackCount;

        /**
         * Number of sub-picture/sub-title tracks.
         */
        private int spuCount;

        /**
         * Collection of title descriptions.
         */
        private List<TrackDescription> titleDescriptions;

        /**
         * Collection of video track descriptions.
         */
        private List<TrackDescription> videoDescriptions;

        /**
         * Collection of audio track descriptions.
         */
        private List<TrackDescription> audioDescriptions;

        /**
         * Collection of sub-title track descriptions.
         */
        private List<TrackDescription> spuDescriptions;

        /**
         * Collection of chapter descriptions for each title.
         */
        private List<List<string>> chapterDescriptions = new List<List<string>>();
 
        public int GetTitleCount() {
            return titleCount;
        }
 
        public void SetTitleCount(int titleCount) {
            this.titleCount = titleCount;
        }
 
        public int GetVideoTrackCount() {
            return videoTrackCount;
        }
 
        public void SetVideoTrackCount(int videoTrackCount) {
            this.videoTrackCount = videoTrackCount;
        }
 
        public int GetAudioTrackCount() {
            return audioTrackCount;
        }
 
        public void SetAudioTrackCount(int audioTrackCount) {
            this.audioTrackCount = audioTrackCount;
        }
 
        public int GetSpuCount() {
            return spuCount;
        }
 
        public void SetSpuCount(int spuCount) {
            this.spuCount = spuCount;
        }
 
        public List<TrackDescription> GetTitleDescriptions() {
            return titleDescriptions;
        }
 
        public void SetTitleDescriptions(List<TrackDescription> titleDescriptions) {
            this.titleDescriptions = titleDescriptions;
        }
 
        public List<TrackDescription> GetVideoDescriptions() {
            return videoDescriptions;
        }
 
        public void SetVideoDescriptions(List<TrackDescription> videoDescriptions) {
            this.videoDescriptions = videoDescriptions;
        }
 
        public List<TrackDescription> GetAudioDescriptions() {
            return audioDescriptions;
        }
 
        public void SetAudioDescriptions(List<TrackDescription> audioDescriptions) {
            this.audioDescriptions = audioDescriptions;
        }
 
        public List<TrackDescription> GetSpuDescriptions() {
            return spuDescriptions;
        }
 
        public void SetSpuDescriptions(List<TrackDescription> spuDescriptions) {
            this.spuDescriptions = spuDescriptions;
        }
 
        public List<List<string>> GetChapterDescriptions() {
            return chapterDescriptions;
        }
 
        public void SetChapterDescriptions(List<List<string>> chapterDescriptions) {
            this.chapterDescriptions = chapterDescriptions;
        }

        public override String ToString() {
            StringBuilder sb = new StringBuilder(200);
            sb.Append("MediaDetails").Append('[');
            sb.Append("titleCount=").Append(titleCount).Append(',');
            sb.Append("videoTrackCount=").Append(videoTrackCount).Append(',');
            sb.Append("audioTrackCount=").Append(audioTrackCount).Append(',');
            sb.Append("spuCount=").Append(spuCount).Append(',');
            sb.Append("titleDescriptions=").Append(titleDescriptions).Append(',');
            sb.Append("videoDescriptions=").Append(videoDescriptions).Append(',');
            sb.Append("audioDescriptions=").Append(audioDescriptions).Append(',');
            sb.Append("spuDescriptions=").Append(spuDescriptions).Append(',');
            sb.Append("chapterDescriptions=").Append(chapterDescriptions).Append(']');
            return sb.ToString();
        }
    }
}
