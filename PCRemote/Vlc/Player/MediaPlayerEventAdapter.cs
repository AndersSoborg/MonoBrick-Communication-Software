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

namespace Caprica.VlcSharp.Player {
    
    /**
     * Default implementation of the media player event listener.
     * <p>
     * Simply override the methods you're interested in.
     * <p>
     * Events are likely <em>not</em> raised on the Swing Event Dispatch thread so if updating user
     * interface components in response to these events care must be taken to use
     * {@link SwingUtilities#invokeLater(Runnable)}.
     */
    public class MediaPlayerEventAdapter : MediaPlayerEventListener {
    
        // === Events relating to the media player ==================================
    
        virtual public void MediaChanged(MediaPlayer mediaPlayer, IntPtr media, string mrl) {
        }
    
        virtual public void Opening(MediaPlayer mediaPlayer) {
        }
    
        virtual public void Buffering(MediaPlayer mediaPlayer, float newCache) {
        }
    
        virtual public void Playing(MediaPlayer mediaPlayer) {
        
		}
    
        virtual public void Paused(MediaPlayer mediaPlayer) {
        }
    
        virtual public void Stopped(MediaPlayer mediaPlayer) {
        }
    
        virtual public void Forward(MediaPlayer mediaPlayer) {
        }
    
        virtual public void Backward(MediaPlayer mediaPlayer) {
        }
    
        virtual public void Finished(MediaPlayer mediaPlayer) {
        }
    
        virtual public void TimeChanged(MediaPlayer mediaPlayer, long newTime) {
        }
    
        virtual public void PositionChanged(MediaPlayer mediaPlayer, float newPosition) {
        }
    
        virtual public void SeekableChanged(MediaPlayer mediaPlayer, int newSeekable) {
        }
    
        virtual public void PausableChanged(MediaPlayer mediaPlayer, int newSeekable) {
        }
    
        virtual public void TitleChanged(MediaPlayer mediaPlayer, int newTitle) {
        }
    
        virtual public void SnapshotTaken(MediaPlayer mediaPlayer, String filename) {
        }
    
        virtual public void LengthChanged(MediaPlayer mediaPlayer, long newLength) {
        }
    
        virtual public void VideoOutput(MediaPlayer mediaPlayer, int newCount) {
        }
    
        virtual public void Error(MediaPlayer mediaPlayer) {
        }
    
        // === Events relating to the current media =================================
    
        virtual public void MediaSubItemAdded(MediaPlayer mediaPlayer, IntPtr subItem) {
        }
    
        virtual public void MediaDurationChanged(MediaPlayer mediaPlayer, long newDuration) {
        }
    
        virtual public void MediaParsedChanged(MediaPlayer mediaPlayer, int newStatus) {
        }
    
        virtual public void MediaFreed(MediaPlayer mediaPlayer) {
        }
    
        virtual public void MediaStateChanged(MediaPlayer mediaPlayer, int newState) {
        }
    
        virtual public void MediaMetaChanged(MediaPlayer mediaPlayer, int metaType) {
        }
    
        // === Synthetic/semantic events ============================================
    
        virtual public void NewMedia(MediaPlayer mediaPlayer) {
        }
    
        virtual public void SubItemPlayed(MediaPlayer mediaPlayer, int subItemIndex) {
        }
    
        virtual public void SubItemFinished(MediaPlayer mediaPlayer, int subItemIndex) {
        }
    
        virtual public void EndOfSubItems(MediaPlayer mediaPlayer) {
        }
    }
}
