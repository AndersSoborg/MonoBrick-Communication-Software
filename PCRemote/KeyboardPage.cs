using System;
using MonoBrick;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;
using ComboBoxExtensions;
using System.Collections.Generic;
using System.Configuration;
using System.Timers;
using MonoBrick.NXT;
public partial class MainWindow
{
	#region Command list store
	public enum KeyCommand {Forward, Backward, SpinLeft, SpinRight, IncVehicleSpeed, DecVehicleSpeed, 
							IncTurnPercent, DecTurnPercent, IncSpinSpeed, DecSpinSpeed, Motor3Fwd, Motor3Rev, 
							IncMotor3Speed, DecMotor3Speed, ReadSensor1, ReadSensor2, ReadSensor3, ReadSensor4,
							SendMessage1, SendMessage2, SendMessage3, SendMessage4, SendMessage5};

	[TreeNode (ListOnly=true)]
    public class KeyNode : Gtk.TreeNode {
		private string commandString;
		private KeyCommand keyCommand;
		private string KeyCommandToString(KeyCommand command){
			string s = "test";
			switch(command){
				case KeyCommand.Backward:
					s = "Vehicle Backward";
				break;
				case KeyCommand.DecMotor3Speed:
					s = "Dec. motor speed";
				break;
				case KeyCommand.DecTurnPercent:
					s = "Dec. turn-percent";
				break;
			 	case KeyCommand.DecVehicleSpeed:
					s = "Dec. vehicle speed";
				break;
				case KeyCommand.Forward:
					s = "Vehicle forward";
				break;
				case KeyCommand.IncMotor3Speed:
					s = "Inc. motor speed";
				break;
				case KeyCommand.IncTurnPercent:
					s = "Inc. turn-percent";
				break;
				case KeyCommand.IncVehicleSpeed:
					s = "Inc. vehicle speed";
				break;
				case KeyCommand.Motor3Fwd:
					s = "Motor forward";
				break;
				case KeyCommand.Motor3Rev:
					s = "Motor reverse";
				break;
				case KeyCommand.SpinLeft:
					s = "Spin left";
				break;
				case KeyCommand.SpinRight:
					s = "Spin right";
				break;
				case KeyCommand.IncSpinSpeed:
					s = "Inc. spin speed";
				break;
				case KeyCommand.DecSpinSpeed:
					s = "Dec. spin speed";
				break;
				case KeyCommand.ReadSensor1:
					s = "Read sensor 1";
				break;
				case KeyCommand.ReadSensor2:
					s = "Read sensor 2";
				break;
				case KeyCommand.ReadSensor3:
					s = "Read sensor 3";
				break;
				case KeyCommand.ReadSensor4:
					s = "Read sensor 4";
				break;
				case KeyCommand.SendMessage1:
					s = "Send Message 1";
				break;
				case KeyCommand.SendMessage2:
					s = "Send Message 2";
				break;
				case KeyCommand.SendMessage3:
					s = "Send Message 3";
				break;
				case KeyCommand.SendMessage4:
					s = "Send Message 4";
				break;
				case KeyCommand.SendMessage5:
					s = "Send Message 5";
				break;
			}
			return s;
		}

		public KeyNode(KeyCommand command, Gdk.Key key)
        {
			this.Key = key;
			commandString = KeyCommandToString(command);
			this.keyCommand = command;
			Pressed = false;
		}
        [Gtk.TreeNodeValue (Column=0)]
		public string CommandString{get{return commandString;}}
        [Gtk.TreeNodeValue (Column=1)]
        public string KeyString {get { return Key.ToString().ToUpper(); } }
		public KeyCommand KeyCommand {get { return keyCommand; } }
		public Gdk.Key Key{get;set;}
		public bool Pressed{get;set;}
	}
	#endregion

	private void FillWithDefaultKeys(){
		Gtk.Application.Invoke (delegate {
			keyNodeStore.Clear();
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Forward, Gdk.Key.w));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Backward,Gdk.Key.s));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SpinLeft,Gdk.Key.a));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SpinRight,Gdk.Key.d));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncVehicleSpeed,Gdk.Key.r));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecVehicleSpeed,Gdk.Key.e));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncSpinSpeed,Gdk.Key.f));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecSpinSpeed,Gdk.Key.g));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncTurnPercent,Gdk.Key.c));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecTurnPercent,Gdk.Key.v));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Motor3Fwd,Gdk.Key.u));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Motor3Rev,Gdk.Key.j));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncMotor3Speed,Gdk.Key.l));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecMotor3Speed,Gdk.Key.k));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor1,Gdk.Key.Key_1));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor2,Gdk.Key.Key_2));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor3,Gdk.Key.Key_3));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor4,Gdk.Key.Key_4));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage1,Gdk.Key.Key_5));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage2,Gdk.Key.Key_6));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage3,Gdk.Key.Key_7));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage4,Gdk.Key.Key_8));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage5,Gdk.Key.Key_9));
		});
	}

	private NodeStore keyNodeStore = new NodeStore (typeof (KeyNode));
	bool doubleClick = false;
	System.Timers.Timer doubleClickTimer;

	[GLib.ConnectBeforeAttribute]
	protected void OnCommandNodeViewButtonPressEvent (object sender, ButtonPressEventArgs e)
	{
		if (doubleClick) 
        { 
			AssignKey();
			doubleClick = false;
        } 
        else 
        { 
            this.doubleClickTimer.Start(); 
            doubleClick = true; 
        } 
	}

	protected void AssignKey(){
		KeyNode selectedKeyNode = (KeyNode) commandNodeView.NodeSelection.SelectedNode;
		String s = "Select the key to associate with\n\"" + selectedKeyNode.CommandString + "\"\nPress ESC to cancel";
		Gtk.Label label = new Gtk.Label(s);
		label.Justify = Justification.Center;
		label.HeightRequest = 70;
		Dialog dialog = new Dialog ("Assign a key", this, Gtk.DialogFlags.DestroyWithParent);
		dialog.TypeHint =  WindowTypeHint.Splashscreen;
		dialog.Modal = true;
		dialog.VBox.Add (label);
		dialog.HasSeparator = false;
		dialog.KeyPressEvent += delegate(object o, Gtk.KeyPressEventArgs args) {
			bool alreadyAssigned = false;
			string keyAssigned = "";
			if(args.Event.KeyValue != (uint)Gdk.Key.Escape){
				foreach (KeyNode node in keyNodeStore){
					if(node.Key == (Gdk.Key)args.Event.KeyValue){
						alreadyAssigned = true;
						keyAssigned = node.CommandString;
						break;
					} 
				}
				if(!alreadyAssigned){
					selectedKeyNode.Key = (Gdk.Key)args.Event.KeyValue;
					SaveKeyCommand(selectedKeyNode.KeyCommand, selectedKeyNode.Key);
				}
				else{
					label.Text = "Key already assigned to \n\"" + keyAssigned + "\"";
				}
			}
			if(!alreadyAssigned){
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
				});
			}
			else{
				Timer timer = new Timer(1500);
				timer.Elapsed += delegate {
					label.Text = s;
					timer.Stop();
				};
				timer.Start();
			}
		};
		Gtk.Application.Invoke (delegate {
			dialog.ShowAll();
		});

	}

	protected void SaveKeyCommand(KeyCommand command, Gdk.Key key){
		switch(command){
			case KeyCommand.Backward:
				settings.Backward = (int) key;
			break;
			case KeyCommand.DecMotor3Speed:
				settings.DecMotor3Speed = (int) key;
			break;
			case KeyCommand.DecTurnPercent:
				settings.DecTurnPercent = (int) key;
			break;
		 	case KeyCommand.DecVehicleSpeed:
				settings.DecVehicleSpeed = (int) key;
			break;
			case KeyCommand.Forward:
				settings.Forward = (int) key;
			break;
			case KeyCommand.IncMotor3Speed:
				settings.IncMotor3Speed = (int) key;
			break;
			case KeyCommand.IncTurnPercent:
				settings.IncTurnPercent = (int) key;
			break;
			case KeyCommand.IncVehicleSpeed:
				settings.IncVehicleSpeed = (int) key;
			break;
			case KeyCommand.Motor3Fwd:
				settings.Motor3Fwd = (int) key;
			break;
			case KeyCommand.Motor3Rev:
				settings.Motor3Rev = (int) key;
			break;
			case KeyCommand.SpinLeft:
				settings.SpinLeft = (int) key;
			break;
			case KeyCommand.SpinRight:
				settings.SpinRight = (int) key;
			break;
			case KeyCommand.IncSpinSpeed:
				settings.IncSpinSpeed = (int) key;
			break;
			case KeyCommand.DecSpinSpeed:
				settings.DecSpinSpeed = (int) key;
			break;
			case KeyCommand.ReadSensor1:
				settings.ReadKeySensor1 = (int) key;
			break;
			case KeyCommand.ReadSensor2:
				settings.ReadKeySensor2 = (int) key;
			break;
			case KeyCommand.ReadSensor3:
				settings.ReadKeySensor3 = (int) key;
			break;
			case KeyCommand.ReadSensor4:
				settings.ReadKeySensor4 = (int) key;
			break;
			case KeyCommand.SendMessage1:
				settings.SendMessage1 = (int) key;
			break;
			case KeyCommand.SendMessage2:
				settings.SendMessage2 = (int) key;
			break;
			case KeyCommand.SendMessage3:
				settings.SendMessage3 = (int) key;
			break;
			case KeyCommand.SendMessage4:
				settings.SendMessage4 = (int) key;
			break;
			case KeyCommand.SendMessage5:
				settings.SendMessage5 = (int) key;
			break;
		}
		settings.Save();
	}
	



	private bool leftPressed = false;
	private bool rightPressed = false;
	private bool backPressed = false;
	private bool forwardPressed = false;
	private bool motorForwardPressed = false;
	private bool motorReversePressed = false;
	protected void OnKeyPressEvent (object sender, Gtk.KeyPressEventArgs args)
	{
		if(enableKeyboardInput.Active){
			Gdk.Key key = (Gdk.Key)args.Event.KeyValue;
			SpawnThread(delegate()
        	{
				KeyCommand? command = null;
				foreach (KeyNode node in keyNodeStore){
					if(node.Key == key){
						if(!node.Pressed){
							node.Pressed = true;
							if(node.KeyCommand == KeyCommand.Backward){
								backPressed = true;
							}
							if(node.KeyCommand == KeyCommand.Forward){
								forwardPressed = true;
							}
							if(node.KeyCommand == KeyCommand.SpinLeft){
								leftPressed = true;
							}
							if(node.KeyCommand == KeyCommand.SpinRight){
								rightPressed = true;
							}
							if(node.KeyCommand == KeyCommand.Motor3Fwd){
								motorForwardPressed = true;
							}
							if(node.KeyCommand == KeyCommand.Motor3Rev){
								motorReversePressed = true;
							}
							command = node.KeyCommand;
						}
						break;
					}
				}
				DoKeyCommand(command);
			});
		}
	}

	protected void OnKeyReleaseEvent (object sender, Gtk.KeyReleaseEventArgs args)
	{
		if(enableKeyboardInput.Active && args != null){
			Gdk.Key key = (Gdk.Key)args.Event.KeyValue;//otherwise unpredictable behaviour when spawning a new thread
			SpawnThread(delegate()
        	{
				foreach (KeyNode node in keyNodeStore){
					if(node.Key == key){
						node.Pressed = false;
						if(node.KeyCommand == KeyCommand.Backward){
							backPressed = false;
							SendVehicleCommand();
						}
						if(node.KeyCommand == KeyCommand.Forward){
							forwardPressed = false;
							SendVehicleCommand();
						}
						if(node.KeyCommand == KeyCommand.SpinLeft){
							leftPressed = false;
							SendVehicleCommand();
						}
						if(node.KeyCommand == KeyCommand.SpinRight){
							rightPressed = false;
							SendVehicleCommand();
						}
						if(node.KeyCommand == KeyCommand.Motor3Fwd){
							motorForwardPressed = false;
							SendMotorCommand();
						}
						if(node.KeyCommand == KeyCommand.Motor3Rev){
							motorReversePressed = false;
							SendMotorCommand();
						}
						break;
					}
				}
			});
		}
	}

	static private int IncStep = 5;

	protected void DoKeyCommand(KeyCommand? command){
		if(command == null){
			return;
		}
		switch(command){
			case KeyCommand.Backward:
				SendVehicleCommand();
			break;
			case KeyCommand.DecMotor3Speed:
				Gtk.Application.Invoke (delegate {
					motor3SpeedScale.Value -= IncStep;
				});
				SendMotorCommand();
			break;
			case KeyCommand.DecTurnPercent:
				Gtk.Application.Invoke (delegate {
					vehicleTurnRatioScale.Value -= IncStep;
				});
				SendVehicleCommand();
			break;
		 	case KeyCommand.DecVehicleSpeed:
				Gtk.Application.Invoke (delegate {
					vehicleSpeedScale.Value -= IncStep;
				});
				SendVehicleCommand();
			break;
			case KeyCommand.Forward:
				SendVehicleCommand();
			break;
			case KeyCommand.IncMotor3Speed:
				Gtk.Application.Invoke (delegate {
					motor3SpeedScale.Value += IncStep;
				});
				SendMotorCommand();
			break;
			case KeyCommand.IncTurnPercent:
				Gtk.Application.Invoke (delegate {
					vehicleTurnRatioScale.Value += IncStep;
				});
				SendVehicleCommand();
			break;
			case KeyCommand.IncVehicleSpeed:
				Gtk.Application.Invoke (delegate {
					vehicleSpeedScale.Value += IncStep;
				});
				SendVehicleCommand();
			break;
			case KeyCommand.Motor3Fwd:
				SendMotorCommand();
			break;
			case KeyCommand.Motor3Rev:
				SendMotorCommand();
			break;
			case KeyCommand.SpinLeft:
				SendVehicleCommand();
			break;
			case KeyCommand.SpinRight:
				SendVehicleCommand();
			break;
			case KeyCommand.IncSpinSpeed:
				Gtk.Application.Invoke (delegate {
					spinSpeedScale.Value += IncStep;
				});
				SendVehicleCommand();
			break;
			case KeyCommand.DecSpinSpeed:
				Gtk.Application.Invoke (delegate {
					spinSpeedScale.Value -= IncStep;
				});
				SendVehicleCommand();
			break;
			case KeyCommand.ReadSensor1:
				OnReadSensor1ButtonClicked(null,null);
			break;
			case KeyCommand.ReadSensor2:
				OnReadSensor2ButtonClicked(null,null);
			break;
			case KeyCommand.ReadSensor3:
				OnReadSensor3ButtonClicked(null,null);
			break;
			case KeyCommand.ReadSensor4:
				OnReadSensor4ButtonClicked(null,null);
			break;
			case KeyCommand.SendMessage1:
				this.SendMessageSetting(0);
			break;
			case KeyCommand.SendMessage2:
				this.SendMessageSetting(1);
			break;
			case KeyCommand.SendMessage3:
				this.SendMessageSetting(2);
			break;
			case KeyCommand.SendMessage4:
				this.SendMessageSetting(3);
			break;
			case KeyCommand.SendMessage5:
				this.SendMessageSetting(4);
			break;


		}
	}
	private IMotor motor3 = null;
	private void SendMotorCommand(){
		if(motorForwardPressed && motorReversePressed){
			SpawnThread(delegate()
        	{
				motor3.Off();
			},false);
		}
		if(!motorForwardPressed && !motorReversePressed){
			SpawnThread(delegate()
        	{
				motor3.Off();
			},false);
		}
		if(!motorForwardPressed && motorReversePressed){
			SpawnThread(delegate()
        	{
				if(motor3ReverseCheckbutton.Active){
					motor3.On((sbyte)motor3SpeedScale.Value);
				}
				else{
					motor3.On((sbyte)-motor3SpeedScale.Value);
				}
			},false);
		}
		if(motorForwardPressed && !motorReversePressed){
			SpawnThread(delegate()
        	{
				if(motor3ReverseCheckbutton.Active){
					motor3.On((sbyte)-motor3SpeedScale.Value);
				}
				else{
					motor3.On((sbyte)motor3SpeedScale.Value);
				}
			},false);
		}
	}

	private void SendVehicleCommand(){
		if(leftPressed && !rightPressed && !backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.SpinLeft((sbyte)vehicleSpeedScale.Value,false);
			},false);
			//Console.WriteLine("Spin left");
		}
		if(leftPressed && rightPressed && !backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(leftPressed && !rightPressed && !backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.TurnLeftForward((sbyte)vehicleSpeedScale.Value, (sbyte) vehicleTurnRatioScale.Value ,false);
			},false);
			//Console.WriteLine("Turn left forward");
		}
		if(leftPressed && !rightPressed && backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.TurnLeftReverse((sbyte)vehicleSpeedScale.Value,(sbyte) vehicleTurnRatioScale.Value ,false);
			},false);
			//Console.WriteLine("Turn left backward");
		}
		if(!leftPressed && rightPressed && !backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.SpinRight((sbyte)vehicleSpeedScale.Value,false);
			},false);
			//Console.WriteLine("Spin right");
		}
		if(!leftPressed && rightPressed && backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.TurnRightReverse((sbyte)vehicleSpeedScale.Value,(sbyte) vehicleTurnRatioScale.Value ,false);
			},false);
			//Console.WriteLine("turn right backward");
		}
		if(!leftPressed && rightPressed && !backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.TurnRightForward((sbyte)vehicleSpeedScale.Value,(sbyte) vehicleTurnRatioScale.Value ,false);
			},false);
			//Console.WriteLine("turn right forward");
		}
		if(!leftPressed && rightPressed && backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(!leftPressed && !rightPressed && backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Backward((sbyte)vehicleSpeedScale.Value,false);
			},false);
			//Console.WriteLine("Backward");
		}
		if(!leftPressed && !rightPressed && backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(!leftPressed && !rightPressed && !backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Forward((sbyte)vehicleSpeedScale.Value,false);
			},false);
			//Console.WriteLine("Forward");
		}
		if(!leftPressed && !rightPressed && !backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(leftPressed && rightPressed && backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(leftPressed && rightPressed && backPressed && !forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(leftPressed && !rightPressed && backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
		if(leftPressed && rightPressed && !backPressed && forwardPressed){
			SpawnThread(delegate()
        	{
				brick.Vehicle.Off(false);
			},false);
			//Console.WriteLine("Off");
		}
	}

	private void DisableVehicleUserInput(){
		SetVehicleUserInput(false);
		assignCommandsButton.Sensitive = false;
	}

	private void EnableVehicleUserInput(){
		SetVehicleUserInput(true);
	}

	private void SetVehicleUserInput(bool set){
		Gtk.Application.Invoke (delegate {
			assignCommandsButton.Sensitive = set;
			commandNodeView.Sensitive = set;
			leftMotorCombobox.Sensitive = set;
			rightMotorCombobox.Sensitive = set;
			leftMotorReverseCheckbutton.Sensitive = set;
			rightMotorReverseCheckbutton.Sensitive = set;
			vehicleTurnRatioScale.Sensitive = set;
			vehicleSpeedScale.Sensitive = set;
			spinSpeedScale.Sensitive = set;
			motor3ReverseCheckbutton.Sensitive = set;
			motor3Combobox.Sensitive = set;
			motor3SpeedScale.Sensitive = set;
			enableKeyboardInput.Sensitive = set;
		});

	}

	private void OnConnectedKeyboardPage(){
		assignCommandsButton.Sensitive = false;
		SpawnThread(delegate()
        {
			brick.Vehicle.LeftPort = (MotorPort)leftMotorCombobox.GetActiveValue();
			brick.Vehicle.RightPort = (MotorPort) rightMotorCombobox.GetActiveValue();
			brick.Vehicle.ReverseLeft = leftMotorReverseCheckbutton.Active;
			brick.Vehicle.ReverseRight = rightMotorReverseCheckbutton.Active;
			switch((MotorPort) motor3Combobox.GetActiveValue()){
				case MotorPort.OutA:
					motor3 = brick.MotorA;
				break;
				case MotorPort.OutB:
					motor3 = brick.MotorB;
				break;
				case MotorPort.OutC:
					motor3 = brick.MotorC;
				break;
			}
			motor3.Reverse = motor3ReverseCheckbutton.Active;
		},false,-1);
	}

	private void OnDisconnectedKeyboardPage(){
		Gtk.Application.Invoke (delegate {
			enableKeyboardInput.Active = false;
		});
	}

	protected void OnLeftMotorComboboxChanged (object sender, System.EventArgs e){
		if(brick == null)
			return;
		settings.LeftMotor = (string) leftMotorCombobox.GetActiveValue(1);
		settings.Save ();
		SpawnThread(delegate()
        {
			brick.Vehicle.LeftPort = (MotorPort)leftMotorCombobox.GetActiveValue();
		});
	}

	protected void OnRightMotorComboboxChanged (object sender, System.EventArgs e){
		if(brick == null)
			return;
		settings.RightMotor = (string) rightMotorCombobox.GetActiveValue(1);
		settings.Save();
		SpawnThread(delegate()
        {
			brick.Vehicle.RightPort =(MotorPort) rightMotorCombobox.GetActiveValue();
		});
	}

	protected void OnLeftMotorReverseCheckbuttonToggled (object sender, EventArgs e)
	{
		if(brick == null)
			return;
		settings.LeftMotorReverse = leftMotorReverseCheckbutton.Active;
		settings.Save();
		SpawnThread(delegate()
        {
			brick.Vehicle.ReverseLeft = leftMotorReverseCheckbutton.Active;
		});
	}

	protected void OnRightMotorReverseCheckbuttonToggled (object sender, EventArgs e)
	{
		if(brick == null)
			return;
		settings.RightMotorReverse = rightMotorReverseCheckbutton.Active;
		settings.Save();
		SpawnThread(delegate()
        {
			brick.Vehicle.ReverseRight = rightMotorReverseCheckbutton.Active;
		});
	}

	protected void OnEnableKeyboardInputToggled (object sender, EventArgs e)
	{
		SetKeyboardUserInputMode(!enableKeyboardInput.Active);
		OnKeyReleaseEvent(null,null);
	}

	private void SetKeyboardUserInputMode(bool set){
		Gtk.Application.Invoke (delegate {
			assignCommandsButton.Sensitive = set;
			commandNodeView.Sensitive = set;
			leftMotorCombobox.Sensitive = set;
			rightMotorCombobox.Sensitive = set;
			leftMotorReverseCheckbutton.Sensitive = set;
			rightMotorReverseCheckbutton.Sensitive = set;
			vehicleTurnRatioScale.Sensitive = set;
			vehicleSpeedScale.Sensitive = set;
			spinSpeedScale.Sensitive = set;
			motor3ReverseCheckbutton.Sensitive = set;
			motor3Combobox.Sensitive = set;
			motor3SpeedScale.Sensitive = set;
		});
	}

	protected void LoadKeyboardPageSettings(){
		leftMotorCombobox.SetActiveValue(settings.LeftMotor);
		leftMotorReverseCheckbutton.Active = settings.LeftMotorReverse;
		rightMotorCombobox.SetActiveValue(settings.RightMotor);
		rightMotorReverseCheckbutton.Active = settings.RightMotorReverse;
		vehicleTurnRatioScale.Adjustment.Value = settings.TurnRatio;
		vehicleSpeedScale.Adjustment.Value = settings.VehicleSpeed;
		spinSpeedScale.Adjustment.Value = settings.SpinSpeed;
		motor3Combobox.SetActiveValue(settings.Motor3);
		motor3ReverseCheckbutton.Active = settings.Motor3Float;
		motor3SpeedScale.Adjustment.Value = settings.Motor3Speed;
		Gtk.Application.Invoke (delegate {
			keyNodeStore.Clear();
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Forward,(Gdk.Key) settings.Forward));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Backward,(Gdk.Key)settings.Backward));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SpinLeft,(Gdk.Key)settings.SpinLeft));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SpinRight,(Gdk.Key)settings.SpinRight));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncVehicleSpeed,(Gdk.Key)settings.IncVehicleSpeed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecVehicleSpeed,(Gdk.Key)settings.DecVehicleSpeed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncSpinSpeed,(Gdk.Key)settings.IncSpinSpeed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecSpinSpeed,(Gdk.Key)settings.DecSpinSpeed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncTurnPercent,(Gdk.Key)settings.IncTurnPercent));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecTurnPercent,(Gdk.Key)settings.DecTurnPercent));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Motor3Fwd,(Gdk.Key)settings.Motor3Fwd));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.Motor3Rev,(Gdk.Key)settings.Motor3Rev));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.IncMotor3Speed,(Gdk.Key)settings.IncMotor3Speed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.DecMotor3Speed,(Gdk.Key)settings.DecMotor3Speed));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor1,(Gdk.Key)settings.ReadKeySensor1));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor2,(Gdk.Key)settings.ReadKeySensor2));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor3,(Gdk.Key)settings.ReadKeySensor3));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.ReadSensor4,(Gdk.Key)settings.ReadKeySensor4));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage1,(Gdk.Key)settings.SendMessage1));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage2,(Gdk.Key)settings.SendMessage2));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage3,(Gdk.Key)settings.SendMessage3));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage4,(Gdk.Key)settings.SendMessage4));
			keyNodeStore.AddNode(new KeyNode(KeyCommand.SendMessage5,(Gdk.Key)settings.SendMessage5));

		});
		//This has nothing to do with settings just needs to be called once --- should be placed elsewhere
		doubleClickTimer = new System.Timers.Timer(50);
		doubleClickTimer.Elapsed += delegate(object o, System.Timers.ElapsedEventArgs elapsedEventArgs) {
			doubleClick = false;
			doubleClickTimer.Stop();
		};
	}

	protected void OnVehicleTurnRatioScaleValueChanged (object sender, EventArgs e)
	{
		settings.TurnRatio = (float) vehicleTurnRatioScale.Adjustment.Value;
		settings.Save();
	}

	protected void OnVehicleSpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.VehicleSpeed = (float) vehicleSpeedScale.Adjustment.Value;
		settings.Save();
	}

	protected void OnSpinSpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.SpinSpeed = (float) spinSpeedScale.Adjustment.Value;
		settings.Save();
	}

	protected void OnMotor3ComboboxChanged (object sender, EventArgs e)
	{
		if(brick == null)
			return;
		switch((MotorPort) motor3Combobox.GetActiveValue()){
			case MotorPort.OutA:
				motor3 = brick.MotorA;
			break;
			case MotorPort.OutB:
				motor3 = brick.MotorB;
			break;
			case MotorPort.OutC:
				motor3 = brick.MotorC;
			break;
		}
		settings.Motor3 = (string) motor3Combobox.GetActiveValue(1);
		settings.Save();
	}

	protected void OnMotor3ReverseCheckbuttonToggled (object sender, EventArgs e)
	{
		if(brick == null)
			return;
		settings.Motor3Float = motor3ReverseCheckbutton.Active;
		settings.Save();
		SpawnThread(delegate()
        {
			motor3.Reverse = leftMotorReverseCheckbutton.Active;
		});
	}

	protected void OnMotor3SpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.Motor3Speed = (float) motor3SpeedScale.Adjustment.Value;
		settings.Save();
	}

	protected void OnAssignCommandsButtonClicked (object sender, EventArgs e)
	{
		AssignKey();
	}
}

