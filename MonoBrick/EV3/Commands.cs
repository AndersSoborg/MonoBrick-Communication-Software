using System;
using System.Text;
using MonoBrick;
using System.Collections.Generic;

namespace MonoBrick.EV3
{
	/// <summary>
	/// Encoded Parameter format
	/// </summary>
	[Flags]
	public enum ParameterFormat : byte  { 
		#pragma warning disable 
		Short = 0x00, 
		Long = 0x80 
		#pragma warning restore
	}
	
	/// <summary>
	/// Encoded Parameter type
	/// </summary>
	[Flags] 
	public enum ParameterType : byte  { 
		#pragma warning disable 
		Constant = 0x00, 
		Variable = 0x40  
		#pragma warning restore
	}
	
	/// <summary>
	/// Encoded Parameter sign when using short constant format
	/// </summary>
	[Flags] 
	enum ShortSign : byte  { 
		#pragma warning disable 
		Positive = 0x00, 
		Negative = 0x20
		#pragma warning restore
	}
	
	
	/// <summary>
	/// Encoded Parameter type when using long constant format
	/// </summary>
	[Flags] 
	public enum ConstantParameterType : byte  { 
		#pragma warning disable 
		Value = 0x00, 
		Label = 0x20  
		#pragma warning restore
	}
	
	/// <summary>
	/// Encoded Parameter scope when using long variable format
	/// </summary>
	[Flags] 
	public enum VariableScope : byte  { 
		#pragma warning disable 
		Local = 0x00, 
		Global = 0x20,
		#pragma warning restore
	}
	
	/// <summary>
	/// Encoded Parameter type when using long variable format
	/// </summary>
	[Flags]
	public enum VariableType : byte  { 
		#pragma warning disable 
		Value = 0x00,
		Handle = 0x10
		#pragma warning restore
	}
	
	/// <summary>
	/// Encoded Parameter following when using long format
	/// </summary>
	[Flags]
	enum FollowType : byte  { 
		#pragma warning disable 
		OneByte		= 0x01, 
		TwoBytes 	= 0x02,
		FourBytes 	= 0x03,
		TerminatedString 	= 0x00,
		TerminatedString2 = 0x04
		#pragma warning restore
	}
	
	/// <summary>
	/// Program slots used by the EV3
	/// </summary>
	public enum ProgramSlots{
		/// <summary>
		/// Program slot reserved for executing the user interface
		/// </summary>
		Gui = 0, 
		
		/// <summary>
		/// Program slot used to execute user projects, apps and tools
		/// </summary>
		User = 1,
		
		/// <summary>
		/// Program slot used for direct commands coming from c_com
		/// </summary>
		Cmd = 2,
		
		/// <summary>
		/// Program slot used for direct commands coming from c_ui
		/// </summary>
		Term = 3,
		
		/// <summary>
		/// Program slot used to run the debug ui
		/// </summary>
		Debug = 4,
		
		/// <summary>
		/// ONLY VALID IN opPROGRAM_STOP
		/// </summary>
		Current = -1
	}
	
	/// <summary>
	/// The daisychain layer
	/// </summary>
	public enum DaisyChainLayer{
		/// <summary>
		/// The EV3
		/// </summary>
		EV3 = 0, 
		
		/// <summary>
		/// First EV3 in the Daisychain
		/// </summary>
		First = 1,
		
		/// <summary>
		/// Second EV3 in the Daisychain
		/// </summary>
		Second = 2,
		
		/// <summary>
		/// Third EV3 in the Daisychain
		/// </summary>
		Third = 3,
	}
	
	
	
	
	/// <summary>
	/// EV3 command type.
	/// </summary>
	public enum CommandType{
		/// <summary>
		/// Direct command
		/// </summary>
		DirectCommand = 0x00,

		/// <summary>
		/// System command.
		/// </summary>
		SystemCommand = 0x01,

		/// <summary>
		/// Direct command reply.
		/// </summary>
		DirectReply = 0x02,

		/// <summary>
		/// System command reply.
		/// </summary>
		SystemReply = 0x03,

		/// <summary>
		/// Direct reply with error.
		/// </summary>
		DirectReplyWithError = 0x04,

		/// <summary>
		/// System reply with error.
		/// </summary>
		SystemReplyWithError = 0x05
	}

	/// <summary>
	/// EV3 system commands
	/// </summary>
	public enum SystemCommand {
		#pragma warning disable 
		None = 0x00,
		BeginDownload = 0x92,
		ContinueDownload = 0x93,
		BeginUpload = 0x94,
		ContinueUpload = 0x95,
		BeginGetFile = 0x96,
		ContinueGetFile = 0x97,
		CloseFileHandle = 0x98,
		ListFiles = 0x99,
		ContinueListFiles = 0x9a,
		CreateDir = 0x9b,
		DeleteFile = 0x9c,
		ListOpenHandles = 0x9d,
		WriteMailbox = 0x9e,
		BluetoothPin = 0x9f,
		EnterFirmwareUpdate = 0xa0
		#pragma warning restore
	}

	/// <summary>
	/// EV3 byte codes
	/// </summary>
	public enum ByteCodes{
		#pragma warning disable 
		//VM
		ProgramStop	= 0x02,
		ProgramStart = 0x03,
		
		//Move
		InitBytes = 0x2F,
		
		//VM
		Info = 0x7C,
  		String = 0x7D,
  		MemoryWrite = 0x7E,
  		MemoryRead = 0x7F, 
		
		//Sound
		Sound = 0x94,
		SoundTest = 095,
		SoundReady = 0x96,
		
		//Input
		InputSample = 0x97,
		InputDeviceList = 0x98,
		InputDevice = 0x99,
		InputRead = 0x9a,
		InputTest = 0x9b,
		InputReady = 0x9c,
		InputReadSI = 0x9d,
		InputReadExt = 0x9e,
		InputWrite = 0x9f,
		
		//output
		OutputGetType = 0xa0,
		OutputSetType = 0xa1,         
  		OutputReset = 0xa2,           
  		OutputStop = 0xA3,
		OutputPower = 0xA4,
		OutputSpeed = 0xA5,
		OutputStart	= 0xA6,
		OutputPolarity = 0xA7,
		OutputRead = 0xA8,
		OutputTest = 0xA9,
		OutputReady = 0xAA,
		OutputPosition = 0xAB,
		OutputStepPower = 0xAC,
		OutputTimePower = 0xAD,
		OutputStepSpeed = 0xAE,
		OutputTimeSpeed = 0xAF,
		OutputStepSync = 0xB0,
		OutputTimeSync = 0xB1,
		OutputClrCount = 0xB2,
		OutputGetCount = 0xB3,

		//Memory
		File = 0xC0,
  		Array = 0xc1,
  		ArrayWrite = 0xc2,
  		ArrayRead = 0xc3,
  		ArrayAppend = 0xc4,
  		MemoryUsage = 0xc5,
  		FileName = 0xc6,
		
		//Mailbox
		MailboxOpen = 0xD8,
		MailboxWrite = 0xD9,
		MailboxRead = 0xDA,
		MailboxTest = 0xDB,
		MailboxReady = 0xDC,
		MailboxClose = 0xDD,

		#pragma warning restore
	}
	
	
	/// <summary>
	/// EV3 sound sub codes
	/// </summary>
	public enum SoundSubCodes{
		 #pragma warning disable 
		 Break = 0,
  		 Tone = 1,
  		 Play = 2,
  		 Repeat = 3,
  		 Service = 4
		 #pragma warning restore
	}
	
	/// <summary>
	/// EV3 input sub codes.
	/// </summary>
	public enum InputSubCodes{
		#pragma warning disable 
		GetFormat = 2,
  		CalMinMax = 3,
  		CalDefault = 4,
		GetTypeMode = 5,
		GetSymbol = 6,
		CalMin = 7,
		CalMax = 8,
		Setup = 9,
		ClearAll = 10,
		GetRaw = 11,
		GetConnection = 12,
		StopAll = 13,
		GetName = 21,
		GetModeName = 22,
		SetRaw = 23,
		GetFigures = 24,
		GetChanges = 25,
		ClrChanges = 26,
		ReadyPCT = 27,
		ReadyRaw = 28,
		ReadySI = 29,
		GetMinMax = 30,
		GetBumps = 31
  		#pragma warning disable
  	}

	/// <summary>
	/// EV3 file sub codes.
	/// </summary>
	public enum FileSubCodes{
		#pragma warning disable
		OpenAppend = 0,
		OpenRead = 1,
		OpenWrite = 2,
		ReadValue = 3,
		WriteValue = 4,
		ReadText = 5,
		WriteText = 6,
		Close = 7,
		LoadImage = 8,
		GetHandle = 9,
		LoadPicture = 10,
		GetPool = 11,
		Unload = 12,
		GetFolders = 13,
		GetIcon = 14,
		GetSubfolderName = 15,
		WriteLog = 16,
		CLoseLog = 17,
		GetImage = 18,
		GetItem = 19,
		GetCacheFiles = 20,
		PutCacheFile = 21,
		GetCacheFile = 22,
		DelCacheFile = 23,
		DelSubfolder = 24,
		GetLogName = 25,
		GetCacheName = 26,
		OpenLog = 27,
		ReadBytes = 28,
		WriteBytes = 29,
		Remove = 30,
		Move = 31,
		#pragma warning restore
	}

	/// <summary>
	/// Memory sub codes
	/// </summary>
	public enum MemorySubCodes{
		#pragma warning disable
		  Delete = 0,
		  Create8 = 1,
		  Create16 = 2,
		  Create32 = 3,
		  CreateTEF = 4,
		  Resize = 5,
		  Fill = 6,
		  Copy = 7,
		  Init8 = 8, 
		  Init16 = 9,
		  Init32 = 10,
		  InitF = 11, 
		  Size = 12,		
		  #pragma warning restore
	}


	/// <summary>
	/// Class for creating a EV3 system command.
	/// </summary>
	public class Command: BrickCommand{
		private SystemCommand systemCommand;
		private CommandType commandType;
		private UInt16 sequenceNumber;
		/// <summary>
		/// The short value maximum size
		/// </summary>
		public const sbyte ShortValueMax = 31;
		
		/// <summary>
		/// The short value minimum size
		/// </summary>
		public const sbyte ShortValueMin = -32;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Command"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		public Command(byte [] data){
			if(data.Length < 4){
				throw new System.ArgumentException("Invalid EV3 Command");
			}
			for(int i = 0; i < data.Length; i++){
				dataArr.Add(data[i]);
			}
			this.sequenceNumber = (UInt16)(0x0000 | dataArr[0] | (dataArr[1] << 2));
			try{
				commandType = (CommandType) (data[2] & 0x7f);
				if(commandType == CommandType.SystemCommand){
					systemCommand = (SystemCommand) data[3];
				}
				else{
					systemCommand = SystemCommand.None;	
				}

			}
			catch(BrickException){
				throw new System.ArgumentException("Invalid EV3 Command");
			}
			replyRequired = !Convert.ToBoolean(data[2]&0x80);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Command"/> class as a system command
		/// </summary>
		/// <param name="systemCommand">System command.</param>
		/// <param name="sequenceNumber">Sequence number.</param>
		/// <param name="reply">If set to <c>true</c> reply will be send from brick</param>
		public Command(SystemCommand systemCommand, UInt16 sequenceNumber, bool reply)
		{
			this.systemCommand = systemCommand;
			this.commandType = CommandType.SystemCommand;
			this.sequenceNumber = sequenceNumber;
			this.Append(sequenceNumber);

			if (reply){
				replyRequired = true;
				dataArr.Add((byte)commandType);
			}
			else{
				replyRequired = false;
				dataArr.Add((byte)((byte) commandType | 0x80));
			}
			dataArr.Add((byte)systemCommand);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Command"/> class as a direct command
		/// </summary>
		/// <param name="byteCode">Bytecode to use for the direct command</param>
		/// <param name="globalVariables">Global variables.</param>
		/// <param name="localVariables">Number of global variables</param>
		/// <param name="sequenceNumber">Number of local variables</param>
		/// <param name="reply">If set to <c>true</c> reply will be send from the brick</param>
		public Command(ByteCodes byteCode, int globalVariables, int localVariables, UInt16 sequenceNumber, bool reply): this(globalVariables, localVariables, sequenceNumber, reply){
			this.Append(byteCode);  
		}
		
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Command"/> as a direct command
		/// </summary>
		/// <param name="globalVariables">Global bytes.</param>
		/// <param name="localVariables">Number of global variables</param>
		/// <param name="sequenceNumber">Number of local variables</param>
		/// <param name="reply">If set to <c>true</c> reply will be send from brick</param>
		public Command(int globalVariables, int localVariables, UInt16 sequenceNumber, bool reply)
		{
			this.systemCommand = SystemCommand.None;
			this.commandType = CommandType.DirectCommand;
			this.sequenceNumber = sequenceNumber;
			this.Append(sequenceNumber);
			if (reply){
				replyRequired = true;
				dataArr.Add((byte)commandType);
			}
			else{
				replyRequired = false;
				dataArr.Add((byte)((byte) commandType | 0x80));
			}
			byte firstByte = (byte)(globalVariables & 0xFF);
			byte secondByte = (byte)((localVariables << 2) | (globalVariables >> 8));
			this.Append(firstByte);
			this.Append(secondByte);
		}

		/// <summary>
		/// Gets the EV3 system command.
		/// </summary>
		/// <value>The system command.</value>
		public SystemCommand SystemCommandType{
			get{return systemCommand;}
		}
		/// <summary>
		/// Gets the EV3 command type
		/// </summary>
		/// <value>The type of the command.</value>
		public CommandType CommandType {
			get{return commandType;}
		}

		/// <summary>
		/// Gets the sequence number
		/// </summary>
		/// <value>The sequence number.</value>
		public UInt16 SequenceNumber {
			get{return sequenceNumber;}
		}
		
		/// <summary>
		/// Append a sensor type value
		/// </summary>
		/// <param name="type">Sensor type to append</param>
		public void Append (SensorType type)
		{
			Append((byte) type, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a sensor mode value
		/// </summary>
		/// <param name="mode">Sensor mode to append</param>
		public void Append (SensorMode mode)
		{
			Append((byte) mode, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a byte code value
		/// </summary>
		/// <param name="byteCode">Byte code to append</param>
		public void Append(ByteCodes byteCode){
			Append((byte) byteCode);	
		}

		/// <summary>
		/// Append a file sub code
		/// </summary>
		/// <param name="code">Code to append.</param>
		public void Append(FileSubCodes code){
			Append((sbyte) code, ParameterFormat.Short);	
		}
		
		/// <summary>
		/// Append a file sub code
		/// </summary>
		/// <param name="code">Code to append.</param>
		public void Append(SoundSubCodes code){
			Append((sbyte) code, ParameterFormat.Short);	
		}
		
		/// <summary>
		/// Append a daisy chain layer 
		/// </summary>
		/// <param name="chain">Daisy chain layer to append</param>
		public void Append(DaisyChainLayer chain){
			Append((sbyte) chain, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a input sub code
		/// </summary>
		/// <param name="subCode">Sub code to append</param>
		public void Append (InputSubCodes subCode)
		{
			Append((sbyte) subCode, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a memory sub code
		/// </summary>
		/// <param name="subCode">Sub code to append</param>
		public void Append (MemorySubCodes subCode)
		{
			Append((sbyte) subCode, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a sensor port
		/// </summary>
		/// <param name="port">Sensor port to append</param>
		public void Append (SensorPort port)
		{
			Append((sbyte) port, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a motor port
		/// </summary>
		/// <param name="port">Motor port to append</param>
		public void Append(MotorPort port){
			Append((sbyte) port, ParameterFormat.Short);
		}
		
		/// <summary>
		/// Append a output bit field
		/// </summary>
		/// <param name="bitField">Bit field to append</param>
		public void Append (OutputBitfield bitField)
		{
			Append((sbyte) bitField, ParameterFormat.Short);
		}
		
		
		/// <summary>
		/// Append a constant parameter encoded byte in either short or long format. Note that if format is long parameter constant type will be a value
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="format">Use either short or long format</param>
		public void Append (byte value, ParameterFormat format)
		{
			if(format == ParameterFormat.Short){
				if(value > (byte) ShortValueMax){
					value = (byte) ShortValueMax;
				}
				byte b  = (byte)((byte)format | (byte) ParameterType.Constant | (byte)(value & ((byte)0x1f)) | (byte)ShortSign.Positive);
				Append (b); 
			}
			else{
				byte b  = (byte)((byte)format | (byte) ParameterType.Constant | (byte) ConstantParameterType.Value | (byte)FollowType.OneByte);
				Append (b);
				Append (value);	
			}
		}
		
		/// <summary>
		/// Append a constant parameter encoded byte in either short or long format. Note that if format is long parameter constant type will be a value
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="format">Use either short or long format</param>
		public void Append(sbyte value, ParameterFormat format)
		{
			if(format == ParameterFormat.Short){
				byte b = 0x00;
				if(value <0 ){
					if(value < ShortValueMin){
						value = ShortValueMin;
					}
					b  = (byte)((byte)format | (byte) ParameterType.Constant | (byte)(value & ((byte)0x1f)));
					b = (byte)((byte)ShortSign.Negative | b);
				}
				else{
					if(value > ShortValueMax){
						value = ShortValueMax;
					}
					b  = (byte)((byte)format | (byte) ParameterType.Constant | (byte)(value & ((byte)0x1f)));
					b = (byte)((byte)ShortSign.Positive | b);
				}
				Append (b); 

			}
			else{
				byte b  = (byte)((byte)format | (byte) ParameterType.Constant | (byte) ConstantParameterType.Value | (byte)FollowType.OneByte);
				Append (b);
				Append (value);	
			}
		}
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">byte to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(sbyte value, ConstantParameterType type)
		{
			Append(type, FollowType.OneByte);
			Append (value);
		}
		
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">byte to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(byte value, ConstantParameterType type)
		{
			Append(type, FollowType.OneByte);
			Append (value);
		}
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">Int16 to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(Int16 value , ConstantParameterType type){
			Append(type, FollowType.TwoBytes);
			Append (value);
		}
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">Int32 to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(Int32 value, ConstantParameterType type){
			Append(type,  FollowType.FourBytes);
			Append(value);
		}
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">UInt32 to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(UInt32 value, ConstantParameterType type){
			Append(type,  FollowType.FourBytes);
			Append(value);
		}
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="value">Float to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(float value, ConstantParameterType type){
			Append(type,  FollowType.FourBytes);
			Append(value);
		}
		
		
		
		/// <summary>
		/// Append a constant parameter encoded
		/// </summary>
		/// <param name="s">String to append</param>
		/// <param name="type">User either value or lable type</param>
		public void Append(string s, ConstantParameterType type){
			Append(type,  FollowType.TerminatedString2);
			Append (s);
		}
		
		/// <summary>
		/// Append a variable parameter encoded byte in short format 
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="scope">Select either global or local scope</param>
		public void Append(byte value, VariableScope scope)
		{
			byte b  = (byte)((byte)ParameterFormat.Short| (byte) ParameterType.Variable | (byte) scope | (byte)(value & ((byte)0x1f)));
			Append (b);	
		}
		
		private void Append(VariableScope scopeType, VariableType variableType, FollowType followType){
			byte b  = (byte)((byte)ParameterFormat.Long| (byte) ParameterType.Variable | (byte) scopeType | (byte)variableType | (byte)followType);
			Append (b);
		}
		
		/// <summary>
		/// Append a variable parameter encoded byte in long format 
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="scope">Select either global or local scope</param>
		/// <param name="type">Select either value or handle scope</param>
		public void Append (byte value, VariableScope scope, VariableType type)
		{
			Append(scope, type, FollowType.OneByte);
			Append (value);
		}
		
		/// <summary>
		/// Append a variable parameter encoded Int16
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="scope">Select either global or local scope</param>
		/// <param name="type">Select either value or handle scope</param>
		public void Append(Int16 value , VariableScope scope, VariableType type){
			Append(scope, type, FollowType.TwoBytes);
			Append (value);
		}

		/// <summary>
		/// Append a variable parameter encoded Int32
		/// </summary>
		/// <param name="value">Value to append</param>
		/// <param name="scope">Select either global or local scope</param>
		/// <param name="type">Select either value or handle scope</param>
		public void Append(Int32 value, VariableScope scope, VariableType type){
			Append(scope, type, FollowType.FourBytes);
			Append(value);
		}
		
		
		/// <summary>
		/// Append a variable parameter encoded string
		/// </summary>
		/// <param name="s">String to append</param>
		/// <param name="scope">Select either global or local scope</param>
		/// <param name="type">Select either value or handle scope</param>
		public void Append(string s, VariableScope scope, VariableType type){
			Append(scope, type, FollowType.TerminatedString2);
			Append (s);
		}
		
		/// <summary>
		/// Append the specified longType and followType.
		/// </summary>
		/// <param name="longType">Long type.</param>
		/// <param name="followType">Follow type.</param>
		private void Append(ConstantParameterType longType, FollowType followType){
			byte b  = (byte)((byte)ParameterFormat.Long| (byte) ParameterType.Constant | (byte) longType | (byte)followType);
			Append (b);
		}
		
		internal void Print(){
			byte[] arr = Data;
			for(int i = 0; i < Length; i++){
				//Console.WriteLine("Command["+i+"]: " + arr[i].ToString("X"));
				Console.WriteLine("Command["+i+"]: " + arr[i].ToString());
				
			}
		} 

	}


	/// <summary>
	/// Class for creating a EV3 reply
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
			get{
				CommandType type = (CommandType)dataArray[2];
				if(type == CommandType.DirectReply || type == CommandType.SystemReply){
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the type of error.
		/// </summary>
		/// <value>
		/// The type of error
		/// </value>
		internal ErrorType ErrorType{
			get{
				return Error.ToErrorType(ErrorCode);
			}
		}

		/// <summary>
		/// Gets the error code.
		/// </summary>
		/// <value>
		/// The error code
		/// </value>
		public byte ErrorCode {
			get {
				if (HasError) {
					if(CommandType == CommandType.SystemReplyWithError){
						if(dataArray.Length >=5){
							byte error = dataArray[4];
							if(Enum.IsDefined(typeof(EV3.BrickError),(int) error)){
								return error;
							}
						}
					}
					return (byte)BrickError.UnknownError;
				}
				return 0;//no error
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Reply"/> class.
		/// </summary>
		public Reply ()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.Reply"/> class.
		/// </summary>
		/// <param name='data'>
		/// The byte array to be used for the reply
		/// </param>
		public Reply(byte[] data){
			dataArray = data;
			if(data.Length < 4){
				throw new System.ArgumentException("Invalid EV3 Reply");
			}
			if(!Enum.IsDefined(typeof (CommandType),data[2])){
				throw new System.ArgumentException("Invalid EV3 Reply");
			}
			CommandType type = (CommandType)data[2];
			if(type == CommandType.SystemReply){
				if(!Enum.IsDefined(typeof(SystemCommand),data[3])){
					throw new System.ArgumentException("Invalid EV3 Reply");
				}	
			}
			if( type == CommandType.SystemCommand || type == CommandType.SystemCommand){
				throw new System.ArgumentException("Invalid EV3 Reply");	
			}
		}


		/// <summary>
		/// Gets the EV3 system command.
		/// </summary>
		/// <value>The system command.</value>
		public SystemCommand SystemCommandType{
			get{
				if(CommandType == CommandType.SystemReply){
					return (SystemCommand) dataArray[3];
				}
				return SystemCommand.None;
			}
		}
		/// <summary>
		/// Gets the EV3 command type
		/// </summary>
		/// <value>The type of the command.</value>
		public CommandType CommandType {
			get{return (CommandType) dataArray[2];}
		}

		/// <summary>
		/// Gets the sequence number.
		/// </summary>
		/// <value>The sequence number.</value>
		public UInt16 SequenceNumber {
			get{return (UInt16)(0x0000 | dataArray[0] | (dataArray[1] << 2));}
		}

		/// <summary>
		/// Gets the command byte as string.
		/// </summary>
		/// <value>
		/// The command byte as string
		/// </value>
		public string CommandTypeAsString{
			get{return BrickCommand.AddSpacesToString(CommandType.ToString()); }
		} 

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

