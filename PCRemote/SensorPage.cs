using System;
using MonoBrick;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;
using ComboBoxExtensions;
using Log;
using System.Configuration;

public partial class MainWindow
{
	private System.Timers.Timer pollTimer = new System.Timers.Timer();
	private SensorLog sensorLog = new SensorLog();
	private bool initializeSensorsOnchange = false;


	#region log FileButton
	private Gtk.Image logFileButImage = new Gtk.Image(Stock.Open, IconSize.Button);
	private Label logFileButLabel = new Label("Choose file...");
	private void ShowLogFileButton(){
		HBox box = new HBox();
        box.PackStart(logFileButImage, true, true, 0);
        box.PackStart(logFileButLabel, true, true, 0);
        setSensorLogNameButton.Add(box);
		setSensorLogNameButton.ShowAll();	
	}
	#endregion

	#region Update Sensor readings
	private string[] sensorValue = new string[4];
	private void UpdataGUISensorValues(string[] val){
		Gtk.Application.Invoke (delegate {
      			if(val[0] != "")
					sensor1ValueEntry.Text = val[0];
				if(val[1] != "")
					sensor2ValueEntry.Text = val[1];
				if(val[2] != "")
					sensor3ValueEntry.Text = val[2];
				if(val[3] != "")
					sensor4ValueEntry.Text = val[3];
		});	
	}
	
	private void UpdateSensorValues(){
    	for(int i = 0; i < 4; i++)
			sensorValue[i] = "";
		SpawnThread(delegate()
	    {
			if(readSensor1checkbutton.Active){
				sensorValue[0] = brick.Sensor1.ReadAsString();
			}
			if(readSensor2checkbutton.Active){
				sensorValue[1] = brick.Sensor2.ReadAsString();
			}
			if(readSensor3checkbutton.Active){
				sensorValue[2] = brick.Sensor3.ReadAsString();
			}
			if(readSensor4checkbutton.Active){
				sensorValue[3] = brick.Sensor4.ReadAsString();
			}
			if(sensorLogCheckbutton.Active){
				sensorLog.Write(sensorValue);
			}
			UpdataGUISensorValues(sensorValue);
		}, false);
		
	}
	#endregion
	
	#region enable/disable sensor selection

	void SetSensorSelection(bool set){
		sensor1Combobox.Sensitive = set;
		sensor2Combobox.Sensitive = set;
		sensor3Combobox.Sensitive = set;
		sensor4Combobox.Sensitive = set;
		readSensor1Button.Sensitive = set;
		readSensor2Button.Sensitive = set;
		readSensor3Button.Sensitive = set;
		readSensor4Button.Sensitive = set;
	}
	#endregion

	private void SetPollSensorUi(bool enableLogOption, string toggleText, bool showToggle, bool sensorSelection){
		setSensorLogNameButton.Sensitive = enableLogOption;
		pollOptionsCombobox.Sensitive = enableLogOption;
		sensorLogCheckbutton.Sensitive = enableLogOption;
		readSensor1checkbutton.Sensitive = sensorSelection;
		readSensor2checkbutton.Sensitive = sensorSelection;
		readSensor3checkbutton.Sensitive = sensorSelection;
		readSensor4checkbutton.Sensitive = sensorSelection;
		readSensorTogglebutton.Sensitive = showToggle;
		readSensorTogglebutton.Label = toggleText;

	}
	private string sensorLogFileName = "";
	protected void OnSetLogFileNameClicked (object sender, System.EventArgs e)
	{
		Gtk.Application.Invoke (delegate {
			Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog("Save log file",
			                            this,
			                            FileChooserAction.Save,
			                            Gtk.Stock.Cancel,ResponseType.Cancel,
			                            Gtk.Stock.Ok,ResponseType.Accept);
			fc.CurrentName = "log.Log";
			if (fc.Run() == (int)ResponseType.Accept) 
			{
					logFileButImage.Stock = Stock.File;
					logFileButLabel.Text = fc.Filename.Replace(fc.CurrentFolder,"");
	                //logFileButLabel.Text.Replace("/","").Replace("//","").Replace(@"\","");
					logFileButLabel.Text = Regex.Replace(logFileButLabel.Text, @"\\", @"");
					if(logFileButLabel.Text.Length > 13){
						logFileButLabel.Text = logFileButLabel.Text.Remove(13) + "...";		
					}
					sensorLogFileName = fc.Filename;
					sensorLogCheckbutton.Active = true;
			}
			fc.Destroy();
		});
	}


	protected void OnSensor1ComboboxChanged (object sender, System.EventArgs e)
	{
		if(brick == null)
			return;
		settings.Sensor1 = (string) sensor1Combobox.GetActiveValue(1);
		settings.Save();
		brick.Sensor1 = (MonoBrick.NXT.Sensor) sensor1Combobox.GetActiveValue();
		if(initializeSensorsOnchange){
			SpawnThread(delegate()
	        {
	 			brick.Sensor1.Initialize();
			});
		}
		Gtk.Application.Invoke (delegate {
				sensor1ValueEntry.Text = "";	
		});
		
	}
	
	protected void OnSensor2ComboboxChanged (object sender, System.EventArgs e)
	{
		if(brick == null)
			return;
		settings.Sensor2 = (string) sensor2Combobox.GetActiveValue(1);
		settings.Save();
		brick.Sensor2 = (MonoBrick.NXT.Sensor) sensor2Combobox.GetActiveValue();
		if(initializeSensorsOnchange){
			SpawnThread(delegate()
			            {
				brick.Sensor2.Initialize();
			});
		}
		Gtk.Application.Invoke (delegate {
				sensor2ValueEntry.Text = "";	
		});
	}
	
	protected void OnSensor3ComboboxChanged (object sender, System.EventArgs e)
	{
		if(brick == null)
			return;
		settings.Sensor3 = (string) sensor3Combobox.GetActiveValue(1);
		settings.Save();
		brick.Sensor3 = (MonoBrick.NXT.Sensor) sensor3Combobox.GetActiveValue();
		if(initializeSensorsOnchange){
			SpawnThread(delegate()
			            {
				brick.Sensor3.Initialize();
			});
		}
		Gtk.Application.Invoke (delegate {
				sensor3ValueEntry.Text = "";	
		});
	}
	
	protected void OnSensor4ComboboxChanged (object sender, System.EventArgs e)
	{
		if(brick == null)
			return;
		settings.Sensor4 = (string) sensor4Combobox.GetActiveValue(1);
		settings.Save();
		brick.Sensor4 = (MonoBrick.NXT.Sensor) sensor4Combobox.GetActiveValue();
		if(initializeSensorsOnchange){
			SpawnThread(delegate()
			            {
				brick.Sensor4.Initialize();
			});
		}
		Gtk.Application.Invoke (delegate {
				sensor4ValueEntry.Text = "";	
		});
	}
	
	protected void OnReadSensorTogglebuttonClicked (object sender, System.EventArgs e)
	{
		if(readSensorTogglebutton.Label ==  "Poll"){
			if(!readSensor1checkbutton.Active && !readSensor2checkbutton.Active && 
			   !readSensor3checkbutton.Active && !readSensor4checkbutton.Active){
				Gtk.Application.Invoke (delegate {//No sensor was selected
              				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease select sensors to poll");
            				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run ();
            				md.Destroy();
       			});
				return;
			}
			int msPollInterval = (int) pollOptionsCombobox.GetActiveValue();
			
			if(sensorLogCheckbutton.Active){
				if(sensorLogFileName == ""){
					Gtk.Application.Invoke (delegate {//No file was selected
      				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease enter a file name...");
    				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
					md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
					md.Run ();
    				md.Destroy();
				});
					return;
				}
				else{
					sensorLog.Open(sensorLogFileName);
				}
			}
			pollTimer.Interval = msPollInterval;
			pollTimer.Enabled = true;
			Gtk.Application.Invoke (delegate {
				SetSensorSelection(false);
				if(sensorLogCheckbutton.Active)
					SetPollSensorUi(false,"Stop",true,false);
				else
					SetPollSensorUi(false,"Stop",true,true);
			});
		}
		else{
			pollTimer.Enabled = false;
			if(sensorLog.IsOpen){
				sensorLog.Close();
			}
			Gtk.Application.Invoke (delegate {
				SetSensorSelection(true);
				SetPollSensorUi(true,"Poll",true,true);
			});
		}
	}

	protected void OnReadSensor1ButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
	    {
			string s = brick.Sensor1.ReadAsString();
			Gtk.Application.Invoke (delegate {
				sensor1ValueEntry.Text = s;
			});
		});
	}

	protected void OnReadSensor2ButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
	    {
			string s = brick.Sensor2.ReadAsString();
			Gtk.Application.Invoke (delegate {
				sensor2ValueEntry.Text = s;
			});
		});
	}


	protected void OnReadSensor3ButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
	    {
			string s = brick.Sensor3.ReadAsString();
			Gtk.Application.Invoke (delegate {
				sensor3ValueEntry.Text = s;
			});
		});
	}
	protected void OnReadSensor4ButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
	    {
			string s = brick.Sensor4.ReadAsString();
			Gtk.Application.Invoke (delegate {
				sensor4ValueEntry.Text = s;
			});
		});
	}

	protected void LoadSensorPageSettings(){
		sensor1Combobox.SetActiveValue(settings.Sensor1);
		sensor2Combobox.SetActiveValue(settings.Sensor2);
		sensor3Combobox.SetActiveValue(settings.Sensor3);
		sensor4Combobox.SetActiveValue(settings.Sensor4);
		pollOptionsCombobox.SetActiveValue(settings.PollOption);
		sensorLogCheckbutton.Active = settings.SaveLog;
		readSensor1checkbutton.Active = settings.ReadSensor1;
		readSensor2checkbutton.Active = settings.ReadSensor2;
		readSensor3checkbutton.Active = settings.ReadSensor3;
		readSensor4checkbutton.Active = settings.ReadSensor4;
	}

	protected void OnReadSensor1checkbuttonToggled (object sender, EventArgs e)
	{
		settings.ReadSensor1 = readSensor1checkbutton.Active;
		settings.Save();
	}

	protected void OnReadSensor3checkbuttonToggled (object sender, EventArgs e)
	{
		settings.ReadSensor3 = readSensor3checkbutton.Active;
		settings.Save();
	}
	protected void OnReadSensor2checkbuttonToggled (object sender, EventArgs e)
	{
		settings.ReadSensor2 = readSensor2checkbutton.Active;
		settings.Save();
	}

	protected void OnReadSensor4checkbuttonToggled (object sender, EventArgs e)
	{
		settings.ReadSensor4 = readSensor4checkbutton.Active;
		settings.Save();
	}
	protected void OnPollOptionsComboboxChanged (object sender, EventArgs e)
	{
		settings.PollOption = (string) pollOptionsCombobox.GetActiveValue(1);
		settings.Save();

	}

	protected void OnSensorLogCheckbuttonToggled (object sender, EventArgs e)
	{
		settings.SaveLog = sensorLogCheckbutton.Active;
		settings.Save();
	}
}

