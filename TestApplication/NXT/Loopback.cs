using System;

namespace MonoBrick.NXT
{
	/// <summary>
	/// Simulate a NXT connection - far from completed
	/// </summary>
	public class Loopback<TBrickCommand,TBrickReply> : Connection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new()

	{
		private Reply reply = null;
		private byte[] brickName = {(byte)'s',(byte)'i',(byte)'m',(byte)'u',(byte)'l',(byte)'a',(byte)'t',(byte)'i',(byte)'o',(byte)'n'};
		private byte[] bluetoothAddress = {0x01,0x02,0x03,0x04,0x05,0x06};
		private UInt32 flashSize = 4024;
		private SensorMode[] sensorMode;
		private SensorType[] sensorType; 

		/// <summary>
		/// Initializes a new instance of the NXT Loopback class.
		/// </summary>
		public Loopback(){
			isConnected = false;
			Array.Resize<byte>(ref brickName,16);
			sensorMode = new SensorMode[4];
			sensorType = new SensorType[4];
			for(int i = 0; i < 4; i++){
				sensorMode[i] = SensorMode.Raw;
				sensorType[i] = SensorType.NoSensor;
			}
		}

		/// <summary>
		/// Open the connection
		/// </summary>
		public override void Open(){
			ConnectionWasOpened();
			isConnected = true;
		}

		/// <summary>
		/// Close the connection
		/// </summary>
		public override void Close()
		{
			ConnectionWasClosed();
			isConnected = false;
		}

		/// <summary>
		/// Send the specified command.
		/// </summary>
		/// <param name='command'>
		/// Command to send
		/// </param>
		public override void Send(TBrickCommand command){
			Command nxtCommand = new Command(command.Data);
			switch(((Command)nxtCommand).CommandByte){
				case CommandByte.SetBrickName:
					byte[] commandData = nxtCommand.Data;
					for (int i = 2; i < commandData.Length; i++)
					{
						brickName[i - 2] = commandData[i];
					}
					if(nxtCommand.ReplyRequired)
						reply = new Reply(CommandType.ReplyCommand,CommandByte.SetBrickName);
					break;
				case CommandByte.GetDeviceInfo:
				{	
					byte[] data = new byte[33];
					data[0] = (byte) CommandType.ReplyCommand;
					data[1] = (byte) CommandByte.GetDeviceInfo;
					data[2] = 0;//no error
					Array.Copy(brickName,0,data,3,brickName.Length);
					Array.Copy(bluetoothAddress,0,data,18,bluetoothAddress.Length);
					Array.Copy(BitConverter.GetBytes(flashSize),0,data,29,4);
					reply = new	Reply(data);
				}
					break;
				case CommandByte.SetInputMode:
					sensorType[nxtCommand.Data[2]] = (SensorType) nxtCommand.Data[3];
					sensorMode[nxtCommand.Data[2]] = (SensorMode) nxtCommand.Data[4];
					reply = new Reply(CommandType.ReplyCommand,((Command)nxtCommand).CommandByte);
					break;
				case CommandByte.ResetInputScaledValue:
					reply = new Reply(CommandType.ReplyCommand,((Command)nxtCommand).CommandByte);
					break;
				case CommandByte.GetFirmware:
				{
					byte[] data = new byte[7];
					data[0] = (byte) CommandType.ReplyCommand;
					data[1] = (byte) CommandByte.GetFirmware;
					data[2] = 0;//no error
					data[3] = 1;
					data[4] = 1;
					data[5] = 1;
					data[6] = 1;
					reply = new Reply(data);					
				}	
					break;
				case CommandByte.GetBatteryLevel:
				{	
					byte[] data = new byte[5];
					data[0] = (byte) CommandType.ReplyCommand;
					data[1] = (byte) CommandByte.GetBatteryLevel;
					data[2] = 0;//no error
					data[3] = 0x40;
					data[4] = 0x1f;
					reply = new Reply(data);					
				}
					break;
				case CommandByte.GetInputValues:
				{	
					byte[] data = new byte[16];
					data[0] = (byte) CommandType.ReplyCommand;
					data[1] = (byte) CommandByte.GetInputValues;
					data[2] = 0;//no error
					data[3] = nxtCommand.Data[2];
					data[4] = 0x01;
					data[5] = 0x01;
					data[6] = (byte) sensorType[nxtCommand.Data[2]];
					data[7] = (byte) sensorMode[nxtCommand.Data[2]];
					byte[] rawValue = {0x02, 0x03};
					for(int i = 0; i < 2; i++){
						data[i +  8] = rawValue[i]; 
						data[i + 10] = rawValue[i];
						data[i + 12] = rawValue[i];
						data[i + 14] = rawValue[i];
					}
					reply = new Reply(data);					
				}
					break;
					
				case CommandByte.MessageWrite:
				{	
					byte[] data = new byte[3];
					data[0] = (byte) CommandType.ReplyCommand;
					data[1] = (byte) CommandByte.MessageWrite;
					data[2] = 0;//no error
					reply = new Reply(data);					
				}
				break;
				default:
					throw new NotImplementedException();
					//break;
			}
		}

		/// <summary>
		/// Receive a reply
		/// </summary>
		public override TBrickReply Receive(){
			var myReply = new TBrickReply();
			myReply.SetData(reply.Data);
			return myReply;
		}	
	}
}

