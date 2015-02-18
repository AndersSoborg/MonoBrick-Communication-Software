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

	public abstract class MediaStream: MediaRecorder,IStream {
		
		private LocalServerSocket lss = null;
		private LocalSocket receiver, sender = null;
		private LocalSocketAddress localSocketAddress = null;
		private static int id = 0;
		private int socketId;
		protected AbstractPacketizer packetizer = null;

		public MediaStream():base() {
			IsStreaming = false;
			try {
				lss = new LocalServerSocket("localServer"+id);
			} catch{
				//throw new IOException("Can't create local socket !");
			}
			socketId = id;
			id++;
		}
		
		public void SetDestination(Java.Net.InetAddress address, int port) {
			packetizer.SetDestination(address, port);
		}
		
		public int DestinationPort{
			get{
				return packetizer.RtpSocket.Port;
			}
		}
		
		public int LocalPort{
			get{
				return packetizer.RtpSocket.LocalPort;
			}
		}
		
		public bool IsStreaming{get;private set;}
		
		public override void Prepare(){
			CreateSockets();
			//SetOutputFile("/sdcard/log.txt");
			SetOutputFile(sender.FileDescriptor);
			base.Prepare();
		}
		
		public override void Start(){
			try {
				base.Start();
				// receiver.getInputStream contains the data from the camera
				// the packetizer encapsulates this stream in an RTP stream and send it over the network
				packetizer.InputStream = receiver.InputStream;
				packetizer.Start();
				IsStreaming = true;
			} catch (IOException) {
				throw new Exception("Something happened with the local sockets :/ Start failed");
			} catch (Exception) {
				throw new Exception("setPacketizer() should be called before start(). Start failed");
			}
		}
		
		public override void Stop() {
			packetizer.Stop();
			if (IsStreaming) {
				try {
					base.Stop();
				}
				catch {}
				finally {
					base.Reset();
					IsStreaming = false;
					CloseSockets();
				}
			}
			
		}
		
		public int SSRC{
			get{
				return packetizer.RtpSocket.SSRC;
			}
		}
		
		public abstract string SessionDescriptor{get;}

		private void CreateSockets(){
			receiver = new LocalSocket();
			localSocketAddress = new LocalSocketAddress("localServer"+socketId);
			receiver.Connect(localSocketAddress);
			receiver.ReceiveBufferSize = 500000;
			receiver.SendBufferSize = 500000;
			sender = lss.Accept();
			sender.ReceiveBufferSize = 500000;
			sender.SendBufferSize = 500000;
		}
		
		private void CloseSockets() {
			try {
				sender.Close();
				receiver.Close();
			} 
			catch{

			}
		}
		
		public override void Release() {
			Stop();
			try {
				lss.Close();
			}
			catch (Exception) {}
			base.Release();
		}
		
	}
}


