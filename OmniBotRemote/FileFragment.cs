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
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Threading;
using Android.Speech.Tts;
using MonoBrick;

namespace OmniBotRemote
{
				
		
		
		public class FileListFragment: ListFragment, ActionMode.ICallback{
			
			private FileItemAdapter adapter;
			private IMenuItem menuConnection = null;
			private IMenuItem menuSettings = null;
			private IMenuItem menuRefresh = null;
			private IMenuItem menuUpload = null;
			private IMenuItem menuFormat = null;
			private IMenuItem menuStart = null;
			private IMenuItem menuStop = null;
			//private IMenuItem menuDelete = null;
			private IMenuItem menuDownload = null;
			private BrickController brickController = null;
			private bool isContextMenuShowing = false;
			ActionMode mode = null;
			public override void OnActivityCreated (Bundle savedInstanceState)
			{
				base.OnActivityCreated (savedInstanceState);
				brickController = BrickController.Instance;
				if(adapter == null){
					adapter = new FileItemAdapter(this.Activity);
					adapter.ItemCheckedChanged += OnItemCheckedChanged;
					
				}
				View.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.backrepeat));
				//View.CacheColorHint = Android.Graphics.Color.Transparent;
				this.ListAdapter = adapter;
				SetHasOptionsMenu(true);
				SetEmptyText("Not connected");
				((TextView) ListView.EmptyView).SetTextColor(new Android.Graphics.Color (0x99, 0x99, 0x99));
			}
			
			public override void OnPause ()
			{
				base.OnPause();
				if(mode != null)
					mode.Finish();
			}
			
			public override void OnResume ()
			{
				if(brickController.NXT.Connection.IsConnected){
					ReloadFileList(false);
					SetEmptyText("No files on brick");
				}
				else{
					SetEmptyText("Not connected");
					adapter.Items.Clear ();
				}
				base.OnResume ();
			}
			
			public void ReloadFileList (bool force = true)
			{
				if(brickController.NXT.Connection.IsConnected) {
					if(force || adapter.Items.Count == 0){
						ProgressDialog progress = null;
						this.Activity.RunOnUiThread(delegate() {
							progress = ProgressDialog.Show(this.View.Context,"Synchronizing file list...","PLease Wait...       ");
						});
						System.Threading.Thread t =  new System.Threading.Thread( delegate(object obj){
							adapter.Items.Clear ();
							try{
								MonoBrick.IBrickFile[] fileList = brickController.NXT.FileSystem.FileList();
								for(int i = 0; i < fileList.Length ; i++){
									adapter.Items.Add(new FileItem(fileList[i]));
								}
								//AddFakeList();
								
							}
							catch(Exception e){
								this.Activity.RunOnUiThread(delegate() {
									TabActivity.ShowToast(this.Activity, "Failed to synchronize file list: " + e.Message);
								});
							}
							finally{
								this.Activity.RunOnUiThread(delegate() {
									adapter.NotifyDataSetChanged();
									progress.Dismiss();
								});
							}
						});
						t.IsBackground = true;
						t.Priority = System.Threading.ThreadPriority.Normal;
						t.Start();
					}
					else{
						this.Activity.RunOnUiThread(delegate() {
							adapter.NotifyDataSetChanged();
						});
					}
				}
				
				 
			}
			
			private void AddFakeList(){
				
				MonoBrick.NXT.BrickFile file = new MonoBrick.NXT.BrickFile("Program.rxe",1, 4000);
				FileItem item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Firmware.rfw",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Onbrick.rpg",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("TryMe.rtm",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Sound.rso",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Graphics.ric",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Log.rdt",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Word.doc",1, 4000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				
				file = new MonoBrick.NXT.BrickFile("Firmware2.rfw",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Onbrick2.rpg",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("TryMe2.rtm",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Sound2.rso",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Graphics2.ric",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Log2.rdt",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
				file = new MonoBrick.NXT.BrickFile("Word2.doc",1, 8000);
				item = new FileItem(file);
				adapter.Items.Add(item);
				
			}
			
			public void UploadFile()
			{
				Console.WriteLine("Upload a file");
			}
			
			public void Format ()
			{
				if(!brickController.NXT.Connection.IsConnected)
					return;
				AlertDialog.Builder dialog = new AlertDialog.Builder(this.View.Context);
				dialog.SetIcon(Android.Resource.Drawable.IcMenuInfoDetails);
				dialog.SetTitle("Format filesytem");
				dialog.SetMessage("Are you sure you want to format the filesystem on the brick?");
				dialog.SetPositiveButton("Yes",delegate(object sender, DialogClickEventArgs e){	
					ProgressDialog progress = null;
					this.Activity.RunOnUiThread(delegate() {
						progress = ProgressDialog.Show(this.View.Context,"Formatting filesystem","PLease Wait...       ");
					});
					System.Threading.Thread t =  new System.Threading.Thread( delegate(object obj){
						brickController.ExecuteOnCurrentThread(delegate() {
							Exception ex = null;
							try{
								brickController.NXT.FileSystem.DeleteFlash();
							}
							catch (Exception excep){
								ex = excep;
							}
							finally
							{
								this.Activity.RunOnUiThread(delegate() {
	           						progress.Dismiss();
								});	
								ReloadFileList ();	
							}
							if(ex != null)
       							throw ex;
							
						});
					});
					t.IsBackground = true;
					t.Priority = System.Threading.ThreadPriority.Normal;
					t.Start();
				});
				dialog.SetNegativeButton("No", delegate(object sender, DialogClickEventArgs e){});
				dialog.Show();
			}
			
			public bool OnActionItemClicked (ActionMode mode, IMenuItem item)
			{
				switch (item.ItemId) {
					case Resource.Id.start:
						brickController.SpawnThread( delegate() {
							FileItem currentItemSelected = adapter.SelectedItems.First();
							if(currentItemSelected.FileType == MonoBrick.FileType.Program){
								brickController.NXT.StartProgram(currentItemSelected.Name, true);
								this.Activity.RunOnUiThread(delegate(){
									TabActivity.ShowToast(this.Activity, "Program successfully started");
								});
								
							}
							else{
								brickController.NXT.PlaySoundFile(currentItemSelected.Name,false,true);
								this.Activity.RunOnUiThread(delegate(){
									TabActivity.ShowToast(this.Activity, "Sound file started");
								});
							}
						});
					break;
					case Resource.Id.stop:
						brickController.SpawnThread( delegate() {
							brickController.NXT.StopProgram(true);
							this.Activity.RunOnUiThread(delegate(){
								TabActivity.ShowToast(this.Activity,"All programs stoped");
							});
						});
					break;
					case Resource.Id.delete:
						
						AlertDialog.Builder dialog = new AlertDialog.Builder(this.View.Context);
						dialog.SetIcon(Android.Resource.Drawable.IcMenuInfoDetails);
						dialog.SetTitle("Delete file(s)");
						string message;
						if(adapter.SelectedItems.Count> 1){
							message = "Are you sure you want to delete " + adapter.SelectedItems.Count + " files ?";
						}
						else{
							message = "Are you sure you want to delete " + adapter.SelectedItems.First().Name + " ?";
						}
						dialog.SetMessage(message);
						dialog.SetPositiveButton("Yes",delegate(object sender, DialogClickEventArgs e){	
							ProgressDialog progress = null;
							this.Activity.RunOnUiThread(delegate() {
								progress = ProgressDialog.Show(this.View.Context,"Deleting files","PLease Wait...       ");
							});
							System.Threading.Thread t =  new System.Threading.Thread( delegate(object obj){
								brickController.ExecuteOnCurrentThread(delegate() {
									Exception ex = null;
									List<FileItem> uncheckList = new List<FileItem>();
									try{
										int i = 0;
										foreach(FileItem myFileItem in adapter.SelectedItems){
											if(adapter.SelectedItems.Count > 1){
												progress.SetMessage("Deleteing file " + (i+1) + " of " + adapter.SelectedItems.Count);
											}
											else{
												progress.SetMessage("Deleteing " + myFileItem.Name);
											}
											brickController.NXT.FileSystem.DeleteFile(myFileItem.Name);
											adapter.Items.Remove(myFileItem);
											uncheckList.Add(myFileItem);
											i++;
										}
									}
									catch (Exception excep){
										ex = excep;
									}
									finally
        							{
           								foreach(FileItem fi in uncheckList){
           									adapter.SelectedItems.Remove(fi);
           								}
           								this.Activity.RunOnUiThread(delegate() {
	           								progress.Dismiss();
											adapter.NotifyDataSetChanged();
											if(mode != null){
	           									mode.Finish();
	           								}
	           							});
	           								
       								}
       								if(ex != null)
       									throw ex;
									
								});
							});
							t.IsBackground = true;
							t.Priority = System.Threading.ThreadPriority.Normal;
							t.Start();
						});
						dialog.SetNegativeButton("No", delegate(object sender, DialogClickEventArgs e){});
						dialog.Show();
					break;
					case Resource.Id.download:
						Console.WriteLine("Download");
					break;
				}
			
				return true;
			}
			
			public override void OnPrepareOptionsMenu (IMenu menu)
			{
				
				menuConnection = menu.FindItem (Resource.Id.menuConnection);
				menuSettings = menu.FindItem (Resource.Id.menuSettings);
				menuRefresh = menu.FindItem(Resource.Id.menuRefresh);
				menuUpload = menu.FindItem(Resource.Id.menuUpload);
				menuFormat = menu.FindItem(Resource.Id.menuFormat);
				
				menuConnection.SetVisible(false);
				menuSettings.SetVisible(false);
				menuRefresh.SetVisible(true);
				//menuUpload.SetVisible(true);//upload is not supported
				menuUpload.SetVisible(false);
				menuFormat.SetVisible(true);
				if(adapter.SelectedItems.Count != 0){
				 	foreach(FileItem item in adapter.SelectedItems){
					  	item.Checked = false;				
					}
					adapter.SelectedItems.Clear();
					adapter.NotifyDataSetChanged();
				}
				base.OnPrepareOptionsMenu(menu);
			}
			
			public override void OnDestroyOptionsMenu ()
			{
				menuConnection.SetVisible(true);
				menuSettings.SetVisible(true);
				menuRefresh.SetVisible(false);
				menuUpload.SetVisible(false);
				menuFormat.SetVisible(false);
				base.OnDestroyOptionsMenu ();
				
			}
			
			private void OnItemCheckedChanged(FileItem item, bool isChecked){
				Console.WriteLine(item.Name + " checked " + isChecked);
				Console.WriteLine("Items checked " + adapter.SelectedItems.Count);
				foreach(FileItem fi in adapter.SelectedItems)
					Console.WriteLine(fi.Name);
				if(adapter.SelectedItems.Count == 0){
					if(mode != null)
						mode.Finish();
					return;	
				}
				if(!isContextMenuShowing){
					this.View.StartActionMode(this);	
				}
				menuDownload.SetVisible(false);
				if(adapter.SelectedItems.Count > 1){
					menuStart.SetVisible(false);
					menuStop.SetVisible(false);
				}
				if(adapter.SelectedItems.Count == 1){
					FileItem currentItemSelected = adapter.SelectedItems.First();
					if(currentItemSelected.FileType == MonoBrick.FileType.Program || currentItemSelected.FileType == MonoBrick.FileType.Sound){
						menuStart.SetVisible(true);
						if(currentItemSelected.FileType == MonoBrick.FileType.Program){
							menuStop.SetVisible(true);
						}
						else{
							menuStop.SetVisible(false);
						}
					}
					else{
						menuStart.SetVisible(false);
						menuStop.SetVisible(false);
					}
				}
			
			}
			
			public bool OnCreateActionMode (ActionMode mode, IMenu menu)
			{	
				mode.Title = "File options";
				mode.MenuInflater.Inflate(Resource.Menu.file,menu);
				
				menuStart = menu.FindItem(Resource.Id.start);
				menuStop = menu.FindItem(Resource.Id.stop);
				//menuDelete = menu.FindItem(Resource.Id.delete);
				menuDownload = menu.FindItem(Resource.Id.download);
				isContextMenuShowing = true;
				this.mode = mode; 
				return true;
			}
			
			public void OnDestroyActionMode (ActionMode mode)
			{
				//int size = adapter.SelectedItems.Count;
				foreach(FileItem item in adapter.SelectedItems){
					item.Checked = false;
				}
				adapter.SelectedItems.Clear();
				this.Activity.RunOnUiThread(delegate() {
	           		adapter.NotifyDataSetChanged();
	           	});
				isContextMenuShowing = false;
			}
		
			public bool OnPrepareActionMode (ActionMode mode, IMenu menu)
			{
				return true;
			}
		}
}

