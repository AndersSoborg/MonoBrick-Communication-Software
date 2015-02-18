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
using System.IO;

namespace OmniBotRemote
{
		[Activity (MainLauncher=true)]			
		public class TabActivity : Activity, Android.Speech.Tts.TextToSpeech.IOnInitListener
		{
			public static TextToSpeech Speech = null;
			private BrickController brickController = null;
			private static bool hasSubscribed = false;
			private ActionBar.Tab vehicleTab = null;
			private ActionBar.Tab sensorTab = null;
			private ActionBar.Tab motorTab = null;
			private ActionBar.Tab mailboxTab = null;
			private ActionBar.Tab fileTab = null;
			private Fragment vehicleFragment = null;
			private Fragment sensorFragment = null;
			private FileListFragment fileFragment = null;
			private IMenuItem connectMenu = null;
			//private IMenuItem settingsMenu = null;
			private RemoteSettings settings = null;
			private delegate void SetSensor(MonoBrick.NXT.Sensor sensor);
			private bool bluetoothDialogIsshowing = false;
			protected override void OnCreate (Bundle savedInstanceState) {
				base.OnCreate(savedInstanceState);
				base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
				brickController = BrickController.Instance;
				settings = RemoteSettings.Instance;
				SetContentView(Resource.Layout.tab);

				ActionBar actionBar = this.ActionBar;
				//actionBar.SetDisplayShowTitleEnabled (false);
				//actionBar.SetDisplayShowHomeEnabled (false);
				actionBar.NavigationMode = ActionBarNavigationMode.Tabs;
				
				vehicleTab = actionBar.NewTab ().SetText ("Vehicle");
				sensorTab = actionBar.NewTab ().SetText ("Sensor");
				motorTab = actionBar.NewTab ().SetText ("Motor");
				fileTab = actionBar.NewTab().SetText("Files");
				mailboxTab = actionBar.NewTab ().SetText ("Mailbox");
				
				vehicleFragment = new VehicleFragment();
				fileFragment = new FileListFragment();
				sensorFragment = new SensorFragment();
				Fragment empty = new Fragment ();

				vehicleTab.SetTabListener (new MyTabsListener(vehicleFragment)); 
				sensorTab.SetTabListener (new MyTabsListener(sensorFragment)); 
				motorTab.SetTabListener(new MyTabsListener(empty )); 
				fileTab.SetTabListener(new MyTabsListener(fileFragment)); 
				mailboxTab.SetTabListener (new MyTabsListener(empty));
				
				settings.Initialize(GetSharedPreferences ("RemoteSettings", 0));
				
				actionBar.AddTab(vehicleTab);
				actionBar.AddTab(sensorTab);
				//actionBar.AddTab(motorTab);
				actionBar.AddTab(fileTab);
				//actionBar.AddTab(mailboxTab);
				if(!hasSubscribed){
					brickController.NewBrick += delegate(MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor> nxt) {
						nxt.Connection.Disconnected += OnDisconnected;
						nxt.Connection.Connected += OnConnected;
					};
					brickController.BrickException += OnBrickException;
					brickController.ThreadNotStarted += OnThreadOnstarted;
					hasSubscribed = true;
				}
			}
			
			protected override void OnResume ()
			{
				base.OnResume ();
				SetupBluetooth();
				if(RemoteSettings.Instance.SensorValueToSpeech){
					StartTextToSpeech();
				}
				else{
					StopTextToSpeech();
				}
				
			}
			
			public void OnBrickException (Exception e)
			{
				RunOnUiThread(delegate() {
					ShowToast(this, e.Message, ToastLength.Long);
					if(e is MonoBrick.MonoBrickException){
					
					}
					if(e is MonoBrick.ConnectionException){
						brickController.NXT.Connection.Close();
					}
				});
			}
			
			public void OnThreadOnstarted()
			{
				RunOnUiThread(delegate() {
					ShowToast(this, "Unable to send command to brick.\nAnother command is pending", ToastLength.Long);
				});
			}
			
			
			public void OnDisconnected(){
				RunOnUiThread (delegate {
					connectMenu.SetIcon(Resource.Drawable.bluetooth_searching);
					ActionBar.SelectTab(ActionBar.SelectedTab);
				});

			}
			
			public void OnConnected(){
				RunOnUiThread (delegate {
					connectMenu.SetIcon(Resource.Drawable.bluetooth_connected);
					ActionBar.SelectTab(ActionBar.SelectedTab);
				});
				
			}
			
			public override bool OnCreateOptionsMenu (IMenu menu)
			{
				base.OnCreateOptionsMenu(menu);
				MenuInflater inflater = this.MenuInflater;
				inflater.Inflate (Resource.Menu.vehicle, menu);
				connectMenu = menu.FindItem (Resource.Id.menuConnection);
				//settingsMenu = menu.FindItem (Resource.Id.menuSettings);
				return base.OnCreateOptionsMenu(menu);
			}
			
			public override bool OnPrepareOptionsMenu (IMenu menu)
			{
				connectMenu = menu.FindItem (Resource.Id.menuConnection);
				//settingsMenu = menu.FindItem (Resource.Id.menuSettings);
				menu.FindItem(Resource.Id.menuRefresh).SetVisible(false);
				menu.FindItem(Resource.Id.menuUpload).SetVisible(false);
				menu.FindItem(Resource.Id.menuFormat).SetVisible(false);
				
				
				if (BrickController.Instance.NXT.Connection.IsConnected) {
					connectMenu.SetTitle ("Disconnect");
				} 
				else {
					connectMenu.SetTitle ("Connect");
				}
				return base.OnPrepareOptionsMenu (menu);
				
			}
			
				public void Connect ()
				{
						var device = MonoBrick.Bluetooth<MonoBrick.EV3.Command, MonoBrick.EV3.Reply>.GetBondDevice("youDeviceName");
						var connection = new Bluetooth<MonoBrick.EV3.Command, MonoBrick.EV3.Reply>(device);
						var brick = new MonoBrick.EV3.Brick<MonoBrick.EV3.Sensor,MonoBrick.EV3.Sensor,MonoBrick.EV3.Sensor,MonoBrick.EV3.Sensor>(connection);
						brick.Connection.Open();
				
				
				}
			

			public override bool OnOptionsItemSelected (IMenuItem item)
			{
				switch (item.ItemId) {
					case Resource.Id.menuConnection:
						if(brickController.NXT.Connection.IsConnected){
							ProgressDialog progress = null;
							RunOnUiThread(delegate() {
								progress = ProgressDialog.Show(this,"Disconnecting From Brick...","Closing connection...       ");
							});
							System.Threading.Thread t =  new System.Threading.Thread( delegate(object obj){
								try{
									brickController.NXT.Connection.Close(); 
								}
								catch{
								
								}
								RunOnUiThread(delegate() {
									progress.Dismiss();
									ShowToast(this,"Disconnected from brick");
								});
							});
							t.IsBackground = true;
							t.Priority = System.Threading.ThreadPriority.Normal;
							t.Start();
						}
						else{
							ProgressDialog progress = null;
							System.Threading.Thread t = new System.Threading.Thread(delegate(object obj) {
								if(settings.DeviceName == ""){
									RunOnUiThread(delegate() {
										AlertDialog dialog = new AlertDialog.Builder(this).Create();
										dialog.SetTitle("Select Bluetooth Device");
										dialog.SetMessage("No Bluetooth Device has been selected. Please go to settings and select a Bluetooth device");
										dialog.SetButton("OK", delegate(object sen, DialogClickEventArgs eventArgs) {
										});
										dialog.SetIcon(Android.Resource.Drawable.IcDialogInfo);
										dialog.Show();
									});
									return;
								}
								RunOnUiThread(delegate() {
									progress =	ProgressDialog.Show(this,"Connecting To Brick...","Opening connection...       ",true,false);
								});
								try{
									if(settings.Type == BrickType.NXT){
									brickController.NXT = new MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor>(
												//new MonoBrick.Loopback()
												new MonoBrick.Bluetooth<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>(MonoBrick.Bluetooth<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>.GetBondDevice(settings.DeviceName))
												);
									}	
								
									brickController.NXT.Connection.Open();
								}
								catch(Exception ex){
									RunOnUiThread(delegate() {
										progress.Dismiss();
										ShowToast(this,"Error connecting: " + ex.Message, ToastLength.Long);
									});
									return;
								}
								Dictionary<string,MonoBrick.NXT.Sensor> sensorDictionary = MonoBrick.NXT.Sensor.SensorDictionary;
								string[] sensorName = new string[]{settings.Sensor1, settings.Sensor2, settings.Sensor3, settings.Sensor4};
								MonoBrick.NXT.Sensor sensorType = new MonoBrick.NXT.NoSensor();
								try{
									SetSensor[] setSensor = new SetSensor[]{
										delegate(MonoBrick.NXT.Sensor sensor){brickController.NXT.Sensor1 = sensor; brickController.NXT.Sensor1.Initialize();},
										delegate(MonoBrick.NXT.Sensor sensor){brickController.NXT.Sensor2 = sensor; brickController.NXT.Sensor2.Initialize();},
										delegate(MonoBrick.NXT.Sensor sensor){brickController.NXT.Sensor3 = sensor; brickController.NXT.Sensor3.Initialize();},
										delegate(MonoBrick.NXT.Sensor sensor){brickController.NXT.Sensor4 = sensor; brickController.NXT.Sensor4.Initialize();},
									};
									//Check if sensors should be initialized with something other than none
									
									for(int i = 0; i < 4; i++){ 
										try{
											if(!sensorDictionary.TryGetValue(sensorName[i], out sensorType)){
												//could not load a value
												sensorName[i] = "None"; 
												sensorType = new MonoBrick.NXT.NoSensor();
											}
											RunOnUiThread(delegate() {
													progress.SetMessage("initialize sensor " + (i+1) + " as " + sensorName[i]);
											});
											setSensor[i](sensorType);
										}
										catch(MonoBrickException nxtEx){
											if(nxtEx is MonoBrick.NXT.BrickException){
												RunOnUiThread(delegate() {
													progress.SetMessage("Sensor " + (i+1) + " initialized as \"no-sensor\"");
												});
												sensorName[i] = "None";
												setSensor[i](new MonoBrick.NXT.Sensor(MonoBrick.NXT.SensorType.NoSensor,MonoBrick.NXT.SensorMode.Raw));
											}
											else{
												RunOnUiThread(delegate() {
													progress.Dismiss();
													ShowToast(this,"Failed to initialize sensors.\n" + nxtEx.Message);
													OnDisconnected();
												});
												brickController.NXT.Connection.Close();
												return;
											}
										}
									}
								}
								catch(MonoBrick.NXT.BrickException ex){
									RunOnUiThread(delegate() {
											progress.Dismiss();
											ShowToast(this,"Failed to initialize sensors.\n" + ex.Message);
									});
									brickController.NXT.Connection.Close();
									return;
								}
								RunOnUiThread(delegate() {
									progress.Dismiss();
									ShowToast(this,"Connected successfully to brick");
								});
							});
							t.IsBackground = true;
							t.Priority = System.Threading.ThreadPriority.Normal;
							t.Start();
						}
					break;
					case Resource.Id.menuSettings:
						StartActivity(new Intent(this,typeof(SettingsActivity)));
					break;
					case Resource.Id.menuRefresh:
						fileFragment.ReloadFileList();
					break;
					case Resource.Id.menuUpload:
						fileFragment.UploadFile();
					break;
					case Resource.Id.menuFormat:
						fileFragment.Format();
					break;
					
					default:
						return base.OnOptionsItemSelected (item);
			
				}
				return true;
			}
			
			protected override void OnDestroy ()
			{
				StopTextToSpeech();
				base.OnDestroy ();
			}
			
			private void StopTextToSpeech(){
				if(Speech != null){
					Speech.Shutdown();
					Speech = null;
				}	
			}
			
			private void StartTextToSpeech ()
			{
				if(Speech == null){
					Intent checkIntent = new Intent();
        			checkIntent.SetAction(TextToSpeech.Engine.ActionCheckTtsData);
        			StartActivityForResult(checkIntent, 100);
        		}
			}
			
			public void OnInit(Android.Speech.Tts.OperationResult result){
				Console.WriteLine(Speech.IsLanguageAvailable(Java.Util.Locale.English));
				Speech.SetLanguage(Java.Util.Locale.English);
			}
			
			protected override void OnActivityResult (int requestCode, Result resultCode, Intent data){
	    		base.OnActivityResult (requestCode, resultCode, data);
	        	if(requestCode == 100)
	        	{
	            	Speech = new TextToSpeech(this, this);
	            	Speech.SetLanguage(Java.Util.Locale.English);
	        	}
	        	else{
	        		Intent installIntent = new Intent();
            		installIntent.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
            		StartActivity(installIntent);
	        	}
	    	}
			
			
			static public void ShowToast(Activity activity, string message, ToastLength duration = ToastLength.Short){
				LayoutInflater inflater = activity.LayoutInflater;
				View layout = inflater.Inflate(Resource.Layout.toast, (ViewGroup) activity.FindViewById(Resource.Id.toast_layout_root));
				TextView text = (TextView) layout.FindViewById(Resource.Id.toast_text);
				text.SetText(message, TextView.BufferType.Normal);
				Toast toast = new Toast(activity.ApplicationContext);
				toast.SetGravity(GravityFlags.CenterVertical, 0, 0);
				toast.Duration = duration;
				toast.View = layout;
				toast.Show();
			}
			
			private void SetupBluetooth(){
				if(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled && !bluetoothDialogIsshowing){
					//Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
					//StartActivityForResult(enableBtIntent,(int) ActivityResult.BluetoothEnabled);
					bluetoothDialogIsshowing = true;
					AlertDialog.Builder dialog = new AlertDialog.Builder(this);
					dialog.SetIcon(Android.Resource.Drawable.IcMenuInfoDetails);
					dialog.SetTitle("Bluetooth permission request");
					dialog.SetMessage("This application is requesting permission to turn on Bluetooth");
	
					dialog.SetPositiveButton("Ok",delegate(object sender, DialogClickEventArgs e){	ProgressDialog progress = ProgressDialog.Show(this,"","Turning on Bluetooth...");
					System.Threading.Thread t = new System.Threading.Thread(
							new ThreadStart(
							delegate()
							{
								MonoBrick.Bluetooth<BrickCommand,BrickReply>.Enable();
								System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
								stopWatch.Start();
								while(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled && stopWatch.ElapsedMilliseconds < 5000){}
								stopWatch.Stop();
								progress.Dismiss();
								bluetoothDialogIsshowing = false;
								if(!MonoBrick.Bluetooth<BrickCommand,BrickReply>.IsEnabled){
									RunOnUiThread(delegate() {
										ShowToast(this,"Failed to turn on Bluetooth");
										//Update GUI
									});
								}
						}));
						t.IsBackground = true;
						t.Priority =  System.Threading.ThreadPriority.Normal;
						t.Start();
					});
					dialog.Show();
				}
				else{
					//Update GUI
				}
			}
			
			
		}
		
		public class VehicleFragment:Fragment {
			//private Drawable grayCircle = null;
			private ImageView circle = null;
			private AnimationDrawable circleAnimation;
			private double x;
			private double y;
			private double angle;
			private bool wasRunningForward = true;
			private float diameter;
			private float speed;
			private int progress;
			private SeekBar seekBar = null;
			private MonoBrick.IMotor additionalMotor = null;
			private TextView angleLabel = null;
			private TextView speedLabel = null;
			private TextView sliderLabel = null;
			
			private TextView angleText = null;
			private TextView speedText = null;
			private TextView sliderText = null;
			private Button sensor1 = null;
			private Button sensor2 = null;
			private Button sensor3 = null;
			private Button sensor4 = null;
			private BrickController brickController = null;
			private RemoteSettings remoteSettings = null;
			private QueueThread queue = null;
			
			public void OnDequeue (Action action)
			{
				brickController.ExecuteOnCurrentThread(action);
			}
			
			public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				return inflater.Inflate (Resource.Layout.Main, container, false);
			}
			
			public override void OnPrepareOptionsMenu (IMenu menu)
			{
				menu.FindItem(Resource.Id.menuRefresh).SetVisible(true);
			}
			
			public override void OnActivityCreated (Bundle savedInstanceState)
			{
				base.OnActivityCreated (savedInstanceState);
				queue =  new QueueThread(OnDequeue);
				brickController = BrickController.Instance;
				remoteSettings = RemoteSettings.Instance;
				circle =  (ImageView) this.View.FindViewById<ImageView> (Resource.Id.circle);
				circleAnimation = (AnimationDrawable) circle.Drawable;
				//grayCircle = Resources.GetDrawable(Resource.Drawable.aniGray);
				seekBar = (SeekBar)this.View.FindViewById<SeekBar> (Resource.Id.seekBar);
				seekBar.Max = 200;
				
				angleLabel = (TextView)this.View.FindViewById<TextView> (Resource.Id.angleLabel);
				speedLabel = (TextView)this.View.FindViewById<TextView> (Resource.Id.speedLabel);
				sliderLabel = (TextView)this.View.FindViewById<TextView> (Resource.Id.seekLabel);
				
				angleText = (TextView)this.View.FindViewById<TextView> (Resource.Id.angleTextView);
				speedText = (TextView)this.View.FindViewById<TextView> (Resource.Id.speedTextView);
				sliderText = (TextView)this.View.FindViewById<TextView> (Resource.Id.seekTextView);
				
				
				sensor1 = (Button)this.View.FindViewById<Button> (Resource.Id.sensor1Button);
				sensor2 = (Button)this.View.FindViewById<Button> (Resource.Id.sensor2Button);
				sensor3 = (Button)this.View.FindViewById<Button> (Resource.Id.sensor3Button);
				sensor4 = (Button)this.View.FindViewById<Button> (Resource.Id.sensor4Button);
				
				sensor1.Click += delegate {
					queue.AddToQueue(
						delegate {
							string s = brickController.NXT.Sensor1.ReadAsString();
							Activity.RunOnUiThread(delegate() {
								//this should not run in the gui thread
								if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
									TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
								}
							});
						},
						true,1 
					);
				};
				
				sensor2.Click += delegate {
					queue.AddToQueue(
						delegate {
							string s = brickController.NXT.Sensor2.ReadAsString();
							Activity.RunOnUiThread(delegate() {
								//this should not run in the gui thread
								if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
									TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
								}
							});
						},
						true,1
					);
				};
				
				sensor3.Click += delegate {
					queue.AddToQueue(
						delegate {
							string s = brickController.NXT.Sensor3.ReadAsString();
							Activity.RunOnUiThread(delegate() {
								//this should not run in the gui thread
								if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
									TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
								}
							});
						},
						true,1
					);
				};
				
				sensor4.Click += delegate {
					queue.AddToQueue(
						delegate {
							string s = brickController.NXT.Sensor4.ReadAsString();
							Activity.RunOnUiThread(delegate() {
								//this should not run in the gui thread
								if(remoteSettings.SensorValueToSpeech && TabActivity.Speech != null){
									TabActivity.Speech.Speak(s, Android.Speech.Tts.QueueMode.Flush, null);
								}
							});
						},
						true,1
					);
				};
				
				seekBar.Progress = 100;
				sliderText.Text = progress.ToString();
				
				seekBar.StopTrackingTouch += delegate(object sender, SeekBar.StopTrackingTouchEventArgs e) {
					e.SeekBar.Progress = 100;
					progress = 0;
					Activity.RunOnUiThread (delegate() {
						sliderText.Text = progress.ToString();
					});
					if(remoteSettings.SendVehicleDataToMailbox){
							queue.AddToQueue(
								delegate {
									SendDataToMailbox();
								},
								true,2
							);
					}
					else{
						queue.AddToQueue(
								delegate {
									additionalMotor.Off();
								},
								true,2
							);
					}	
				};
				seekBar.ProgressChanged += delegate(object sender, SeekBar.ProgressChangedEventArgs e) {
					progress = e.Progress - 100;
					Activity.RunOnUiThread (delegate() {
						sliderText.Text = progress.ToString();
					});
					if(remoteSettings.SendVehicleDataToMailbox){
							queue.AddToQueue(
								delegate {
									SendDataToMailbox();
								},
								true,2
							);
					}
					else{
						queue.AddToQueue(
								delegate {
									additionalMotor.On((sbyte) (e.Progress -100));
								},
								true,2
							);
					}	

				};
				circle.Touch += delegate(object sender, View.TouchEventArgs e) {
					if(e.Event.Action == MotionEventActions.Up){
						angleText.Text = "0.0";
						speedText.Text = "0.0";
						angle = 0.0f;
						speed = 0.0f;
						wasRunningForward = true;
						if(remoteSettings.SendVehicleDataToMailbox){
							queue.AddToQueue(
								delegate {
									SendDataToMailbox();
								},
								true,2
							);
						}
						else{
							queue.AddToQueue(
								delegate {
									brickController.NXT.Vehicle.Off();
								},
								true,3
							);
						
						
						}
						circle.SetImageResource(Resource.Drawable.animation);
						circleAnimation = (AnimationDrawable) circle.Drawable;
						circleAnimation.Start();
					}
					if(e.Event.Action == MotionEventActions.Move || e.Event.Action == MotionEventActions.Down) {
						//movingOnCircle = true;
						if(diameter == 0){
							diameter = circle.Width / 2;
						}
						x = e.Event.GetX()-diameter;
						y = -(e.Event.GetY()-diameter);
						float outside = diameter*(float)1.06;
						if( !(x > outside || x < -outside || y > outside || y < -outside) ){
							if(x > 0.0){
								angle = Math.Atan(y/x);
								if( y < 0.0)
									angle = Math.Atan(y/x)+ 2*Math.PI;
							}
							else{
								angle = Math.Atan(y/x) + Math.PI;
							}
							angle = angle + remoteSettings.DegreeOffset;
							//convert radians to degree
							angle = (angle * (180.0 / Math.PI))%360;
							speed = (float)Math.Sqrt( x*x + y*y);
							speed = speed/diameter * 100;  
							if(speed > 100.0){
								speed = (float)100.0;
							}
						}
						else{
							speed = 0.0f;
							angle = 0.0f;
						}
						
						int intSpeed =(int) speed;
						Activity.RunOnUiThread (delegate() {
						
							if(intSpeed < 10){
								circle.SetImageResource(Resource.Drawable.ani0);
							}
							else if(intSpeed < 20){
								circle.SetImageResource(Resource.Drawable.ani1);
							
							}
							else if(intSpeed < 30){
								circle.SetImageResource(Resource.Drawable.ani2);
							
							}
							else if(intSpeed < 40){
								circle.SetImageResource(Resource.Drawable.ani3);
							
							}
							else if(intSpeed < 50){
								circle.SetImageResource(Resource.Drawable.ani4);
							
							}
							else if(intSpeed < 60){
								circle.SetImageResource(Resource.Drawable.ani5);
							
							}
							else if(intSpeed < 70){
								circle.SetImageResource(Resource.Drawable.ani6);
							
							}
							else if(intSpeed < 80){
							
								circle.SetImageResource(Resource.Drawable.ani7);
							}
							else if(intSpeed < 90){
								circle.SetImageResource(Resource.Drawable.ani8);
							
							}
							else{
								circle.SetImageResource(Resource.Drawable.ani9);
							
							}
							circleAnimation.Stop();
							//angleText.Text = "Angle: " + string.Format("{0,5:###.0}", angle).PadLeft(7);
							//speedText.Text = "Speed: " + string.Format("{0,5:###.0}", speed).PadLeft(7);
							angleText.Text = string.Format("{0,5:###.0}", angle);
							speedText.Text = string.Format("{0,5:###.0}", speed);

						});
						if(remoteSettings.SendVehicleDataToMailbox){
							queue.AddToQueue(
								delegate {
									SendDataToMailbox();
								},
								true,2
							);
						}
						else{
							queue.AddToQueue(
								delegate {
									if(angle >= 10.0f && angle < 80.0f){
										brickController.NXT.Vehicle.TurnLeftForward((sbyte)speed, (sbyte)(100.0f-(angle-10.0f)/70.0f * 100.0f));
										wasRunningForward = true;
										return;
									}
									if(angle >= 100.0f && angle < 170.0f){
										brickController.NXT.Vehicle.TurnLeftReverse((sbyte)speed, (sbyte)(100.0f-(170-(angle-10.0f))/70.0f * 100.0f));
										wasRunningForward = false;
										return;
									}
									if(angle >= 190.0f && angle < 260.0f){
										brickController.NXT.Vehicle.TurnRightReverse((sbyte)speed, (sbyte)(100.0f-((angle-10.0f)-190)/70.0f * 100.0f));
										wasRunningForward = false;
										return;
									}
									if(angle >= 280.0f && angle <= 350.0f){
										brickController.NXT.Vehicle.TurnRightForward((sbyte)speed, (sbyte)(100.0f-(350-(angle-10.0f))/70.0f * 100.0f));
										wasRunningForward = true;
										return;
									}
									if( (angle < 10.0f && angle >= 0.0f) || (angle > 350.0f) ){
										brickController.NXT.Vehicle.Forward((sbyte)speed);
										return;
									}
									if( angle >= 170.0f && angle < 190.0f){
										brickController.NXT.Vehicle.Backward((sbyte)speed);
										return;
									}
									if( angle >= 80.0f && angle < 100.0f){
										if(wasRunningForward){
											brickController.NXT.Vehicle.SpinLeft((sbyte)speed);
										}
										else{
											brickController.NXT.Vehicle.SpinRight((sbyte)speed);
										}
										return;
									}
									if( angle >= 260.0f && angle < 280.0f){
										if(wasRunningForward){
											brickController.NXT.Vehicle.SpinRight((sbyte)speed);
										}
										else{
											brickController.NXT.Vehicle.SpinLeft((sbyte)speed);
										}
										return;
									}
								},
								true,3
							);
						}
					}
				};
				circleAnimation.Start();
			}
			
			private void SendDataToMailbox(){
				brickController.NXT.Mailbox.Send(
						angle.ToString(System.Globalization.CultureInfo.InvariantCulture)+ ";" + 
						speed.ToString(System.Globalization.CultureInfo.InvariantCulture)+";" + 
						progress.ToString(System.Globalization.CultureInfo.InvariantCulture) ,
				remoteSettings.VehicleMailbox);
				
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
				if(remoteSettings.SendVehicleDataToMailbox){
					sliderLabel.Text = "Slider:";
				}
				else{
					sliderLabel.Text = "Motor:";
				}
				sensor1.Enabled = enable;
				sensor2.Enabled = enable;
				sensor3.Enabled = enable;
				sensor4.Enabled = enable;
				seekBar.Enabled = enable;
				circle.Enabled = enable;
				Android.Graphics.Color color = new Android.Graphics.Color (0x99, 0x99, 0x99);
				if (!enable) {
					circle.SetImageResource(Resource.Drawable.aniGray);
					circleAnimation.Stop();
				} 
				else 
				{
					color =	new Android.Graphics.Color (0xf2, 0x61, 0x00);
					circle.SetImageResource(Resource.Drawable.animation);
					circleAnimation = (AnimationDrawable) circle.Drawable;
					circleAnimation.Start();
					if(remoteSettings.SensorValueToSpeech){
						sensor1.Enabled = true;
						sensor2.Enabled = true;
						sensor3.Enabled = true;
						sensor4.Enabled = true;
					}
					else{
						sensor1.Enabled = false;
						sensor2.Enabled = false;
						sensor3.Enabled = false;
						sensor4.Enabled = false;
					}
				}
				angleText.SetTextColor(color);
				speedText.SetTextColor(color);
				sliderText.SetTextColor(color);
				angleLabel.SetTextColor(color);
				speedLabel.SetTextColor(color);
				sliderLabel.SetTextColor(color);
			}
			
			public override void OnResume ()
			{
				angleText.Text = "0.0";
			    speedText.Text = "0.0";
				brickController.NXT.Vehicle.LeftPort = remoteSettings.LeftPort;
				brickController.NXT.Vehicle.RightPort = remoteSettings.RightPort;
				brickController.NXT.Vehicle.ReverseLeft = remoteSettings.ReverseLeft;
				brickController.NXT.Vehicle.ReverseRight = remoteSettings.ReverseRight;
				
				switch(remoteSettings.AdditionalPort){
					case MonoBrick.NXT.MotorPort.OutA:
						additionalMotor = brickController.NXT.MotorA;
					break;
					case MonoBrick.NXT.MotorPort.OutB:
						additionalMotor = brickController.NXT.MotorB;
					break;
					case MonoBrick.NXT.MotorPort.OutC:
						additionalMotor = brickController.NXT.MotorC;
					break;
				}
				additionalMotor.Reverse = remoteSettings.ReverseAdditional; 
				
				if (brickController.NXT.Connection.IsConnected) {
					SetUiEnable (true);
				} 
				else {
					SetUiEnable (false);
				}

				base.OnResume ();
			}
			
			public override void OnPause ()
			{
				angle = 0.0f;
				speed = 0.0f;
				progress = 0;
				if(brickController.NXT.Connection.IsConnected){
					queue.Restart();
					if(remoteSettings.SendVehicleDataToMailbox){
						queue.AddToQueue(
								delegate {
							        SendDataToMailbox();
								},
								true,2
							);
					}
					else{
						queue.AddToQueue(
							delegate {
								brickController.NXT.Vehicle.Off();
								additionalMotor.Off();
							},
							false,3
						);
					
					
					}
				}
				base.OnPause();
				
			}
			
			public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
			{
				inflater.Inflate (Resource.Menu.vehicle,menu);
			}
			
			
			
			public class QueueThread
			{
				private class QueueCommand{
					public int Id{get;set;}
					public bool KeepInQueue{get;set;}
					public Action Command{get;set;}
				}
				private Thread thread;
				private const int maxQueueSize = 300;
				private readonly Queue<QueueCommand> queue = new Queue<QueueCommand>();
				private readonly object queueLock = new Object();
				private readonly Semaphore queueCounter = new Semaphore(0, maxQueueSize);
				private readonly Action<Action> handler;
				private readonly System.Threading.ThreadPriority threadPriority;
				private bool threadStopped;
				private readonly ManualResetEvent queueDone =  new ManualResetEvent(false);
				
				private void ThreadMain()
				{
					while (!threadStopped)
					{
						QueueCommand item = default(QueueCommand);
						bool hasHandler = false;
						queueCounter.WaitOne();
						lock (queueLock)
						{
							if(queue.Count > 0)
							{
								item = queue.Dequeue();
								hasHandler = true;
							}
						}
						
						Thread.MemoryBarrier();
						if(hasHandler && !threadStopped)
						{
							handler(item.Command);
						}
						
						Thread.MemoryBarrier();
					}
					queueDone.Set();
				}
				
				private void CreateThread(System.Threading.ThreadPriority threadPriority)
				{
					thread = new Thread(ThreadMain);
					thread.IsBackground = true;
					thread.Priority = threadPriority;
					thread.Start();
				}
				public QueueThread(Action<Action> handler)
					: this(handler, System.Threading.ThreadPriority.Normal)
				{
					
				}
				
				public QueueThread(Action<Action> handler, System.Threading.ThreadPriority threadPriority)
				{
					if (handler == null)
					{
						throw new ArgumentNullException("handler");
					}
					
					this.handler = handler;
					this.threadPriority = threadPriority;
					
					CreateThread(threadPriority);
				}
				
				public void AddToQueue (Action action, bool replace, int id)
				{
					if (replace) {
						int queueSize = 0;
						lock (queueLock) {
								QueueCommand newCommand = new QueueCommand ();
								List<QueueCommand> l = new List<QueueCommand> ();
								foreach (QueueCommand command in queue) {
										if (command.KeepInQueue && command.Id != id)
												l.Add (command);
								}
								queue.Clear ();
								foreach (QueueCommand command in l) {
										queue.Enqueue (command);
								}
								newCommand.KeepInQueue = false;
								newCommand.Command = action;
								newCommand.Id = id;
								queue.Enqueue (newCommand);
								queueSize = l.Count + 1;
						}
						queueCounter.Release (queueSize);
					}
					else{
						lock (queueLock) {
							QueueCommand newCommand = new QueueCommand ();
							newCommand.KeepInQueue = true;
							newCommand.Command = action;
							queue.Enqueue (newCommand);
						}
						queueCounter.Release();	
					}
					Console.WriteLine(queue.Count);
				}
				
				public void Close()
				{
					threadStopped = true;
					queueCounter.Release();
					
					if(!queueDone.WaitOne(10*1000))
					{
						throw new TimeoutException("Failed to shut down queue thread in time.");
					}
				}
				
				public void Restart()
				{
					Close();
					lock (queueLock)
					{
						queue.Clear();
					}
					threadStopped = false;
					CreateThread(threadPriority);
				}
			}
		}

		class MyTabsListener: Java.Lang.Object, ActionBar.ITabListener {
			public Fragment fragment;

			public MyTabsListener(Fragment fragment) {
				this.fragment = fragment;
			}

			public void OnTabReselected (ActionBar.Tab tab, FragmentTransaction ft){
				fragment.OnResume ();
			}
			
			public void OnTabSelected (ActionBar.Tab tab, FragmentTransaction ft){
				ft.Replace (Resource.Id.fragment_container, fragment);
			}
			
			public void OnTabUnselected (ActionBar.Tab tab, FragmentTransaction ft){
				ft.Remove(fragment);
			}
				
		}



}

