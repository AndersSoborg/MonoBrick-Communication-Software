using System.IO;
using System.Net.Sockets;
using System.Threading;
using System;
using System.ComponentModel;
using System.Net;


namespace MonoBrick
{
	/// <summary>
	/// Abstract class for creating a Network connection
	/// </summary>
	public abstract class NetworkConnection<TBrickCommand,TBrickReply> : Connection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new ()
	{
		/// <summary>
		/// The network stream to use for communication.
		/// </summary>
		protected NetworkStream stream = null;
		
		/// <summary>
		/// Open the connection or wait for tunnel
		/// </summary>
		public override void Open(){
		
		}

		/// <summary>
		/// Close the connection
		/// </summary>
		public override void Close(){
		
		}

		/// <summary>
		/// Receive a reply
		/// </summary>
		public override TBrickReply Receive(){
			byte[] lengthBytes = new byte[2];
			byte[] data = null;
			int expectedlength = 0;
			try
			{
				stream.ReadAll(lengthBytes);
				expectedlength = (ushort)(0x0000 | lengthBytes[0] | (lengthBytes[1] << 2));
				if(expectedlength > 0){
					data = new byte[expectedlength];
					stream.ReadAll(data);
				}
				
			}
			catch(Exception e) {
				throw new ConnectionException(ConnectionError.ReadError, e);
			}
			if(expectedlength == 0){
				throw new ConnectionException(ConnectionError.NoReply);
			}
			var reply = new TBrickReply();
			reply.SetData(data);
			ReplyWasReceived(reply);
			return reply;
		}

		/// <summary>
		/// Send a command.
		/// </summary>
		/// <param name='command'>
		/// The command to send
		/// </param>
		public override void Send(TBrickCommand command){
			byte[] data = null;
			ushort length = (ushort) command.Length;
			data = new byte[length+2];
			data[0] = (byte) (length & 0x00ff);
			data[1] = (byte)((length&0xff00) >> 2);
			Array.Copy(command.Data,0,data,2,command.Length);
			CommandWasSend(command);
			try
			{
				stream.Write(data, 0, data.Length);
			}
			catch (Exception e) {
				throw new ConnectionException(ConnectionError.WriteError,e);
			}
		}	
	
	
	}
	
	/// <summary>
	/// Class for creating a tunnel connection
	/// </summary>
	public class TunnelConnection<TBrickCommand,TBrickReply> : NetworkConnection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new ()
	{
		private TcpClient tcpClient;
		private TcpListener tcpListener = null;
		private bool waitForTunnel;
		private string address;
		private ushort port;
		
		
		/// <summary>
		/// Initializes a tunnel connection where the connection waits for a tunnel to connect
		/// </summary>
		/// <param name='port'>
		/// The port to listen for incomming connections from a tunnel
		/// </param>
		public TunnelConnection(ushort port){
			this.port = port;
			isConnected = false;
			//IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
			tcpListener = new TcpListener(IPAddress.Any , port);
			waitForTunnel = true;
		}
		
		/// <summary>
		/// Initializes a tunnel connection where the bricks connects to a tunnel
		/// </summary>
		/// <param name='ipAddress'>
		/// IP address.
		/// </param>
		/// <param name='port'>
		/// Port
		/// </param>
		public TunnelConnection(string ipAddress, ushort port){
			address = ipAddress;
			this.port = port;
			isConnected = false;
			waitForTunnel = false;
		}

		/// <summary>
		/// Open the connection or wait for tunnel
		/// </summary>
		public override void Open(){
			if(waitForTunnel){
				try
				{
					tcpListener.Start();
					tcpClient = this.tcpListener.AcceptTcpClient();//blocking call
					tcpClient.NoDelay = true;
					address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
					ConnectionWasOpened();
					isConnected = true;
					stream = tcpClient.GetStream();
					tcpClient.SendTimeout = 3000;
					tcpClient.ReceiveTimeout = 3000;
					tcpListener.Stop();
				}
				catch(Exception e) {
					throw new ConnectionException(ConnectionError.OpenError, e);
				}
				ConnectionWasOpened();
				isConnected = true;
			}
			else
			{
				try
				{
					tcpClient = new TcpClient(address, port);
					tcpClient.NoDelay = true;
					stream = tcpClient.GetStream();
					tcpClient.SendTimeout = 3000;
					tcpClient.ReceiveTimeout = 3000;
					//add something more here
				}
				catch(Exception e) {
					throw new ConnectionException(ConnectionError.OpenError, e);
				}
				isConnected = true;
				ConnectionWasOpened();
			}
		}

		/// <summary>
		/// Close the connection
		/// </summary>
		public override void Close(){
			if(waitForTunnel){
				try{tcpListener.Stop();}catch{}
				try{stream.Close();}catch{}
				try{tcpClient.Close();}catch{}
				ConnectionWasClosed();
				isConnected = false;
			}
			else{
				try
				{
					stream.Close();
					tcpClient.Close();
				}
				catch (Exception) {
					
				}
				isConnected = false;
				ConnectionWasClosed();
			}
		}
	}
	
	
	/// <summary>
	/// Network connection
	/// </summary>
	public class WiFiConnection<TBrickCommand,TBrickReply> : NetworkConnection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new ()

	{
		private class UdpInfo{
			public UdpInfo (string udpInfo)
			{
				var info = udpInfo.Split(new char[] {(char)0x0d, (char)0x0a});
				foreach(var s in info){
					if(s.Contains("Serial-Number")){
						SerialNumber = s.Substring(s.IndexOf(":") +1).ToUpper();
					}
					if(s.Contains("Port")){
						Port = int.Parse(s.Substring(s.IndexOf(":") +1));
					}
					if(s.Contains("Protocol")){
						Protocol = s.Substring(s.IndexOf(":") +1);
					}
					if(s.Contains("Name")){
						Name = s.Substring(s.IndexOf(":") +1);
					}
				}
			}
		
			public string SerialNumber{get; private set;}
			public int Port{get; private set;}
			public string Name{get; private set;}
			public string Protocol{get; private set;}
			public byte[] TcpUnlockData 
			{
				get{
					string serialString = "GET /target?sn=" + SerialNumber + "VMTP1.0";
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					foreach (var ch in serialString)
	   					sb.Append(ch);
	   				sb.Append((char)0x0d);
	   				sb.Append((char)0x0a);
	   				string protocolString = "Protocol:" + Protocol;
	   				foreach (var ch in protocolString)
	   					sb.Append(ch);
	   				sb.Append((char)0x0d);
	   				sb.Append((char)0x0a);
	   				sb.Append((char)0x0d);
	   				sb.Append((char)0x0a);
	   				return System.Text.Encoding.ASCII.GetBytes(sb.ToString());
				}
			}
		}
		
		private TcpClient tcpClient;
		int timeOut = 0;
		
		/// <summary>
		/// Initializes a new instance of the Wifi connection 
		/// </summary>
		/// <param name="connectionTimeoutMs">Time out when trying to connect if set to zero wait forever</param>
		public WiFiConnection(int connectionTimeoutMs = 0){
			isConnected = false;
			this.timeOut = connectionTimeoutMs;
		}

		/// <summary>
		/// Open the connection to the EV3 over a WiFi connection - this will block
		/// </summary>
		public override void Open(){
			bool hasError = false;
			bool failedToLocateEV3 = true;
			int listenPort = 3015;
			int tcpIpPort = 5555;
			UdpClient listener = null;
			UdpClient sender = null;
	        try 
	        {
	            listener = new UdpClient(listenPort);
	        	IPEndPoint groupEP = new IPEndPoint(IPAddress.Any,listenPort);
				byte[] bytes = null;
                var resetEvent = new ManualResetEvent(false);
    			Thread t = new Thread(
    			new ThreadStart(
	                delegate()
	                {
	            		try{
	            			bytes = listener.Receive( ref groupEP);
	            			resetEvent.Set();
	            		}
	            		catch{
	            			
	            		}
	            		
				    }
	            ));
		        t.IsBackground = true;
				t.Priority = ThreadPriority.Normal;
		        t.Start();
				if(timeOut != 0){
					if(!resetEvent.WaitOne(timeOut))
						listener.Close();
				}
				else{
					resetEvent.WaitOne(); //wait forever
				}
				if(bytes != null){
					failedToLocateEV3 = false;
					UdpInfo udpInfo = new UdpInfo(System.Text.Encoding.ASCII.GetString(bytes,0,bytes.Length));
	                Thread.Sleep(100);
	                sender = new UdpClient();
	                sender.Send( new byte[]{0x00}, 1, groupEP);
	                Thread.Sleep(100);
	                tcpClient = new TcpClient(groupEP.Address.ToString(), tcpIpPort);
					tcpClient.NoDelay = true;
					stream = tcpClient.GetStream();
					tcpClient.SendTimeout = 3000;
					tcpClient.ReceiveTimeout = 3000;
					stream.Write(udpInfo.TcpUnlockData, 0, udpInfo.TcpUnlockData.Length);
					byte[] unlockReply = new byte[16];
					stream.ReadAll(unlockReply);
				}
				else{
					hasError = true;	
				}
				
	        } 
	        catch 
	        {
	        	hasError = true;    
	        }
	        finally
	        {
	            if(listener != null)
	            	listener.Close();
	            if(sender != null)
	            	sender.Close();
	            
	        }
	        if(hasError){
	        	if(failedToLocateEV3){
	        		throw new ConnectionException(ConnectionError.OpenError, new Exception("Failed to find EV3"));
	        	}
	        	else{
	        		throw new ConnectionException(ConnectionError.OpenError);
	        	}
	        	
	        }
		}

		/// <summary>
		/// Close the connection
		/// </summary>
		public override void Close(){
			try{stream.Close();}catch{}
			try{tcpClient.Close();}catch{}
			ConnectionWasClosed();
			isConnected = false;
		}
	}
}

