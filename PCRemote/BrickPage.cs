using System;
using MonoBrick;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;
using ComboBoxExtensions;
using System.Configuration;
using Utilities;
using PCRemote;
using MonoBrick.NXT;
public partial class MainWindow
{
	#region Connection buttons
	private void OnUSBSelected(object obj, EventArgs args)
    {
		comportCombobox.Sensitive = false;
		portSpinbutton.Sensitive = false;
		ipEntry.Sensitive = false;
		waitForConnectioncheckbutton.Sensitive = false;
		settings.ConnectionType = "usb";
		settings.Save();

	}
	
	private void OnBluetoothSelected(object obj, EventArgs args)
    {
		comportCombobox.Sensitive = true;
		portSpinbutton.Sensitive = false;
		ipEntry.Sensitive = false;
		waitForConnectioncheckbutton.Sensitive = false;
		settings.ConnectionType = "bluetooth";
		settings.Save();

	}

	private void OnNetworkSelected(object obj, EventArgs args)
    {
		comportCombobox.Sensitive = false;
		portSpinbutton.Sensitive = true;
		ipEntry.Sensitive = true;
		waitForConnectioncheckbutton.Sensitive = true;
		settings.ConnectionType = "network";
		ipEntry.Sensitive = !waitForConnectioncheckbutton.Active;
		settings.Save();

	}
	
	private void DisableConnectionSelection(){
		USBRadiobutton.Sensitive = false;
		portSpinbutton.Sensitive = false;
		ipEntry.Sensitive = false;
		comportCombobox.Sensitive = false;
		bluetoothRadiobutton.Sensitive = false;
		networkRadiobutton.Sensitive = false;
		waitForConnectioncheckbutton.Sensitive = false;
	}
	
	private void EnableConnectionSelection(){
		if(networkRadiobutton.Active){
			OnNetworkSelected(null,null);
		}
		if(USBRadiobutton.Active){
			OnUSBSelected(null,null);
		}
		if(bluetoothRadiobutton.Active){
			OnBluetoothSelected(null,null);
		}
		bluetoothRadiobutton.Sensitive = true;
		USBRadiobutton.Sensitive = true;
		networkRadiobutton.Sensitive = true;

	}
	
	#endregion 

	#region Update brick information
	private void UpdateDeviceInfo(DeviceInfo info){
		bluetoothAddressLabel.Text = info.BluetoothAddress;
		showBrickNameLabel.Text = info.BrickName;
		freeFlashLabel.Text = info.FreeFlashMemory.ToString() + " bytes";
	}
	
	private void UpdateFirmwareInfo(DeviceFirmware firmware){
			firmwareLabel.Text = firmware.FirmwareVersion;
	}
	
	private void UpdateBatteryInfo(UInt16 level){
		double fraction = ((double)level)/((double)9000.0);
		if(fraction > 9000.0)
			fraction = 9000.0;
		batteryProgressbar.Fraction = fraction;
		batteryProgressbar.Text = level.ToString() + " mV";
	}
	#endregion
	private MonoBrick.BrickType brickType = BrickType.NXT;
	delegate void SetSensor(Sensor sensor);
	protected void OnConnectButtonClicked (object sender, System.EventArgs e)
	{
			SpawnThread(delegate()
            {
            	if(USBRadiobutton.Active){
					brick = new MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor>(new USB<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>());
				}
				if(bluetoothRadiobutton.Active){
				brick = new MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor,Sensor,Sensor,Sensor>(new Bluetooth<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>(comportCombobox.ActiveText));
				}
				if(networkRadiobutton.Active){
					if(waitForConnectioncheckbutton.Active){
						brick = new MonoBrick.NXT.Brick<Sensor,Sensor,Sensor,Sensor>(new TunnelConnection<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>(ushort.Parse(portSpinbutton.Text)));
						Label networkLabel = new Label();
						//label.HeightRequest = 160;
						networkLabel.WidthRequest = 300;
						string[] addresses = NetworkHelper.GetLocalIPAddress();
							for(int i = 0; i < addresses.Length; i++){
							networkLabel.Text = networkLabel.Text + "Local IP address: " + addresses[i].PadRight(80)+"\n";
						}
						networkLabel.Text = networkLabel.Text+ "External IP Address: ";
						Dialog networkDialog = new Dialog ("Wait for connection", this, Gtk.DialogFlags.DestroyWithParent);
						networkDialog.TypeHint =  WindowTypeHint.Splashscreen;
						networkDialog.BorderWidth = 4;
						networkDialog.Modal = true;
				        networkDialog.VBox.Add (networkLabel);
						//dialog.VBox.Add (progressBar);
						networkDialog.HasSeparator = false;
						networkDialog.AddButton ("Cancel", ResponseType.Close);
						networkDialog.Response += delegate(object obj, ResponseArgs args){ 
							brick.Connection.Close();
						};
						Gtk.Application.Invoke (delegate {
							networkDialog.ShowAll();
						});
						networkLabel.Text = networkLabel.Text + Utilities.NetworkHelper.GetExternalIP();
						//MonoBrick.ConnectionDelegate onNetworkConnection = delegate(){};
						//brick.Connection.Connected += onNetworkConnection;
						try{
							brick.Connection.Open();//This will block and wait for a network connection
						}
						catch(Exception exception){
							//brick.Connection.Connected -= onNetworkConnection;
							Gtk.Application.Invoke (delegate {
								networkDialog.Destroy();
							});
							throw(exception);
						}
						//brick.Connection.Connected -= onNetworkConnection;
						Gtk.Application.Invoke (delegate {
							networkDialog.Destroy();
						});	
						
					}
					else{
						brick = new MonoBrick.NXT.Brick<Sensor,Sensor,Sensor,Sensor>(new TunnelConnection<Command,Reply>(ipEntry.Text, ushort.Parse(portSpinbutton.Text)));
					}
				}
				System.Timers.Timer timer = new System.Timers.Timer(100);
				ProgressBar progress = new ProgressBar();
				//progress.Fraction = 0.0;
				progress.Orientation = ProgressBarOrientation.LeftToRight;
				timer.Elapsed += (obj1,obj2) => {
					progress.Pulse();
				};
				Label connectLabel = new Label("Opening connection...");
				//label.HeightRequest = 160;
				connectLabel.WidthRequest = 200;
				Dialog connectDialog = new Dialog ("Connecting to the NXT", this, Gtk.DialogFlags.DestroyWithParent);
				connectDialog.TypeHint =  WindowTypeHint.Splashscreen;
				connectDialog.Modal = true;
				connectDialog.VBox.Add (connectLabel);
				connectDialog.HasSeparator = false;
				connectDialog.VBox.Add (progress);
				Gtk.Application.Invoke (delegate {
						connectDialog.ShowAll();
						timer.Start();
				});
				if(!brick.Connection.IsConnected){
					try{
						brick.Connection.Open();
					}
					catch(Exception ex){
						Gtk.Application.Invoke (delegate {
							connectDialog.Destroy();
							timer.Stop();
						});
						throw ex;
					}
				}
				brick.Connection.CommandSend += OnCommandSend;
				brick.Connection.ReplyReceived += OnReplyRecieved;
				//brick.ConnectionOpened += OnConnected;
				brick.Connection.Disconnected += Disconnected;
				try{
					Gtk.Application.Invoke (delegate {
						connectLabel.Text = "Retrieving brick information...";
					});
					
					DeviceInfo info = brick.GetDeviceInfo();
					DeviceFirmware firmware = brick.GetDeviceFirmware();
					ushort batteryLevel = brick.GetBatteryLevel();
					SetSensor[] setSensor = new SetSensor[]{
						delegate(Sensor sensor){brick.Sensor1 = sensor; brick.Sensor1.Initialize();},
						delegate(Sensor sensor){brick.Sensor2 = sensor; brick.Sensor2.Initialize();},
						delegate(Sensor sensor){brick.Sensor3 = sensor; brick.Sensor3.Initialize();},
						delegate(Sensor sensor){brick.Sensor4 = sensor; brick.Sensor4.Initialize();},
					};
					ComboBox[] comboArray = new ComboBox[]{sensor1Combobox, sensor2Combobox,sensor3Combobox, sensor4Combobox};
					//Check if sensors should be initialized with something other than none
					for(int i = 0; i < 4; i++){ 
						Console.WriteLine(i);
						Gtk.Application.Invoke (delegate {
							connectLabel.Text = "Setting up sensor " + (i+1);
						});
						if(comboArray[i].Active != 0){
							try{
								setSensor[i]((Sensor) comboArray[i].GetActiveValue());						
							}
							catch(MonoBrickException nxtEx){
								if(nxtEx is MonoBrickException){
									comboArray[i].Active = 0;//unable to set the sensor value set the comboBox to none
									setSensor[i](new Sensor(SensorType.NoSensor,SensorMode.Raw));
								}
								else
									throw nxtEx;
							}
						}
					}
					Gtk.Application.Invoke (delegate {
						UpdateDeviceInfo(info);
						UpdateFirmwareInfo(firmware);
						UpdateBatteryInfo(batteryLevel);
					});
					Connected();
					DisableAllTunnelInput();
					if(isConnectedViaNetwork()){
						Gtk.Application.Invoke (delegate {
							connectLabel.Text = "Checking tunnel settings...";
						});
						if(brickType == BrickType.NXT){
						var reply = brick.Connection.SendAndReceive(new Command(CommandType.TunnelCommand,CommandByte.GetTunnelCommands,true));
							if(!reply.HasError){
								for(int i = 0; i < reply.Data.Length-3; i++){
									switch(reply.Data[i+3]){
										case (byte)CommandByte.GetTunnelRTSPSettings:
											EnableVideoStreamInput();
											break;
										case (byte)CommandByte.GetTunnelGPSPosition:
											EnableGPSInput();
											break;
										case (byte)CommandByte.TunnelSpeak:
											EnableSpeakInput();
											break;
										}
									}
							}
							else{
								//The tunnel does not support any commands
							}
						}
					}
				}
				catch(MonoBrickException ex){
					Gtk.Application.Invoke (delegate {
						connectDialog.Destroy();
						timer.Stop();
					});
					brick.Connection.Close();
					throw ex;
				}
				Gtk.Application.Invoke (delegate {
					connectDialog.Destroy();
					timer.Stop();
				});
				
				
			});
	}
		
	protected void OnDisconnectButtonClicked (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
            pollTimer.Enabled = false;
			if(sensorLog.IsOpen){
				sensorLog.Close();
			}
			Gtk.Application.Invoke (delegate {
				SetSensorSelection(true);
				SetPollSensorUi(true,"Poll",true,true);
			});
			brick.Connection.Close();
			Gtk.Application.Invoke (delegate {
				ShowDisconnectIcon();
			});
			brick.Connection.CommandSend -= OnCommandSend;
			brick.Connection.ReplyReceived -= OnReplyRecieved;
			//brick.ConnectionOpened -= OnConnected;
			brick.Connection.Disconnected -= Disconnected;
			brick = null;
        });
	}

	protected void OnSetNameButtonClicked (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
       		brick.SetBrickName(setNameEntry.Text,true);
			string newName = brick.GetBrickName();
			Gtk.Application.Invoke (delegate {
      			setNameEntry.Text = "Name...";
				showBrickNameLabel.Text = newName;
			});
        });
	}

	protected void LoadBrickPageSettings(){
		ipEntry.Text = settings.IpAddress;
		comportCombobox.SetActiveValue(settings.Comport,0);
		portSpinbutton.Text = settings.Port;
		waitForConnectioncheckbutton.Active = settings.WaitForConnection;
		switch(settings.ConnectionType.ToLower()){
			case "usb":
				USBRadiobutton.Active = true;
			break;
			case "bluetooth":
				bluetoothRadiobutton.Active = true;
			break;
			case "network":
				networkRadiobutton.Active = true;
			break;
			default:
				USBRadiobutton.Active = true;
			break;
		}
	}

	protected void OnIpEntryChanged (object sender, EventArgs e)
	{
		settings.IpAddress = ipEntry.Text;
		settings.Save();
	}

	protected void OnComportComboboxChanged (object sender, EventArgs e)
	{
		settings.Comport = comportCombobox.ActiveText;
		settings.Save();
	}

	protected void OnPortSpinbuttonChanged (object sender, EventArgs e)
	{
		settings.Port = portSpinbutton.Text;
		settings.Save();
	}

	protected void OnWaitForConnectioncheckbuttonToggled (object sender, EventArgs e)
	{
		Gtk.Application.Invoke (delegate {
			ipEntry.Sensitive = !waitForConnectioncheckbutton.Active;		
			settings.WaitForConnection = waitForConnectioncheckbutton.Active;
		});
		settings.Save();
	}
}

