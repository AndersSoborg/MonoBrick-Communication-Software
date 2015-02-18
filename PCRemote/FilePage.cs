using System;
using MonoBrick;
using MonoBrick.NXT;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;
using ComboBoxExtensions;
public partial class MainWindow
{
	#region File list store
	[TreeNode (ListOnly=true)]
    public class FileNode : Gtk.TreeNode {
		BrickFile file;
		string type;

		public FileNode (MonoBrick.NXT.BrickFile file)
        {
			this.file = file;
			switch(file.FileType){
				case FileType.Datalog:
					type = "Datalog";
				break;
				case FileType.Firmware:
					type = "Firmware";
				break;
				case FileType.Graphics:
					type = "Graphics";
				break;
				case FileType.OnBrickProgram:
					type = "On-Brick";
				break;
				case FileType.Program:
					type = "Program";
				break;
				case FileType.Sound:
					type = "Sound";
				break;
				case FileType.TryMeProgram:
					type = "Try-Me";
				break;
				case FileType.Unknown:
					type = "Unknown";
				break;
				default:
					type = "Unknown";
				break;
			}
        }
        [Gtk.TreeNodeValue (Column=0)]
		public string FileName{get{return file.Name;}}
        [Gtk.TreeNodeValue (Column=1)]
        public string Extension {get { return type; } }
		[Gtk.TreeNodeValue (Column=2)]
        public uint Size{get{return file.Size;}}
		public FileType FileType{get{return file.FileType;}}
	}
	#endregion

	private NodeStore fileNodeStore = new NodeStore (typeof (FileNode));

	private void DisableFileUserInput(){
		SetFileUserInput(false);
	}

	private void EnableFileUserInput(){
		SetFileUserInput(true);
	}

	private bool isFileListLoaded = false;

	private void SetFileUserInput(bool set){
		Gtk.Application.Invoke (delegate {
			FileNodeView.Sensitive = set;
			uploadFileChooserButton.Sensitive = set;
			uploadFileButton.Sensitive = set;
			downloadFolderButton.Sensitive = set;
			downloadFileButton.Sensitive = set;
			stopFileButton.Sensitive = set;
			startFileButton.Sensitive = set;
			deleteFileButton.Sensitive = set;
			formatButton.Sensitive = set;
			refreshfilelistButton.Sensitive = set;
		});
		
	}

	#region download FileButton
	private Gtk.Image downloadButImage = new Gtk.Image(Stock.Directory, IconSize.Button);
	private Label downloadButLabel = new Label("Folder...");
	private void ShowDownloadFileButton(){
		HBox box = new HBox();
        box.PackStart(downloadButImage , true, true, 0);
        box.PackStart(downloadButLabel, true, true, 0);
        downloadFolderButton.Add(box);
		downloadFolderButton.ShowAll();	
	}
	#endregion

	private string downloadFolder = "";
	protected void OnDownloadFolderClicked (object sender, System.EventArgs e)
	{
		Gtk.Application.Invoke (delegate {
			Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog("Save log file",
			                            this,
			                            FileChooserAction.SelectFolder,
			                            Gtk.Stock.Cancel,ResponseType.Cancel,
			                            Gtk.Stock.Ok,ResponseType.Accept);
			if (fc.Run() == (int)ResponseType.Accept) 
			{
					downloadButImage.Stock = Stock.Harddisk;
					downloadButLabel.Text = fc.CurrentFolder;
					downloadFolder = fc.CurrentFolder;
					if(downloadButLabel.Text.Length > 10){
						downloadButLabel.Text = downloadButLabel.Text.Remove(10) + "...";		
					}
					//sensorLogFileName = fc.Filename;
					//sensorLogCheckbutton.Active = true;
			}
			fc.Destroy();
		});
	}

	protected void OnDownloadFileButtonPressed(object sender, System.EventArgs e){
		FileNode selectedFileNode = (FileNode)FileNodeView.NodeSelection.SelectedNode;
		if(selectedFileNode == null){
			Gtk.Application.Invoke (delegate {
             	MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select a file to download");
            	md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
				md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
				md.Run ();
            	md.Destroy();
       		});
			return;
		}
		if(downloadFolder == ""){
			Gtk.Application.Invoke (delegate {
             	MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select a folder");
            	md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
				md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
				md.Run ();
            	md.Destroy();
       		});
			return;
		}
		SpawnThread(delegate()
        {
			Gtk.ProgressBar progressBar = new Gtk.ProgressBar();
			progressBar.WidthRequest = 170;
			progressBar.PulseStep = 0.1;
			progressBar.Fraction = 0;
			progressBar.Text = (0).ToString()  + "%";
			Label label = new Label("Downloading file...");
			//label.HeightRequest = 10;
			Dialog dialog = new Dialog ("Progress", this, Gtk.DialogFlags.DestroyWithParent);
			dialog.TypeHint =  WindowTypeHint.Splashscreen;
			dialog.Modal = true;
	        dialog.VBox.Add (label);
			dialog.VBox.Add (progressBar);
			dialog.HasSeparator = false;
			//dialog.AddButton ("Cancel", ResponseType.Close);
			//dialog.Response += delegate(object obj, ResponseArgs args){ dialog.Destroy();};
			Gtk.Application.Invoke (delegate {
				dialog.ShowAll();
			});
			int totalBytes = 0;
			double target = (double) selectedFileNode.Size;
			System.Action<int> onBytesRead = delegate(int bytesRead){
				Gtk.Application.Invoke (delegate {
					totalBytes += bytesRead;
					//label.Text = "Downloading file...\n" + totalBytes + " of " + selectedFileNode.Size.ToString() + " bytes";
					progressBar.Fraction = ((double)totalBytes/target);
					progressBar.Text = ((int) (progressBar.Fraction*100)).ToString()  + "%";
				});
			};
			brick.FileSystem.OnBytesRead += onBytesRead;
			try{
				brick.FileSystem.DownloadFile(downloadFolder+"\\"+selectedFileNode.FileName,selectedFileNode.FileName);
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
				});
			}
			catch(Exception exception){
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\nFailed to download file\nError: " + exception.Message);
            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
            		md.Destroy();
				});
				if(brick != null)
					brick.FileSystem.OnBytesRead -= onBytesRead;
				if(exception is MonoBrickException)
					throw(exception);
				return;
			}
			brick.FileSystem.OnBytesRead -= onBytesRead;
			Gtk.Application.Invoke (delegate {
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "\nFile downloaded successfully");
            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
            		md.Destroy();
			});
		});
	}

	protected void OnUploadFileButtonPressed(object sender, System.EventArgs e){
		if(uploadFileChooserButton.Filename == null){
			Gtk.Application.Invoke (delegate {
             	MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select a file to upload");
            	md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
				md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
				md.Run ();
            	md.Destroy();
       		});
			return;
		}
		SpawnThread(delegate()
        {
			Gtk.ProgressBar progressBar = new Gtk.ProgressBar();
			progressBar.WidthRequest = 170;
			progressBar.PulseStep = 0.1;
			progressBar.Fraction = 0;
			progressBar.Text = (0).ToString()  + "%";
			Label label = new Label("Uploading file...");
			//label.HeightRequest = 10;
			Dialog dialog = new Dialog ("Progress", this, Gtk.DialogFlags.DestroyWithParent);
			dialog.TypeHint =  WindowTypeHint.Splashscreen;
			dialog.Modal = true;
        	dialog.VBox.Add (label);
			dialog.VBox.Add (progressBar);
			dialog.HasSeparator = false;
			//dialog.AddButton ("Cancel", ResponseType.Close);
			//dialog.Response += delegate(object obj, ResponseArgs args){ dialog.Destroy();};
			Gtk.Application.Invoke (delegate {
				dialog.ShowAll();
			});
			int totalBytes = 0;
			double target = (double)  new  System.IO.FileInfo(uploadFileChooserButton.Filename).Length;
			//Console.WriteLine(target);
			System.Action<int> onBytesWritten = delegate(int bytesWritten){
				Gtk.Application.Invoke (delegate {	
					totalBytes += bytesWritten;
					//label.Text = "Downloading file...\n" + totalBytes + " of " + selectedFileNode.Size.ToString() + " bytes";
					progressBar.Fraction = ((double)totalBytes/target);
					progressBar.Text = ((int) (progressBar.Fraction*100)).ToString()  + "%";
				});

			};
			brick.FileSystem.OnBytesWritten += onBytesWritten;
			try{
				brick.FileSystem.UploadFile(uploadFileChooserButton.Filename, System.IO.Path.GetFileName(uploadFileChooserButton.Filename));
				brick.FileSystem.OnBytesWritten -= onBytesWritten;
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "\nFile successfully uploaded.");
            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
            		md.Destroy();
				});
				LoadFileList();
			}
			catch(Exception exception){
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\nFailed to upload file\nError: " + exception.Message);
            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
            		md.Destroy();
				});
				if(brick != null)
					brick.FileSystem.OnBytesWritten -= onBytesWritten;
				if(exception is MonoBrickException)
					throw(exception);
				return;
			}
		});
	}

	private void LoadFileList(){
		SpawnThread(delegate()
    	{
			System.Timers.Timer timer = new System.Timers.Timer(100);
			ProgressBar progress = new ProgressBar();
			//progress.Fraction = 0.0;
			progress.Orientation = ProgressBarOrientation.LeftToRight;
			timer.Elapsed += (obj1,obj2) => {
				progress.Pulse();
			};
			Label label = new Label("Please wait...");
			//label.HeightRequest = 160;
			label.WidthRequest = 200;
			Dialog dialog = new Dialog ("Retrieving file list", this, Gtk.DialogFlags.DestroyWithParent);
			dialog.TypeHint =  WindowTypeHint.Splashscreen;
			dialog.Modal = true;
			dialog.VBox.Add (label);
			dialog.HasSeparator = false;
			dialog.VBox.Add (progress);
			Gtk.Application.Invoke (delegate {
				dialog.ShowAll();
				timer.Start();
			});
			try{
				IBrickFile[] files = brick.FileSystem.FileList();
				Gtk.Application.Invoke (delegate {
					fileNodeStore.Clear();
					if(files != null){
						foreach(MonoBrick.NXT.BrickFile file in files){
							fileNodeStore.AddNode(new FileNode(file));
						}
					}
					EnableFileUserInput();
					isFileListLoaded = true;
				});
			}
			catch(Exception e){
				Gtk.Application.Invoke (delegate {
					timer.Stop();
					dialog.Destroy();
					DisableFileUserInput();
					isFileListLoaded = false;
					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nFailed to get file list");
            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
            		md.Destroy();
       			});
				throw(e);
			}
			Gtk.Application.Invoke (delegate {
				timer.Stop();
				dialog.Destroy();
			});
		});
	}
	/*private FileNode selectedFileNode;
	private void OnFileSelectionChanged (object o, System.EventArgs args)
    {
     	Gtk.NodeSelection selection = (Gtk.NodeSelection) o;
       	selectedFileNode = (FileNode) selection.SelectedNode;
    }*/

	protected void OnDeleteFileButtonPressed(object sender, System.EventArgs e){
		FileNode selectedFileNode = (FileNode)FileNodeView.NodeSelection.SelectedNode;
		if(selectedFileNode == null){
			Gtk.Application.Invoke (delegate {
             	MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select a file to delete");
				md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
				md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
				md.Run ();
            	md.Destroy();
       		});
			return;
		}
		ResponseType response = ResponseType.Ok;
		Gtk.Application.Invoke (delegate {
			MessageDialog dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "\nAre you sure you want to delete " + selectedFileNode.FileName);
        	dialog.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
			dialog.WindowPosition = Gtk.WindowPosition.CenterOnParent;
			response = (ResponseType) dialog.Run ();
        	dialog.Destroy();
			if(response == ResponseType.Yes){
				SpawnThread(delegate()
	    		{
					brick.FileSystem.DeleteFile(selectedFileNode.FileName);
					LoadFileList();
				});
			}
		});
		return;
	}

	protected void OnFormatButtonPressed(object sender, System.EventArgs e){
		ResponseType response = ResponseType.Ok;
		Gtk.Application.Invoke (delegate {
			MessageDialog dialog = new MessageDialog (this, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "\nAre you sure you want to delete the NXT flash.\n All files on the NXT will be lost");
        	dialog.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
			response = (ResponseType) dialog.Run ();
        	dialog.Destroy();
			if(response == ResponseType.Yes){
				SpawnThread(delegate()
	    		{
						brick.FileSystem.DeleteFlash();
						LoadFileList();
				});
			}
		});
		return;
	}

	protected void OnStopFileButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
    	{
			brick.StopProgram(true);
			//update running program
		});
	}

	protected void OnStartFileButtonPressed(object sender, System.EventArgs e){
		FileNode selectedFileNode = (FileNode)FileNodeView.NodeSelection.SelectedNode;
		SpawnThread(delegate()
    	{
			if(selectedFileNode == null){
				Gtk.Application.Invoke (delegate {
	             	MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select a file");
	            	md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
	            	md.Destroy();
       			});
				return;
			}
			switch(selectedFileNode.FileType){
				case FileType.Program:
					brick.StartProgram(selectedFileNode.FileName,true);
					//update running program
				break;
				case FileType.Sound:
					brick.PlaySoundFile(selectedFileNode.FileName,false,true);
				break;
				default:
					Gtk.Application.Invoke (delegate {
	             		MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nFile-type can not be started");
	            		md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
						md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
						md.Run ();
	            		md.Destroy();
       				});
				break;
			}
		});
	}

	protected void OnRefreshfilelistButtonPressed(object sender, System.EventArgs e){
		LoadFileList();
		//update running program
	}







}

