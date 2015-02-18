using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonoBrick;
using System.Net;

namespace AndroidTunnel
{
	public class StreamingSettings{
		public int RTSPPort = 8086;
		public int VideoPort = 9000;
		public bool EnableRTSP = true;
		public bool EnableGPS = false;
		public bool EnableSpeak = false;
		public VideoQuality VideoQuality = VideoQuality.DefaultVideoQualiy;
	}

	[Service]
	public class TunnelService: Service
	{
		//private NotificationManager nm;
		private IBinder tunnelBinder;
		private Tunnel tunnel;
		private Android.OS.PowerManager.WakeLock wakeLock = null;
		private Intent intent;
		private RtspServer rtspServer = null;
		//private Handler handler;
		NotificationManager notificationManager = null; 
		private StreamingSettings streamingSettings = null;
		private IPAddress clientIpAddress;

		private void OnTunnelStated(){
			//ShowConnected();
			this.StartService(intent);
			PowerManager mgr = (PowerManager)GetSystemService(Android.Content.Context.PowerService);
			wakeLock = mgr.NewWakeLock( WakeLockFlags.ScreenDim, "MyWakeLock");
			wakeLock.Acquire();
		}

		private void OnTunnelStoped(){
			//ShowDisconnected();
			if(wakeLock != null)
				wakeLock.Release();
			this.rtspServer.Stop();
			this.StopSelf();
		}

		public TunnelService(){
			//handler = new Handler();
			tunnel = new Tunnel();
			streamingSettings = new StreamingSettings();
			rtspServer = new RtspServer ();
			tunnelBinder = new TunnelBinder(tunnel,streamingSettings,rtspServer);
			tunnel.Started += OnTunnelStated;
			tunnel.Stopped += OnTunnelStoped;
			tunnel.ClientConnected += OnClientConnected;
			tunnel.ClientDisConnected += OnClientDisconnected;
		}

		public void OnClientConnected(Tunnel.Client client){
			client.TunnelCommandReceived += OnTunnelCommand;
			clientIpAddress = client.Address;
			Console.WriteLine(clientIpAddress);
		}

		public void OnClientDisconnected(Tunnel.Client client){
			client.TunnelCommandReceived -= OnTunnelCommand;
		}

		public BrickReply OnTunnelCommand(BrickCommand command){
			BrickReply reply = null;
			BrickCommand replyCommand = null;
			if(command is MonoBrick.NXT.Command){
				var nxtCommand = new MonoBrick.NXT.Command (command.Data);
				switch(nxtCommand.CommandByte){
					case MonoBrick.NXT.CommandByte.GetTunnelCommands:
						replyCommand = new MonoBrick.NXT.Command(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte,true);
						replyCommand.Append((byte) 0x00);//no error
						if(streamingSettings.EnableRTSP){
							replyCommand.Append((byte) MonoBrick.NXT.CommandByte.GetTunnelRTSPSettings);
							replyCommand.Append((byte) MonoBrick.NXT.CommandByte.StartTunnelRTSP);
						}
						if(streamingSettings.EnableGPS){
							replyCommand.Append((byte) MonoBrick.NXT.CommandByte.GetTunnelGPSPosition);
						}
						if(streamingSettings.EnableSpeak){
							replyCommand.Append((byte) MonoBrick.NXT.CommandByte.TunnelSpeak);
						}
						reply = new MonoBrick.NXT.Reply(replyCommand.Data);
					break;
					case MonoBrick.NXT.CommandByte.GetTunnelRTSPSettings:
						if(streamingSettings.EnableRTSP){
							replyCommand = new MonoBrick.NXT.Command(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte,true);
							replyCommand.Append((byte) 0x00);//no error
							replyCommand.Append((Int32)streamingSettings.RTSPPort);
							replyCommand.Append((Int32)streamingSettings.VideoPort);
							reply = new MonoBrick.NXT.Reply(replyCommand.Data);
						}
						else{
							reply = new MonoBrick.NXT.Reply(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte,(byte) TunnelError.ErrorExecuting);
						}
					break;

					case MonoBrick. NXT.CommandByte.StartTunnelRTSP:
						if(rtspServer.Start(clientIpAddress.ToString(),streamingSettings.RTSPPort,streamingSettings.VideoPort,streamingSettings.VideoQuality)){
							reply = new MonoBrick.NXT.Reply(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte);
						}
						else{
							reply = new MonoBrick.NXT.Reply(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte,(byte) TunnelError.ErrorExecuting);
						}
					break;
					default:
						reply = new MonoBrick.NXT.Reply(MonoBrick.NXT.CommandType.ReplyCommand,nxtCommand.CommandByte,(byte) TunnelError.UnsupportedCommand);
					break;
				}
			}
			else{
				throw new NotImplementedException ("EV3 support has been implemented");
			}




			return reply;
		}

		public class TunnelBinder:Binder {
			private Tunnel tunnel;
			private StreamingSettings settings;
			private RtspServer rtspServer;
			public TunnelBinder(Tunnel tunnel, StreamingSettings settings, RtspServer rtspServer){
				this.tunnel = tunnel;
				this.settings = settings;
				this.rtspServer = rtspServer;
			}
			public Tunnel Tunnel{
				get{return tunnel;}
			}

			public RtspServer RtspServer{
				get{return rtspServer;}
			}

			public StreamingSettings StreamingSettings{
				get{return settings;}
				set{settings = value;}
			}
		}

		public override void OnCreate ()
		{
			//nm = (NotificationManager) GetSystemService (NotificationService);
		}

		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			return StartCommandResult.Sticky;
		}

		public override IBinder OnBind (Intent intent)
		{
			this.intent = intent;
			//notificationManager = (NotificationManager)this.GetSystemService(Android.Content.Context.NotificationService);
			return tunnelBinder;
		}

		public override bool OnUnbind (Intent intent)
		{
			return base.OnUnbind (intent);
		}

		public override void OnLowMemory ()
		{
			base.OnLowMemory ();
		}


		public override void OnDestroy ()
		{
			base.OnDestroy();
		}

		void ShowConnected()
		{
			Intent startActivity = new Intent(this, typeof(TunnelService));
			startActivity.SetFlags(ActivityFlags.SingleTop);
			PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, startActivity, PendingIntentFlags.UpdateCurrent);
			
			
			var notification = new Notification(Resource.Drawable.addClient, "NXT Tunnel is running", DateTime.Now.Second);
			notification.SetLatestEventInfo(this, "NXT Tunnel", "Tunnel is running", pendingIntent);
			notificationManager.Notify( 0, notification);
		}

		void ShowDisconnected()
		{
			Intent startActivity = new Intent(this, typeof(TunnelService));
			startActivity.SetFlags(ActivityFlags.SingleTop);
			PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, startActivity, PendingIntentFlags.UpdateCurrent);
			
			
			var notification = new Notification(Resource.Drawable.addClient, "NXT Tunnel has stopped", DateTime.Now.Second);
			notification.SetLatestEventInfo(this, "NXT Tunnel", "Tunnel has stopped", pendingIntent);
			notificationManager.Notify( 0, notification);
		}
	}

	public class TunnelServiceConnection :Java.Lang.Object, IServiceConnection 
	{ 
		private Action<TunnelService.TunnelBinder> onConnect = delegate {};
		private Action<TunnelService.TunnelBinder> onDisconnect = delegate {};
		private TunnelService.TunnelBinder binder;
		public TunnelServiceConnection(Action<TunnelService.TunnelBinder> onConnect, Action<TunnelService.TunnelBinder> onDisconnect){
			this.onConnect = onConnect;
			this.onDisconnect = onDisconnect;
		}
		public void OnServiceConnected(ComponentName name, IBinder service) 
		{ 
			binder = (TunnelService.TunnelBinder) service;
			onConnect(binder);
		} 
		
		public void OnServiceDisconnected(ComponentName name) 
		{ 
			onDisconnect(binder);
		} 
	}
}




