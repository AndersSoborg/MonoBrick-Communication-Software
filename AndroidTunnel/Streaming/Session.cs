using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Graphics.Drawables;
using Android.Text;
using System.IO;
using Java.Util;
using Android.Bluetooth;
using System.Threading;
using System.Diagnostics;
using System.Net;
using MonoBrick;
using Android.Media;
using Android.Net;
using Android.Hardware;


namespace AndroidTunnel
{
	/**
	 * This class makes use of all the streaming package
	 * It represents a streaming session between a client and the phone
	 * A stream is designated by the word "track" in this class
	 * To add tracks to the session you need to call addVideoTrack() or addAudioTrack()
	 */
	public class Session {
		// Default configuration
		private static VideoEncoder defaultVideoEncoder = VideoEncoder.H263; 
		private static AudioEncoder defaultAudioEncoder = AudioEncoder.AmrNb;
		private static VideoQuality defaultVideoQuality = VideoQuality.DefaultVideoQualiy.Clone();
		private static Android.Hardware.CameraFacing defaultCamera = Android.Hardware.CameraFacing.Back ;
		private static ISurfaceHolder surfaceHolder;
		
		private static bool cameraInUse = false;
		private static bool micInUse = false;
		
		class Track {
			public static int AUDIO = 1;
			public static int VIDEO = 2;
			public Track(IStream stream, int type) {
				this.stream = stream;
				this.type = type;
			}
			public IStream stream;
			public int type;
		}
		
		private List<Track> tracks = new List<Track>(); 
		private Java.Net.InetAddress destination;
		private int videoPort;
		
		public Session(Java.Net.InetAddress destination, int videoPort) {
			this.destination = destination;
			this.videoPort = videoPort;
		}

		public event Action StreamStarted = delegate {};
		public event Action StreamStoped = delegate {};

		/** Set default video stream quality, it will be used by addVideoTrack */
		public static void SetDefaultVideoQuality(VideoQuality quality) {
			defaultVideoQuality = quality;
		}
		
		/** Set the default audio encoder, it will be used by addAudioTrack */
		public static void SetDefaultAudioEncoder(AudioEncoder encoder) {
			defaultAudioEncoder = encoder;
		}
		
		/** Set the default video encoder, it will be used by addVideoTrack() */
		public static void SetDefaultVideoEncoder(VideoEncoder encoder) {
			defaultVideoEncoder = encoder;
		}
		
		/** Set the Surface required by MediaRecorder to record video */
		public static void SetSurfaceHolder(ISurfaceHolder sh) {
			surfaceHolder = sh;
		}

		/** Set the Surface required by MediaRecorder to record video */
		public static ISurfaceHolder GetSurfaceHolder() {
			return surfaceHolder;
		}

		/** Add the default video track with default configuration */
		public void AddVideoTrack(){
			AddVideoTrack(defaultVideoEncoder,defaultCamera,defaultVideoQuality,false);
		}
		
		/** Add default audio track with default configuration */
		public void AddVideoTrack(VideoEncoder encoder,Android.Hardware.CameraFacing camera, VideoQuality videoQuality, bool flash) {
			IStream stream = null;
			VideoQuality.Merge(videoQuality,defaultVideoQuality);
			switch (encoder) {
				case VideoEncoder.H263:
					stream = new H263Stream(camera);
					break;
				case VideoEncoder.H264:
					throw new NotImplementedException();
					//break;
			}
			if (stream != null) {
				//Log.d(TAG,"Quality is: "+videoQuality.resX+"x"+videoQuality.resY+"px "+videoQuality.frameRate+"fps, "+videoQuality.bitRate+"bps");
				((VideoStream) stream).SetVideoQuality(videoQuality);
				((VideoStream) stream).SetPreviewDisplay(surfaceHolder);
				((VideoStream) stream).SetFlashState(flash);
				stream.SetDestination(destination, videoPort);
				tracks.Add(new Track(stream,Track.VIDEO));
			}
			else{
				//do something here
			}
		}

		public void AddAudioTrack() {
			AddAudioTrack(defaultAudioEncoder);
		}
		
		public void AddAudioTrack(AudioEncoder encoder) {
			throw new NotImplementedException();
			/*IStream stream = null;
			switch (encoder) {
			case AUDIO_AMRNB:
				Log.d(TAG,"Audio streaming: AMR");
				stream = new AMRNBStream();
				break;
			case AUDIO_ANDROID_AMR:
				Log.d(TAG,"Audio streaming: GENERIC");
				stream = new GenericAudioStream();
				break;
			case AUDIO_AAC:
				Log.d(TAG,"Audio streaming: AAC (experimental)");
				stream = new AACStream();
				break;
			}
			
			if (stream != null) {
				stream.setDestination(destination, 5004);
				tracks.add(new Track(stream,Track.AUDIO));
			}*/
			
		}
		
		/** Return a session descriptor that can be stored in a file or sent to a client with RTSP
		 * @return The session descriptor
		 * @throws IllegalStateException
		 * @throws IOException
		 */
		object sessionLock = new object();

		public String GetSessionDescriptor(){
			String sessionDescriptor = "";
			// Prevent two different sessions from using the same peripheral at the same time
			lock(sessionLock) {
				for (int i=0;i<tracks.Count;i++) {
					Track track = tracks[i];
					if ((track.type == Track.VIDEO && !cameraInUse) || (track.type == Track.AUDIO && !micInUse)) {
						sessionDescriptor += track.stream.SessionDescriptor;
						sessionDescriptor += "a=control:trackID="+i+"\r\n";
					}
				}
			}
			return sessionDescriptor;
		}
		
		public bool trackExists(int id) {
			try{
				if(tracks[id] != null){
					return true;
				}
				return false;
			} catch (Exception) {
				return false;
			}
		}
		
		public int GetTrackDestinationPort(int id) {
			return tracks[id].stream.DestinationPort;
		}
		
		public int GetTrackLocalPort(int id) {
			return tracks[id].stream.LocalPort;
		}
		
		public void SetTrackDestinationPort(int id, int port) {
			tracks[id].stream.SetDestination(destination,port);
		}
		
		public int GetTrackSSRC(int id) {
			return tracks[id].stream.SSRC;
		}
		
		/** The destination address for all the streams of the session
		 * @param destination The destination address
		 */
		public void SetDestination(Java.Net.InetAddress destination) {
			this.destination =  destination;
		}
		
		/** Start stream with id trackId */
		public void Start(int trackId) {
			Track track = tracks[trackId];
			//String type = track.type==Track.VIDEO ? "Video stream" : "Audio stream";
			IStream stream = track.stream;
			try {
				if (stream!=null && !stream.IsStreaming) {
					// Prevent two different sessions from using the same peripheral at the same time
					lock(sessionLock) {
						if (track.type == Track.VIDEO) {
							if (!cameraInUse) {
								stream.Prepare();
								stream.Start();
								cameraInUse = true;
							}
						}
						if (track.type == Track.AUDIO) {
							if (!micInUse) {
								stream.Prepare();
								stream.Start();
								micInUse = true;
							}
						}
					}
				}
				if(StreamStarted != null)
					StreamStarted();
			} 
			catch 
			{
			
			} 
		}
		
		/** Start existing streams */
		public void StartAll() {
			for (int i=0;i<tracks.Count;i++) {
				Start(i);
			}
		}
		
		/** Stop existing streams */
		public void StopAll() {
			foreach(Track track in tracks){
				track.stream.Stop();
			}
			if(StreamStoped != null)
				StreamStoped();
		}
		
		/** Delete all existing tracks & release associated resources */
		public void Flush() {
			foreach(Track track in tracks){
				track.stream.Release();
				if (track.type == Track.VIDEO) cameraInUse = false;
				else if (track.type == Track.AUDIO) micInUse = false;
			}
		}
	}
}