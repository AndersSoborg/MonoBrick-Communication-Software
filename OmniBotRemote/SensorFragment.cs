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

namespace OmniBotRemote
{
		public class SensorFragment:Fragment {
			private BrickController brickController = null;
			private RemoteSettings remoteSettings = null;
			private Spinner[] sensorSpinner = new Spinner[4];
			private EditText[] sensorValue = new EditText[4];
			private Button[] sensorReadButton = new Button[4];
			private Button[] sensorSayButton = new Button[4];
		
			public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				return inflater.Inflate (Resource.Layout.sensor, container, false);
			}
			
			public override void OnPrepareOptionsMenu (IMenu menu)
			{
				menu.FindItem(Resource.Id.menuRefresh).SetVisible(true);
			}
			
			public override void OnActivityCreated (Bundle savedInstanceState)
			{
				base.OnActivityCreated (savedInstanceState);
				brickController = BrickController.Instance;
				remoteSettings = RemoteSettings.Instance;
				
				sensorSpinner[0] = this.View.FindViewById<Spinner> (Resource.Id.sensorSpinner1);
				sensorSpinner[1] = this.View.FindViewById<Spinner> (Resource.Id.sensorSpinner2);
				sensorSpinner[2] = this.View.FindViewById<Spinner> (Resource.Id.sensorSpinner3);
				sensorSpinner[3] = this.View.FindViewById<Spinner> (Resource.Id.sensorSpinner4);
			
				sensorReadButton[0] = this.View.FindViewById<Button> (Resource.Id.readButton1);
				sensorReadButton[1] = this.View.FindViewById<Button> (Resource.Id.readButton2);
				sensorReadButton[2] = this.View.FindViewById<Button> (Resource.Id.readButton3);
				sensorReadButton[3] = this.View.FindViewById<Button> (Resource.Id.readButton4);
			
				sensorSayButton[0] = this.View.FindViewById<Button> (Resource.Id.sayButton1);
				sensorSayButton[1] = this.View.FindViewById<Button> (Resource.Id.sayButton2);
				sensorSayButton[2] = this.View.FindViewById<Button> (Resource.Id.sayButton3);
				sensorSayButton[3] = this.View.FindViewById<Button> (Resource.Id.sayButton4);
			
				sensorValue[0] = this.View.FindViewById<EditText> (Resource.Id.sensorValue1);
				sensorValue[1] = this.View.FindViewById<EditText> (Resource.Id.sensorValue2);
				sensorValue[2] = this.View.FindViewById<EditText> (Resource.Id.sensorValue3);
				sensorValue[3] = this.View.FindViewById<EditText> (Resource.Id.sensorValue4);
			
			
				ArrayAdapter<string> adapter = new ArrayAdapter<string>(this.Activity,Android.Resource.Layout.SimpleSpinnerItem);
				string[] sensorNames = MonoBrick.NXT.Sensor.SensorDictionary.Keys.ToArray();
				foreach(string s in sensorNames){
					adapter.Add(s);
				}
				for(int i = 0; i < 4; i ++){
					sensorSpinner[i].Adapter = adapter;
				}
				sensorReadButton[0].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor1.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[0].Text = s;
						});
					});
				};
			
				sensorReadButton[1].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor2.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[1].Text = s;
						});
					});
				};
			
				sensorReadButton[2].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor3.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[2].Text = s;
						});
					});
				};
			
				sensorReadButton[3].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor4.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[3].Text = s;
						});
					});
				};
				
				sensorSayButton[0].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor1.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[0].Text = s;
							//this should not run in the gui thread
							if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
								TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
							}
						});
						
					});
				};
				
				
				sensorSayButton[1].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor2.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[1].Text = s;
							//this should not run in the gui thread
							if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
								TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
							}
						});
						
					});
				};
				
				
				sensorSayButton[2].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor3.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[2].Text = s;
							//this should not run in the gui thread
							if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
								TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
							}
						});
						
					});
				};
				
				sensorSayButton[3].Click += delegate {
					brickController.SpawnThread(delegate(){
						string s = brickController.NXT.Sensor4.ReadAsString();
						Activity.RunOnUiThread(delegate() {
							sensorValue[3].Text = s;
							//this should not run in the gui thread
							if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
								TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
							}
						});
						
					});
				};
			
				sensorSpinner[0].Id = 0;
				sensorSpinner[0].ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					Spinner spinner = (Spinner)sender;
	    			MonoBrick.NXT.Sensor newSensor = MonoBrick.NXT.Sensor.SensorDictionary[(string) spinner.GetItemAtPosition(e.Position)];
					remoteSettings.Sensor1 = (string) spinner.GetItemAtPosition(e.Position);
					brickController.SpawnThread(delegate(){
						brickController.NXT.Sensor1 = newSensor;
					});
				};
			
				sensorSpinner[1].Id = 0;
				sensorSpinner[1].ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					Spinner spinner = (Spinner)sender;
	    			MonoBrick.NXT.Sensor newSensor = MonoBrick.NXT.Sensor.SensorDictionary[(string) spinner.GetItemAtPosition(e.Position)];
					remoteSettings.Sensor2 = (string) spinner.GetItemAtPosition(e.Position);
					brickController.SpawnThread(delegate(){
						brickController.NXT.Sensor2 = newSensor;
					});
				};
			
				sensorSpinner[2].Id = 0;
				sensorSpinner[2].ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					Spinner spinner = (Spinner)sender;
	    			MonoBrick.NXT.Sensor newSensor = MonoBrick.NXT.Sensor.SensorDictionary[(string) spinner.GetItemAtPosition(e.Position)];
					remoteSettings.Sensor3 = (string) spinner.GetItemAtPosition(e.Position);
					brickController.SpawnThread(delegate(){
						brickController.NXT.Sensor3 = newSensor;
					});
				};
			
				sensorSpinner[3].Id = 0;
				sensorSpinner[3].ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					Spinner spinner = (Spinner)sender;
	    			MonoBrick.NXT.Sensor newSensor = MonoBrick.NXT.Sensor.SensorDictionary[(string) spinner.GetItemAtPosition(e.Position)];
					remoteSettings.Sensor4 = (string) spinner.GetItemAtPosition(e.Position);
					brickController.SpawnThread(delegate(){
						brickController.NXT.Sensor4 = newSensor;
					});
				};
				
			}
			
			public void EnableUi(){
				Activity.RunOnUiThread (delegate() {
					SetUiEnable(true);		
				});
			}
			
			public void DisableUi(){
				Activity.RunOnUiThread (delegate() {
					SetUiEnable(false);	
				});
			}
			
			public void SetUiEnable (bool enable)
			{
				for(int i = 0; i < 4; i++){
					sensorSpinner[i].Enabled = enable;
					sensorValue[i].Enabled = enable;
					sensorReadButton[i].Enabled = enable;
					sensorSayButton[i].Enabled = enable;
				}
				
				if(!(enable && TabActivity.Speech != null && remoteSettings.SensorValueToSpeech)){
					for(int j = 0; j < 4; j++){
						sensorSayButton[j].Enabled = false;	
					}
				}
			}
			
			public override void OnResume ()
			{
				string[] sensorNames = MonoBrick.NXT.Sensor.SensorDictionary.Keys.ToArray();
				string[] sensorSettings = new string[]{remoteSettings.Sensor1, remoteSettings.Sensor2, remoteSettings.Sensor3, remoteSettings.Sensor4};
				for(int i = 0; i < 4; i ++){
					int idx = 0;
					for(int j = 0; j < sensorNames.Length;j++){
						if(sensorNames[j].ToLower() == sensorSettings[i].ToLower()){
							idx = j;
							break;	 
						}
					}
					sensorSpinner[i].SetSelection(idx);
				}
				
				
				if (brickController.NXT.Connection.IsConnected) {
					SetUiEnable (true);
				} 
				else {
					SetUiEnable (false);
				}
				base.OnResume ();
			}
			
			public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
			{
				inflater.Inflate (Resource.Menu.vehicle,menu);
			}
			
		}
}

