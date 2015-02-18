using System;
using MonoBrick;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Android.Runtime;

namespace MonoBrick
{
	//to make monoBrick complile
	public class USB<TBrickCommand, TBrickReply> : Connection<TBrickCommand, TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new()
	{
		
		public USB(){
			throw new NotImplementedException();
		}
		
		public override void Open(){
			throw new NotImplementedException();
		}
		
		public override void Close()
		{
			throw new NotImplementedException();
		}
		
		public override void Send(TBrickCommand command){
			throw new NotImplementedException();
		}
		
		public override TBrickReply Receive(){
			throw new NotImplementedException();
		}	
	}
}
