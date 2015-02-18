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
using System.IO;
using Java.Util;
using Android.Bluetooth;
using MonoBrick;
using Android.Speech.Tts;

namespace AndroidTunnel
{
	
	[Activity (Label = "Log")]			
	public class CameraViewActivity : ActivityWithOptionMenu
	{
		private ISurfaceHolder holder;
		private SurfaceView camera;
		//private VideoQuality defaultVideoQuality = new VideoQuality();


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.CameraView);
			camera = (SurfaceView)FindViewById(Resource.Id.smallcameraview);
			camera.Holder.SetType(SurfaceType.PushBuffers);
			holder = camera.Holder;
			Session.SetSurfaceHolder(holder);

			/*defaultVideoQuality.resX = 200;
			defaultVideoQuality.resY = 200;
			defaultVideoQuality.frameRate = 15;
			defaultVideoQuality.bitRate = 500*1000; // 500 kb/s

			Session.setDefaultVideoQuality(defaultVideoQuality);*/


		}
		
		protected override void OnBoundToTunnelService (Tunnel tunnel)
		{

		}
		
		protected override void OnResume(){
			base.OnResume();
		}
		
		protected override void OnPause ()
		{
			base.OnPause ();
		}
	}
}

