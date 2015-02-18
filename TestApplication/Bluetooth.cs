using System.IO.Ports;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System;
using System.ComponentModel;
using System.Net;

namespace MonoBrick
{
	/// <summary>
	/// Bluetooth connection for use on Windows and MAC
	/// </summary>
	public class Bluetooth<TBrickCommand, TBrickReply> : Connection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new()

	{
		private SerialPort comPort = null;
		string port;
		/// <summary>
		/// Initializes a new instance of the Bluetooth class.
		/// </summary>
		/// <param name="comport">he serial port to use</param>
		public Bluetooth(string comport){
			port = comport;
			isConnected = false;
		}

		/// <summary>
		/// Send a command
		/// </summary>
		/// <param name='command'>
		/// The command to send
		/// </param>
		public override void Send(TBrickCommand command){
			byte[] data = null;
			ushort length = (ushort) command.Length;
			data = new byte[length+2];
			data[0] = (byte) (length & 0x00ff);
			data[1] = (byte)((length&0xff00) >> 2);
			Array.Copy(command.Data,0,data,2,command.Length);
			CommandWasSend(command);
			try{
				comPort.Write(data, 0, data.Length);
			}
			catch (Exception e){
				throw new ConnectionException(ConnectionError.WriteError, e);
			}

		}

		/// <summary>
		/// Receive a reply
		/// </summary>
		public override TBrickReply Receive(){
			byte[] data = new byte[2];
			byte[] payload;
			int expectedlength = 0;
			int replyLength = 0;
			try{
				expectedlength = 2;
				replyLength = comPort.Read(data,0,2);
				expectedlength = (ushort)(0x0000 | data[0] | (data[1] << 2));
				payload = new byte[expectedlength];
				replyLength = 0;
				replyLength = comPort.Read(payload,0,expectedlength);
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
				throw new ConnectionException(ConnectionError.ReadError, tEx);
			}
			catch (Exception e){
				throw new ConnectionException(ConnectionError.ReadError, e);
			}
			TBrickReply reply = new TBrickReply();
			reply.SetData(payload);
			ReplyWasReceived(reply);
			return reply;
		}

		/// <summary>
		/// Open connection
		/// </summary>
		public override void Open(){
			try
			{
				comPort = new SerialPort(port);
				comPort.Open();
				comPort.WriteTimeout = 5000;
				comPort.ReadTimeout = 5000;
			}
			catch (Exception e){
				throw new ConnectionException(ConnectionError.OpenError, e);
			}
			isConnected = true;
			ConnectionWasOpened();
			Thread.Sleep(1000);
		}

		/// <summary>
		/// Close connection
		/// </summary>
		public override void Close(){
			try{
				comPort.Close();
			}
			catch (Exception){
			}
			isConnected = false;
			ConnectionWasClosed();
		}

		/// <summary>
		/// Gets a list of available serial ports
		/// </summary>
		/// <returns>
		/// A list of available serial ports
		/// </returns>
		static public string[] GetPortNames(){
			return System.IO.Ports.SerialPort.GetPortNames();
		}
		
	}
}

