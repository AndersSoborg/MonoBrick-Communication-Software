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
	[Activity (Label = "MonoBrick Tunnel Settings")]			
	public class SettingsActivity: PreferenceActivity, Preference.IOnPreferenceChangeListener,Preference.IOnPreferenceClickListener {
		//private static IpFilter[] ipFilter = new IpFilter[1];
		public static TunnelSettings TunnelSettings = new TunnelSettings();

		private EditTextPreference portNumberPref = null;
		private ListPreference deviceListPref = null;
		private CheckBoxPreference listenForClientsPref= null;
		private CheckBoxPreference enableStreaming = null;
		private EditTextPreference rtspPort = null;
		private EditTextPreference videoPort = null;
		private ListPreference resolution = null;
		private ListPreference videoQuality = null;
		private ListPreference frameRate = null;
		//private TunnelInstance tunnel = TunnelInstance.Instance;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			AddPreferencesFromResource(Resource.Layout.Settings);

			portNumberPref = (EditTextPreference) FindPreference("tunnelPort");
			deviceListPref = (ListPreference) FindPreference("bluetoothDevice");
			listenForClientsPref = (CheckBoxPreference) FindPreference("listenForClients");

			enableStreaming = (CheckBoxPreference) FindPreference("streamVideo");;
			rtspPort = (EditTextPreference) FindPreference("rtspPort");;
			videoPort = (EditTextPreference) FindPreference("videoPort");;
			resolution = (ListPreference) FindPreference("videoResolution");;
			videoQuality = (ListPreference) FindPreference("videoQuality");;
			frameRate = (ListPreference) FindPreference("frameRate");;

			portNumberPref.OnPreferenceChangeListener = this;
			deviceListPref.OnPreferenceChangeListener = this;
			listenForClientsPref.OnPreferenceChangeListener = this;

			enableStreaming.OnPreferenceChangeListener = this;
			rtspPort.OnPreferenceChangeListener = this;
			videoPort.OnPreferenceChangeListener = this;
			resolution.OnPreferenceChangeListener = this;
			videoQuality.OnPreferenceChangeListener = this;
			frameRate.OnPreferenceChangeListener = this;
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
			entries.Add ("Very Low");
			entries.Add ("Low");
			entries.Add ("Normal");
			entries.Add ("High");
			entries.Add ("Very High");
			videoQuality.SetEntries(entries.ToArray());
			videoQuality.SetEntryValues(entries.ToArray());

			entries = new List<string> ();
			entries.Add ("320x240");
			entries.Add ("640x480");
			entries.Add ("960x720");
			resolution.SetEntries(entries.ToArray());
			resolution.SetEntryValues(entries.ToArray());

			entries = new List<string> ();
			entries.Add ("10");
			entries.Add ("15");
			entries.Add ("20");
			entries.Add ("25");
			entries.Add ("30");
			entries.Add ("35");
			entries.Add ("40");
			frameRate.SetEntries(entries.ToArray());
			frameRate.SetEntryValues(entries.ToArray());
		} 

		private void LoadSettings(){
			var pref = PreferenceManager.GetDefaultSharedPreferences(this.BaseContext); 
			portNumberPref.Summary = pref.GetString("tunnelPort", "1500");
			if (pref.GetBoolean ("listenForClients",true)) {
				listenForClientsPref.Summary = "Client can connect to tunnel";
			}
			else{
				listenForClientsPref.Summary = "Client can not connect tunnel";
			}
			SetVideoSettingsSensitive(pref.GetBoolean ("streamVideo",true));
			rtspPort.Summary = pref.GetString("rtspPort","8086");
			videoPort.Summary = pref.GetString("videoPort","9000");
			resolution.Summary =  pref.GetString("videoResolution","640x480");
			videoQuality.Summary = pref.GetString("videoQuality","Normal");
			frameRate.Summary = pref.GetString("frameRate","15") + " fps";
		}

		public bool OnPreferenceClick(Preference preference){
			return true;
		}

		public bool OnPreferenceChange (Preference preference, Java.Lang.Object newValue){

			if(preference.Equals(deviceListPref)){
				preference.Summary = (string) newValue;
			}
			if(preference.Equals(portNumberPref)){
				preference.Summary = (string) newValue;
			}
			if(preference.Equals(listenForClientsPref)){
				if((bool) newValue){
					preference.Summary = "Client can connect to tunnel";
				}
				else{
					preference.Summary = "Client can not connect to tunnel";
				}
			}
			if (preference.Equals (enableStreaming)) {
				SetVideoSettingsSensitive((bool) newValue);
			}
			if (preference.Equals (rtspPort)) {
				preference.Summary = (string) newValue;
			}
			if (preference.Equals (videoPort)) {
				preference.Summary = (string) newValue;
			}
			if (preference.Equals (resolution)) {
				preference.Summary = (string) newValue;
			}
			if (preference.Equals (videoQuality)) {
				preference.Summary = (string) newValue;
			}
			if (preference.Equals (frameRate)) {
				preference.Summary = (string) newValue + " fps";
			}
			return true;
		}

		private void SetVideoSettingsSensitive(bool isSensitive){
			rtspPort.Enabled = isSensitive;
			videoPort.Enabled = isSensitive;
			resolution.Enabled = isSensitive;
			videoQuality.Enabled = isSensitive;
		}


		private void UISettingsSensitive(bool isSensitive){
			portNumberPref.Enabled = isSensitive;
			deviceListPref.Enabled = isSensitive;
			listenForClientsPref.Enabled = isSensitive;
		}

		private bool CreateDeviceList(){
			List<string> entries = new List<string>();
			foreach(BluetoothDevice device in MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices){
				entries.Add(device.Name);
			}
			deviceListPref.SetEntries(entries.ToArray());
			deviceListPref.SetEntryValues(entries.ToArray());
			string deviceName = PreferenceManager.GetDefaultSharedPreferences(this).GetString("device","");
			if(deviceName == ""){
				if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices.Count() != 0){
					deviceListPref.SetValueIndex(0);
					deviceListPref.Summary = MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices[0].Name;
				}
				else{
					if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
						deviceListPref.Summary = "No device paired with phone";
						return false;
					}
					else{
						deviceListPref.Summary = "Not available";
						return false;
					}
				}
			}
			else{
				bool foundMatch = false;
				foreach(BluetoothDevice device in MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices){
					if(device.Name == deviceName){
						foundMatch = true;
						deviceListPref.Summary = deviceName;
						deviceListPref.Value = deviceName;
					}
				}
				if(!foundMatch){
					if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices.Count() != 0){
						deviceListPref.SetValueIndex(0);
						deviceListPref.Summary = MonoBrick.Bluetooth<BrickCommand,BrickReply>.BondDevices[0].Name;
					}
					else{
						if(MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
							deviceListPref.Summary = "No device paired with phone";
							return false;
						}
						else{
							deviceListPref.Summary = "Not available";
							return false;
						}
					}	
				}
			}
			return true;
		}
	}
}


