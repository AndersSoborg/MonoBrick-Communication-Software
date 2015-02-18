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

namespace OmniBotRemote
{

	public sealed class RemoteSettings
	{
		private static readonly RemoteSettings instance = new RemoteSettings();
		private static ISharedPreferences preference = null;
		private RemoteSettings(){

		}
		public static MonoBrick.NXT.MotorPort StringToMotorPort(string s){
			if (s.ToLower () == "motor a")
				return MonoBrick.NXT.MotorPort.OutA;
			if (s.ToLower () == "motor b")
				return MonoBrick.NXT.MotorPort.OutB;
			if (s.ToLower () == "motor c")
				return MonoBrick.NXT.MotorPort.OutC;
			//default
			return MonoBrick.NXT.MotorPort.OutA;
		}
		
		public static String MotorPortToString(MonoBrick.NXT.MotorPort port){
			if (port == MonoBrick.NXT.MotorPort.OutA)
				return "Motor A";
			if (port == MonoBrick.NXT.MotorPort.OutB)
				return "Motor B";
			if (port == MonoBrick.NXT.MotorPort.OutC)
				return "Motor C";
			//default
			return "Motor A";
		}
		
		
		public void Initialize(ISharedPreferences pref){
			preference = pref;	
			
			leftPort = StringToMotorPort(preference.GetString("leftPort", "Motor A"));
			rightPort = StringToMotorPort(preference.GetString("rightPort", "Motor C"));
			additionalPort = StringToMotorPort(preference.GetString("additionalPort", "Motor B"));
			
			reverseLeft =  preference.GetBoolean("reverseLeft", false);
			reverseRight = preference.GetBoolean("reverseRight", false);
			reverseAdditional = preference.GetBoolean("reverseAdditional", false);
			
			sendVehicleDataToMailbox = preference.GetBoolean("sendVehicleDataToMailbox", false);
			sensorValueToSpeech = preference.GetBoolean("sensorValueToSpeech", true);
			deviceName = preference.GetString("deviceName", "");
			type = StringToBrickType(preference.GetString("brickType", "EV3"));
			vehicleMailbox = StringToMailbox(preference.GetString("vehicleMailbox", "Mailbox 0"));
			degreeOffset = StringToDegreeOffset(preference.GetString("degreeOffset", "Up"));
			
			sensor1 = preference.GetString("sensor1", "None");
			sensor2 = preference.GetString("sensor2", "None");
			sensor3 = preference.GetString("sensor3", "None");
			sensor4 = preference.GetString("sensor4", "None");
		}
		
		private string sensor1;
		public string Sensor1{ 
			get{return sensor1;} 
			set{
				preference.Edit().PutString("sensor1", value).Commit();
				sensor1 = value;
			}
		}
		
		private string sensor2;
		public string Sensor2{ 
			get{return sensor2;} 
			set{
				preference.Edit().PutString("sensor2", value).Commit();
				sensor2 = value;
			}
		}
		
		private string sensor3;
		public string Sensor3{ 
			get{return sensor3;} 
			set{
				preference.Edit().PutString("sensor3", value).Commit();
				sensor3 = value;
			}
		}
		
		private string sensor4;
		public string Sensor4{ 
			get{return sensor4;} 
			set{
				preference.Edit().PutString("sensor4", value).Commit();
				sensor4 = value;
			}
		}
		
		private MonoBrick.NXT.MotorPort leftPort;
		public MonoBrick.NXT.MotorPort LeftPort
		{
			get{return leftPort;} 
			set{
				preference.Edit().PutString("leftPort", MotorPortToString(value)).Commit();
				leftPort = value;
			}
		}
		
		private MonoBrick.NXT.MotorPort rightPort;
		public MonoBrick.NXT.MotorPort RightPort
		{
			get{return rightPort;} 
			set{
				preference.Edit().PutString("rightPort", MotorPortToString(value)).Commit();
				rightPort = value;
			}
		}
		 
		private MonoBrick.NXT.MotorPort additionalPort;
		public MonoBrick.NXT.MotorPort AdditionalPort
		{
			get{return additionalPort;} 
			set{
				preference.Edit().PutString("additionalPort", MotorPortToString(value)).Commit();
				additionalPort = value;
			}
		} 
		
		private bool reverseLeft;
		public bool ReverseLeft
		{
			get{return reverseLeft;} 
			set{
				preference.Edit().PutBoolean("reverseLeft", value).Commit();
				reverseLeft = value;
			}
		}
		
		private bool reverseRight;
		public bool ReverseRight
		{
			get{return reverseRight;} 
			set{
				preference.Edit().PutBoolean("reverseRight", value).Commit();
				reverseRight = value;
			}
		}
		
		private bool reverseAdditional;
		public bool ReverseAdditional
		{
			get{return reverseAdditional;} 
			set{
				preference.Edit().PutBoolean("reverseAdditional", value).Commit();
				reverseAdditional = value;
			}
		}
		
		private bool sendVehicleDataToMailbox;
		public bool SendVehicleDataToMailbox 
		{
			get{return sendVehicleDataToMailbox;} 
			set{
				preference.Edit().PutBoolean("sendVehicleDataToMailbox", value).Commit();
				sendVehicleDataToMailbox = value;
			}
		}
		
		private bool sensorValueToSpeech;
		public bool SensorValueToSpeech{ 
			get{return sensorValueToSpeech;} 
			set{
				preference.Edit().PutBoolean("sensorValueToSpeech", value).Commit();
				sensorValueToSpeech = value;
			}
		}
		
		private string deviceName;
		public string DeviceName{ 
			get{return deviceName;} 
			set{
				preference.Edit().PutString("deviceName", value).Commit();
				deviceName = value;
			}
		}
		
		private BrickType type;
		public BrickType Type { 
			get{ return type;} 
			set{
				preference.Edit().PutString("brickType", BrickTypeToString(value)).Commit();
				type = value;
			}
		}
		
		private MonoBrick.NXT.Box vehicleMailbox;
		public MonoBrick.NXT.Box VehicleMailbox { 
			get{ return vehicleMailbox;} 
			set{
				preference.Edit().PutString("vehicleMailbox", MailBoxToString(value)).Commit();
				vehicleMailbox = value;
			}
		}
		
		private float degreeOffset;
		public float DegreeOffset{
			get{ return degreeOffset;} 
			set{
				preference.Edit().PutString("degreeOffset", DegreeOffsetToString(value)).Commit();
				degreeOffset = value;
			}
		}
		

		public static MonoBrick.NXT.Box StringToMailbox(string s){
			if (s.ToLower () == "mailbox 0")
				return MonoBrick.NXT.Box.Box0;  
			if (s.ToLower () == "mailbox 1")
				return MonoBrick.NXT.Box.Box1;  
			if (s.ToLower () == "mailbox 2")
				return MonoBrick.NXT.Box.Box2;  
			if (s.ToLower () == "mailbox 3")
				return MonoBrick.NXT.Box.Box3;  
			if (s.ToLower () == "mailbox 4")
				return MonoBrick.NXT.Box.Box4;  
			if (s.ToLower () == "mailbox 5")
				return MonoBrick.NXT.Box.Box5;  
			if (s.ToLower () == "mailbox 6")
				return MonoBrick.NXT.Box.Box6;  
			if (s.ToLower () == "mailbox 7")
				return MonoBrick.NXT.Box.Box7;  
			if (s.ToLower () == "mailbox 8")
				return MonoBrick.NXT.Box.Box8;  
			if (s.ToLower () == "mailbox 9")
				return MonoBrick.NXT.Box.Box9;
			//default
			return MonoBrick.NXT.Box.Box0;
		}
		
		public static String MailBoxToString(MonoBrick.NXT.Box box){
			if (box == MonoBrick.NXT.Box.Box0)
				return "mailbox 0";  
			if (box == MonoBrick.NXT.Box.Box1)
				return "mailbox 1";  
			if (box == MonoBrick.NXT.Box.Box2)
				return "mailbox 2";  
			if (box == MonoBrick.NXT.Box.Box3)
				return "mailbox 3";  
			if (box == MonoBrick.NXT.Box.Box4)
				return "mailbox 4";  
			if (box == MonoBrick.NXT.Box.Box5)
				return "mailbox 5";  
			if (box == MonoBrick.NXT.Box.Box6)
				return "mailbox 6";  
			if (box == MonoBrick.NXT.Box.Box7)
				return "mailbox 7";  
			if (box == MonoBrick.NXT.Box.Box8)
				return "mailbox 8";  
			if (box == MonoBrick.NXT.Box.Box9)
				return "mailbox 9";
			//default
			return "mailbox 0";
		}
		
		
		public static float StringToDegreeOffset (string s)
		{
			if (s.ToLower () == "right")
				return 0.0f;  
			if (s.ToLower () == "up")
				return ((float)Math.PI/2) * 3.0f;  
			if (s.ToLower () == "left")
				return ((float)Math.PI/2) * 2.0f;  
			if (s.ToLower () == "down")
				return ((float)Math.PI/2) * 1.0f;  
			//default
			return 0;
		}
		
		public static string DegreeOffsetToString (float offset)
		{
			if (offset == 0.0f)
				return "Right";  
			if (offset == (((float)Math.PI/2) * 3.0f))
				return "Up";  
			if (offset == (((float)Math.PI/2) * 2.0f))
				return "Left";  
			if (offset == (((float)Math.PI/2) * 1.0f))
				return "Down";  
			//default
			return "Right";
		
		
		
		}
		

		public static BrickType StringToBrickType(string s){
			if (s.ToLower () == "ev3"){
				return BrickType.EV3;	
			} 
			if (s.ToLower () == "nxt"){
				return BrickType.NXT;	
			} 
			return BrickType.EV3;
		}
		
		public static string BrickTypeToString (BrickType type)
		{
			if (type == BrickType.EV3){
				return "EV3";	
			} 
			if (type == BrickType.NXT){
				return "NXT";	
			}
			return "EV3"; 
		
		}


		public static RemoteSettings Instance
		{
			get{return instance;}
		}
		
	}




	[Activity (Label = "MonoBrick Remote Settings")]	
	public class SettingsActivity: PreferenceActivity, Preference.IOnPreferenceChangeListener,Preference.IOnPreferenceClickListener {
		
		private ListPreference deviceList = null;
		private ListPreference brickType = null;

		private ListPreference leftMotor = null;
		private ListPreference rightMotor = null;
		private ListPreference additionalMotor = null;

		private CheckBoxPreference leftMotorReverse= null;
		private CheckBoxPreference rightMotorReverse= null;
		private CheckBoxPreference additionalMotorReverse= null;

		private CheckBoxPreference sendVehicleDataToMailbox= null;
		private ListPreference vehicleMailbox = null;
		private ListPreference vehicleAngleOffset = null;
		
		private CheckBoxPreference sensorValueToSpeech= null;

		private BrickController brickController = null;

		private const string SendVehicleCommandsToMailboxText = "Vehicle raw data is send to the mailbox";
		private const string SendVehicleCommandsToBrickText = "Motor commands are send to the brick";
		private const string SensorToSpeechEnabledText = "Sensor value can be spoken aloud";
		private const string SensorToSpeechDisabledText = "Sensor value can not be spoken aloud";

		private RemoteSettings settings = null;



		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			settings = RemoteSettings.Instance;
			brickController = BrickController.Instance;

			AddPreferencesFromResource(Resource.Layout.Settings);

			deviceList = (ListPreference) FindPreference("bluetoothDevice");
			brickType = (ListPreference) FindPreference("brickType");


			leftMotor = (ListPreference) FindPreference("leftMotor");
			rightMotor = (ListPreference) FindPreference("rightMotor");
			additionalMotor = (ListPreference) FindPreference("additionalMotor");

			leftMotorReverse = (CheckBoxPreference) FindPreference("reverseLeftMotor");
			rightMotorReverse = (CheckBoxPreference) FindPreference("reverseRightMotor");
			additionalMotorReverse = (CheckBoxPreference) FindPreference("reverseAdditionalMotor");

			sendVehicleDataToMailbox = (CheckBoxPreference) FindPreference("sendVehicleDataToMailbox");
			vehicleMailbox = (ListPreference) FindPreference("vehicleMailbox");
			vehicleAngleOffset = (ListPreference) FindPreference("vehicleAngleOffset");

			sensorValueToSpeech = (CheckBoxPreference) FindPreference("sensorValueToSpeech");

			deviceList.OnPreferenceChangeListener = this;
			brickType.OnPreferenceChangeListener = this;


			leftMotor.OnPreferenceChangeListener = this;
			rightMotor.OnPreferenceChangeListener = this;
			additionalMotor.OnPreferenceChangeListener = this;

			leftMotorReverse.OnPreferenceChangeListener = this;
			rightMotorReverse.OnPreferenceChangeListener = this;
			additionalMotorReverse.OnPreferenceChangeListener = this;

			sendVehicleDataToMailbox.OnPreferenceChangeListener = this;
			vehicleMailbox.OnPreferenceChangeListener = this;

			sensorValueToSpeech.OnPreferenceChangeListener = this;
			
			vehicleAngleOffset.OnPreferenceChangeListener = this;
			
		}

		protected override void OnResume(){
			base.OnResume();
			FillListPreference ();
			LoadSettings();
			if (!CreateDeviceList ()) {
				AlertDialog.Builder dialog = new AlertDialog.Builder(new ContextThemeWrapper(this, Android.Resource.Style.ThemeTranslucentNoTitleBar));
				//AlertDialog	dialog = new  AlertDialog.Builder(this).Create();
				dialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
				dialog.SetTitle("Select Bluetooth Device");
				dialog.SetMessage("No Bluetooth Device has been paired with Phone. Press Ok to pair a device");
				dialog. SetPositiveButton("OK", delegate(object sen, DialogClickEventArgs eventArgs) {
					Intent intentBluetooth = new Intent();
					intentBluetooth.SetAction(Android.Provider.Settings.ActionBluetoothSettings);
					StartActivity(intentBluetooth);
				});
				dialog.SetNegativeButton("Cancel",delegate {});
				dialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
				dialog.Show();
			}
		}

		private void FillListPreference(){
			List<string> entries = new List<string> ();
			entries.Add ("Motor A");
			entries.Add ("Motor B");
			entries.Add ("Motor C");

			rightMotor.SetEntries (entries.ToArray());
			rightMotor.SetEntryValues (entries.ToArray());

			leftMotor.SetEntries (entries.ToArray());
			leftMotor.SetEntryValues (entries.ToArray());

			additionalMotor.SetEntries (entries.ToArray());
			additionalMotor.SetEntryValues (entries.ToArray());

			entries = new List<string> ();
			entries.Add ("NXT");
			entries.Add ("EV3");
			brickType.SetEntries(entries.ToArray());
			brickType.SetEntryValues(entries.ToArray());

			entries = new List<string> ();
			entries.Add ("Mailbox 0");
			entries.Add ("Mailbox 1");
			entries.Add ("Mailbox 2");
			entries.Add ("Mailbox 3");
			entries.Add ("Mailbox 4");
			entries.Add ("Mailbox 5");
			entries.Add ("Mailbox 6");
			entries.Add ("Mailbox 7");
			entries.Add ("Mailbox 8");
			entries.Add ("Mailbox 9");

			vehicleMailbox.SetEntries(entries.ToArray());
			vehicleMailbox.SetEntryValues(entries.ToArray());
			
			entries = new List<string> ();
			entries.Add ("Right");
			entries.Add ("Up");
			entries.Add ("Left");
			entries.Add ("Down");
			
			vehicleAngleOffset.SetEntries(entries.ToArray());
			vehicleAngleOffset.SetEntryValues(entries.ToArray());
			
		}

		private void LoadSettings(){
			//var pref = PreferenceManager.GetDefaultSharedPreferences(this.BaseContext); 
			deviceList.Enabled = !brickController.NXT.Connection.IsConnected;
			
			brickType.Summary = RemoteSettings.BrickTypeToString(settings.Type);
			
			brickType.Enabled = !brickController.NXT.Connection.IsConnected;
			
			//The only brick type supported at this point is the NXT
			brickType.Enabled = false;
			brickType.Summary = "NXT";
			

			leftMotor.Summary =  RemoteSettings.MotorPortToString(settings.LeftPort);
			rightMotor.Summary = RemoteSettings.MotorPortToString(settings.RightPort);
			additionalMotor.Summary = RemoteSettings.MotorPortToString(settings.AdditionalPort); 

			leftMotorReverse.Checked = settings.ReverseLeft;
			rightMotorReverse.Checked = settings.ReverseRight;
			additionalMotorReverse.Checked = settings.ReverseAdditional;
			

			vehicleMailbox.Summary = RemoteSettings.MailBoxToString(settings.VehicleMailbox);
			
			vehicleAngleOffset.Summary =  RemoteSettings.DegreeOffsetToString(settings.DegreeOffset);
			if(vehicleAngleOffset.Summary.ToLower() == "up"){
				vehicleAngleOffset.Summary = "0 degrees will be up on the circle";
			}
			if(vehicleAngleOffset.Summary.ToLower() == "left"){
				vehicleAngleOffset.Summary = "0 degrees will be to the left on the circle";
			}
			if(vehicleAngleOffset.Summary.ToLower() == "right"){
				vehicleAngleOffset.Summary = "0 degrees will be to the right on the circle";
			}
			if(vehicleAngleOffset.Summary.ToLower() == "down"){
				vehicleAngleOffset.Summary = "0 degrees will be down on the circle";
			}
		
			
			sendVehicleDataToMailbox.Checked = settings.SendVehicleDataToMailbox;
			if(sendVehicleDataToMailbox.Checked){
				sendVehicleDataToMailbox.Summary = SendVehicleCommandsToMailboxText;
			}
			else{
				sendVehicleDataToMailbox.Summary = SendVehicleCommandsToBrickText;
			}
			
			sensorValueToSpeech.Checked = settings.SensorValueToSpeech;
			if (sensorValueToSpeech.Checked) {
				sensorValueToSpeech.Summary = SensorToSpeechEnabledText;
			}
			else{
				sensorValueToSpeech.Summary = SensorToSpeechDisabledText;
			}

			//update the UI
			SetVehicleMailbox(sendVehicleDataToMailbox.Checked);

		}

		public bool OnPreferenceClick(Preference preference){
			return true;
		}

		public bool OnPreferenceChange (Preference preference, Java.Lang.Object newValue){

			if(preference.Equals(deviceList)){
				preference.Summary = (string) newValue;
				settings.DeviceName = preference.Summary;
			}
			if(preference.Equals(brickType)){
				preference.Summary = (string) newValue;
				settings.Type = RemoteSettings.StringToBrickType (preference.Summary);
			}
			if(preference.Equals(leftMotor)){
				preference.Summary = (string) newValue;
				settings.LeftPort = RemoteSettings.StringToMotorPort (preference.Summary);
			}
			if(preference.Equals(rightMotor)){
				preference.Summary = (string) newValue;
				settings.RightPort = RemoteSettings.StringToMotorPort (preference.Summary);
			}
			if(preference.Equals(additionalMotor)){
				preference.Summary = (string) newValue;
				settings.AdditionalPort = RemoteSettings.StringToMotorPort (preference.Summary);
			}
			if(preference.Equals(vehicleMailbox)){
				preference.Summary = (string) newValue;
				settings.VehicleMailbox = RemoteSettings.StringToMailbox (preference.Summary);
			}
			if(preference.Equals(vehicleAngleOffset)){
				if(((string) newValue).ToLower() == "up"){
					preference.Summary = "0 degrees will be up on the circle";
				}
				if( ((string) newValue).ToLower() == "left"){
					preference.Summary = "0 degrees will be to the left on the circle";
				}
				if( ((string) newValue).ToLower() == "right"){
					preference.Summary = "0 degrees will be to the right on the circle";
				}
				if( ((string) newValue).ToLower() == "down"){
					preference.Summary = "0 degrees will be down on the circle";
				}
				settings.DegreeOffset = RemoteSettings.StringToDegreeOffset((string) newValue);
			}
			if (preference.Equals (sendVehicleDataToMailbox)) {
				if((bool) newValue){
					preference.Summary = SendVehicleCommandsToMailboxText;
				}
				else{
					preference.Summary = SendVehicleCommandsToBrickText;
				}
				SetVehicleMailbox((bool) newValue);
				settings.SendVehicleDataToMailbox = (bool) newValue;
			}
			if(preference.Equals(sensorValueToSpeech)){
				if((bool) newValue){
					preference.Summary = SensorToSpeechEnabledText;
				}
				else{
					preference.Summary = SensorToSpeechDisabledText;
				}
				settings.SensorValueToSpeech = (bool)newValue;
			}
			if(preference.Equals(leftMotorReverse)){
				settings.ReverseLeft = (bool)newValue;
			}
			if(preference.Equals(rightMotorReverse)){
				settings.ReverseRight = (bool)newValue;
			}
			if(preference.Equals(additionalMotorReverse)){
				settings.ReverseAdditional = (bool)newValue;
			}
			return true;
		}

		private void SetVehicleMailbox(bool isSensitive){
			
			leftMotor.Enabled = !isSensitive;
			rightMotor.Enabled = !isSensitive;
			additionalMotor.Enabled = !isSensitive;

			leftMotorReverse.Enabled = !isSensitive;
			rightMotorReverse.Enabled = !isSensitive;
			additionalMotorReverse.Enabled = !isSensitive;

			vehicleMailbox.Enabled = isSensitive;
		}

		private bool CreateDeviceList(){
			List<string> entries = new List<string>();
			foreach(BluetoothDevice device in MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices){
				entries.Add(device.Name);
			}
			deviceList.SetEntries(entries.ToArray());
			deviceList.SetEntryValues(entries.ToArray());
			string deviceName = settings.DeviceName;

			if(deviceName == ""){
				if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices.Count() != 0){
					deviceList.SetValueIndex(0);
					deviceList.Summary = MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices[0].Name;
					settings.DeviceName = deviceList.Summary;
				}
				else{
					if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
						deviceList.Summary = "No device paired with phone";
						return false;
					}
					else{
						deviceList.Summary = "Not available";
						return false;
					}
				}
			}
			else{
				bool foundMatch = false;
				foreach(BluetoothDevice device in MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices){
					if(device.Name == deviceName){
						foundMatch = true;
						deviceList.Summary = deviceName;
						deviceList.Value = deviceName;
					}
				}
				if(!foundMatch){
					if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices.Count() != 0){
						deviceList.SetValueIndex(0);
						deviceList.Summary = MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices[0].Name;
						settings.DeviceName = deviceList.Summary;
					}
					else{
						if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
							deviceList.Summary = "No device paired with phone";
							return false;
						}
						else{
							deviceList.Summary = "Not available";
							return false;
						}
					}	
				}
			}
			return true;
		}
	}
}


