using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using Gtk;
using Gdk;
using MonoBrick;
using MonoBrick.EV3;
using MonoBrick.NXT;
using Pango;
using ComboBoxExtensions;
using System.Diagnostics;
using QueueHelper;
using System.Configuration;
using PCRemote;

public partial class MainWindow: Gtk.Window
{	
	#region Comunication Thread
	private delegate void NoParamDelegate();
    private Semaphore threadMutex = new Semaphore(1, 1); //Ensure that  only one operation is started at the same time
    //private event NoParamDelegate ThreadNotStarted; // This event is signaled if a background thread couldn't be started because another one is runniung
	private void SpawnThread(NoParamDelegate action, bool showPendingError = true, int wait = 500)
	{
        Thread t = new Thread(
            new ThreadStart(
                delegate()
                {
                    if (threadMutex.WaitOne(wait))
                    {
                        try
                        {	
							Gtk.Application.Invoke (delegate {
              					replyStatusbar.Pop(1);
								ShowConnectIcon();
							});
							action();
                        }
						catch (MonoBrickException e){
							Gtk.Application.Invoke (delegate {
              					if(e is MonoBrickException)
									ShowWarningIcon();	
								else
									ShowErrorIcon();
								replyStatusbar.Pop(1);
								replyStatusbar.Push(1,e.Message);
						 	});
							if(e is ConnectionException)
								brick.Connection.Close();
						}
						catch(Exception e){
							Gtk.Application.Invoke (delegate {
              					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\n" + e.Message);
            					md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
								md.Run ();
								md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
            					md.Destroy();
       					 	});
						
						}
                        finally
                        {
                            threadMutex.Release();
                        }
                    }
                    else
                    {
                        if(showPendingError){
							Gtk.Application.Invoke (delegate {
	              					MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\nUnable to send command to NXT.\nAnother command is pending");
	            					md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
									md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
									md.Run ();
	            					md.Destroy();
	       					});
						}
                    }
                }
                ));
        t.IsBackground = true;
		t.Priority = ThreadPriority.AboveNormal;
        t.Start();
    }
 	#endregion
	
	#region Icons
	public static string MessageDialogIconName = "PCRemote.mono_logo.gif";
	private Pixbuf pixWarning = null;
	private Pixbuf pixConnect = null;
	private Pixbuf pixError = null;
	private Pixbuf pixDisconnect = null;
	private void ShowStatusImage(string icon, Pixbuf buf){
		if(buf == null){
			Gtk.Widget widget = (Gtk.Widget) this;
    		buf = widget.RenderIcon(icon, Gtk.IconSize.LargeToolbar, null);
		}
		//if(statusImage.Pixbuf != buf)
			statusImage.Pixbuf = buf;
	}
	
	private void ShowConnectIcon(){
		ShowStatusImage("gtk-apply", pixConnect);
	}
	
	private void ShowDisconnectIcon(){
		ShowStatusImage("gtk-disconnect", pixDisconnect);
	}
	
	private void ShowWarningIcon(){
		ShowStatusImage("gtk-dialog-warning", pixWarning);
	}
	
	private void ShowErrorIcon(){
		ShowStatusImage("gtk-dialog-error", pixError);
	}
	#endregion

	#region On close/open connection 
	private void EnableUserInput(bool set){
		connectButton.Sensitive = !set;
		disconnectButton.Sensitive = set;
		deviceInfoFrame.Sensitive = set;
		brickNameFrame.Sensitive = set;
		sensorContainer.Sensitive = set;
		motorContainer.Sensitive = set;
		filesContainer.Sensitive = set;
		keyboardContainer.Sensitive = set;
		tunnelContainer.Sensitive = set;
		mailboxContainer.Sensitive = set;
		//videoDrawingarea.Sensitive = set;
	}
	
	private void Connected(){
		initializeSensorsOnchange = true;
		Gtk.Application.Invoke (delegate {
			DisableConnectionSelection();
			EnableUserInput(true);
		    ShowConnectIcon();
		});
		OnConnectedKeyboardPage();
		OnConnectedTunnelPage();
	}

	private void Disconnected(){
		initializeSensorsOnchange = false;
		OnDisconnectedKeyboardPage();
		OnDisconnectedTunnelPage();
		pollTimer.Enabled = false;
		startPollTimer = false;
		if(sensorLog.IsOpen){
			sensorLog.Close();
		}
		Gtk.Application.Invoke (delegate {
			SetSensorSelection(true);
			SetPollSensorUi(true,"Poll",true,true);
		});
		DisableMotorTachoTimers();
		isFileListLoaded = false;
		Gtk.Application.Invoke (delegate {
			EnableConnectionSelection();
			EnableUserInput(false);
			showBrickNameLabel.Text = "";
			bluetoothAddressLabel.Text = "";
			firmwareLabel.Text = "";
			freeFlashLabel.Text = "";
			batteryProgressbar.Fraction = 0.0;
			batteryProgressbar.Text = "0 mV";
			sendStatusbar.Pop(1);
			sendStatusbar.Push(1,"Disconnected");

		});
	}
	#endregion
	
	#region On Send/Recieve data
	private void OnCommandSend(BrickCommand command){
		Gtk.Application.Invoke (delegate {
        	sendStatusbar.Pop(1);
			if(brickType == BrickType.NXT){
				sendStatusbar.Push(1,((MonoBrick.NXT.Command)command).CommandByteAsString);
			}
		});	
	}
	private void OnReplyRecieved(BrickReply reply){
		if(brickType == BrickType.NXT){

			if(!((MonoBrick.NXT.Reply)reply).HasError){
				Gtk.Application.Invoke (delegate {
					replyStatusbar.Pop(1);
						replyStatusbar.Push(1,"Received " + reply.Length + " bytes");
				});
			}
		}
	}
	#endregion

	private MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor> brick = null;
	private RemoteSettings settings = new RemoteSettings();

	private bool isConnectedViaUSB(){
		if (brickType == BrickType.NXT) {
			return brick.Connection is MonoBrick.USB<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>;
		}
		return false;
	}

	private bool isConnectedViaNetwork(){
		if (brickType == BrickType.NXT) {
			return brick.Connection is MonoBrick.TunnelConnection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>;
		}
		return false;
	}


	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		USBRadiobutton.Clicked += OnUSBSelected;
		bluetoothRadiobutton.Clicked += OnBluetoothSelected;
		networkRadiobutton.Clicked += OnNetworkSelected;
		
		//Populate comport combo box
		comportCombobox.Populate(Bluetooth<BrickCommand,BrickReply>.GetPortNames());
		
		//Populate sensor combo boxes
		sensor1Combobox.Populate(SensorList.EntryList);
		sensor2Combobox.Populate(SensorList.EntryList);
		sensor3Combobox.Populate(SensorList.EntryList);
		sensor4Combobox.Populate(SensorList.EntryList);

		//Populate tunnel network caching combobox
		networkCachingCombobox.Populate (NetworkCachingOption.EntryList);
				
		//Populate poll combo boxes
		pollOptionsCombobox.Populate(PollOption.EntryList);
		
		//Populate motor selections in vehicle
		leftMotorCombobox.Populate(MotorOption.EntryList);
		rightMotorCombobox.Populate(MotorOption.EntryList);
		motor3Combobox.Populate(MotorOption.EntryList);

		//Populate mailbox combo boxes
		outboxMailboxCombobox.Populate(MailboxOption.EntryList);
		inboxMailboxCombobox.Populate(MailboxOption.EntryList);

		//populate message format combo boxes
		outboxFormatCombobox.Populate(FormatOption.EntryList);
		inboxFormatCombobox.Populate(FormatOption.EntryList);

		//populate message settings combo boxes
		outboxMessageSettingsCombobox.Populate(MessageSettingOption.EntryList);


		//Add function to the poll timer
		pollTimer.Elapsed += delegate {
			UpdateSensorValues();
		};
		//Add function to tacho timers
		motorATachoTimer.Elapsed += OnUpdateTachoMotorA;
		motorATachoTimer.Enabled = false;
		motorBTachoTimer.Elapsed += OnUpdateTachoMotorB;
		motorBTachoTimer.Enabled = false;
		motorCTachoTimer.Elapsed += OnUpdateTachoMotorC;
		motorCTachoTimer.Enabled = false;

		
		//populate the file node view
		FileNodeView.AppendColumn("File Name".PadRight(MonoBrick.NXT.FilSystem.MaxFileNameLength+4), new Gtk.CellRendererText (), "text", 0);
		FileNodeView.AppendColumn("Type".PadRight(10), new Gtk.CellRendererText (), "text", 1);
		FileNodeView.AppendColumn("Size".PadRight(10), new Gtk.CellRendererText (), "text", 2);
		FileNodeView.NodeStore = fileNodeStore;

		//Key Command list setup
		commandNodeView.AppendColumn("Command".PadRight(20), new Gtk.CellRendererText (), "text", 0);
		commandNodeView.AppendColumn("Key".PadRight(8), new Gtk.CellRendererText (), "text", 1);
		commandNodeView.NodeStore = keyNodeStore;

		//start settings
		Disconnected();
		ShowDisconnectIcon();
		Pages.CurrentPage = 0;
		Pages.ShowBorder = false;
		
		//add contens to the sensor log file chooser
		ShowLogFileButton();

		//add contens to the download file chooser
		ShowDownloadFileButton();

		//load settings
		LoadBrickPageSettings();
		LoadMailboxPageSettings();
		LoadSensorPageSettings();
		LoadMotorPageSettings();
		LoadKeyboardPageSettings();
		LoadVLCSettings();

		//disable tunnel page
		//tunnelContainer.Visible = false;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		if(brick != null)
			brick.Connection.Close();
		Application.Quit();
		a.RetVal = true;
	}

	private bool informOnPoll = true;
	private bool startPollTimer = false;

	//This must be in the order that the pages appear
	private enum Page{Brick = 0, Sensor = 1, Motor = 2, File = 3, Mailbox = 4, Keyboard = 5, Tunnel = 6}
	private Page lastPage = Page.Brick;
	protected void OnPagesSwitchPage (object o, Gtk.SwitchPageArgs args)
	{
		if(Pages.CurrentPage == (int)Page.Brick){
			lastPage = Page.Brick;
		}

		if(Pages.CurrentPage != (int)Page.Brick){

		}

		if(Pages.CurrentPage == (int)Page.Sensor){//Sensor page
			lastPage = Page.Sensor;
			if(brick != null){
				if(sensorLogCheckbutton.Active && pollTimer.Enabled){//log for file
					startPollTimer = false;
					Gtk.Application.Invoke (delegate {
							SetSensorSelection(false);
							SetPollSensorUi(false,"Stop",true,false);
					});
				}
				else{
					if(!startPollTimer && !enableKeyboardInput.Active){
						pollTimer.Enabled = false;
						startPollTimer = false;
						Gtk.Application.Invoke (delegate {
							SetSensorSelection(true);
							SetPollSensorUi(true,"Poll",true,true);
						});
					}

					if(startPollTimer && !enableKeyboardInput.Active){
						pollTimer.Enabled = true;
						startPollTimer = false;
						Gtk.Application.Invoke (delegate {
							SetSensorSelection(false);
							SetPollSensorUi(false,"Stop",true,true);
						});
					}

					if(startPollTimer && enableKeyboardInput.Active){//ignore that poll sensor was set
						pollTimer.Enabled = false;
						startPollTimer = false;
						Gtk.Application.Invoke (delegate {
							SetSensorSelection(true);
							SetPollSensorUi(false,"Poll",false,false);
						});

					}

					if(!startPollTimer && enableKeyboardInput.Active){
						pollTimer.Enabled = false;
						startPollTimer = false;
						Gtk.Application.Invoke (delegate {
							SetSensorSelection(true);
							SetPollSensorUi(false,"Poll",false,false);
						});
					}
				}
			}
		}

		if(Pages.CurrentPage != (int)Page.Sensor){//not sensor page
			if(pollTimer.Enabled && informOnPoll && sensorLogCheckbutton.Active){
				Gtk.Application.Invoke (delegate {
              				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease note that communicating with the NXT while polling and saving sensor values might interfere with sensor readings. Certain options has been disabled");
            				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run ();
            				md.Destroy();
       			});
				informOnPoll = false;
			}
			if(pollTimer.Enabled && !sensorLogCheckbutton.Active){
				pollTimer.Enabled = false;
				startPollTimer = true;
			}
		}
		
		if(Pages.CurrentPage == (int)Page.Motor){//Motor page
			lastPage = Page.Motor;
			if(pollTimer.Enabled && sensorLogCheckbutton.Active || enableKeyboardInput.Active){//logging sensor values and saving to file or keyboad to move vehicle
				DisableTachoUserInput();
			}
			else{
				if(brick != null && brick.Connection.IsConnected){
					EnableMotorTachoTimers();
				}
			}
		}
		if(Pages.CurrentPage != (int)Page.Motor){//Not motor page
			EnableTachoUserInput();
			DisableMotorTachoTimers();
		}

		if(Pages.CurrentPage ==(int) Page.File){//File page
			lastPage = Page.File;
			if(pollTimer.Enabled && sensorLogCheckbutton.Active){
				DisableFileUserInput();
			}
			else{
				EnableFileUserInput();
				if(brick != null && brick.Connection.IsConnected){
					if(!isFileListLoaded){
						LoadFileList();
					}
				}
			}
		}

		if(Pages.CurrentPage != (int)Page.File){//Not file page
			//fileNodeStore.Clear();
			if(lastPage == Page.File){
				//no settings to save
			}
		}

		if(Pages.CurrentPage == (int)Page.Keyboard){//keyboard page
			lastPage = Page.Keyboard;
			if(pollTimer.Enabled && sensorLogCheckbutton.Active){
				DisableVehicleUserInput();
			}
			else{
				if(!enableKeyboardInput.Active){
					EnableVehicleUserInput();
				}
			}
		
		}

		if(Pages.CurrentPage != (int)Page.Keyboard){//Not keyboard page

		}

		if(Pages.CurrentPage == (int)Page.Mailbox){//Mailbox page
			if(brick != null && brick.Connection.IsConnected){
				EnableReadMessage (! isConnectedViaUSB());
			}
		}
		
		if(Pages.CurrentPage != (int)Page.Mailbox){//Not Mailbox page
			
		}

		if(Pages.CurrentPage == (int)Page.Tunnel){//Tunnel page

		}
		
		if(Pages.CurrentPage != (int)Page.Tunnel){//Not Tunnel page
			
		}

	}
}
