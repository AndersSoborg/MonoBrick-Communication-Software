using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics.Drawables;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Speech.Tts;
using MonoBrick;
using System.Threading;
using System.Diagnostics;
using Android.Bluetooth;
using Android.Preferences;

namespace AndroidTunnel
{
	[Activity (MainLauncher=true, Theme="@android:style/Theme.NoTitleBar")]
	//[Activity (MainLauncher=true, Label="NXT Tunnel")]
	//public class MainTabWidget: ActivityWithOptionMenu
	public class MainWidget : Activity
	{	
		//private ISurfaceHolder holder;
		private SurfaceView camera;

		private ImageButton startButton = null;
		private ImageButton connectButton = null;
		private ImageButton settingsButton = null;
		private ImageView connectLight = null;
		private ImageView streamingLight = null;
		private TextView connectText = null;
		private TextView streamText = null;

		private TextView logView = null;
		private ScrollView scrollView = null;
		private int MaxLines = 40;
		private static string savedText = "";

		private Tunnel tunnel = null;
		private RtspServer rtspServer = null;
		private IServiceConnection serviceConnection;
		private Connection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply> connection;

		private StreamingSettings streamingSettings = new StreamingSettings();
		private TunnelSettings tunnelSettings = new TunnelSettings();
		private string bluetoothDeviceName;

		protected override void OnCreate (Bundle bundle)
		{
			/*base.OnCreate (bundle);
			SetContentView(Resource.Layout.CameraView);
			camera = (SurfaceView)FindViewById(Resource.Id.smallcameraview);
			camera.Holder.SetType(SurfaceType.PushBuffers);
			holder = camera.Holder;
			Session.setSurfaceHolder(holder);*/

			base.OnCreate (bundle);
			base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
			SetContentView (Resource.Layout.Main);
			startButton = FindViewById<ImageButton> (Resource.Id.startButton);
			connectButton = FindViewById<ImageButton> (Resource.Id.connectToClientButton);
			settingsButton = FindViewById<ImageButton> (Resource.Id.settingsButton);

			connectLight = FindViewById<ImageView> (Resource.Id.connectLight);
			streamingLight = FindViewById<ImageView> (Resource.Id.streamingLight);

			connectText = FindViewById<TextView> (Resource.Id.connectStatus);
			streamText = FindViewById<TextView> (Resource.Id.streamingStatus);

			logView = FindViewById<TextView> (Resource.Id.log);
			scrollView = FindViewById<ScrollView> (Resource.Id.logScrollView);
			scrollView.FullScroll(FocusSearchDirection.Down);
			logView.SetText(savedText,TextView.BufferType.Editable);
			scrollView.ScrollTo(0,logView.Height);
			scrollView.FullScroll(FocusSearchDirection.Down);

			camera = (SurfaceView)FindViewById(Resource.Id.smallcameraview);
			camera.Holder.SetType(SurfaceType.PushBuffers);
			//holder = camera.Holder;
			Session.SetSurfaceHolder(camera.Holder);


			startButton.Click += OnStartClicked;
			connectButton.Click += OnConnectToClientClicked;
			settingsButton.Click += OnStettingsClicked;

			//this should be set based on something
			DisableButton(connectButton);
			EnableButton(settingsButton);
			SetStartICon();

			SetDisableLight(connectLight);
			SetDisableLight(streamingLight);

			connectText.Text = "Client is not connected";
			streamText.Text = "Not streaming";

			PreferenceManager.SetDefaultValues(this, Resource.Layout.Settings,false);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		protected override void OnStart ()
		{

			Console.WriteLine("On Start");
			base.OnStart ();
		}


		protected override void OnStop ()
		{


			Console.WriteLine("On Stop");
			base.OnStop ();
		}

		protected override void OnResume(){
			serviceConnection = new TunnelServiceConnection (OnBoundToTunnelService, OnUnBoundToTunnelService);
			ApplicationContext.BindService (new Intent (this, typeof(TunnelService)), serviceConnection, Bind.AutoCreate);
			Console.WriteLine("On Resume");
			logView.SetText(savedText,TextView.BufferType.Editable);
			scrollView.ScrollTo(0,logView.Height);
			scrollView.FullScroll(FocusSearchDirection.Down);
			SetupBluetooth();
			base.OnResume();
		}
		
		protected override void OnPause ()
		{
			ApplicationContext.UnbindService(serviceConnection);
			Console.WriteLine("On pause");
			base.OnPause ();
		}

		protected void OnBoundToTunnelService(TunnelService.TunnelBinder binder){
			Console.WriteLine("Bound to tunnel service");
			if(tunnel == null || !tunnel.Equals(binder.Tunnel)){
				tunnel = binder.Tunnel;
				rtspServer = binder.RtspServer;
				streamingSettings = binder.StreamingSettings;
				tunnel.Started += OnTunnelStarted;
				tunnel.Stopped += OnTunnelStopped;
				tunnel.ClientConnected += OnClientConnected;
				tunnel.ClientDisConnected += OnClientDisconnected;
				tunnel.LogEvent += WriteLine;
				rtspServer.StreamingStarted += OnStreamStarted;
				rtspServer.StreamingStoped += OnStreamStopped;
			}
			if (tunnel.IsRunning) {
				OnTunnelStarted();
				if(tunnel.ClientsConnected != 0){
					OnClientConnected(null);
				}
				else{
					OnClientDisconnected(null);
				}
				SetStreamStatus(rtspServer.IsStreaming);
			}
			else{
				OnTunnelStopped();
				OnClientDisconnected(null);
				SetStreamStatus(false);
			}
			//Do something here
		}

		protected void OnUnBoundToTunnelService(TunnelService.TunnelBinder binder){
			tunnel.Started -= OnTunnelStarted;
			tunnel.Stopped -= OnTunnelStopped;
			tunnel.ClientConnected -= OnClientConnected;
			tunnel.ClientDisConnected -= OnClientDisconnected;
			tunnel.LogEvent -= WriteLine;
			rtspServer.StreamingStarted -= OnStreamStarted;
			rtspServer.StreamingStoped -= OnStreamStopped;
			Console.WriteLine("Unbound from tunnel service");
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

		private bool IsTunnelServiceRunning(){
			ActivityManager manager = (ActivityManager) GetSystemService(Android.Content.Context.ActivityService);
			IList<ActivityManager.RunningServiceInfo> serviceList  = manager.GetRunningServices(int.MaxValue);
			for(int i = 0; i < serviceList.Count; i++){
				if(serviceList[i].Service.ClassName.ToLower().Contains("tunnelservice"))
					return true;
			}
			return false;
		}

		protected void SetDisableLight(ImageView image){
			image.SetImageResource(Resource.Drawable.circle_white);
		}

		protected void SetEnableLight(ImageView image){
			image.SetImageResource(Resource.Drawable.circle_green_light);
		}

		protected void DisableButton(ImageButton button){
			button.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.circle_white));
		}

		protected void EnableButton(ImageButton button){
			button.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.buttonFocus));
		}

		protected void SetStopIcon(){
			startButton.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.stopWhite));
		}

		protected void SetStartICon(){
			startButton.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.startWhite));
		}

		protected void OnTunnelStarted(){
			RunOnUiThread(delegate() {
				DisableButton(settingsButton);
				EnableButton(connectButton);
				SetStopIcon();
			});
		}

		protected void OnTunnelStopped(){
			RunOnUiThread(delegate() {
				EnableButton(settingsButton);
				DisableButton(connectButton);
				SetStartICon();
			});

		}

		protected void OnClientConnected(Tunnel.Client client){
			if (client != null) {
				client.LogId = false;
			}
			RunOnUiThread(delegate() {
				SetEnableLight(connectLight);
				connectText.Text = "Client is connected";
				DisableButton(connectButton);
			});
		}

		protected void OnClientDisconnected(Tunnel.Client client){
			RunOnUiThread(delegate() {
			SetDisableLight(connectLight);
				connectText.Text = "Client is not connected";
				if(tunnel.IsRunning)
					EnableButton(connectButton);
				else
					DisableButton(connectButton);
			});
		}

		private void OnStreamStarted(){
			WriteLine("Stream started");
			SetStreamStatus (true);
		}
		
		private void OnStreamStopped(){
			WriteLine("Stream stoped");
			SetStreamStatus (false);
		}

		private void SetStreamStatus(bool isStreaming){
			if(isStreaming){
				RunOnUiThread(delegate() {
					SetEnableLight(streamingLight);
					streamText.Text = "Streaming";
				});
			}
			else{
				RunOnUiThread(delegate() {
					SetDisableLight(streamingLight);
					streamText.Text = "Not streaming";
				});
			}
		}

		protected void OnStettingsClicked(object sender, EventArgs e){
			if(!tunnel.IsRunning)
				StartActivity(new Intent(this,typeof(SettingsActivity)));
		}


		protected void OnStartClicked(object sender, EventArgs e){

			if(tunnel.IsRunning){
				ProgressDialog progress = null;
				RunOnUiThread(delegate() {
					progress = ProgressDialog.Show(this,"Disconnecting From Brick...","Closing connection...       ");
				});
				System.Threading.Thread t =  new System.Threading.Thread( delegate(object obj){
					tunnel.Stop();
					RunOnUiThread(delegate() {
						progress.Dismiss();
						ShowToast("Tunnel stoped");
					});
				});
				t.IsBackground = true;
				t.Priority = System.Threading.ThreadPriority.Normal;
				t.Start();
			}
			else{
				ProgressDialog progress = null;
				System.Threading.Thread t = new System.Threading.Thread(delegate(object obj){
					UpdateSettings();
					if(bluetoothDeviceName == ""){
						RunOnUiThread(delegate() {
							AlertDialog dialog = new AlertDialog.Builder(this).Create();
							dialog.SetTitle("Select Bluetooth Device");
							dialog.SetMessage("No Bluetooth Device has been selected. Please go to settings and select a Bluetooth device");
							dialog.SetButton("OK", delegate(object sen, DialogClickEventArgs eventArgs) {
							});
							dialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
							dialog.Show();
						});
						return;
					}
					RunOnUiThread(delegate() {
						progress =	ProgressDialog.Show(this,"Connecting To Brick...","Opening connection...       ",true,false);
					});
					connection = new MonoBrick.Bluetooth<MonoBrick.NXT.Command, MonoBrick.NXT.Reply>(bluetoothDeviceName);
					//connection = new Loopback();
					if(tunnel.Start(tunnelSettings, connection)){
						RunOnUiThread(delegate() {
							progress.Dismiss();
							ShowToast("Tunnel started");
						});
					}
					else{
						RunOnUiThread(delegate() {
							progress.Dismiss();
							ShowToast("Failed to start tunnel");
						});
					}
				});
				t.IsBackground = true;
				t.Priority = System.Threading.ThreadPriority.Normal;
				t.Start();
			}
		}

		protected void OnConnectToClientClicked(object sender, EventArgs e){
			if(tunnel.ClientsConnected > 0 || !tunnel.IsRunning){
				return;
			}
			AlertDialog.Builder dialog = new AlertDialog.Builder(this);
			dialog.SetTitle("Enter IP Address");
			EditText ipAddress = new EditText(this);
			dialog.SetView(ipAddress);
			ISharedPreferences pref = GetSharedPreferences ("clientSettings", 0);
			string clientToConnectTo = pref.GetString ("clientToConnectTo", "192.168.1.100");
			ipAddress.Text = clientToConnectTo;
			dialog.SetPositiveButton("Connect",delegate{
				// Save settings
				ISharedPreferencesEditor editor = pref.Edit();
				editor.PutString("clientToConnectTo",ipAddress.Text);
				editor.Commit();
				ProgressDialog progress = ProgressDialog.Show(this,"","Connecting to client...");
				Thread t = new Thread(
					new ThreadStart(
					delegate()
					{
					if(tunnel.ConnectToClient(ipAddress.Text)){
						RunOnUiThread(delegate() {
							ShowToast("Successfully connected to client.", ToastLength.Short);
						});
					}
					else{
						RunOnUiThread(delegate() {
							ShowToast("Failed to connect to client.", ToastLength.Short);
						});
					}
					progress.Dismiss();
				}));
				t.IsBackground = true;
				t.Priority =  System.Threading.ThreadPriority.Normal;
				t.Start();
			});
			dialog.SetNegativeButton("Cancel",delegate {
				
			});
			dialog.Show();
		}

		private void SetupBluetooth(){
			if(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
				//Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
				//StartActivityForResult(enableBtIntent,(int) ActivityResult.BluetoothEnabled);
				AlertDialog.Builder dialog = new AlertDialog.Builder(new ContextThemeWrapper(this, Android.Resource.Style.ThemeTranslucentNoTitleBar));
				//AlertDialog	dialog = new  AlertDialog.Builder(this).Create();
				dialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
				dialog.SetTitle("Bluetooth permission request");
				dialog.SetMessage("This application is requesting permission to turn on Bluetooth");

				dialog.SetPositiveButton("Yes",delegate(object sender, DialogClickEventArgs e){	ProgressDialog progress = ProgressDialog.Show(this,"","Turning on Bluetooth...");
					Thread t = new Thread(
						new ThreadStart(
						delegate()
						{
						MonoBrick.Bluetooth<BrickCommand,BrickReply>.Enable();
							Stopwatch stopWatch = new Stopwatch();
							stopWatch.Start();
							while(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled && stopWatch.ElapsedMilliseconds < 5000){}
							stopWatch.Stop();
							progress.Dismiss();
							if(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
								RunOnUiThread(delegate() {
									ShowToast("Failed to turn on Bluetooth");
									//Update GUI
								});
						}
					}));
					t.IsBackground = true;
					t.Priority =  System.Threading.ThreadPriority.Normal;
					t.Start();
				});
				dialog.Show();
			}
			else{
				//Update GUI
			}
		}

		private void UpdateSettings() {
			// Get the xml/preferences.xml preferences
			var pref = PreferenceManager.GetDefaultSharedPreferences(this.BaseContext); 

			tunnelSettings.IPFilter.IPRangeMode = IPRangeMode.Disable;
			tunnelSettings.ListenForClients = pref.GetBoolean("listenForClients",true);
			tunnelSettings.LogFileName = "";//disable log to a file
			tunnelSettings.MaxConnections = 1;
			tunnelSettings.Port = int.Parse(pref.GetString("tunnelPort","1500"));
			bluetoothDeviceName = pref.GetString("bluetoothDevice","");
			tunnelSettings.Connection = bluetoothDeviceName;  


			streamingSettings.EnableGPS = false;
			streamingSettings.EnableRTSP = pref.GetBoolean("streamVideo",true);
			streamingSettings.EnableSpeak = false;
			streamingSettings.RTSPPort = int.Parse(pref.GetString("rtspPort","8086"));
			streamingSettings.VideoPort = int.Parse(pref.GetString("videoPort","9000"));
			string quality = pref.GetString("videoQuality","Normal");
			string resolution = pref.GetString("videoResolution","640x480");
			int x,y;
			int frameRate = int.Parse (pref.GetString ("frameRate", "15"));
			switch(resolution.ToLower()){
				case "320x240":
					x = 320;
					y = 240;	
					break;
				case "640x480":
					x = 640;
					y = 480;	
					break;
				case "960x720":
					x = 960;
					y = 720;	
					break;
				default:
					x = 640;
					y = 480;
				break;
			}
			switch(quality.ToLower()){
				case "very low":
					streamingSettings.VideoQuality = new VideoQuality(x,y,frameRate,50000);
				break;
				case "low":
					streamingSettings.VideoQuality = new VideoQuality(x,y,frameRate,100000);
				break;
				case "normal":
					streamingSettings.VideoQuality = new VideoQuality(x,y,frameRate,250000);
				break;
				case "high":
					streamingSettings.VideoQuality = new VideoQuality(x,y,frameRate,500000);
				break;
				case "very high":
					streamingSettings.VideoQuality = new VideoQuality(x,y,frameRate,750000);
				break;
				default:
					streamingSettings.VideoQuality = new VideoQuality(x,y,25,25000);
				break;
			}


		}

		private void ShowToast(string message, ToastLength duration = ToastLength.Short){
			LayoutInflater inflater = LayoutInflater;
			View layout = inflater.Inflate(Resource.Layout.toast, (ViewGroup) FindViewById(Resource.Id.toast_layout_root));
			//ImageView image = (ImageView) layout.FindViewById(Resource.Id.toast_image);
			//image.SetImageResource(Resource.Drawable.Icon);
			TextView text = (TextView) layout.FindViewById(Resource.Id.toast_text);
			text.SetText(message, TextView.BufferType.Normal);
			Toast toast = new Toast(ApplicationContext);
			toast.SetGravity(GravityFlags.CenterVertical, 0, 0);
			toast.Duration = duration;
			toast.View = layout;
			toast.Show();
		}



	}

}

