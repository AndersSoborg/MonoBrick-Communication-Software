using System;
using MonoBrick;
using Gtk;
using Gdk;
using System.Text.RegularExpressions;
using ComboBoxExtensions;
using System.Configuration;
using Caprica.VlcSharp.Player;
using Caprica.VlcSharp.Player.Embedded;
using Caprica.VlcSharp.Player.Embedded.VideoSurface.Gtk;
using Caprica.VlcSharp.Player.Embedded.VideoSurface.Windows;
using Caprica.VlcSharp.Binding;
using Caprica.VlcSharp.Binding.Internal;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Sockets;
using PCRemote;
using System.Diagnostics;
using System.IO;
using MonoBrick.NXT;

public partial class MainWindow
{
	private RtspTunnel rtspTunnel;
	private int imagePort;
	private int rtspPort;
	private Process vlcWindow;
	private void EnableVideoStreamInput(){
		if(brickType == BrickType.NXT){
			var reply = brick.Connection.SendAndReceive(new Command(CommandType.TunnelCommand,CommandByte.GetTunnelRTSPSettings,true));
			rtspPort = (int)reply.GetInt32(3);
			imagePort = (int)reply.GetInt32(7);
			Gtk.Application.Invoke (delegate {
				rtspPortLabel.Text = rtspPort.ToString();
				imagePortLabel.Text = imagePort.ToString();
				startStreamButton.Sensitive = true;
				networkCachingCombobox.Sensitive = true;
			});
			if(settings.Path == ""){
				MonoBrickHelper.Platform platform = MonoBrickHelper.RunningPlatform();
				if(platform == MonoBrickHelper.Platform.Mac){
					settings.Path = "/Applications/VLC.app/Contents/MacOS/VLC";
				}
				if(platform == MonoBrickHelper.Platform.Linux){
					settings.Path = "vlc";
				}
				if(platform == MonoBrickHelper.Platform.Windows){
					settings.Path = @"C:\Program Files (x86)\VideoLAN\VLC\vlc";
				}
				settings.Save();
			}
		}
	}

	private void DisableVideoStreamInput(){
		Gtk.Application.Invoke (delegate {
			rtspPortLabel.Text = "N/A";
			imagePortLabel.Text = "N/A";
			startStreamButton.Sensitive = false;
			networkCachingCombobox.Sensitive = false;
		});	
	}

	private void EnableSpeakInput(){
		Gtk.Application.Invoke (delegate {
			speakButton.Sensitive = true;
		});	
	}

	private void DisableSpeakInput(){
		Gtk.Application.Invoke (delegate {
			speakButton.Sensitive = false;
		});	
	}

	private void DisableCaching(){
		Gtk.Application.Invoke (delegate {
			networkCachingCombobox.Sensitive = false;
		});	
	}

	private void EnableCaching(){
		Gtk.Application.Invoke (delegate {
			networkCachingCombobox.Sensitive = true;
		});	
	}

	private void DisableAllTunnelInput(){
		Gtk.Application.Invoke (delegate {
			startStreamButton.Sensitive = false;
			speakButton.Sensitive = false;
			toggleLightButton.Sensitive = false;
			gpsButton.Sensitive = false;
			gpsCoordinate1.Sensitive = false;
			gpsCoordinate2.Sensitive = false;
			networkCachingCombobox.Sensitive = false;
		});	
	}

	private void EnableGPSInput(){
		Gtk.Application.Invoke (delegate {
			gpsButton.Sensitive = true;
			gpsCoordinate1.Sensitive = true;
			gpsCoordinate2.Sensitive = true;
		});	
	}
	
	private void DisableGPSInput(){
		Gtk.Application.Invoke (delegate {
			gpsButton.Sensitive = true;
			gpsCoordinate1.Sensitive = true;
			gpsCoordinate2.Sensitive = true;
		});	
	}

	private void OnConnectedTunnelPage(){

	}

	private void OnDisconnectedTunnelPage(){
		if (vlcWindow != null) {
			try{
				vlcWindow.Kill();
			}
			catch
			{

			}
		}
	}

	private void OnRTSPTunnelStopped(){
		Console.WriteLine("RTSP tunnel stopped");
		if(vlcWindow != null){
			try{
				vlcWindow.Kill();
			}
			catch{

			}
		}
		Gtk.Application.Invoke (delegate {
			startStreamButton.Label = "Start Stream";
			EnableCaching();
		});
	}

	private void OnRTSPTunnelStarted(){
		Gtk.Application.Invoke (delegate {
			startStreamButton.Label = "Stop Stream";
			DisableCaching();
		});
	}



	private void OnUpdateStreamRate(double bytesPrSec){
		Gtk.Application.Invoke (delegate {
			String.Format("{0:0.00}", 123.4567);
			dataRateLabel.Text = String.Format("{0:0.000}", bytesPrSec/1000);
		});
	}

	protected void LoadVLCSettings(){
		networkCachingCombobox.SetActiveValue (settings.Caching);
	}

	protected void OnStartStreamButtonClicked (object sender, EventArgs e)
	{
		bool vlcError = false;
		if(rtspTunnel != null && rtspTunnel.Running){
			SpawnThread(delegate()
			{
				vlcWindow.Kill();
			});
			return;
		}
		else{
			SpawnThread(delegate()
			{
				int stepTimeOut = 10000;
				bool hasError = false;
				Gtk.ProgressBar progressBar = new Gtk.ProgressBar();
				progressBar.WidthRequest = 170;
				Label label = new Label("Opening RTSP Connection");
				//label.HeightRequest = 10;
				Dialog dialog = new Dialog ("Starting video stream", this, Gtk.DialogFlags.DestroyWithParent);
				dialog.TypeHint =  WindowTypeHint.Splashscreen;
				dialog.Modal = true;
				dialog.VBox.Add (label);
				dialog.VBox.Add (progressBar);
				dialog.HasSeparator = false;
				System.Timers.Timer timer = new System.Timers.Timer(100);
				timer.Elapsed += delegate {
					Gtk.Application.Invoke (delegate {
						progressBar.Pulse();
					});
				};
				Gtk.Application.Invoke (delegate {
					dialog.ShowAll();
				});
				timer.Start();
				if(brickType == BrickType.NXT)
					rtspTunnel = new RtspTunnel(rtspPort,imagePort,brick.Connection);
				else
					throw new NotImplementedException("What to do with EV3");
				rtspTunnel.Start();
				string errorMessage = "";
				if(rtspTunnel.RTSPWaitHandle.WaitOne(stepTimeOut)){
					Gtk.Application.Invoke (delegate {
						label.Text = "Starting stream";
					});
					vlcWindow = new Process();
					string argument = "rtsp://127.0.0.1:" + rtspPort + " --network-caching="+ networkCachingCombobox.GetActiveValue();
					Console.WriteLine(argument);
					vlcWindow.StartInfo = new ProcessStartInfo(settings.Path,
					argument);
					try{
						vlcError = false;
						vlcWindow = Process.Start(new ProcessStartInfo(settings.Path,argument));
						/*if(vlcWindow.Start()){
							vlcError = false;
						}*/
					   	
					}
					catch{
						vlcError = true;					
					}
					if(!vlcError){
						Gtk.Application.Invoke (delegate {
							label.Text = "Waiting for image stream";
						});
						if(rtspTunnel.StreamWaitHandle.WaitOne(stepTimeOut)){
							//Everything is ok
							rtspTunnel.Stopped += OnRTSPTunnelStopped;
							rtspTunnel.NewImageDataRate += OnUpdateStreamRate;
							OnRTSPTunnelStarted();
						}
						else{
							errorMessage = "Failed to receive image data";
							hasError = true;
						}
					}
					else{
						errorMessage = "Failed to start vlc window";
						hasError = true;
					}
				}
				else{
					errorMessage = "Failed to start remote RTSP-server";
					hasError = true;
				}
				timer.Stop();
				Gtk.Application.Invoke (delegate {
					dialog.Destroy();
				});
				if(hasError && !vlcError){
					if(rtspTunnel != null && rtspTunnel.Running){
						rtspTunnel.Stop();
					}
					Gtk.Application.Invoke (delegate {
						MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\n" + errorMessage);
						md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
						md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
						md.Run ();
						md.Destroy();
					});
				}
				if(vlcError){
					rtspTunnel.Stop();
					Gtk.Application.Invoke (delegate {
						/*if(Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix){
							//Mac OS and Linux
							MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, 
							                                      "\n" + "Failed to start VLC\n" + 
							                                      "Make sure you have installed VLC");
							md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							md.Run();
							md.Destroy();
							
						}
						else{*/
							//windows
							MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.YesNo, 
							                                      "\n" + "Failed to start VLC\n" + 
							                                      "VLC executable path is currently set to:\n"
							                                      + settings.Path +"\n" +
							                                      "Do you want to change this");
							md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
							md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
							if(md.Run() == (int) ResponseType.Yes){
								md.Destroy();
								
								Gtk.FileChooserDialog fc=
									new Gtk.FileChooserDialog("Locate VLC executable",
									                          this,
									                          FileChooserAction.Open,
									                          Gtk.Stock.Cancel,ResponseType.Cancel,
									                          Gtk.Stock.Ok,ResponseType.Accept);
								if (fc.Run() == (int)ResponseType.Accept) 
								{
									settings.Path = fc.Filename;
									settings.Save();
								}
								fc.Destroy();
							}
							else{
								md.Destroy();
							}
						//}
					});
				}
			});
		}

	}
}
