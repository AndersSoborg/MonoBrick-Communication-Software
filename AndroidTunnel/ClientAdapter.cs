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
using MonoBrick;

namespace AndroidTunnel
{
	public class ClientListAdapter : BaseAdapter
	{
		private Activity context;
		public List<Tunnel.Client> items;

		public ClientListAdapter(Activity context): base()
		{
			this.context = context;
			items = new List<Tunnel.Client>();

		}

		public void Add(Tunnel.Client client){
			items.Add(client);
		}

		public void Remove(Tunnel.Client client){
			items.Remove(client);
		}		
		public override int Count
		{
			get { return items.Count; }
		}
		
		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}
		
		public override long GetItemId(int position)
		{
			return position;
		}
		
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//Get our object for this position
			Tunnel.Client item = items[position];			
			
			//Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// This gives us some performance gains by not always inflating a new view
			// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
			LayoutInflater inflater = context.LayoutInflater;
			View row = inflater.Inflate(Resource.Layout.ClientListItem,parent,false);
			
			//Find references to each subview in the list item's view
			//var imageItem = view.FindViewById(Resource.Id.image_item) as ImageView;
			var textTop = row.FindViewById(Resource.Id.text_top) as TextView;
			var textBottom = row.FindViewById(Resource.Id.text_bottom) as TextView;
			
			//Assign this item's values to the various subviews
			//imageItem.SetImageResource(item.Image);
			textTop.SetText(item.ID.ToString(), TextView.BufferType.Normal);
			textBottom.SetText(item.Address.ToString(), TextView.BufferType.Normal);
			
			//Finally return the view
			return row;
		}
		
		public Tunnel.Client GetItemAtPosition(int position)
		{
			return items[position];
		}

		public Tunnel.Client this[int i]{
			get{return items[i];}
		}
	}
}

