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
using System.Net.Sockets;

namespace AndroidTunnel
{

	public static class Helper
	{
		private static List<string> lines = new List<string>();
		private static int offset = 0;
		const int maxSize = 1500;
		private static byte[] buf = new byte[maxSize];
		public static string ReadLine(this NetworkStream stream){
			if(lines.Count != 0){
				string s = lines[0];
				lines.RemoveAt(0);
				return s;
			}

			int bytesRead = stream.Read(buf, offset, maxSize-offset);
			if (bytesRead <= 0){
				return null;
			}
			int size = 0;
			int start = 0;
			int idx = 0;
			while(idx < bytesRead){
				if(buf[idx] == 0x0a && idx > 0 && buf[idx-1] == 0x0d){
					if(size != 0){
						lines.Add(System.Text.ASCIIEncoding.ASCII.GetString(buf,start, size-1));
					}
					else{
						lines.Add(null);
					}
					size = -1;
					start = idx+1;
				}
				idx++;
				size++;
			}
			if(size != 0){
				Array.Copy(buf,start,buf,0,size-1);
				offset = size-1;
			}
			else{
				offset = 0;
			}
			if(lines.Count == 0){
				return null;
			}
			string firstString = lines[0];
			lines.RemoveAt(0);
			return firstString; 	
		}
	}

	public class RtspServer {
		
		private static Thread workThread;
		private bool running;
		private int port;
		private int videoPort;
		private Session session;
		//private VideoQuality videoQuality;
		private NetworkStream stream;

		public event Action StreamingStarted;
		public event Action StreamingStoped;


		public RtspServer() {
			IsStreaming = false;
		}

		private void OnStreamStarted(){
			IsStreaming = true;
			if(StreamingStarted != null)
				StreamingStarted();
		}

		private void OnStreamStoped(){
			IsStreaming = false;
			if (StreamingStoped != null)
				StreamingStoped();
		}

		public bool IsStreaming{ get; private set;}


		public bool Start(string address, int port, int videoPort, VideoQuality videoQuality){
			bool ok = true;
			this.port = port;
			this.videoPort = videoPort;
			//this.videoQuality = videoQuality;
			try{
				TcpClient tcpClient = new TcpClient(address, port);
				stream = tcpClient.GetStream();
				this.session = new Session(Java.Net.InetAddress.GetByName(address), videoPort+5);
				session.AddVideoTrack(VideoEncoder.H263,CameraFacing.Back,videoQuality,false);
				session.StreamStarted += OnStreamStarted;
				session.StreamStoped  += OnStreamStoped; 
				workThread = new Thread(new ThreadStart(Run));
				running = true;
				workThread.IsBackground = true;
				workThread.Start();
			}
			catch(Exception e){
				Console.WriteLine(e.Message);
				ok = false;
			}
			return ok;
		}
		
		public void Stop() {
			try {
				running = false;
				stream.Close();
			} 
			catch (Exception) {
				//Log.e(TAG,"Error when close was called on serversocket: "+e.getMessage());
			}
			StreamClosed.WaitOne(2000);
			if (session != null) {
				session.StreamStarted -= OnStreamStarted;
				session.StreamStoped -= OnStreamStoped;
			}
		}
		private ManualResetEvent StreamClosed = new ManualResetEvent(false);

		private void Run() {
			Request request;
			Response response;
			
			//log("Connection from "+ client.InetAddress.HostAddress);
			StreamClosed.Reset ();
			while (running) {
				try {
					// Parse the request
					request = Request.ParseRequest(stream);
					// Do something accordingly
					response = ProcessRequest(request);
					// Send response
					response.Send(stream);
				} 
				catch (Exception e) {
					Console.WriteLine(e.Message);
					running = false;
				} 
			}
			// Streaming stops when client disconnects
			session.StopAll();
			session.Flush();
			StreamClosed.Set();
		}

		public Response ProcessRequest(Request request){
			Response response = new Response(request);
			
			/* ********************************************************************************** */
			/* ********************************* Method DESCRIBE ******************************** */
			/* ********************************************************************************** */
			if (request.Method.ToUpper() == "DESCRIBE") {

				// Parse the requested URI and configure the session
				//UriParser.parse(request.Uri,session);
				String requestContent = session.GetSessionDescriptor();
				String requestAttributes = 
					"Content-Base: "+"127.0.0.1"+":"+port+"/\r\n" +
						"Content-Type: application/sdp\r\n";
				
				response.Status = Response.StatusOk;
				response.Attributes = requestAttributes;
				response.Content = requestContent;
				
			}
			
			/* ********************************************************************************** */
			/* ********************************* Method OPTIONS ********************************* */
			/* ********************************************************************************** */
			else if (request.Method.ToUpper() == "OPTIONS") {
				response.Status = Response.StatusOk;
				response.Attributes = "Public: DESCRIBE,SETUP,TEARDOWN,PLAY,PAUSE\r\n";
			}
			
			/* ********************************************************************************** */
			/* ********************************** Method SETUP ********************************** */
			/* ********************************************************************************** */
			else if (request.Method.ToUpper() ==  "SETUP") {
				Java.Util.Regex.Pattern p; Java.Util.Regex.Matcher m;
				int p2, p1, ssrc, trackId, src;
				
				p = Java.Util.Regex.Pattern.Compile("trackID=(\\w+)",Java.Util.Regex.RegexOptions.CaseInsensitive);
				m = p.Matcher(request.Uri);
				
				if (!m.Find()) {
					response.Status = Response.StatusBadRequest;
					return response;
				} 
				
				trackId = int.Parse(m.Group(1));
				
				if (!session.trackExists(trackId)) {
					response.Status = Response.StatusNotFound;
					return response;
				}
				
				p = Java.Util.Regex.Pattern.Compile("client_port=(\\d+)-(\\d+)",Java.Util.Regex.RegexOptions.CaseInsensitive);
				m = p.Matcher(new Java.Lang.String(request.Headers["Transport"] as string));
				
				if (!m.Find()) {
					int port = session.GetTrackDestinationPort(trackId);
					p1 = port;
					p2 = port+1;
				}
				else {
					p1 = int.Parse(m.Group(1)); 
					p2 = int.Parse(m.Group(2));
				}
				ssrc = session.GetTrackSSRC(trackId);
				src = session.GetTrackLocalPort(trackId);
				session.SetTrackDestinationPort(trackId, videoPort);
				
				try {
					session.Start(trackId);
					response.Attributes = "Transport: RTP/AVP/UDP;unicast;client_port="+p1+"-"+p2+";server_port="+src+"-"+(src+1)+";ssrc="+ ssrc.ToString("X") +";mode=play\r\n" +
						"Session: "+ "1185d20035702ca" + "\r\n" +
							"Cache-Control: no-cache\r\n";
					response.Status = Response.StatusOk;
				} catch (Exception) {
					response.Status = Response.StatusInternalServerError;
					throw new Exception("Could not start stream, configuration probably not supported by phone");
				}
				
			}
			
			/* ********************************************************************************** */
			/* ********************************** Method PLAY *********************************** */
			/* ********************************************************************************** */
			else if (request.Method.ToUpper() == "PLAY") {
				String requestAttributes = "RTP-Info: ";
				if (session.trackExists(0)) requestAttributes += "url=rtsp://"+"127.0.0.1" +":"+port+"/trackID="+0+";seq=0,";
				if (session.trackExists(1)) requestAttributes += "url=rtsp://"+"127.0.0.1"+":"+port+"/trackID="+1+";seq=0,";
				requestAttributes = requestAttributes.Substring(0, requestAttributes.Length-1) + "\r\nSession: 1185d20035702ca\r\n";
				
				response.Status = Response.StatusOk;
				response.Attributes = requestAttributes;
			}
			
			
			/* ********************************************************************************** */
			/* ********************************** Method PAUSE ********************************** */
			/* ********************************************************************************** */
			else if (request.Method.ToUpper() == "PAUSE") {
				response.Status = Response.StatusOk;
			}
			
			/* ********************************************************************************** */
			/* ********************************* Method TEARDOWN ******************************** */
			/* ********************************************************************************** */
			else if (request.Method.ToUpper() == "TEARDOWN") {
				response.Status = Response.StatusOk;
				session.StopAll();
				session.Flush();
				//stream.Close();
			}
			
			/* Method Unknown */
			else {
				//Log.e(TAG,"Command unknown: "+request);
				response.Status = Response.StatusBadRequest;
			}
			
			return response;
		}
		
		public class Request {
			
			// Parse method & uri
			public static Java.Util.Regex.Pattern RegexMethod = Java.Util.Regex.Pattern.Compile("(\\w+) (\\S+) RTSP",Java.Util.Regex.RegexOptions.CaseInsensitive);
			// Parse a request header
			public static Java.Util.Regex.Pattern RexegHeader = Java.Util.Regex.Pattern.Compile("(\\S+):(.+)",Java.Util.Regex.RegexOptions.CaseInsensitive);
			
			public String Method;
			public String Uri;
			public System.Collections.Hashtable Headers = new System.Collections.Hashtable();
			
			/** Parse the method, uri & headers of a RTSP request */
			public static Request ParseRequest(NetworkStream input){
				Request request = new Request();
				string line = null;
				Java.Util.Regex.Matcher matcher;
				
				// Parsing request method & uri
				if ((line = input.ReadLine())==null) 
					throw new Exception("Client disconnected");
				matcher = RegexMethod.Matcher(line);
				matcher.Find();
				request.Method = matcher.Group(1);
				request.Uri = matcher.Group(2);
				
				// Parsing headers of the request
				while ( (line = input.ReadLine()) != null && line.Length >3 ) {
					matcher = RexegHeader.Matcher(line);
					matcher.Find();
					request.Headers.Add(matcher.Group(1),matcher.Group(2));
				}
				if (line==null) 
					throw new Exception("Client disconnected");
				
				//Log.e(TAG,request.method+" "+request.uri);
				
				return request;
			}
		}
		
		public class Response {
			
			// Status code definitions
			public static String StatusOk = "200 OK";
			public static String StatusBadRequest = "400 Bad Request";
			public static String StatusNotFound = "404 Not Found";
			public static String StatusInternalServerError = "500 Internal Server Error";
			
			public String Status = StatusOk;
			public String Content = "";
			public String Attributes = "";
			private Request request;
			
			public Response(Request request) {
				this.request = request;
			}
			
			public void Send(NetworkStream output){
				int seqid = -1;
				
				try {
					seqid = int.Parse(request.Headers["Cseq"] as String );
				} 
				catch (Exception) {
				
				}
				
				string response = 	"RTSP/1.0 "+Status+"\r\n" +
					"Server: NXT Tunnel RTSP Server\r\n" +
						(seqid>=0?("Cseq: " + seqid + "\r\n"):"") +
						"Content-Length: " + Content.Length + "\r\n" +
						Attributes +
						"\r\n" + 
						Content;
				
				byte[] a = System.Text.Encoding.ASCII.GetBytes(response);
				output.Write(a,0,a.Length);
			}
		}

	}
}