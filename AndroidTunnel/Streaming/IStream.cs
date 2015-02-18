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
	public interface IStream {
		void Start();
		void Prepare();
		void Stop();
		void Release();
		
		void SetDestination(Java.Net.InetAddress address, int port);
		
		int LocalPort{get;}
		int DestinationPort{get;}
		int SSRC{get;}
		string SessionDescriptor{get;}
		bool IsStreaming{get;}
	}
}


