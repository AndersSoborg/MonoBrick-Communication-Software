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


namespace AndroidTunnel
{

	public class ActivityWithOptionMenu : Activity
	{
		private IMenuItem startButton = null;
		private IMenuItem connectButton = null;
		private IMenuItem exitButton = null;
		protected Tunnel tunnel = null;
		private IServiceConnection serviceConnection;
		//private bool isBound = false;


		private bool IsTunnelServiceRunning(){
			ActivityManager manager = (ActivityManager) GetSystemService(Android.Content.Context.ActivityService);
			IList<ActivityManager.RunningServiceInfo> serviceList  = manager.GetRunningServices(int.MaxValue);
			for(int i = 0; i < serviceList.Count; i++){
				if(serviceList[i].Service.ClassName.ToLower().Contains("tunnelservice"))
					return true;
			}
			return false;
		}

		protected virtual void OnBoundToTunnelService(Tunnel tunnel){

		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			serviceConnection = new TunnelServiceConnection(
				delegate(TunnelService.TunnelBinder binder){//On connect
					tunnel = binder.Tunnel;
					OnBoundToTunnelService(tunnel);
					//isBound = true;
				},
				delegate(TunnelService.TunnelBinder binder){//On disconnect
					//isBound = false;
				}
			);
			ApplicationContext.BindService (new Intent (this, typeof (TunnelService)), serviceConnection, Bind.AutoCreate);
		}

		protected override void OnStart ()
		{
			base.OnStart ();
		}

		protected override void OnStop ()
		{
			base.OnStop ();
			ApplicationContext.UnbindService(serviceConnection);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			base.OnOptionsItemSelected(item);
			if(item.ItemId == Resource.Id.option_start_stop){
				if(tunnel.IsRunning){
					tunnel.Stop();
				}
				else{
					
					tunnel.Start(new TunnelSettings("Loopback",1500), new MonoBrick.NXT.Loopback<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>());
				}
			}
			if(item.ItemId == Resource.Id.option_connect){
				AlertDialog.Builder dialog = new AlertDialog.Builder(this);
				dialog.SetTitle("Enter IP Address");
				EditText ipAddress = new EditText(this);
				dialog.SetView(ipAddress);
				dialog.SetPositiveButton("Connect",delegate(object sender, DialogClickEventArgs e) {
					ProgressDialog progress = ProgressDialog.Show(this,"","Connecting to client...");
					Thread t = new Thread(
						new ThreadStart(
						delegate()
						{
						if(tunnel.ConnectToClient(ipAddress.Text)){
							RunOnUiThread(delegate() {
								Toast.MakeText(ApplicationContext,"Successfully connected to client.", ToastLength.Short).Show();
							});
						}
						else{
							RunOnUiThread(delegate() {
								Toast.MakeText(ApplicationContext,"Failed to connect to client.", ToastLength.Short).Show();
							});
						}
						progress.Dismiss();
					}));
					t.IsBackground = true;
					t.Priority =  System.Threading.ThreadPriority.Normal;
					t.Start();
				});
				dialog.SetNegativeButton("Cancel",delegate(object sender, DialogClickEventArgs e) {
					
				});
				dialog.Show();
			}
			
			if(item.ItemId == Resource.Id.option_exit){

				//ApplicationContext.BindService(new Intent(this,typeof(TunnelService)), serviceConnection,Bind.AutoCreate);
			}
			return true;
		}

		public override bool OnCreateOptionsMenu(IMenu menu){
			//bool isRunning = IsTunnelServiceRunning();
			base.OnCreateOptionsMenu (menu);
			MenuInflater.Inflate(Resource.Layout.OptionMenu,menu);
			startButton = menu.FindItem(Resource.Id.option_start_stop);
			connectButton = menu.FindItem(Resource.Id.option_connect);;
			exitButton = menu.FindItem(Resource.Id.option_exit);

			connectButton.SetTitle("Connect To Client");
			connectButton.SetIcon(Resource.Drawable.addClient);

			exitButton.SetIcon(Resource.Drawable.exit);
			exitButton.SetTitle("Exit");
			return true;
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			if(tunnel.IsRunning){
				startButton.SetIcon(Resource.Drawable.stopDark);
				startButton.SetTitle("Stop Tunnel");
			}
			else{
				startButton.SetIcon(Resource.Drawable.startDark);
				startButton.SetTitle("Start Tunnel");
			}
			connectButton.SetEnabled(tunnel.IsRunning);
			return base.OnPrepareOptionsMenu (menu);
		}
	}
}




