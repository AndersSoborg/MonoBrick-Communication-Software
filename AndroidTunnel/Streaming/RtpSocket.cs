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
using System.Net.Sockets;

namespace AndroidTunnel
{
	
	public class RtpSocket {
		
		private Java.Net.DatagramSocket usock;
		private Java.Net.DatagramPacket upack;

		//private UdpClient connection; 
		//private IPEndPoint ipEndPoint;


		private byte[] buffer = new byte[MTU];
		private int seq = 0;
		private bool upts = false;
		private int ssrc;
		private int port = -1;
		
		public static int HeaderLength = 12;
		public static int MTU = 1500;
		
		public RtpSocket(byte[] buffer, Java.Net.InetAddress dest, int dport) {
			upack.Port = dport;
			upack.Address = dest;
		}
		
		public RtpSocket() {
			
			/*							     Version(2)  Padding(0)					 					*/
			/*									 ^		  ^			Extension(0)						*/
			/*									 |		  |				^								*/
			/*									 | --------				|								*/
			/*									 | |---------------------								*/
			/*									 | ||  -----------------------> Source Identifier(0)	*/
			/*									 | ||  |												*/
			buffer[0] = (byte) Java.Lang.Integer.ParseInt("10000000",2);
			
			/* Payload Type */
			buffer[1] = (byte) 96;
			
			/* Byte 2,3        ->  Sequence Number                   */
			/* Byte 4,5,6,7    ->  Timestamp                         */
			
			/* Byte 8,9,10,11  ->  Sync Source Identifier            */
			SetLong((ssrc=(new Random()).NextInt()),8,12);
			
			try {
				usock = new Java.Net.DatagramSocket();
			} catch{
				
			}
			upack = new Java.Net.DatagramPacket(buffer, 1);
			
		}
		
		public void Close() {
			usock.Close();
		}
		
		public int SSRC{
			set{
				this.ssrc= value; 
				SetLong(ssrc,8,12);
			}
			get{
				return this.ssrc;
			}
			
		}
		
		public void Destination(Java.Net.InetAddress address, int port) {
			this.port = port;
			upack.Port = port;
			upack.Address = address;
			/*ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.35"), port);
			connection = new UdpClient();
			connection.Connect(ipEndPoint);*/
		}
		
		public byte[] Buffer{
			get{
				return buffer;
			}
		}
		
		public int Port{
			get{
				return port;
			}
		}
		
		public int LocalPort{
			get{
				//return 0;
				return usock.LocalPort;
			}
		}
		
		/* Send RTP packet over the network */
		public void Send(int length){
			
			UpdateSequence();
			/*byte[] sendBuffer = new byte[length];
			System.Array.Copy(buffer,sendBuffer,length);
			connection.Send(sendBuffer,length);*/
			upack.SetData(buffer,0,length);
			upack.Length = length;

			usock.Send(upack);
			
			if (upts) {
				upts = false;
				buffer[1] -= 0x80;
			}
			
		}
		
		private void UpdateSequence() {
			SetLong(++seq, 2, 4);
		}
		
		public void UpdateTimestamp(long timestamp) {
			SetLong(timestamp, 4, 8);
		}
		
		public void MarkNextPacket() {
			upts = true;
			buffer[1] += 0x80; // Mark next packet
		}
		
		private void SetLong(long n, int begin, int end) {
			for (end--; end >= begin; end--) {
				buffer[end] = (byte) (n % 256);
				n >>= 8;
			}
		}	
		
	}
	
}

