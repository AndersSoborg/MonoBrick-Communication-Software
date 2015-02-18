using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using MonoBrick;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Specialized;


namespace NXTBridge
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
		[XmlArrayItem("Range", typeof(IPAddressRange))]
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

		[XmlElement("MaxConnections")]
	    private int maxConnections = 16;
		
		[XmlElement("IPFilter")]
		public IPFilter IPFilter { get; set; }

		public TunnelSettings(){
			IPFilter = new NetworkTunnel.IPFilter();
			IPFilter.IPRanges = new ArrayList();
			IPFilter.IPRanges.Add(new IPAddressRange());
			IPFilter.IPRangeMode = NetworkTunnel.IPRangeMode.Disable;
		}

	    public TunnelSettings(String connection, int port)
	    {
	        this.Connection = connection;
	        this.Port = port;
			IPFilter = new NetworkTunnel.IPFilter();
			IPFilter.IPRanges = new ArrayList();
			IPFilter.IPRanges.Add(new IPAddressRange());
			IPFilter.IPRangeMode = NetworkTunnel.IPRangeMode.Disable;
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

		public void SerializeToXML(String filepath)
	    {
	        XmlSerializer serializer = new XmlSerializer(typeof(TunnelSettings));
	        TextWriter textWriter = new StreamWriter(filepath);
	        serializer.Serialize(textWriter, this);
	        textWriter.Close();

	    }

	    public TunnelSettings DeserializeFromXML(String filepath)
	    {
			XmlSerializer deserializer = new XmlSerializer(typeof(TunnelSettings));
	        TextReader textReader = new StreamReader(filepath);
	        Object obj = deserializer.Deserialize(textReader);

	        // Create the new Config that we'll return
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

		public  IPAddressRange():this("127.0.0.1","127.0.0.1")
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
 
	public delegate void LogDelegate(string logMessage);
	public delegate void ClientDelegate(Tunnel.Client client);
	public class Tunnel
	{
		#region shared with clients
		private static Mutex mutex = new Mutex();
		private static Connection NXT = null;
		private static QueueThread<string> logQueue;

		#endregion

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
		}

		private void OnNXTDisconnected(){
			if(NXTDisconnected != null){
				NXTDisconnected();
			}
			logQueue.AddToQueue("NXT disconnected");
			if(IsRunning){
				Stop();
			}
		}

		private void OnNXTConnected(){
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

		private void ListenForClients(){
			tcpListener.Start();
			logQueue.AddToQueue("Maximum number of connections " + settings.MaxConnections);
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
			logQueue.AddToQueue("Waiting for client(s) at port: " + settings.Port);

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
					bool accept = false;
					if(addressRange != null && settings.IPFilter.IPRangeMode != IPRangeMode.Disable && addressRange.Length > 0){
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
					if(!accept){
						logQueue.AddToQueue("Connection from " + address + " was rejected");
					}
					else{
						if(clients.Count == settings.MaxConnections){
							accept = false;
							logQueue.AddToQueue("Connection from " + address + " was rejected. Maximum number of connections reached");
						}
					}

					if(accept){
						Client client = new Client(clientID,tcpClient, ClientDisConnected);
						if(ClientConnected!= null)
							ClientConnected(client);
						lock(clientsLock){
							clients.Add(clientID,client);
						}
						clientID++;
						logQueue.AddToQueue("Clients connected " + clients.Count);
					}

				}
			 } 
		}

		public event LogDelegate LogEvent;
		public event ClientDelegate ClientConnected;
		public event ClientDelegate ClientDisConnected;
		public event Action NXTDisconnected;
		public event Action NXTConnected;
		public event Action NoClientsConnected;
		public Tunnel(TunnelSettings settings)
		{
			this.settings = settings;
			switch(settings.Connection.ToLower()){
				case  "usb":
					NXT = new USB();
					break;
				case "simulation":
					NXT = new Simulation();
					break;
				default:
					NXT = new Bluetooth(settings.Connection);
					break;
			}
            tcpListener = new TcpListener(IPAddress.Any, settings.Port);
      		listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.IsBackground = false;
			IsRunning = false;
			ClientDisConnected += ClientDisconnected;//subscribe to event thrown by client thread
			this.addressRange = settings.IPRangeToArray();
			NXT.Disconnected += OnNXTDisconnected;
			NXT.Connected += OnNXTConnected;
			logQueue = new QueueThread<string>(OnLogQueue);
		}

		public bool IsRunning{get;private set;}
		public bool IsConnectedToNXT{
			get{return NXT.IsConnected;}
		}

		public void Start(){
			logQueue.AddToQueue("Connecting to NXT Brick through: " + settings.Connection);
			NXT.Connect();
			logQueue.AddToQueue("Connected...");
			listenThread.Start();
			IsRunning = true;
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
			// To get the keys alone, use the Keys property.
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
				ThrowOffAllClients();
				this.NoClientsConnected += OnTunnelStoped;
			}
			IsRunning = false;
			try{
				tcpListener.Stop();
			}
			catch(Exception){}
			try{
				if(NXT.IsConnected)
					NXT.Disconnect();
			}
			catch(Exception){}
			if(!throwOffClints){
				OnTunnelStoped();
			}

		}

		private void OnTunnelStoped ()
		{
			logQueue.AddToQueue("Tunnel stoped");
			this.NoClientsConnected -= OnTunnelStoped;
			//add more here if you need
		}


		public class Client {
	        private IPAddress address;
	        private DateTime connectTime;
	        private Thread thread;
			private NetworkStream networkStream;
			private ClientDelegate onDisconnected;
			private bool wasThrownOff = false;
			private void ClientThread(){
				byte[] message = null;
	  			int bytesRead;
				Command command = null;
				Reply reply = null;
	  			logQueue.AddToQueue("Client " + ID + ": Connected from " + address);
				IsConnected = true;
				bool run = true;
				while (run)
	  			{
	    			bytesRead = 0;
					message = new byte[4096];
				    try
				    {
				      	bytesRead = networkStream.Read(message, 0, 4096);
						if(bytesRead > 0){
							bool CommandValid = true;
							Array.Resize<byte>(ref message, bytesRead);
							try{
								for(int i = 0; i < bytesRead; i++){
									logQueue.AddToQueue("Commandbyte[" + i +"]=0x" + message[i].ToString("X"));
								}
								command = new Command(message);
							}
							catch(Exception){
								CommandValid = false;
								logQueue.AddToQueue("Client " + ID + ": Invalid command is ignored");
							}
							if(CommandValid){
								reply = null;
								mutex.WaitOne();
								try{
									if(command.ReplyRequired){
										logQueue.AddToQueue("Client " + ID + ": Forward ".PadRight(12) +  command.CommandByte.ToString() + " with reply to NXT");
									}
									else{
										logQueue.AddToQueue("Client " + ID + ": Forward ".PadRight(12) + command.CommandByte.ToString() + " without reply to NXT");
									}
									NXT.Send(command);
									if(command.ReplyRequired){
										reply = NXT.Receive();
										networkStream.Write(reply.Data,0,reply.Data.Length);
										logQueue.AddToQueue("Client " + ID + ": Received ".PadRight(12) + reply.CommandByte.ToString() + " from NXT");
									}
								}
								catch(Exception e){
									if(e is NXTException)
									{
										if(e is ConnectionException){
											logQueue.AddToQueue("Client " + ID + ": " + e.Message);
											logQueue.AddToQueue("Closing connection with NXT");
											NXT.Disconnect();
											run = false;
										}
										else{
											logQueue.AddToQueue("Client " + ID + ": " + e.Message);
											if(reply == null){//try to send the message
												reply = new Reply(CommandType.ReplyCommand, command.CommandByte,((NXTException)e).ErrorCode);
												networkStream.Write(reply.Data,0,reply.Data.Length);
											}
										}
									}
									else{
										logQueue.AddToQueue("Client " + ID + ": " + e.Message);
										logQueue.AddToQueue("Client " + ID + ": " + e.StackTrace);
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
						if(!wasThrownOff)
							logQueue.AddToQueue("Client " + ID + ": Error occurred: " + e.Message);
						break;
				    }
				}
				try{
					networkStream.Close();
				}
				catch{}
				logQueue.AddToQueue("Client " + ID + ": Disconnected");
				if(onDisconnected != null)
					onDisconnected(this);
				IsConnected = false;
			}

			public Client(int id, TcpClient tcpClient, ClientDelegate clientDelegate) { 
				address =  ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;
				connectTime = DateTime.Now;
				thread = new Thread(ClientThread);
                thread.IsBackground = true;
				thread.Priority = ThreadPriority.AboveNormal;
				networkStream = tcpClient.GetStream();
				onDisconnected = clientDelegate;
				this.ID = id;
				thread.Start();
	        }

			public void ThrowOff(){
				logQueue.AddToQueue("Client " + ID + ": Throw off signaled");
				wasThrownOff = true;
				if(IsConnected)
					networkStream.Close();
			}

			public IPAddress Address{get{ return address;}}
	        public DateTime ConnectTime { get { return connectTime; } }
			public int ID{get; private set;}
			public bool IsConnected{get; private set;}
    	}
	}


}

