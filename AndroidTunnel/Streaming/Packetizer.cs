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
	public abstract class AbstractPacketizer {
		
		protected static int rtphl = RtpSocket.HeaderLength;
		
		protected RtpSocket socket = null;
		protected bool running = false;
		protected byte[] buffer;
		
		public AbstractPacketizer() {
			socket = new RtpSocket();
			buffer = socket.Buffer;
		}	
		
		public AbstractPacketizer(System.IO.Stream fis):base() {
			InputStream = fis;
		}
		
		public RtpSocket RtpSocket{
			get{
				return socket;
			}
		}
		
		public System.IO.Stream InputStream{get;set;}
		
		public void SetDestination(Java.Net.InetAddress address, int port) {
			socket.Destination(address, port);
		}
		
		public abstract void Stop();
		public abstract void Start();
		
	}

	/**
	 * RFC 4629
	 */
	public class H263Packetizer: AbstractPacketizer{
		
		private static int MaxPacketSize = 1400;
		private Thread thread = null;
		public H263Packetizer():base() {
		}
		
		public override void Start() {
			if (!running) {
				running = true;
				thread = new Thread(new ThreadStart(Run));
				thread.Start(); 
			}
		}
		
		public override void Stop() {
			running = false;
		}
		
		public void Run() {
			long time, duration = 0, ts = 0;
			int i = 0, j = 0;
			bool firstFragment = true;
			
			try {
				SkipHeader();
			} catch (IOException) {
				//Log.e(TAG,"Couldn't skip mp4 header :/");
				return;
			}
			
			// Each packet we send has a two byte long header (See section 5.1 of RFC 4629)
			buffer[rtphl] = 0;
			buffer[rtphl+1] = 0;
			
			try { 
				while (running) {
					time = SystemClock.ElapsedRealtime();
					if (Fill(rtphl+j+2,MaxPacketSize-rtphl-j-2)<0) 
						return;
					duration += SystemClock.ElapsedRealtime() - time;
					j = 0;
					// Each h263 frame starts with: 0000 0000 0000 0000 1000 00??
					// Here we search where the next frame begins in the bit stream
					for (i=rtphl+2;i<MaxPacketSize-1;i++) {
						if (buffer[i]==0 && buffer[i+1]==0 && (buffer[i+2]&0xFC)==0x80) {
							j=i;
							break;
						}
					}
					// Parse temporal reference
					//tr = (buffer[i+2]&0x03)<<6 | (buffer[i+3]&0xFF)>>2;
					//Log.d(TAG,"j: "+j+" buffer: "+printBuffer(rtphl, rtphl+5)+" tr: "+tr);
					if (firstFragment) {
						// This is the first fragment of the frame -> header is set to 0x0400
						buffer[rtphl] = 4;
						firstFragment = false;
					} else {
						buffer[rtphl] = 0;
					}
					if(j>0) {
						// We have found the end of the frame
						//Log.d(TAG,"End of frame ! duration: "+duration);
						ts+= duration; duration = 0;
						// The last fragment of a frame has to be marked
						socket.MarkNextPacket();
						socket.Send(j);
						socket.UpdateTimestamp(ts*90);
						//Java.Lang.JavaSystem.Arraycopy(buffer,j+2,buffer,rtphl+2,MAXPACKETSIZE-j-2); 
						System.Array.Copy(buffer,j+2,buffer,rtphl+2,MaxPacketSize-j-2);
						j = MaxPacketSize-j-2;
						firstFragment = true;
					} else {
						// We have not found the beginning of another frame
						// The whole packet is a fragment of a frame
						socket.Send(MaxPacketSize);
					}
				}
			} 
			catch (System.Exception){
				running = false;
				//Log.e(TAG,"IOException: "+e.getMessage());
				//e.printStackTrace();
			}
			try{
				socket.Close();
			}
			catch
			{

			}
		}
		
		private int Fill(int offset,int length){
			
			int sum = 0, len;
			
			while (sum<length) {
				
				len = this.InputStream.Read(buffer, offset+sum, length-sum);
				if (len<0) {
					//Log.e(TAG,"End of stream");
					return -1;
				}
				else sum+=len;
			}
			
			return sum;
			
		}
		
		// The InputStream may start with a header that we need to skip
		private void SkipHeader(){
			
			int len = 0;
			
			// Skip all atoms preceding mdat atom
			while (true) {
				this.InputStream.Read(buffer,rtphl,8);
				if (buffer[rtphl+4] == 'm' && buffer[rtphl+5] == 'd' && buffer[rtphl+6] == 'a' && buffer[rtphl+7] == 't') break;
				len = (buffer[rtphl+3]&0xFF) + (buffer[rtphl+2]&0xFF)*256 + (buffer[rtphl+1]&0xFF)*65536;
				if (len<8 || len>1000) {
					//Log.e(TAG,"Malformed header :/ len: "+len+" available: "+is.available());
					break;
				}
				//Log.d(TAG,"Atom skipped: "+printBuffer(rtphl+4,rtphl+8)+" size: "+len);
				this.InputStream.Read(buffer,rtphl,len-8);
			}
			
			// Some phones do not set length correctly when stream is not seekable, still we need to skip the header
			if (len<=0 || len>1000) {
				while (true) {
					while (this.InputStream.ReadByte() != 'm');
					this.InputStream.Read(buffer,rtphl,3);
					if (buffer[rtphl] == 'd' && buffer[rtphl+1] == 'a' && buffer[rtphl+2] == 't') break;
				}
			}
			len = 0;
			
		}
		
	}
}
