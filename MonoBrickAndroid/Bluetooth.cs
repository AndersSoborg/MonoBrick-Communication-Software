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
	public class Bluetooth<TBrickCommand,TBrickReply> : Connection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new()
	{
		private static BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
		private BluetoothSocket socket;
		private bool addLength = true;
		public Bluetooth(string deviceName):this(GetBondDevice(deviceName)){

		}

		public Bluetooth(BluetoothDevice device){
			/*this.device = device;
			this.socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));*/
			//Hack
			IntPtr createRfcommSocket = JNIEnv.GetMethodID( device.Class.Handle, "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;" ); 
			IntPtr socketHandle = JNIEnv.CallObjectMethod( device.Handle, createRfcommSocket, new JValue( 1 ) ); 
			this.socket = Java.Lang.Object.GetObject<BluetoothSocket>( socketHandle, JniHandleOwnership.TransferLocalRef );
			
		}
		
		static public BluetoothAdapter Adapter{get{return adapter;}}
		
		static public BluetoothDevice[] BondDevices{
			 get{
				List<BluetoothDevice> list = new List<BluetoothDevice>();
				foreach (Android.Bluetooth.BluetoothDevice dev in adapter.BondedDevices)
    			{
        			list.Add(dev);
				}
				return list.ToArray();
			}
		}
		
		static public BluetoothDevice GetBondDevice(string deviceName){
			foreach(BluetoothDevice device in BondDevices){
				if(device.Name == deviceName)
					return device;
			}
			return null;
		}
		
		
		public override void Open(){
			try
            {
				socket.Connect();
            }
            catch(Exception e) {
                Close();
				throw new ConnectionException(ConnectionError.OpenError, e);
            }
			ConnectionWasOpened();
			isConnected = true;
		}
		
		public override void Close()
    	{
			try
            {
                socket.Close();
            }
            catch (Exception) {
            }
			isConnected = false;
			ConnectionWasClosed();
		}
		
		public static void Enable(){
			adapter.Enable();
		}
		
		public static void Disable(){
			adapter.Disable();
		}
		
		public static bool IsEnabled{
			get{return adapter.IsEnabled;}
		}		
	
		public override void Send(TBrickCommand command){
			
			CommandWasSend(command);
			try
            {
				byte[] data = null;
				if(addLength){
					ushort length = (ushort) command.Length;
					data = new byte[length+2];
					data[0] = (byte) (length & 0x00ff);
					data[1] = (byte)((length&0xff00) >> 2);
					Array.Copy(command.Data,0,data,2,command.Length);
				}
				else{
					data = command.Data;
				}
				socket.OutputStream.Write(data, 0, data.Length);
            }
            catch (Exception e) {
                Close();
				throw new ConnectionException(ConnectionError.WriteError,e);
            }
		}

		public override TBrickReply Receive(){
			byte[] data = new byte[2];
			byte[] payload;
			int expectedlength = 0;
			int replyLength = 0;
			try{
				if(addLength){
					expectedlength = 2;
					replyLength = socket.InputStream.Read(data,0,2);
					expectedlength = (ushort)(0x0000 | data[0] | (data[1] << 2));
					payload = new byte[expectedlength];
					replyLength = 0;
					replyLength = socket.InputStream.Read(payload,0,expectedlength);
				}
				else{
					throw new NotImplementedException("Receiving without length has not been implemented for Bluetooth");
				}
			}
			catch (TimeoutException tEx){
				if( replyLength == 0){
					throw new ConnectionException(ConnectionError.NoReply, tEx);
				}
				else if(replyLength != expectedlength){
					if(typeof(TBrickCommand) == typeof(NXT.Command)){
						throw new NXT.BrickException(NXT.BrickError.WrongNumberOfBytes, tEx);
					}
					else{
						throw new EV3.BrickException(EV3.BrickError.WrongNumberOfBytes,tEx);
					}
				}
				Close();
				throw new ConnectionException(ConnectionError.ReadError, tEx);
			}
			catch (Exception e){
				Close();
				throw new ConnectionException(ConnectionError.ReadError, e);
			}
			var reply = new TBrickReply();
			reply.SetData (payload);
			ReplyWasReceived(reply);
			return reply;
		}	
	}
}



