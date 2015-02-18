using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using MonoBrick;
using MonoBrick.EV3;
using MonoBrick.NXT;


namespace MonoBrick
{
	public class QueueThread<T>
	{
		private Thread thread;
		private const int maxQueueSize = 5000;
		private readonly Queue<T> queue = new Queue<T>();
		private readonly object queueLock = new Object();
		private readonly Semaphore queueCounter = new Semaphore(0, maxQueueSize);
		private readonly Action<T> handler;
		private readonly ThreadPriority threadPriority;
		private bool threadStopped;
		private readonly ManualResetEvent queueDone =  new ManualResetEvent(false);
		
		private void ThreadMain()
		{
			while (!threadStopped)
			{
				T item = default(T);
				bool hasHandler = false;
				queueCounter.WaitOne();
				lock (queueLock)
				{
					if(queue.Count > 0)
					{
						item = queue.Dequeue();
						hasHandler = true;
					}
				}
				
				Thread.MemoryBarrier();
				if(hasHandler && !threadStopped)
				{
					handler(item);
				}
				
				Thread.MemoryBarrier();
			}
			queueDone.Set();
		}
		
		private void CreateThread(ThreadPriority threadPriority)
		{
			thread = new Thread(ThreadMain);
			thread.IsBackground = true;
			thread.Priority = threadPriority;
			thread.Start();
		}
		public QueueThread(Action<T> handler)
			: this(handler, ThreadPriority.Normal)
		{
			
		}
		
		public QueueThread(Action<T> handler, ThreadPriority threadPriority)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			
			this.handler = handler;
			this.threadPriority = threadPriority;
			
			CreateThread(threadPriority);
		}
		
		public void AddToQueue(T item)
		{
			lock (queueLock)
			{
				queue.Enqueue(item);
			}
			queueCounter.Release();
		}
		
		public void Close()
		{
			threadStopped = true;
			queueCounter.Release();
			
			if(!queueDone.WaitOne(10*1000))
			{
				throw new TimeoutException("Failed to shut down queue thread in time.");
			}
		}
		
		public void Restart()
		{
			Close();
			lock (queueLock)
			{
				queue.Clear();
			}
			threadStopped = false;
			CreateThread(threadPriority);
		}
	}
	
	public class IPFilter{
		[XmlElement("Mode")]
		private IPRangeMode rangeMode = IPRangeMode.Reject;
		[XmlArray("IPAddresses")]
		[XmlArrayItem("Range", typeof(IPAddres3sRange))]
		public ArrayList IPRanges{get;set;}
		
		public IPRangeMode IPRangeMode{
			get {return rangeMode;}
			set	{ rangeMode = value;}
		}
	}
	
	[XmlRoot("ConfigRoot")]
	public class TunnelSettings
	{
		[XmlElement("Connection")]
		private string connection = "usb";
		
		[XmlElement("Port")]
		private int port = 1500;
		
		[XmlElement("ListenForClients")]
		public bool ListenForClients = true;
		
		[XmlElement("LogFile")]
		public string LogFileName = "log.txt";


		[XmlElement("MaxConnections")]
		private int maxConnections = 1;

		[XmlElement("BrickType")]
		public BrickType BrickType = BrickType.NXT;

		public IPFilter IPFilter { get; set;}
		
		public TunnelSettings():this("usb",1500){
			
		} 
		
		public TunnelSettings(String connection, int port)
		{
			
			this.Connection = connection;
			this.Port = port;
			
			IPFilter = new IPFilter();
			IPFilter.IPRanges = new ArrayList();
			IPFilter.IPRanges.Add(new IPAddressRange());
			IPFilter.IPRangeMode = IPRangeMode.Disable;
		}
		
		public void AddIPRange(IPAddressRange range)
		{
			this.IPFilter.IPRanges.Add(range);
		}
		
		public IPAddressRange[] IPRangeToArray(){
			return (IPAddressRange[]) IPFilter.IPRanges.ToArray(typeof(IPAddressRange));
		}
		
		public string Connection
		{
			get { return connection; }
			set { connection = value; }
		}
		
		public int Port
		{
			get { return port; }
			set { port = value; }
		}
		
		public int MaxConnections{
			get {return maxConnections;}
			set	{ maxConnections = value;}
		}

		public bool SaveToXML(String filepath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(TunnelSettings));
			TextWriter textWriter = new StreamWriter(filepath);
			serializer.Serialize(textWriter, this);
			textWriter.Close();
			return true;
		}
		
		public TunnelSettings LoadFromXML(String filepath)
		{
			XmlSerializer deserializer = new XmlSerializer(typeof(TunnelSettings));
			TextReader textReader = new StreamReader(filepath);
			Object obj = deserializer.Deserialize(textReader);
			TunnelSettings myNewSettings = (TunnelSettings)obj;
			textReader.Close();
			return myNewSettings;
		}
	}
	
	public enum IPRangeMode{Disable, Reject, Accept}
	
	public class IPAddressRange
	{
		[XmlElement("Lower")]
		private string lower = "";
		[XmlElement("Upper")]
		private string upper = "";
		[NonSerialized()]
		private byte[] lowerBytes = null;
		private byte[] upperBytes = null;
		private AddressFamily? addressFamily;
		private IPAddress lowerAddress;
		private IPAddress upperAddress;

		public string Lower{ 
			get{ return lower;}
			set{  
				SetLower(value);
			}
		}
		
		public string Upper{ 
			get{ return upper;}
			set{
				SetUpper(value);
			}
		}
		
		private void SetLower(string low){
			lower = low;
			lowerAddress = IPAddress.Parse(low);
			lowerBytes = lowerAddress.GetAddressBytes();
			IsFamilyUpdated = false;
		}
		
		private void SetUpper(string up){
			upper = up;
			upperAddress = IPAddress.Parse(up);
			upperBytes = upperAddress.GetAddressBytes();
			IsFamilyUpdated = false;
		}
		
		private void UpdateFamily(){
			if(upperAddress.AddressFamily != lowerAddress.AddressFamily){
				addressFamily = null;
			}
			else{
				addressFamily = upperAddress.AddressFamily;
			}
			IsFamilyUpdated = true;
		}
		
		private bool IsFamilyUpdated;
		
		public  IPAddressRange():this("192.168.0.100","192.168.0.150")
		{
			
		}
		
		public  IPAddressRange(string lower, string upper)
		{
			SetLower(lower);
			SetUpper(upper);
			UpdateFamily();
		}
		
		public  IPAddressRange(IPAddress lower, IPAddress upper):this(lower.ToString(),upper.ToString())
		{
			
		}
		
		public bool IsInRange(string address){
			return IsInRange(IPAddress.Parse(address));
		}
		
		public bool IsInRange(IPAddress address)
		{
			if(!IsFamilyUpdated)
				UpdateFamily();
			if (addressFamily == null || address.AddressFamily != addressFamily)
				return false;
			byte[] addressBytes = address.GetAddressBytes();
			
			bool lowerBoundary = true, upperBoundary = true;
			
			for (int i = 0; i < this.lowerBytes.Length && (lowerBoundary || upperBoundary); i++)
			{
				if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) || (upperBoundary && addressBytes[i] > upperBytes[i]))
				{
					return false;
				}
				
				lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
				upperBoundary &= (addressBytes[i] == upperBytes[i]);
			}
			
			return true;
		}
	}
	
	//public delegate void LogDelegate(string logMessage);
	//public delegate void ClientDelegate(Tunnel.Client client);
	public class Tunnel
	{
		#region shared with clients
		private Mutex mutex = new Mutex();
		private Connection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply> Brick = null;
		private QueueThread<string> logQueue;
		#endregion

		private LogFile logFile;

		private TcpListener tcpListener;
		private Thread listenThread;
		private TunnelSettings settings;
		private Dictionary<int, Client> clients = new Dictionary<int, Client>();

		private int clientID = 1; 
		private IPAddressRange[] addressRange = null;
		private object clientsLock = new object();
		
		private void OnLogQueue(string message){
			if(LogEvent != null)
				LogEvent(message);
			if(logFile.IsOpen)
				logFile.Write(message);
		}
		
		private void OnBrickDisconnected(){
			if(NXTDisconnected != null){
				NXTDisconnected();
			}
			logQueue.AddToQueue("NXT disconnected");
			if(IsRunning){
				Stop();
			}
		}
		
		private void OnBrickConnected(){
			if(NXTConnected != null){
				NXTConnected();
			}
			logQueue.AddToQueue("Connected to NXT");
		}
		
		private void ClientDisconnected(Client client){
			lock (clientsLock)
			{
				clients.Remove(client.ID);
			}
			if(clients.Count == 0){
				logQueue.AddToQueue("Clients connected " + clients.Count);
				if(NoClientsConnected != null)
					NoClientsConnected();
			}
		}
		
		private bool AcceptIPAddress(IPAddress address){
			bool accept = false;
			if(addressRange != null && (settings.IPFilter.IPRangeMode != IPRangeMode.Disable) && addressRange.Length > 0){
				foreach (IPAddressRange range in addressRange) 
				{
					if(settings.IPFilter.IPRangeMode == IPRangeMode.Accept){
						if(range.IsInRange(address)){
							accept = true;
							break;
						}
					}
					if(settings.IPFilter.IPRangeMode == IPRangeMode.Reject){
						if(!range.IsInRange(address)){
							accept = true;
						}
					}
				}
			}
			else{
				accept = true;
			}
			return accept;
		}
		private event Action NoClientsConnected;
		public event Action<string> LogEvent;
		public event Action<Client> ClientConnected;
		public event Action<Client> ClientDisConnected;
		public event Action NXTDisconnected;
		public event Action NXTConnected;
		public event Action Started;
		public event Action Stopped;
		public Tunnel()
		{
			logFile = new LogFile();
			logQueue = new QueueThread<string>(OnLogQueue);
			ClientDisConnected += ClientDisconnected;//subscribe to event thrown by client thread
		}
		public string[] ListeningAddress{get; private set;}
		public int ListeningPort{get; private set;}
		public bool IsRunning{get;private set;}
		public bool IsConnectedToBrick{
			get{
				return Brick.IsConnected;
			}
		}

		public bool Start(TunnelSettings settings){
			Connection<MonoBrick.NXT.Command, MonoBrick.NXT.Reply> connection =null;
			switch(settings.Connection.ToLower()){
			case  "usb":
				connection = new USB<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>();
				break;
			case "loopback":
				connection = new Loopback<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>();
				break;
			default:
				connection = new Bluetooth<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>(settings.Connection);
				break;
			}
			return Start (settings,connection);
		}

		public bool Start(TunnelSettings settings, Connection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply> connection){
			if(settings.LogFileName != ""){
				try{
					logFile.Open(settings.LogFileName);
				}
				catch
				{
					logQueue.AddToQueue("Failed to open log file " + settings.LogFileName);
				}
			}
			this.settings = settings;
			Brick = connection;
			string sHostName = Dns.GetHostName (); 
			IPHostEntry ipE = Dns.GetHostByName (sHostName); 
			IPAddress [] ipv4Addresses = ipE.AddressList;
			List<string> list = new List<string>();
			for(int i = 0; i < ipv4Addresses.Length; i++){
				if(ipv4Addresses[i].AddressFamily == AddressFamily.InterNetwork){
					list.Add(ipv4Addresses[i].ToString());
				}
			}
			ListeningAddress = list.ToArray();
			//IPAddress = ipv4Addresses[0].ToString();
			ListeningPort = settings.Port;
			tcpListener = new TcpListener( System.Net.IPAddress.Any, settings.Port);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.IsBackground = true;
			IsRunning = false;
			this.addressRange = settings.IPRangeToArray();

			/*Unsibscribed in "on tunnel stopped"*/
			Brick.Disconnected += OnBrickDisconnected;
			Brick.Connected += OnBrickConnected;

			logQueue.AddToQueue("Connecting to Brick using " + settings.Connection);
			try{
				Brick.Open();
			}
			catch{
				logQueue.AddToQueue("Tunnel not started. Failed to connect to Brick");
				return false;
			}
			logQueue.AddToQueue("Maximum number of connections " + settings.MaxConnections);
			if(settings.ListenForClients){
				listenThread.Start();
			}
			else{
				logQueue.AddToQueue("Tunnel is not listening for clients");
			}
			IsRunning = true;
			if(Started != null)
				Started();
			return true;

		}

		
		public bool ConnectToClient(string address){
			return ConnectToClient(address,ListeningPort);
		}

		public int ClientsConnected{
			get{
				return clients.Count;
			}
		}

		public bool ConnectToClient(string address, int port){
			if(!IsRunning){
				logQueue.AddToQueue("Unable to connect to client at " + address + " tunnel is not running");
				return false;
			}
			logQueue.AddToQueue("Attempting to connect to client at " + address);
			Client client;
			try{
				TcpClient tcpClient = new TcpClient ();
				tcpClient.Connect (address, port);
				client = new Client(clientID, tcpClient, ClientDisConnected, mutex,Brick,logQueue,settings.BrickType);
			}
			catch(Exception e){
				logQueue.AddToQueue("Failed to connect to client at: " + address + "\n" + e.Message);
				return false;
			}
			if(ClientConnected!= null)
				ClientConnected(client);
			lock(clientsLock){
				clients.Add(clientID,client);
			}
			client.Run();
			clientID++;
			logQueue.AddToQueue("Clients connected " + clients.Count);
			return true;
		}
		
		public bool ThrowOffClient(Client client){
			return ThrowOffClient(client.ID);
		}

		public bool ThrowOffClient(int id){
			Client value = null;
			bool ok = false;
			lock(clientsLock){
				if (clients.TryGetValue(id, out value))
				{
					value.ThrowOff();
					ok = true;
				}
			}
			return ok;
		}
		
		public void ThrowOffAllClients(){
			List<int> list = null;
			lock(clientsLock)
			{
				list = new List<int>(clients.Keys);
			}
			
			foreach( int id in list)
			{
				ThrowOffClient(id);
			}
		}
		
		public void Stop(){
			bool throwOffClints = Convert.ToBoolean(clients.Count);
			if(throwOffClints){
				this.NoClientsConnected += OnTunnelStopped;//is unregistred in OnTunnelStopped
				ThrowOffAllClients();
			}

			try{
				tcpListener.Stop();
			}
			catch(Exception){

			}
			if(!throwOffClints){
				OnTunnelStopped();
			}
		}
		
		private void OnTunnelStopped ()
		{
			this.NoClientsConnected -= OnTunnelStopped;
			IsRunning = false;
			try{
				if(Brick.IsConnected)
					Brick.Close();

			}
			catch(Exception){
				
			}
			logQueue.AddToQueue("Tunnel stopped");
			if(Stopped != null)
				Stopped();
			if(logFile.IsOpen)
				logFile.Close();
			//add more here if you need
		}
		
		private void ListenForClients(){
			try{
				tcpListener.Start();
			}
			catch(Exception){
				logQueue.AddToQueue("Failed to start listening thread. Another service might be using the port!!");
				logQueue.AddToQueue("Connection from clients will not be accepted!!!");
				return;
			}
			logQueue.AddToQueue("Listening  for clients at:");
			for(int i = 0; i < ListeningAddress.Length; i++){
				logQueue.AddToQueue(ListeningAddress[i] + ":" + ListeningPort);
			}
			if((settings.IPFilter.IPRangeMode == IPRangeMode.Accept || settings.IPFilter.IPRangeMode == IPRangeMode.Reject) && addressRange != null && addressRange.Length != 0){
				if(settings.IPFilter.IPRangeMode == IPRangeMode.Accept)
					logQueue.AddToQueue("The following IP addresses will be accepted: ");
				if(settings.IPFilter.IPRangeMode == IPRangeMode.Reject)
					logQueue.AddToQueue("The following IP addresses will be rejected: ");
				foreach (IPAddressRange range in addressRange) 
				{
					if(range.Lower == range.Upper)
						logQueue.AddToQueue(range.Upper);
					else
						logQueue.AddToQueue(range.Lower + " to " + range.Upper);
				}
			}
			bool running = true;
			TcpClient tcpClient = null;
			while (running)
			{
				//blocks until a client has connected to the server
				try{
					tcpClient = this.tcpListener.AcceptTcpClient();
				}
				catch(Exception){
					//program was closed
					running = false;
				}
				if(running && tcpClient != null){
					IPAddress address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
					logQueue.AddToQueue("Connection attempt from " + address);
					bool accept = AcceptIPAddress(address);
					if(accept){
						if(clients.Count == settings.MaxConnections){
							accept = false;
							logQueue.AddToQueue("Connection from " + address + " was rejected. Maximum number of connections reached");
						}
					}
					else{
						logQueue.AddToQueue("Connection from " + address + " was rejected by IP-filter");
					}
					if(accept){

						Client client = new Client(clientID,tcpClient, ClientDisConnected,mutex,Brick,logQueue, settings.BrickType);
						if(ClientConnected!= null)
							ClientConnected(client);
						lock(clientsLock){
							clients.Add(clientID,client);
						}
						client.Run();
						clientID++;
						logQueue.AddToQueue("Clients connected " + clients.Count);
					}
				}
			}
		}

		public class Client {
			private IPAddress address;
			private DateTime connectTime;
			private NetworkStream networkStream;
			private Action<Client> onDisconnected;
			private bool wasThrownOff = false;
			private Mutex mutex;
			private Connection<MonoBrick.NXT.Command, MonoBrick.NXT.Reply> Brick;
			private QueueThread<string> logQueue;
			private bool logId;
			private BrickType type;
			private string IdName = "";
			internal Client(int id, TcpClient tcpClient, Action<Client> disconnected, Mutex mutex, Connection<MonoBrick.NXT.Command, MonoBrick.NXT.Reply> NXT, QueueThread<string> logQueue, BrickType type) { 
				this.mutex = mutex;
				this.Brick = NXT;
				this.type = type;
				this.logQueue = logQueue;
				address =  ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
				connectTime = DateTime.Now;
				tcpClient.NoDelay = true;
				networkStream = tcpClient.GetStream();
				networkStream.ReadTimeout = Timeout.Infinite;
				networkStream.WriteTimeout = 1000;
				onDisconnected = disconnected;
				this.ID = id;
				LogId = true;
				this.LogActivity = true;
				logQueue.AddToQueue("Client created");
			}
			
			public void Run(){
				ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread));
				/*thread = new Thread(new ThreadStart(ClientThread));
				thread.Priority = ThreadPriority.AboveNormal;
				thread.IsBackground = true;
				thread.Start();*/
			}
			
			public void ThrowOff(){
				logQueue.AddToQueue("Client " + ID + ": Throw off signaled");
				wasThrownOff = true;
				if(IsConnected)
					networkStream.Close();
			}

			public bool LogId{
				get{return logId;}
				set{
					logId = value;
					if(logId){
						IdName = "Client " + ID + ": ";
					}
					else{
						IdName = "";
					}
				}
			} 

			public bool LogActivity{get;set;}
			public IPAddress Address{get{ return address;}}
			public DateTime ConnectTime { get { return connectTime; } }
			public int ID{get; private set;}
			public bool IsConnected{get; private set;}
			public event Func<BrickCommand,BrickReply> TunnelCommandReceived;

			private void SendNetworkReply(BrickReply reply){
				ushort length = (ushort) reply.Data.Length;
				byte[] data = new byte[length+2];
				data[0] = (byte) (length & 0x00ff);
				data[1] = (byte)((length&0xff00) >> 2);
				Array.Copy(reply.Data,0,data,2,reply.Data.Length);
				networkStream.Write(data,0,data.Length);
			}

			private void ClientThread(object stateInfo){
				byte[] message = null;
				byte[] lengthBytes = new byte[2];
				int bytesToRead;
				int bytesRead;
				//Stopwatch stopWatch = new Stopwatch();
				//Stopwatch stopWatch2 = new Stopwatch();
				NXT.Command nxtCommand = null;
				NXT.Reply nxtReply = null;
				EV3.Command ev3Command = null;
				EV3.Reply ev3Reply = null;
				logQueue.AddToQueue(IdName + "Connect from " + address);
				IsConnected = true;
				bool run = true;
				while (run)
				{
					bytesToRead = 0;
					bytesRead = 0;
					message = null;
					try
					{
						bytesRead = networkStream.ReadAll(lengthBytes);
						if(bytesRead > 0){
							bytesToRead = (ushort)(0x0000 | lengthBytes[0] | (lengthBytes[1] << 2));
							message = new byte[bytesToRead];
							bytesRead = 0;
							bytesRead = networkStream.ReadAll(message);
							bool CommandValid = true;
							if(bytesRead == bytesToRead){
								try{
									if(type == BrickType.NXT){
										nxtCommand = new NXT.Command(message);
									}
									else{
										ev3Command = new EV3.Command(message);
									}
								}
								catch(Exception){
									CommandValid = false;
									logQueue.AddToQueue(IdName + "Invalid command is ignored");
								}
							}
							else{
								CommandValid = false;
								run = false;
								logQueue.AddToQueue(IdName + "Not enough bytes read");
							}
							if(CommandValid){
								nxtReply = null;
								mutex.WaitOne();
								try{
									if(type == BrickType.NXT){
										if((nxtCommand).CommandType == NXT.CommandType.TunnelCommand){
											if(TunnelCommandReceived == null){
												nxtReply = new NXT.Reply(NXT.CommandType.ReplyCommand, nxtCommand.CommandByte,(byte) TunnelError.UnsupportedCommand);
											}
											else{
												var brickReply = TunnelCommandReceived(nxtCommand);
												nxtReply = new NXT.Reply(brickReply.Data);
											}
											SendNetworkReply(nxtReply);
										}
										else{
											if(nxtCommand.ReplyRequired){
												if(LogActivity)
													logQueue.AddToQueue(IdName + "Forward ".PadRight(12) +  nxtCommand.CommandByte.ToString() + " with reply to NXT");
											}
											else{
												if(LogActivity)
													logQueue.AddToQueue(IdName + "Forward ".PadRight(12) + nxtCommand.CommandByte.ToString() + " without reply to NXT");
											}
											Brick.Send(nxtCommand);
											if(nxtCommand.ReplyRequired){
												nxtReply = (NXT.Reply)Brick.Receive();
												SendNetworkReply(nxtReply);
												if(LogActivity){
													logQueue.AddToQueue(IdName + "Received ".PadRight(12) + nxtReply.CommandByte.ToString() + " from NXT");
												}

											}
										}
									}
									else{
										throw new NotImplementedException("EV3 support is not implemented");
									}
								}
								catch(Exception e){
									if(e is MonoBrickException)
									{
										if(e is ConnectionException){
											logQueue.AddToQueue(IdName + e.Message);
											logQueue.AddToQueue("Closing connection to client " + ID);
											run = false;
										}
										else{
											logQueue.AddToQueue(IdName + e.Message);
											if(nxtReply == null){//try to send the message with error
												nxtReply = new NXT.Reply(NXT.CommandType.ReplyCommand, nxtCommand.CommandByte,((MonoBrickException)e).ErrorCode);
												SendNetworkReply(nxtReply);
											}
											if(ev3Reply == null){
												throw new NotImplementedException("EV3 support is not implemented");
											}
										}
									}
									else{
										if(!wasThrownOff){
											logQueue.AddToQueue(IdName + e.Message);
											logQueue.AddToQueue(IdName + "--------- Stack Trace --------\n" + e.StackTrace);
										}
										run = false;
									}
								}
								mutex.ReleaseMutex();
							}
						}
						else{
							run = false;
						}
					}
					catch(Exception e)
					{
						if(!wasThrownOff){
							logQueue.AddToQueue(IdName + e.Message);
							logQueue.AddToQueue(IdName + "--------- Stack Trace --------\n" + e.StackTrace);
						}
						break;
					}
				}
				try{
					networkStream.Close();
				}
				catch{}
				logQueue.AddToQueue(IdName +"Disconnected");
				if(onDisconnected != null)
					onDisconnected(this);
				IsConnected = false;
			}
		}
	}

	public class LogFile
	{
		private StreamWriter stream = null;
		private QueueThread<string> queue = null;
		public LogFile(){}
		public bool IsOpen{get
			{
				if(stream == null){
					return false;
				}
				return true;
			}
		}

		public void Open(string fileName)
		{
			if(stream!= null){
				queue.Restart();
				Close();
			}
			else{
				queue = new QueueThread<string>(WriteToQueue);
			}
			if(!File.Exists(fileName))
				stream = new StreamWriter(fileName);
			else
				stream = File.AppendText(fileName);
			stream.AutoFlush = true;
		}

		public void Write(string message){
			queue.AddToQueue(DateTime.Now.ToString() + ": " + message);		
		}

		public void Close(){
			queue.Close();
			stream.Flush();
			stream.Close();
			stream = null;			
		}

		private void WriteToQueue(string message){
			if(stream == null)
				Open("log.txt");
			stream.WriteLine(message);
		}
	}




}

