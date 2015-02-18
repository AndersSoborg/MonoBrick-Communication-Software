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
	public class VideoQuality {
		
		/** Default video stream quality */
		public static VideoQuality DefaultVideoQualiy = new VideoQuality(320,240,25,5000).Clone();
		
		public VideoQuality() {}
		
		public VideoQuality(int resX, int resY, int frameRate, int bitRate) {
			
			this.FrameRate = frameRate;
			this.BitRate = bitRate;
			this.ResX = resX;
			this.ResY = resY;
			
		}
		
		public int FrameRate = 0;
		public int BitRate = 0;
		public int ResX = 0;
		public int ResY = 0;
		public int Orientation = 90;
		
		public bool Equals(VideoQuality quality) {
			if (quality==null) return false;
			return (quality.ResX == this.ResX 				&
			        quality.ResY == this.ResY 				&
			        quality.FrameRate == this.FrameRate	&
			        quality.BitRate == this.BitRate 		);
		}
		
		public VideoQuality Clone() {
			return new VideoQuality(ResX,ResY,FrameRate,BitRate);
		}
		
		public static VideoQuality ParseQuality(String str) {
			VideoQuality quality = new VideoQuality(0,0,0,0);
			if (str != null) {
				String[] config = str.Split('-');
				try {
					quality.BitRate = int.Parse(config[0])*1000; // conversion to bit/s
					quality.FrameRate = int.Parse(config[1]);
					quality.ResX = int.Parse(config[2]);
					quality.ResY = int.Parse(config[3]);
				}
				catch (Exception) {}
			}
			return quality;
		}
		
		public static void Merge(VideoQuality videoQuality, VideoQuality defaultVideoQuality) {
			if (videoQuality.ResX==0) videoQuality.ResX = defaultVideoQuality.ResX;
			if (videoQuality.ResY==0) videoQuality.ResY = defaultVideoQuality.ResY;
			if (videoQuality.FrameRate==0) videoQuality.FrameRate = defaultVideoQuality.FrameRate;
			if (videoQuality.BitRate==0) videoQuality.BitRate = defaultVideoQuality.BitRate;
		}
		
	}
}
