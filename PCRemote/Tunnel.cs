using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using MonoBrick;
using System.Text.RegularExpressions;
using MonoBrick.EV3;
using MonoBrick.NXT;

namespace PCRemote
{

	public class RtspTunnel{
		
		public RtspTunnel(int rtspPort, int imagePort, Connection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply> nxtConnection){
			Running = false;
			RTSPPort = rtspPort;
			this.nxtConnection = nxtConnection;
			this.ev3Connection = null;
			this.hostImagePort = imagePort;
		}

		public RtspTunnel(int rtspPort, int imagePort, Connection<MonoBrick.EV3.Command,MonoBrick.EV3.Reply> ev3Connection){
			Running = false;
			RTSPPort = rtspPort;
			this.nxtConnection = null;
			this.ev3Connection = ev3Connection;
			this.hostImagePort = imagePort;
		}

		public ManualResetEvent RTSPWaitHandle = new ManualResetEvent(false);
		public ManualResetEvent ClientWaitHandle = new ManualResetEvent(false);
		public ManualResetEvent StreamWaitHandle = new ManualResetEvent(false);

		public bool Start(){

			if(!Running){
				RTSPWaitHandle.Reset();
				ClientWaitHandle.Reset();
				StreamWaitHandle.Reset();

				try{
					mainThread = new Thread(new ThreadStart(MainThread));
					mainThread.IsBackground = true;
					mainThread.Start();
					Running = true;
				}
				catch(Exception e){
					Console.WriteLine("Listen for clients: " + e.Message);
					return false;
				}
			}
			return true;
		}
		
		public void Stop(){
			//Do somthing else
			if(Running){
				try{
					CloseAllConnections();
				}
				catch(Exception e){
					Console.WriteLine("Failed to stop tunnel" + e.Message);
				}
				Running = false;
			}
		}

		public event Action Stopped;
		public event Action<double> NewImageDataRate;
		public int RTSPPort{get;private set;}
		public bool Running{get;private set;}

		private const int BufferSize = 1500;
		private int hostImagePort;
		private int clientImagePort;

		private Thread mainThread;
		private Thread hostToClientThread;
		private Thread clientToHostThread;
		private Thread imageThread;

		private NetworkStream clientNetworkStream;
		private NetworkStream hostNetworkStream;
		private Connection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply> nxtConnection;
		private Connection<MonoBrick.EV3.Command,MonoBrick.EV3.Reply> ev3Connection;
		private TcpListener remoteListener;
		private TcpListener clientListener;
		private void MainThread(){
			try{
				((ManualResetEvent)waitHandles[0]).Reset();
				remoteListener = new TcpListener(System.Net.IPAddress.Any,RTSPPort);
				remoteListener.Start();
				bool hasError = true;
				if(nxtConnection != null){
					var reply = nxtConnection.SendAndReceive(new MonoBrick.NXT.Command(MonoBrick.NXT.CommandType.TunnelCommand,MonoBrick.NXT.CommandByte.StartTunnelRTSP,true));
					hasError = reply.HasError;
				}
				if(ev3Connection != null){
					throw new NotImplementedException("RTSP has not been implemented for EV3");
				}
				if(!hasError){
					TcpClient remoteTcpClient = remoteListener.AcceptTcpClient();
					hostNetworkStream = remoteTcpClient.GetStream();
					remoteListener.Stop();

					((ManualResetEvent)waitHandles[1]).Reset();
					clientListener = new TcpListener(System.Net.IPAddress.Loopback, RTSPPort);
					clientListener.Start();
					RTSPWaitHandle.Set();//signal


					TcpClient tcpClient = clientListener.AcceptTcpClient();
					ClientWaitHandle.Set();
					clientListener.Stop();

					clientNetworkStream = tcpClient.GetStream();
					clientToHostThread = new Thread(new ThreadStart(delegate{ForwardBytesFromClientToHost();}));
					clientToHostThread.IsBackground = true;
					clientToHostThread.Start();

					ClientWaitHandle.Set();//signal 

					hostToClientThread = new Thread(new ThreadStart(delegate{ForwardBytesFromHostToClient();}));
					hostToClientThread.IsBackground = true;
					hostToClientThread.Start();
					WaitHandle.WaitAny(waitHandles);
				}
			}
			catch(Exception e){
				Console.WriteLine("Tunnel was closed:" + e.Message);
			}
			try{
				CloseAllConnections();
			}
			catch{
				Console.WriteLine("Failed to clean up RTSP tunnel");
			}
			Console.WriteLine("Waiting for handles");
			WaitHandle.WaitAll(waitHandles);
			Running = false;
			if(Stopped != null){
				Stopped();
			}
			Console.WriteLine("Done waiting for handles");
		}

		private void CloseAllConnections(){
			if(hostNetworkStream != null)
				hostNetworkStream.Dispose();			
			if(clientNetworkStream != null)
				clientNetworkStream.Dispose();
			if(remoteListener != null)
				remoteListener.Stop();
			if(clientListener != null)
				clientListener.Stop();
			if(udpFromHost != null)
				udpFromHost.Close();
			if(udpToClient != null)
				udpToClient.Close();

		}

		private void ForwardBytesFromHostToClient(){
			byte[] message = new byte[BufferSize];
			int bytesRead;
			bool run = true;
			while(run){
				try{
					bytesRead = hostNetworkStream.Read(message,0,BufferSize);
					if(bytesRead != 0){
						string command = System.Text.ASCIIEncoding.ASCII.GetString(message, 0,bytesRead);
						string pattern = "client_port=";
						int startIdx = command.IndexOf("client_port");
						if(startIdx != -1){
							((ManualResetEvent)waitHandles[2]).Reset();
							int endIdx = command.IndexOf("-",startIdx);
							string s = command.Substring(startIdx+pattern.Length,(endIdx-startIdx)-pattern.Length);
							clientImagePort = int.Parse(s);
							imageThread = new Thread(new ThreadStart(delegate{ImageDataFromHostToClient();}));
							imageThread.IsBackground = true;
							imageThread.Priority = ThreadPriority.Highest;
							imageThread.Start();
						}
						clientNetworkStream.Write(message,0,bytesRead);
					}
					else{
						run = false;
					}
					
				}
				catch(Exception e){
					Console.WriteLine("From host to client failed: " + e.Message);
					run = false;
				}
			}
			Console.WriteLine("From host to clien finished");
			((ManualResetEvent)waitHandles[1]).Set();
		}

		private void ForwardBytesFromClientToHost(){
			byte[] message = new byte[BufferSize];
			int bytesRead;
			bool run = true;
			while(run){
				try{
					bytesRead = clientNetworkStream.Read(message,0,BufferSize);
					if(bytesRead != 0){
						hostNetworkStream.Write(message,0,bytesRead);
					}
					else{
						run = false;
					}
				}
				catch(Exception e){
					Console.WriteLine("From client to host failed: " + e.Message);
					run = false;
				}
			}
			Console.WriteLine("From client to host finished");
			((ManualResetEvent)waitHandles[0]).Set();
		}
		private UdpClient udpFromHost;
		private UdpClient udpToClient;
		private void ImageDataFromHostToClient(){
			bool run = true;
			bool setHandle = true;
			object bytesReadLock = new object();
			int bytesRead = 0;
			int byteRate = 0;
			IPEndPoint clientEndPoint = null;
			IPEndPoint hostEndPoint = null;
			System.Timers.Timer timer = new System.Timers.Timer(1000);
			timer.Elapsed += delegate {
				lock(bytesReadLock){
					byteRate = bytesRead;
					bytesRead = 0;
				}
				if(NewImageDataRate != null)
					NewImageDataRate((double)byteRate/(double)1);
			};
			timer.Start();
			try{
				clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), clientImagePort);
				hostEndPoint = new IPEndPoint(IPAddress.Any, hostImagePort);
				udpFromHost = new UdpClient(hostImagePort);
				udpToClient = new UdpClient();
				udpToClient.Connect(clientEndPoint);
			}
			catch(Exception){
				Console.WriteLine("Failed to start UDP stream");
				run = false;
			}
			while(run){
				try{
					byte[] bytes = udpFromHost.Receive( ref hostEndPoint);
					if(setHandle){
						Console.WriteLine("Data from UDP");
						setHandle = false;
						StreamWaitHandle.Set();
					}
					lock(bytesReadLock){
						bytesRead += bytes.Length;
					}
					udpToClient.Send(bytes,bytes.Length);
				}
				catch(Exception e){
					Console.WriteLine("UDP stream failed " + e.Message);
					run = false;
				}
			}
			timer.Stop();
			if(NewImageDataRate != null)
				NewImageDataRate(0);
			Console.WriteLine("UDP streaming tunnel finished");
			((ManualResetEvent)waitHandles[2]).Set();
		}

		private WaitHandle[] waitHandles = new WaitHandle[] 
		{
			new ManualResetEvent(false),
			new ManualResetEvent(false),
			new ManualResetEvent(false),
		};

	}
}

