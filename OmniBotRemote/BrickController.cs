using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonoBrick;


namespace OmniBotRemote
{

	public sealed class BrickController
	{
		private Semaphore threadMutex = new Semaphore(1, 1); //Ensure that  only one operation is started at the same time
		private static readonly BrickController instance = new BrickController();
		private MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor> nxt = null;
		private BrickController(){
			NXT = new MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor>(new MonoBrick.NXT.Loopback<MonoBrick.NXT.Command,MonoBrick.NXT.Reply>());
		}
		public MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor> NXT{
			get{return nxt;}
			set{
				nxt = value;
				NewBrick (nxt);
			}
		}
		public static BrickController Instance
		{
			get{return instance;}
		}
		public event Action ThreadNotStarted = delegate {}; // This event is signaled if a background thread couldn't be started because another one is runniung
		public event Action<MonoBrick.NXT.Brick<MonoBrick.NXT.Sensor, MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor,MonoBrick.NXT.Sensor>> NewBrick= delegate {};
		public event Action<Exception> BrickException=delegate {};
		public void	ExecuteOnCurrentThread (Action action, bool reportThreadNotStarted = true)
		{
			if (threadMutex.WaitOne(100))
			{
				try
				{	
					action();
				}
				catch(Exception e){
					BrickException(e);
				}
				finally
				{
					threadMutex.Release();
				}
			}
			else
			{
				if(reportThreadNotStarted){
					ThreadNotStarted();
				}
			}
			
		}
		
		public void SpawnThread(Action action, bool reportThreadNotStarted = true)
		{
			Thread t = new Thread(
				new ThreadStart(
				delegate()
				{
				if (threadMutex.WaitOne(100))
				{
					try
					{	
						action();
					}
					catch(Exception e){
						BrickException(e);
					}
					finally
					{
						threadMutex.Release();
					}
				}
				else
				{
					if(reportThreadNotStarted){
						ThreadNotStarted();
					}
				}
			}
			));
			t.IsBackground = true;
			t.Priority =  System.Threading.ThreadPriority.AboveNormal;
			t.Start();
		}
	}
}



