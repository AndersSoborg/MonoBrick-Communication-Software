using System;
using MonoBrick.EV3;

namespace MonoBrick.EV3
{
	/// <summary>
	/// Class for creating a EV3 brick
	/// </summary>
	public class Brick<TSensor1,TSensor2,TSensor3,TSensor4>
		where TSensor1 : Sensor, new()
		where TSensor2 : Sensor, new()
		where TSensor3 : Sensor, new()
		where TSensor4 : Sensor, new()
	{
		#region wrapper for connection, filesystem, sensor and motor
		private Connection<Command,Reply> connection = null;
		private TSensor1 sensor1;
		private TSensor2 sensor2;
		private TSensor3 sensor3;
		private TSensor4 sensor4;
		private FilSystem fileSystem = new FilSystem();
		private Motor motorA = new Motor();
		private Motor motorB = new Motor();
		private Motor motorC = new Motor();
		private Motor motorD = new Motor();
		private Memory memory = new Memory();
		private MotorSync motorSync = new MotorSync();
		private Vehicle vehicle = new Vehicle(MotorPort.OutA,MotorPort.OutC);
		private Mailbox mailbox = new Mailbox();
		private void Init(){
			Sensor1 = new TSensor1();
			Sensor2 = new TSensor2();
			Sensor3 = new TSensor3();
			Sensor4 = new TSensor4();
			fileSystem.Connection = connection;
			motorA.Connection = connection;
			motorA.BitField = OutputBitfield.OutA; 
			motorB.Connection = connection;
			motorB.BitField = OutputBitfield.OutB;
			motorC.Connection = connection;
			motorC.BitField = OutputBitfield.OutC;
			motorD.Connection = connection;
			motorD.BitField = OutputBitfield.OutD;
			motorSync.Connection = connection;
			motorSync.BitField = OutputBitfield.OutA | OutputBitfield.OutD;
			memory.Connection = connection;
			mailbox.Connection = connection;
			vehicle.Connection = connection;
			
		}
		
		/// <summary>
		/// Manipulate memory on the EV3
		/// </summary>
		/// <value>The memory.</value>
		/*public Memory Memory {
			get{return memory;}
		}
		*/
		
		/// <summary>
		/// Message system used to write and read data to/from the brick
		/// </summary>
		/// <value>
		/// The message system
		/// </value>
		public Mailbox Mailbox{
			get{ return mailbox;}
		}

		/// <summary>
		/// Motor A
		/// </summary>
		/// <value>
		/// The motor connected to port A
		/// </value>
		public Motor MotorA{
			get{ return motorA;}
		}

		/// <summary>
		/// Motor B
		/// </summary>
		/// <value>
		/// The motor connected to port B
		/// </value>
		public Motor MotorB{
			get{ return motorB;}
		}

		/// <summary>
		/// Motor C
		/// </summary>
		/// <value>
		/// The motor connected to port C
		/// </value>
		public Motor MotorC{
			get{ return motorC;}
		}
		
		/// <summary>
		/// Motor D
		/// </summary>
		/// <value>
		/// The motor connected to port D
		/// </value>
		public Motor MotorD{
			get{ return motorD;}
		}
		
		/// <summary>
		/// Synchronise two motors
		/// </summary>
		/// <value>The motor sync.</value>
		public MotorSync MotorSync{
			get{ return motorSync;}
		}

		/// <summary>
		/// Use the brick as a vehicle
		/// </summary>
		/// <value>
		/// The vehicle
		/// </value>
		public Vehicle Vehicle{
			get{ return vehicle;}
		}

		/// <summary>
		/// Gets or sets the sensor connected to port 1
		/// </summary>
		/// <value>
		/// The sensor connected to port 1
		/// </value>
		public TSensor1 Sensor1{
			get{ return sensor1;}
			set{ 
				sensor1 = value;
				sensor1.Port = SensorPort.In1;
				sensor1.Connection = connection;
			}
		}

		/// <summary>
		/// Gets or sets the sensor connected to port 2
		/// </summary>
		/// <value>
		/// The sensor connected to port 2
		/// </value>
		public TSensor2 Sensor2{
			get{ return sensor2;}
			set{ 
				sensor2 = value;
				sensor2.Port = SensorPort.In2; 
				sensor2.Connection = connection;
			}
		}

		/// <summary>
		/// Gets or sets the sensor connected to port 3
		/// </summary>
		/// <value>
		/// The sensor connected to port 3
		/// </value>
		public TSensor3 Sensor3{
			get{ return sensor3;}
			set{ 
				sensor3 = value;
				sensor3.Port = SensorPort.In3; 
				sensor3.Connection = connection;
			}
		}

		/// <summary>
		/// Gets or sets the sensor connected to port 4
		/// </summary>
		/// <value>
		/// The sensor connected to port 4
		/// </value>
		public TSensor4 Sensor4{
			get{ return sensor4;}
			set{ 
				sensor4 = value;
				sensor4.Port = SensorPort.In4; 
				sensor4.Connection = connection;
			}
		}

		/// <summary>
		/// The file system 
		/// </summary>
		/// <value>
		/// The file system
		/// </value>
		public FilSystem FileSystem{
			get{return fileSystem;}
		}

		/// <summary>
		/// Gets the connection that the brick uses
		/// </summary>
		/// <value>
		/// The connection
		/// </value>
		public Connection<Command,Reply> Connection{
			get{return connection;}
		}


		/// <summary>
		/// Initializes a new instance of the Brick class.
		/// </summary>
		/// <param name='connection'>
		/// Connection to use
		/// </param>
		public Brick(Connection<Command,Reply> connection){
			this.connection = connection;
			Init();
		}

		/// <summary>
		/// Initializes a new instance of the Brick class with bluetooth, usb or WiFi connection
		/// </summary>
		/// <param name='connection'>
		/// Can either be a serial port name for bluetooth connection or "usb" for usb connection and finally "wiFi" for WiFi connection
		/// </param>
		public Brick(string connection)
		{

			switch(connection.ToLower()){
				case "usb":
					this.connection = new USB<Command,Reply>();
				break;
				case "wifi":
					this.connection = new WiFiConnection<Command,Reply>(10000); //10 seconds timeout when connecting
				break;
				case "loopback":
					throw new NotImplementedException("Loopback connection has not been implemented for EV3");
				default:
				this.connection = new Bluetooth<Command,Reply>(connection);
				break;
			}
			Init();
		}

		/// <summary>
		/// Initializes a new instance of the Brick class with a tunnel connection
		/// </summary>
		/// <param name='ipAddress'>
		/// The IP address to use
		/// </param>
		/// <param name='port'>
		/// The port number to use
		/// </param>
		public Brick(string ipAddress, ushort port){
			connection = new TunnelConnection<Command, Reply>(ipAddress, port);
			Init();
		}

		#endregion

		#region brick functions

		/// <summary>
		/// Start a program on the brick
		/// </summary>
		/// <param name="file">File to start</param>
		public void StartProgram (BrickFile file)
		{
			StartProgram(file,false);
		}
		
		/// <summary>
		/// Start a program on the brick
		/// </summary>
		/// <param name="file">File to stat.</param>
		/// <param name="reply">If set to <c>true</c> reply from brick will be send</param>
		public void StartProgram (BrickFile file, bool reply)
		{
			StartProgram(file.FullName,reply);
		}
		
		/// <summary>
		/// Start a program on the brick
		/// </summary>
		/// <param name='name'>
		/// The name of the program to start
		/// </param>
		public void StartProgram(string name){
			StartProgram(name, false);
		}

		/// <summary>
		/// Starts a program on the brick
		/// </summary>
		/// <param name='name'>
		/// The of the program to start
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void StartProgram(string name, bool reply){
			var command = new Command(0,8, 400,reply);
			command.Append(ByteCodes.File);
			command.Append(FileSubCodes.LoadImage);
			command.Append((byte)ProgramSlots.User,ConstantParameterType.Value);
			command.Append(name, ConstantParameterType.Value);
			command.Append(0, VariableScope.Local);
			command.Append(4, VariableScope.Local);
			command.Append(ByteCodes.ProgramStart);
			command.Append((byte)ProgramSlots.User);
			command.Append(0, VariableScope.Local);
			command.Append(4, VariableScope.Local);
			command.Append(0,ParameterFormat.Short);
			connection.Send(command);
			System.Threading.Thread.Sleep(5000);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,400);
			}
		}

		/// <summary>
		/// Stops all running programs
		/// </summary>
		public void StopProgram(){
			StopProgram(false);	
		} 

		/// <summary>
		/// Stops all running programs
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> reply the brick will send a reply
		/// </param>
		public void StopProgram(bool reply){
			var command = new Command(0,0, 401,reply);
			command.Append(ByteCodes.ProgramStop);
			command.Append((byte)ProgramSlots.User, ConstantParameterType.Value);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,401);
			}
		}

		/// <summary>
		/// Get the name of the program that is curently running
		/// </summary>
		/// <returns>
		/// The running program.
		/// </returns>
		public string GetRunningProgram(){
			return "";
		}

		/// <summary>
		/// Play a tone.
		/// </summary>
		/// <param name="volume">Volume.</param>
		/// <param name="frequency">Frequency of the tone</param>
		/// <param name="durationMs">Duration in ms.</param>
		public void PlayTone(byte volume,UInt16 frequency, UInt16 durationMs){
			PlayTone(volume, frequency,durationMs,false);
		}

		/// <summary>
		/// Play a tone.
		/// </summary>
		/// <param name="volume">Volume.</param>
		/// <param name="frequency">Frequency of the tone</param>
		/// <param name="durationMs">Duration in ms.</param>
		/// <param name="reply">If set to <c>true</c> reply from brick will be send</param>
		public void PlayTone(byte volume,UInt16 frequency, UInt16 durationMs, bool reply){
			var command = new Command(0,0,123,reply);
			command.Append(ByteCodes.Sound);
			command.Append(SoundSubCodes.Tone);
			command.Append(volume, ParameterFormat.Short);
			command.Append(frequency, ConstantParameterType.Value);
			command.Append(durationMs, ConstantParameterType.Value);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,123);
			}		
		}

		/// <summary>
		/// Make the brick say beep
		/// </summary>
		/// <param name="volume">Volume of the beep</param>
		/// <param name="durationMs">Duration in ms.</param>
		public void Beep(byte volume, UInt16 durationMs){
			Beep(volume,durationMs,false);
		}

		/// <summary>
		/// Make the brick say beep
		/// </summary>
		/// <param name="volume">Volume of the beep</param>
		/// <param name="durationMs">Duration in ms.</param>
		/// <param name="reply">If set to <c>true</c> reply from the brick will be send</param>
		public void Beep(byte volume, UInt16 durationMs, bool reply){
			PlayTone(volume,1000, durationMs,reply);
		}

		/// <summary>
		/// Play a sound file.
		/// </summary>
		/// <param name="name">Name the name of the file to play</param>
		/// <param name="volume">Volume.</param>
		/// <param name="repeat">If set to <c>true</c> the file will play in a loop</param>
		public void PlaySoundFile(string name, byte volume, bool repeat){
			PlaySoundFile(name, volume, repeat, false);
		}

		/// <summary>
		/// Play a sound file.
		/// </summary>
		/// <param name="name">Name the name of the file to play</param>
		/// <param name="volume">Volume.</param>
		/// <param name="repeat">If set to <c>true</c> the file will play in a loop</param>
		/// <param name="reply">If set to <c>true</c> a reply from the brick will be send</param>
		public void PlaySoundFile(string name, byte volume, bool repeat ,bool reply){
			Command command = null;
			if(repeat){
				command = new Command(0,0,200,reply);
				command.Append(ByteCodes.Sound);
				command.Append(SoundSubCodes.Repeat);
				command.Append(volume, ConstantParameterType.Value);
				command.Append(name, ConstantParameterType.Value);
				command.Append(ByteCodes.SoundReady);//should this be here?
			}
			else{
				command = new Command(0,0,200,reply);
				command.Append(ByteCodes.Sound);
				command.Append(SoundSubCodes.Play);
				command.Append(volume, ConstantParameterType.Value);
				command.Append(name, ConstantParameterType.Value);
				command.Append(ByteCodes.SoundReady);//should this be here?
			}
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,200);
			}
		}
		
		/// <summary>
		/// Stops all sound playback.
		/// </summary>
		/// <param name="reply">If set to <c>true</c> reply from brick will be send</param>
		public void StopSoundPlayback(bool reply = false){
			var command = new Command(0,0,123,reply);
			command.Append(ByteCodes.Sound);
			command.Append(SoundSubCodes.Break);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,123);
			}
		}
		
		/// <summary>
		/// Gets the sensor types of all four sensors
		/// </summary>
		/// <returns>The sensor types.</returns>
		public SensorType[] GetSensorTypes ()
		{
			var command = new Command(5,0,200,true);
			command.Append(ByteCodes.InputDeviceList);
			command.Append((byte)4,ParameterFormat.Short);
			command.Append((byte)0, VariableScope.Global);
			command.Append((byte)4, VariableScope.Global);
			var reply = Connection.SendAndReceive(command);
			SensorType[] type = new SensorType[4];
			for(int i = 0; i < 4; i++){
				if(Enum.IsDefined(typeof(SensorType), (int) reply[i + 3])){
					type[i] = (SensorType)reply[i + 3];
				}
				else{
					type[i] = SensorType.Unknown;
				}
			}
			return type;
		}	
		#endregion
	}
}

