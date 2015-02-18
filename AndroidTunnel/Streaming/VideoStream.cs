using Android.Hardware;
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

namespace AndroidTunnel
{
	public abstract class VideoStream: MediaStream, ISurfaceHolderCallback{
		
		protected VideoQuality quality = VideoQuality.DefaultVideoQualiy.Clone();
		protected ISurfaceHolderCallback surfaceHolderCallback = null;
		protected ISurfaceHolder surfaceHolder = null;
		protected bool flashState = false,  qualityHasChanged = false;
		protected Android.Hardware.CameraFacing cameraId;
		protected Camera camera;
		protected Android.Media.VideoEncoder videoEncoder;
		public VideoStream(Android.Hardware.CameraFacing cameraId):base() {
			this.cameraId = cameraId;
		}
		
		public override void Stop() {
			if (IsStreaming) {
				try {
					base.Stop();
				} catch {
					// stop() can throw a RuntimeException when called too quickly after start() !
					//Log.d(TAG,"stop() called too quickly after start() but it's okay");
				} 
				try {
					// We reconnect to camera just to stop the preview
					if (camera != null) {
						camera.Reconnect();
						camera.StopPreview();
					}
				} catch (IOException) {}
			}
		}
		
		public override void Prepare(){
			
			if (camera == null) {
				camera = Camera.Open();
			}
			
			// We reconnect to camera to change flash state if needed
			camera.Reconnect();
			Camera.Parameters parameters = camera.GetParameters();
			parameters.FlashMode = flashState?Camera.Parameters.FlashModeTorch:Camera.Parameters.FlashModeOff;
			camera.SetParameters(parameters);
			camera.SetDisplayOrientation(quality.Orientation);
			camera.StopPreview();
			camera.Unlock();
			base.SetCamera(camera);

			base.SetVideoSource(Android.Media.VideoSource.Camera);
			base.SetOutputFormat(Android.Media.OutputFormat.ThreeGpp);
			base.SetMaxDuration(0);
			base.SetMaxFileSize(int.MaxValue);

			base.SetVideoEncoder(videoEncoder);
			base.SetVideoSize(quality.ResX,quality.ResY);
			base.SetVideoFrameRate(quality.FrameRate);
			base.SetVideoEncodingBitRate(quality.BitRate);


			//SetAudioSource(Android.Media.AudioSource.Mic);
			/*SetVideoSource(Android.Media.VideoSource.Camera);
			
			SetOutputFormat(Android.Media.OutputFormat.ThreeGpp);
			//SetAudioEncoder(Android.Media.AudioEncoder.AmrNb);
			SetVideoEncoder(Android.Media.VideoEncoder.H263);
			SetVideoSize(640,480);
			SetVideoFrameRate(15);
			SetVideoEncodingBitRate(500000);*/

			base.SetPreviewDisplay(surfaceHolder.Surface);
			base.Prepare();
			// Reset flash state to ensure that default behavior is to turn it off
			flashState = false;
			
			// Quality has been updated
			qualityHasChanged = false;
			
		}
		
		/**
		 * Call this one instead of setPreviewDisplay(Surface sv) and don't worry about the SurfaceHolder.Callback
		 */
		public void SetPreviewDisplay(ISurfaceHolder sh) {
			surfaceHolder = sh;
			sh.AddCallback(this);
		}

		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			//Log.d(TAG,"Surface changed !");
			surfaceHolder = holder;
		}
		
		public void SurfaceCreated(ISurfaceHolder holder)
		{
			//Log.d(TAG,"Surface created !");
			surfaceHolder = holder;
		}
		
		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			if (IsStreaming) 
				Stop();
			//Log.d(TAG,"Surface destroyed !");
		}

		/** Turn flash on or off if phone has one */
		public void SetFlashState(bool state) {
			// Test if phone has a flash
			//if (context.getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
			// Takes effect when configure() is called
			flashState = state;
			//}
		}
		
		public override void SetVideoSize(int width, int height) {
			if (quality.ResX != width || quality.ResY != height) {
				quality.ResX = width;
				quality.ResY = height;
				qualityHasChanged = true;
			}
		}
		
		public override void SetVideoFrameRate(int rate) {
			if (quality.FrameRate != rate) {
				quality.FrameRate = rate;
				qualityHasChanged = true;
			}
		}
		
		public override void SetVideoEncodingBitRate(int bitRate) {
			if (quality.BitRate != bitRate) {
				quality.BitRate = bitRate;
				qualityHasChanged = true;
			}
		}
		
		public void SetVideoQuality(VideoQuality videoQuality) {
			if (!quality.Equals(videoQuality)) {
				quality = videoQuality;
				qualityHasChanged = true;
			}
		}
		
		public override void SetVideoEncoder(Android.Media.VideoEncoder videoEncoder) {
			this.videoEncoder = videoEncoder;
		}
		
		//public String generateSessionDescriptor();
		
		public override void Release() {
			Stop();
			if (camera != null) camera.Release();
			if (surfaceHolderCallback != null && surfaceHolder != null) {
				surfaceHolder.RemoveCallback(surfaceHolderCallback);
			}
			base.Release();
		}
		
	}
}
