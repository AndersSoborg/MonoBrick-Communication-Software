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
	public class H263Stream: VideoStream {


		public H263Stream(Android.Hardware.CameraFacing cameraId):base(cameraId) {
			SetVideoEncoder(Android.Media.VideoEncoder.H263);
			this.packetizer = new H263Packetizer();
		}
		
		public override string SessionDescriptor{
			get{

				return 	"m=video "+ DestinationPort.ToString() + 
						" RTP/AVP 96\r\n" + "b=RR:0\r\n" + "a=rtpmap:96 H263-1998/90000\r\n";
			}
		}
		
	}
}

