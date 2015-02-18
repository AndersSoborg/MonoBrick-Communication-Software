using System;
using MonoBrick;
using Gtk;
using Gdk;
using System.Configuration;

public partial class MainWindow
{
	private void DisableMotorTachoTimers(){
		motorATachoTimer.Enabled = false;
		motorBTachoTimer.Enabled = false;
		motorCTachoTimer.Enabled = false;
	}
	private void EnableMotorTachoTimers(){
		motorATachoTimer.Enabled = true;
		motorBTachoTimer.Enabled = true;
		motorCTachoTimer.Enabled = true;
	}

	private void EnableTachoUserInput(){
		SetTachoUserInput(true);
	}

	private void DisableTachoUserInput(){
		SetTachoUserInput(false);
	} 

	private void SetTachoUserInput(bool set){
		motorATachoEntry.Sensitive = set;
		motorBTachoEntry.Sensitive = set;
		motorCTachoEntry.Sensitive = set;
		motorAReadTachoButton.Sensitive = set;
		motorBReadTachoButton.Sensitive = set;
		motorCReadTachoButton.Sensitive = set;
		motorAMoveToButton.Sensitive = set;
		motorBMoveToButton.Sensitive = set;
		motorCMoveToButton.Sensitive = set;
		motorAResetTachoButton.Sensitive = set;
		motorBResetTachoButton.Sensitive = set;
		motorCResetTachoButton.Sensitive = set;
	}



	#region Motor A
	public System.Timers.Timer motorATachoTimer = new System.Timers.Timer();
	private int lastTachoValueMotorA = 0;
	private void OnUpdateTachoMotorA(object source, System.Timers.ElapsedEventArgs e){
		SpawnThread(delegate()
        {
			MonoBrick.NXT.OutputState	state = new MonoBrick.NXT.OutputState();
			bool ok = true;
			int tacho;
			try{
				state = brick.MotorA.GetOutputState();
				tacho = state.RotationCount;
			}
			catch(Exception){
				ok = false;
				tacho = lastTachoValueMotorA;
			}
			if(ok && (state.RunState == MonoBrick.NXT.MotorRunState.Idle && lastTachoValueMotorA == tacho) || (state.Speed == 0 && state.RunState == MonoBrick.NXT.MotorRunState.Running)){
				motorATachoTimer.Enabled = false;
				//Console.WriteLine("Tacho timer is off");
			}
			lastTachoValueMotorA = tacho;
			Gtk.Application.Invoke (delegate {
				motorATachoEntry.Text = tacho.ToString();	
			});
		},false);
	}

	protected void OnMotorAFwdButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorA.On((sbyte)motorASpeedScale.Value);
			if(!(pollTimer.Enabled && sensorLogCheckbutton.Active) && isConnectedViaUSB()){
				motorATachoTimer.Enabled = true;
			}

		});
	}

	protected void OnMotorARevButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorA.On( (sbyte)-motorASpeedScale.Value);
			if(! (pollTimer.Enabled && sensorLogCheckbutton.Active) && isConnectedViaUSB()){
				motorATachoTimer.Enabled = true;
			}

		});
	}	

	protected void OnMotorABrakeButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorA.Brake();	
		});
	}

	protected void OnMotorAOffButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorA.Off();	
		});
	}

	protected void OnMotorAFwdButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorAStopCheckbutton.Active){
				brick.MotorA.Off();	
			}				
		});
	}

	protected void OnMotorARevButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorAStopCheckbutton.Active){
				brick.MotorA.Off();	
			}
		});
	}	

	protected void OnMotorAReadTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			int tacho = brick.MotorA.GetTachoCount();
			//Console.WriteLine(tacho);
			Gtk.Application.Invoke (delegate {
				motorATachoEntry.Text = tacho.ToString();	
			});

		});
	}

	protected void OnMotorAResetTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			brick.MotorA.ResetTacho(true);
			Gtk.Application.Invoke (delegate {
				motorATachoEntry.Text = "0";	
			});

		});
	}

	protected void OnMotorAMoveToButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			Int32 target;

			if(!Int32.TryParse(motorATachoEntry.Text,out target)){
				Gtk.Application.Invoke (delegate {
              				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease enter a valid tacho value");
            				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run ();
            				md.Destroy();
       			});
				return;
			}
			brick.MotorA.MoveTo((byte)motorASpeedScale.Value, target, true);
			motorATachoTimer.Enabled = true;
		});
	}

	#endregion

	#region Motor B
	public System.Timers.Timer motorBTachoTimer = new System.Timers.Timer();
	private int lastTachoValueMotorB = 0;
	private void OnUpdateTachoMotorB(object source, System.Timers.ElapsedEventArgs e){
		SpawnThread(delegate()
        {
			MonoBrick.NXT.OutputState	state = new MonoBrick.NXT.OutputState();
			bool ok = true;
			int tacho;
			try{
				state = brick.MotorB.GetOutputState();
				tacho = state.RotationCount;
			}
			catch(Exception){
				ok = false;
				tacho = lastTachoValueMotorB;
			}
			if(ok && (state.RunState == MonoBrick.NXT.MotorRunState.Idle && lastTachoValueMotorB == tacho) || (state.Speed == 0 && state.RunState == MonoBrick.NXT.MotorRunState.Running)){
				motorBTachoTimer.Enabled = false;
				//Console.WriteLine("Tacho timer is off");
			}
			lastTachoValueMotorB = tacho;
			Gtk.Application.Invoke (delegate {
				motorBTachoEntry.Text = tacho.ToString();	
			});
		},false);
	}



	protected void OnMotorBFwdButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorB.On((sbyte)motorBSpeedScale.Value);
			if(! (pollTimer.Enabled && sensorLogCheckbutton.Active) && isConnectedViaUSB()){
					motorBTachoTimer.Enabled = true;
			}

		});
	}

	protected void OnMotorBRevButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorB.On( (sbyte)-motorBSpeedScale.Value);
			if(! (pollTimer.Enabled && sensorLogCheckbutton.Active)  && isConnectedViaUSB()){
				motorBTachoTimer.Enabled = true;
			}
		});
	}	

	protected void OnMotorBBrakeButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorB.Brake();	
		});
	}

	protected void OnMotorBOffButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorB.Off();	
		});
	}

	protected void OnMotorBFwdButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorBStopCheckbutton.Active){
				brick.MotorB.Off();	
			}				
		});
	}

	protected void OnMotorBRevButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorBStopCheckbutton.Active){
				brick.MotorB.Off();	
			}
		});
	}	

	protected void OnMotorBReadTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			int tacho = brick.MotorB.GetTachoCount();
			//Console.WriteLine(tacho);
			Gtk.Application.Invoke (delegate {
				motorBTachoEntry.Text = tacho.ToString();	
			});

		});
	}

	protected void OnMotorBResetTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			brick.MotorB.ResetTacho(true);
			Gtk.Application.Invoke (delegate {
				motorBTachoEntry.Text = "0";	
			});

		});
	}

	protected void OnMotorBMoveToButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			Int32 target;

			if(!Int32.TryParse(motorBTachoEntry.Text,out target)){
				Gtk.Application.Invoke (delegate {
              				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease enter a valid tacho value");
            				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run ();
            				md.Destroy();
       			});
				return;
			}
			brick.MotorB.MoveTo((byte)motorBSpeedScale.Value, target, true);
			motorBTachoTimer.Enabled = true;
		});
	}

	#endregion

	#region Motor C
	public System.Timers.Timer motorCTachoTimer = new System.Timers.Timer();
	private int lastTachoValueMotorC = 0;
	private void OnUpdateTachoMotorC(object source, System.Timers.ElapsedEventArgs e){
		SpawnThread(delegate()
        {
			MonoBrick.NXT.OutputState	state = new MonoBrick.NXT.OutputState();
			bool ok = true;
			int tacho;
			try{
				state = brick.MotorC.GetOutputState();
				tacho = state.RotationCount;
			}
			catch(Exception){
				ok = false;
				tacho = lastTachoValueMotorC;
			}
			if(ok && (state.RunState == MonoBrick.NXT.MotorRunState.Idle && lastTachoValueMotorC == tacho) || (state.Speed == 0 && state.RunState == MonoBrick.NXT.MotorRunState.Running)){
				motorCTachoTimer.Enabled = false;
				//Console.WriteLine("Tacho timer is off");
			}
			lastTachoValueMotorC = tacho;
			Gtk.Application.Invoke (delegate {
				motorCTachoEntry.Text = tacho.ToString();	
			});
		},false);
	}



	protected void OnMotorCFwdButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorC.On((sbyte)motorCSpeedScale.Value);
			if(! (pollTimer.Enabled && sensorLogCheckbutton.Active)  && isConnectedViaUSB()){
				motorCTachoTimer.Enabled = true;
			}

		});
	}

	protected void OnMotorCRevButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorC.On( (sbyte)-motorCSpeedScale.Value);
			if(! (pollTimer.Enabled && sensorLogCheckbutton.Active)  && isConnectedViaUSB()){
				motorCTachoTimer.Enabled = true;
			}
		});
	}	

	protected void OnMotorCBrakeButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorC.Brake();	
		});
	}

	protected void OnMotorCOffButtonPressed (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			brick.MotorC.Off();	
		});
	}

	protected void OnMotorCFwdButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorCStopCheckbutton.Active){
				brick.MotorC.Off();	
			}				
		});
	}

	protected void OnMotorCRevButtonReleased (object sender, System.EventArgs e)
	{
		SpawnThread(delegate()
        {
			if(motorCStopCheckbutton.Active){
				brick.MotorC.Off();	
			}
		});
	}	

	protected void OnMotorCReadTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			int tacho = brick.MotorC.GetTachoCount();
			//Console.WriteLine(tacho);
			Gtk.Application.Invoke (delegate {
				motorCTachoEntry.Text = tacho.ToString();	
			});

		});
	}

	protected void OnMotorCResetTachoButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			brick.MotorC.ResetTacho(true);
			Gtk.Application.Invoke (delegate {
				motorCTachoEntry.Text = "0";	
			});

		});
	}

	protected void OnMotorCMoveToButtonPressed(object sender, System.EventArgs e){
		SpawnThread(delegate()
        {
			Int32 target;

			if(!Int32.TryParse(motorCTachoEntry.Text,out target)){
				Gtk.Application.Invoke (delegate {
              				MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "\nPlease enter a valid tacho value");
            				md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run ();
            				md.Destroy();
       			});
				return;
			}
			brick.MotorC.MoveTo((byte)motorCSpeedScale.Value, target, true);
			motorCTachoTimer.Enabled = true;
		});
	}

	protected void OnMotorBStopCheckbuttonToggled (object sender, EventArgs e)
	{
		settings.MotorBStop = motorBStopCheckbutton.Active;
		settings.Save();
	}

	protected void OnMotorCStopCheckbuttonToggled (object sender, EventArgs e)
	{
		settings.MotorCStop = motorCStopCheckbutton.Active;
		settings.Save();
	}

	protected void OnMotorAStopCheckbuttonToggled (object sender, EventArgs e)
	{
		settings.MotorAStop = motorAStopCheckbutton.Active;
		settings.Save();
	}

	protected void OnMotorCSpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.MotorCSpeed = (float) motorCSpeedScale.Adjustment.Value;
		settings.Save();
	}

	protected void OnMotorBSpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.MotorBSpeed = (float) motorBSpeedScale.Adjustment.Value;
		settings.Save();
		
	}
	protected void OnMotorASpeedScaleValueChanged (object sender, EventArgs e)
	{
		settings.MotorASpeed = (float) motorASpeedScale.Adjustment.Value;
		settings.Save();
	}

	#endregion

	protected void LoadMotorPageSettings(){
		motorAStopCheckbutton.Active = settings.MotorAStop;
		motorBStopCheckbutton.Active = settings.MotorBStop;
		motorCStopCheckbutton.Active = settings.MotorCStop;
		motorASpeedScale.Adjustment.Value = settings.MotorASpeed;
		motorBSpeedScale.Adjustment.Value = settings.MotorBSpeed;
		motorCSpeedScale.Adjustment.Value = settings.MotorCSpeed;
	}
}

