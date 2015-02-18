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
using System.IO;
using Java.Util;
using Android.Bluetooth;
using MonoBrick;
using Android.Speech.Tts;

namespace AndroidTunnel
{
	
	[Activity (Label = "Clients")]			
	public class ClientsActivity : ActivityWithOptionMenu
	{
		private ClientListAdapter listAdapter;
		//private TunnelInstance tunnel = TunnelInstance.Instance;
				
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			
			SetContentView(Resource.Layout.ClientList);
			listAdapter = new ClientListAdapter(this);
			ListView listView = FindViewById<ListView>(Resource.Id.list_view); //attach adapter to listview
			listView.Adapter = listAdapter;
			//tunnel.ClientConnected += OnClientConnected;
			//tunnel.ClientDisconnected += OnClientDisconnected;
			RegisterForContextMenu(listView);

		}

		private void OnClientConnected(Tunnel.Client client){
			listAdapter.Add(client);
			RunOnUiThread(delegate() {
				listAdapter.NotifyDataSetChanged();
			});
		}

		private void OnClientDisconnected(Tunnel.Client client){
			listAdapter.Remove(client);
			RunOnUiThread(delegate() {
				listAdapter.NotifyDataSetChanged();
			});
		}

		public override void OnCreateContextMenu (IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, view, menuInfo);
			AdapterView.AdapterContextMenuInfo info =(AdapterView.AdapterContextMenuInfo) menuInfo;
			Tunnel.Client client = listAdapter[info.Position];
			menu.SetHeaderTitle("Client " + client.ID+" at " + client.Address);
			menu.Add(Menu.None,0,0,"Throw off");

		}

		public override bool OnContextItemSelected (IMenuItem item)
		{
			//AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
			//Tunnel.Client client = listAdapter[info.Position];
			if(item.ItemId == 0){
				//tunnel.ThrowOffClient(this,client);
			}
			return true;
		}
	}
}



