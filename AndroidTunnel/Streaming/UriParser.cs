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

	/**
	 * Parse a URI and configure a Session accordingly
	 * 
	 */
	public class UriParser {
		
		public static void parse(String uri, Session session){
			//bool flash = false;
			//Android.Hardware.CameraFacing camera = CameraFacing.Back;
			
			/*List<NameValuePair> parameters = URLEncodedUtils.parse(URI.create(uri),"UTF-8");
			if (parameters.size()>0) {
				
				// Those parameters must be parsed first or else they won't necessarily be taken into account
				for (Iterator<NameValuePair> it = parameters.iterator();it.hasNext();) {
					NameValuePair param = it.next();


					// FLASH ON/OFF
					if (param.getName().equals("flash")) {
						if (param.getValue().equals("on")) flash = true;
						else flash = false;
					}
					
					// CAMERA -> client can choose between the front facing camera and the back facing camera
					else if (param.getName().equals("camera")) {
						if (param.getValue().equals("back")) camera = CameraInfo.CAMERA_FACING_BACK;
						else if (param.getValue().equals("front")) camera = CameraInfo.CAMERA_FACING_FRONT;
					}
					
				}
				
				for (Iterator<NameValuePair> it = parameters.iterator();it.hasNext();) {
					NameValuePair param = it.next();
					
					// H264
					if (param.getName().equals("h264")) {
						VideoQuality quality = VideoQuality.parseQuality(param.getValue());
						session.addVideoTrack(Session.VIDEO_H264, camera, quality, flash);
					}
					
					// H263
					else if (param.getName().equals("h263")) {
						VideoQuality quality = VideoQuality.parseQuality(param.getValue());
						session.addVideoTrack(Session.VIDEO_H263, camera, quality, flash);
					}
					
					// AMRNB
					else if (param.getName().equals("amrnb")) {
						session.addAudioTrack(Session.AUDIO_AMRNB);
					}
					
					// AMR -> just for convenience: does the same as AMRNB
					else if (param.getName().equals("amr")) {
						session.addAudioTrack(Session.AUDIO_AMRNB);
					}
					
					// AAC -> experimental
					else if (param.getName().equals("aac")) {
						session.addAudioTrack(Session.AUDIO_AAC);
					}
					
					// Generic Audio Stream -> make use of api level 12
					// TODO: Doesn't work :/
					else if (param.getName().equals("testnewapi")) {
						session.addAudioTrack(Session.AUDIO_ANDROID_AMR);
					}
					
				}
			} 
			// Uri has no parameters: the default behavior is to add one h264 track and one amrnb track
			else {
				session.addVideoTrack();
				session.addAudioTrack();
			}*/
			session.AddVideoTrack();
		}
	}
}

