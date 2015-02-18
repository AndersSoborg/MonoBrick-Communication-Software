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
	public class LogActivity : ActivityWithOptionMenu
	{
		private TextView logView = null;
		private ScrollView scrollView = null;
		private int MaxLines = 40;
		private static string savedText = "";
		private bool listenForLog = false;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Log);
			logView = FindViewById<TextView> (Resource.Id.log);
			scrollView = FindViewById<ScrollView> (Resource.Id.logScrollView);
			scrollView.FullScroll(FocusSearchDirection.Down);
			logView.SetText(savedText,TextView.BufferType.Editable);
			scrollView.ScrollTo(0,logView.Height);
			scrollView.FullScroll(FocusSearchDirection.Down);
		}

		protected override void OnBoundToTunnelService (Tunnel tunnel)
		{
			if(!listenForLog && tunnel != null){
				tunnel.LogEvent += WriteLine;
				listenForLog = true;
			}
		}

		protected override void OnResume(){
			base.OnResume();
			logView.SetText(savedText,TextView.BufferType.Editable);
			scrollView.ScrollTo(0,logView.Height);
			scrollView.FullScroll(FocusSearchDirection.Down);
			if(!listenForLog && tunnel != null){
				tunnel.LogEvent += WriteLine;
				listenForLog = true;
			}

		}

		protected override void OnPause ()
		{
			base.OnPause ();
			if(listenForLog){
				tunnel.LogEvent -= WriteLine;
				listenForLog = false;
			}
		}

		private void WriteLine(string message){
			RunOnUiThread(delegate() {
				logView.Append(message+"\n");
				int excessLineNumber = logView.LineCount - MaxLines;
				savedText = logView.Text;
				if (excessLineNumber > 0) {
					int eolIndex = -1;
					for(int i=0; i <excessLineNumber; i++) {
						do {
							eolIndex++;
						} while(eolIndex < savedText.Length && savedText[eolIndex] != '\n');             
					}
					logView.EditableText.Delete(0,eolIndex+1);
				}
			});
		}

	}
}

