using System;
using System.Text;
using MonoBrick;
using System.Collections.Generic;

namespace MonoBrick.NXT
{
	/// <summary>
	/// NXT command types
	/// </summary>
	public enum CommandType{ 
		/// <summary>
		/// Direct LEGO command.
		/// </summary>
		DirecCommand = 0x00,
		/// <summary>
		/// LEGO system command
		/// </summary>
		SystemCommand = 0x01,
		/// <summary>
		/// Reply command
		/// </summary>
		ReplyCommand = 0x02,
		/// <summary>
		/// A tunnel command
		/// </summary>
		TunnelCommand = 0x03
	};

	/// <summary>
	/// NXT and tunnel commands
	/// </summary>
	public enum CommandByte{	
		#pragma warning disable 
		GetTunnelCommands = 40, GetTunnelRTSPSettings = 0x41, StartTunnelRTSP = 0x42, TunnelSpeak = 0x43, GetTunnelGPSPosition = 0x44,//these are tunnel commands						

		OpenRead = 0x80, OpenWrite = 0x81,Read = 0x82, Write = 0x83, 
		Close = 0x84, Delete = 0x85, FindFirst = 0x86, FindNext = 0x87, 
		GetFirmware = 0x88, OpenWriteLinear = 0x89, OpenReadLinear = 0x8a, 
		OpenWriteData = 0x8b, OpenAppendData = 0x8c, Boot = 0x97, SetBrickName = 0x98, 
		GetDeviceInfo = 0x9b, DeleteUserFlash = 0xA0, 

		StartProgram = 0x00, StopProgram = 0x01, 
		PlaySoundFile = 0x02, PlayTone = 0x03, SetOutputState = 0x04,
		SetInputMode = 0x05, GetOutputState = 0x06, GetInputValues = 0x07,
		ResetInputScaledValue = 0x08, MessageWrite = 0x09, ResetMotorPosition = 0x0a,
		GetBatteryLevel = 0x0b, StopSoundPlayback = 0x0c, KeepAlive = 0x0d,
		LsGetStatus = 0x0e, LsWrite = 0x0f, LsRead = 0x10,GetCurrentProgramName = 0x11,
		MessageRead = 0x13
		#pragma warning restore

	};



	/// <summary>
	/// Class for creating a NXT command.
	/// </summary>
	public class Command: BrickCommand
	{
		private CommandType commandType;
		private CommandByte commandByte;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Command"/> class.
		/// </summary>
		/// <param name='type'>
		/// The command type. Can be a system command, direct command or reply command
		/// </param>
		/// <param name='commandByte'>
		/// The command byte
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the NXT will send a reply
		/// </param>
		public Command(CommandType type, CommandByte commandByte, bool reply)
		{
			commandType = type;
			this.commandByte = commandByte;
			if (reply){
				replyRequired = true;
				dataArr.Add((byte)type);
			}
			else{
				replyRequired = false;
				dataArr.Add((byte)((byte) type | 0x80));
			}
			dataArr.Add((byte)commandByte);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Command"/> class using an array of bytes
		/// </summary>
		/// <param name='data'>
		/// The data to be used for the command
		/// </param>
		public Command(byte [] data){
			if(data.Length < 2){
				throw new System.ArgumentException("Invalid NXT Command");
			}
			for(int i = 0; i < data.Length; i++)
				dataArr.Add(data[i]);
			try{
				commandType = (CommandType) (data[0] & 0x7f);
				commandByte = (CommandByte) data[1];
			}
			catch(Exception){
				throw new System.ArgumentException("Invalid NXT Command");
			}
			replyRequired = !Convert.ToBoolean(data[0]&0x80);

		}

		/// <summary>
		/// Gets the command byte 
		/// </summary>
		/// <value>
		/// The command byte
		/// </value>
		public CommandByte CommandByte{
			get{return commandByte;}
		}

		/// <summary>
		/// Gets type of the command
		/// </summary>
		/// <value>
		/// The command type. Can be a system command, direct command or reply command
		/// </value>
		public CommandType CommandType{
			get{return commandType;}
		}

		/// <summary>
		/// Gets the command byte as string.
		/// </summary>
		/// <value>
		/// The command byte as string.
		/// </value>
		public string CommandByteAsString{
			get{
				return AddSpacesToString(CommandByte.ToString());	
			}
		}

		internal void Print(){
			byte[] arr = Data;
			Console.WriteLine("Command: " + CommandType);
			Console.WriteLine("Length: " + Length);
			for(int i = 0; i < Length; i++){
				Console.WriteLine("Command["+i+"]: " + arr[i].ToString("X"));
			}
		}


	}

	/// <summary>
	/// Class for creating a NXT reply
	/// </summary>
	public class Reply: BrickReply
	{
		/// <summary>
		/// Gets a value indicating whether this instance has error.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has error; otherwise, <c>false</c>.
		/// </value>
		public bool HasError{
			get{return Convert.ToBoolean(ErrorCode);}
		}

		/// <summary>
		/// Gets the type of error.
		/// </summary>
		/// <value>
		/// The type of error
		/// </value>
		internal ErrorType ErrorType{
			get{return Error.ToErrorType(ref dataArray[2]);}
		}

		/// <summary>
		/// Gets the error code.
		/// </summary>
		/// <value>
		/// The error code
		/// </value>
		public byte ErrorCode{get{return dataArray[2];}}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class with no payload
		/// </summary>
		public Reply ()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class.
		/// </summary>
		/// <param name='data'>
		/// The byte array to be used for the reply
		/// </param>
		public Reply(byte[] data){
			dataArray = data;
			if(data.Length < 3){
				throw new System.ArgumentException("Invalid NXT Reply");
			}
			if(!Enum.IsDefined(typeof (CommandType),data[0]) || !Enum.IsDefined(typeof(CommandByte),data[1])){
				throw new System.ArgumentException("Invalid NXT Reply");
			}
			if((CommandType) data[0] != CommandType.ReplyCommand){
				throw new System.ArgumentException("Invalid NXT Reply");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class.
		/// </summary>
		/// <param name='type'>
		/// The command type. Can be a system command, direct command or reply command
		/// </param>
		/// <param name='command'>
		/// The command byte
		/// </param>
		/// <param name='data'>
		/// Byte array to be used for reply payload
		/// </param>
		/// <param name='errorCode'>
		/// Error code
		/// </param>
		public Reply(CommandType type, CommandByte command, byte[] data, byte errorCode){
			if(data!= null)
				dataArray = new byte[data.Length+3];
			else
				dataArray = new byte[3];
			dataArray[0] = (byte) type;
			dataArray[1] = (byte) command;
			dataArray[2] = errorCode;
			if(data != null){
				for(int i = 0; i < data.Length; i++){
					dataArray[i+3] = data[i];
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class without errors
		/// </summary>
		/// <param name='type'>
		/// The command type. Can be a system command, direct command or reply command
		/// </param>
		/// <param name='command'>
		/// The command byte
		/// </param>
		/// <param name='data'>
		/// Byte array to be used for reply payload
		/// </param>
		public Reply(CommandType type, CommandByte command, byte[] data):this(type,command,data,0){

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class without payload and errors
		/// </summary>
		/// <param name='type'>
		/// The command type. Can be a system command, direct command or reply command
		/// </param>
		/// <param name='command'>
		/// The command byte
		/// </param>
		public Reply(CommandType type, CommandByte command):this(type,command,null,0){

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Reply"/> class without payload with an error code
		/// </summary>
		/// <param name='type'>
		/// The command type. Can be a system command, direct command or reply command
		/// </param>
		/// <param name='command'>
		/// The command byte
		/// </param>
		/// <param name='errorCode'>
		/// Error code
		/// </param>
		public Reply(CommandType type, CommandByte command, byte errorCode):this(type,command,null,errorCode){

		} 

		/// <summary>
		/// Gets the command byte.
		/// </summary>
		/// <value>
		/// The command byte
		/// </value>
		public CommandByte CommandByte{get{return (CommandByte) dataArray[1];}}

		/// <summary>
		/// Gets the command byte as string.
		/// </summary>
		/// <value>
		/// The command byte as string
		/// </value>
		public string CommandByteAsString{
			get{return BrickCommand.AddSpacesToString(CommandByte.ToString()); }
		} 

		/// <summary>
		/// The command type. Can be a system command, direct command or reply command
		/// </summary>
		/// <value>
		/// The type of the command.
		/// </value>
		public CommandType CommandType{get{return (CommandType) dataArray[0];}}

		internal void print(){
			Console.WriteLine("Command: " + CommandType.ToString());
			Console.WriteLine("Length: " + Length);
			Console.WriteLine("Errorcode: " + ErrorCode);
			for(int i = 0; i < Length; i++){
				Console.WriteLine("Reply["+i+"]: " + dataArray[i].ToString("X"));
			}

		}
	}

}

