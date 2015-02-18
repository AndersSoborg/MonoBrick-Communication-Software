using System;
using Gtk;
using MonoBrick;
using System.Diagnostics;
using System.Net.Sockets;
namespace PCRemote
{
	class MainClass
	{
		public static void Init(){
			// This code automatically loads required dlls from embedded resources.
			AppDomain.CurrentDomain.AssemblyResolve += delegate(System.Object sender, ResolveEventArgs delArgs)
			{
				string assemblyName = new System.Reflection.AssemblyName(delArgs.Name).Name;
				String resourceName = assemblyName + ".dll";
				using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				{
					if (stream == null)
						return null;
					Byte[] assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return System.Reflection.Assembly.Load(assemblyData);
				}
			};
		}
		
		public static void Main (string[] args)
		{
			Init();
			Main2(args);
		}

		public static void Main2 (string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show();
			Application.Run();
			win.Hide();
		}


		
		static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
    		Exception exception = (Exception) e.ExceptionObject;
			Stopwatch stopWatch = new Stopwatch();
        	stopWatch.Start();	
			Gtk.Application.Invoke (delegate {
            	MessageDialog md = new MessageDialog (null, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "Unhandled Exception \n" + exception.Message);
            	md.Icon = global::Gdk.Pixbuf.LoadFromResource (MainWindow.MessageDialogIconName);
				md.WindowPosition = WindowPosition.Center;
				md.Run ();
            	md.Destroy();
       		});
			while(e.IsTerminating && stopWatch.ElapsedMilliseconds < 3000){}
			stopWatch.Stop();
		}
	}
}

