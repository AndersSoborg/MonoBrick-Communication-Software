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

namespace OmniBotRemote
{
		public class FileItem
		{
			private MonoBrick.IBrickFile brickFile;
			public FileItem(MonoBrick.IBrickFile brickFile){
				this.brickFile = brickFile;
				this.Checked = false;
			}
			public string Name { get { return brickFile.Name; }}
			public uint Size { get { return brickFile.Size; } }
			public MonoBrick.FileType FileType{get { return brickFile.FileType; }}
			public bool Checked{get;set;}
			public string TypeAsString 
			{
				get{
					string type;
					switch(brickFile.FileType){
						case MonoBrick.FileType.Datalog:
							type = "Datalog";
						break;
						case MonoBrick.FileType.Firmware:
							type = "Firmware";
						break;
						case MonoBrick.FileType.Graphics:
							type = "Graphics";
						break;
						case MonoBrick.FileType.OnBrickProgram:
							type = "On-Brick";
						break;
						case MonoBrick.FileType.Program:
							type = "Program";
						break;
						case MonoBrick.FileType.Sound:
							type = "Sound";
						break;
						case MonoBrick.FileType.TryMeProgram:
							type = "Try-Me";
						break;
						case MonoBrick.FileType.Unknown:
							type = "Unknown";
						break;
						default:
							type = "Unknown";
						break;
					}
					return type;
				}
			}
		}
		
		public class FileItemAdapter : BaseAdapter, View.IOnClickListener 
		{
			private Activity context;
			public List<FileItem> Items;
			public List<FileItem> SelectedItems;
			public event Action<FileItem,bool> ItemCheckedChanged = delegate {};
			
			public  FileItemAdapter(Activity context) 
				: base()
			{
				this.context = context;
				this.Items = new List<FileItem>();
				this.SelectedItems = new List<FileItem>();
			}
			public override int Count
			{
				get { return Items.Count; }
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
				var item = Items[position];			
	
				//Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
				// This gives us some performance gains by not always inflating a new view
				// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
				var view = (convertView ?? 
					context.LayoutInflater.Inflate(
						Resource.Layout.ListRow, 
						parent, 
						false)) as RelativeLayout;
	
				//Find references to each subview in the list item's view
				var name = view.FindViewById(Resource.Id.fileName) as TextView;
				var type = view.FindViewById(Resource.Id.fileType) as TextView;
				var size = view.FindViewById(Resource.Id.fileSize) as TextView;
				var thumnail = view.FindViewById(Resource.Id.list_image) as ImageView;
				var checkBox = view.FindViewById(Resource.Id.check) as CheckBox;
				name.SetText(item.Name,TextView.BufferType.Normal);
				type.SetText(item.TypeAsString,TextView.BufferType.Normal);
				size.SetText(item.Size.ToString() + " kB",TextView.BufferType.Normal);
				
				checkBox.Checked = item.Checked;
				checkBox.SetOnClickListener(this);
				checkBox.Tag = position.ToString();
				//Assign this item's values to the various subviews
				int drawableId;
				switch(item.FileType){
					case MonoBrick.FileType.Datalog:
						drawableId = Resource.Drawable.ic_action_stock;
					break;
					case MonoBrick.FileType.Firmware:
						drawableId = Resource.Drawable.ic_action_machine;
					break;
					case MonoBrick.FileType.Graphics:
						drawableId = Resource.Drawable.picture;
					break;
					case MonoBrick.FileType.OnBrickProgram:
						drawableId = Resource.Drawable.ic_action_file;
					break;
					case MonoBrick.FileType.Program:
						drawableId = Resource.Drawable.ic_action_work;
					break;
					case MonoBrick.FileType.Sound:
						drawableId = Resource.Drawable.volume_on;
					break;
					case MonoBrick.FileType.TryMeProgram:
						drawableId = Resource.Drawable.ic_action_bulb;
					break;
					case MonoBrick.FileType.Unknown:
						drawableId = Resource.Drawable.help;
					break;
					default:
        				drawableId = Resource.Drawable.help;
        			break;
				}
				thumnail.SetBackgroundResource(drawableId);
				return view;
			}
			
			public void OnClick(Android.Views.View v){
				CheckBox checkBox =(CheckBox) v.FindViewById<CheckBox>(Resource.Id.check);
				int position = int.Parse((string) v.Tag);
				Items[position].Checked = checkBox.Checked;
				if(checkBox.Checked){
					SelectedItems.Add(Items[position]);
				}
				else{
					SelectedItems.Remove(Items[position]);
				}
				ItemCheckedChanged(Items[position], checkBox.Checked);

			}
	
			public FileItem GetItemAtPosition(int position)
			{
				return Items[position];
			}
			
			public FileItem[] GetSelectedItems ()
			{
				List<FileItem> selectedList = new List<FileItem>();
				foreach(FileItem item in Items){
					if(item.Checked)
						selectedList.Add(item);
				}
				return selectedList.ToArray();
			}
			
		}
		
		
		
		
}

