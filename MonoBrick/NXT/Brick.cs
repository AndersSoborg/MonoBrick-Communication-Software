using System;
using System.Threading;
using System.Text;
using MonoBrick;
using System.Reflection;

namespace MonoBrick.NXT
{
	/// <summary>
	/// Class that holds the brick device information
	/// </summary>
	public class DeviceInfo{
		private string brickName;
		private string btAddress;
		private UInt32 freeFlashMemory;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.DeviceInfo"/> class.
		/// </summary>
		/// <param name='name'>
		/// The name of the brick
		/// </param>
		/// <param name='address'>
		/// The bluetooth address of the brick
		/// </param>
		/// <param name='flashSize'>
		/// Available flash memory in bytes
		/// </param>
		public DeviceInfo(string name, string address, UInt32 flashSize)
		{
			brickName = name; 
			btAddress = address; 
			freeFlashMemory = flashSize;
		}
		/// <summary>
		/// Gets the name of the brick.
		/// </summary>
		/// <value>
		/// The name of the brick.
		/// </value>
		public string BrickName{
			get{return brickName;}
		}

		/// <summary>
		/// Gets the bluetooth address.
		/// </summary>
		/// <value>
		/// The bluetooth address.
		/// </value>
		public string BluetoothAddress{
			get{return btAddress;}
		}

		/// <summary>
		/// Gets the free flash memory in bytes
		/// </summary>
		/// <value>
		/// The free flash memory.
		/// </value>
		public uint FreeFlashMemory{
			get{return freeFlashMemory;}
		}
	}

	/// <summary>
	/// Holds information about the firmware
	/// </summary>
	public class DeviceFirmware{
		private string firmware;
		private string protocol;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.DeviceFirmware"/> class.
		/// </summary>
		/// <param name='firmwareVersion'>
		/// Firmware version.
		/// </param>
		/// <param name='protocolVersion'>
		/// Protocol version.
		/// </param>
		public DeviceFirmware(string firmwareVersion, string protocolVersion)
		{
			firmware = firmwareVersion; 
			protocol = protocolVersion;  
		}

		/// <summary>
		/// Gets the protocol version.
		/// </summary>
		/// <value>
		/// The protocol version.
		/// </value>
		public string ProtocolVersion{
			get{return protocol;}
		}

		/// <summary>
		/// Gets the firmware version.
		/// </summary>
		/// <value>
		/// The firmware version.
		/// </value>
		public string FirmwareVersion{
			get{return firmware;}
		}
	}

	/// <summary>
	/// Class for NXT brick
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
		private Vehicle vehicle = new Vehicle(MotorPort.OutA,MotorPort.OutC);
		private Mailbox mailbox = new Mailbox();
		private void Init(){
			Sensor1 = new TSensor1();
			Sensor2 = new TSensor2();
			Sensor3 = new TSensor3();
			Sensor4 = new TSensor4();
			fileSystem.Connection = connection;
			motorA.Connection = connection;
			motorA.Port = MotorPort.OutA;
			motorB.Connection = connection;
			motorB.Port = MotorPort.OutB;
			motorC.Connection = connection;
			motorC.Port = MotorPort.OutC;
			vehicle.Connection = connection;
			mailbox.Connection = connection;
		}

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
                //sensor[0].Initialize();
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
                //sensor[1].Initialize();
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
                //sensor[2].Initialize();
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
                //sensor[3].Initialize();
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
		/// Initializes a new instance of the Brick class with bluetooth, usb or loopback connection
		/// </summary>
		/// <param name='connection'>
		/// Can either be a serial port name for bluetooth connection or "usb" for usb connection and finally "loopback" for loopback connection
		/// </param>
		public Brick(string connection)
		{

			switch(connection.ToLower()){
				case "usb":
					this.connection = new USB<Command,Reply>();
					break;
				case "loopback":
					this.connection = new Loopback<Command,Reply>();
					break;
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
		/// Sets the name of the brick.
		/// </summary>
		/// <param name='name'>
		/// The new name of the brick
		/// </param>
		public void SetBrickName(string name){
			SetBrickName(name,false);		
		}
		
		/*public bool IsConnected{
			get{return connection.IsConnected;}
		}*/

		/// <summary>
		/// Sets the name of the brick.
		/// </summary>
		/// <param name='name'>
		/// The new name of the brick
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void SetBrickName(string name, bool reply){
			Command command = new Command(CommandType.SystemCommand,CommandByte.SetBrickName, reply);
			command.Append(name,15,true);
			connection.Send(command);
			//command.Print();
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}		
		}

		/// <summary>
		/// Gets the device info of the brick
		/// </summary>
		/// <returns>
		/// The device info
		/// </returns>
		public DeviceInfo GetDeviceInfo(){
			var reply = connection.SendAndReceive(new Command(CommandType.SystemCommand, CommandByte.GetDeviceInfo, true));
			Error.CheckForError(reply,33);
			StringBuilder btAddHex = new StringBuilder(17);
  			for(int i = 0; i < 5; i++){
				btAddHex.AppendFormat("{0:x2}", reply[18+i]);
				btAddHex.Append(':');
			}
			btAddHex.AppendFormat("{0:x2}", reply[23]);
			return new DeviceInfo(reply.GetString(3),btAddHex.ToString().ToUpper(),reply.GetUInt32(29));
		}

		/// <summary>
		/// Gets the name of the brick.
		/// </summary>
		/// <returns>
		/// The brick name
		/// </returns>
		public string GetBrickName(){
			return GetDeviceInfo().BrickName;
		}

		/// <summary>
		/// Gets the bluetooth address.
		/// </summary>
		/// <returns>
		/// The bluetooth address
		/// </returns>
		public string GetBluetoothAddress(){
			return GetDeviceInfo().BluetoothAddress;
		}
		
		/// <summary>
		/// Gets the free flash memory in bytes
		/// </summary>
		/// <returns>
		/// The free flash memory in bytes
		/// </returns>
		public UInt32 GetFreeFlashMemory(){
			return GetDeviceInfo().FreeFlashMemory;
		}

		/// <summary>
		/// Gets the battery level in mV
		/// </summary>
		/// <returns>
		/// The battery level in mV
		/// </returns>
		public UInt16 GetBatteryLevel(){
			var reply = connection.SendAndReceive(new Command(CommandType.DirecCommand, CommandByte.GetBatteryLevel, true));
			Error.CheckForError(reply,5);
			return reply.GetUInt16(3);
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
			Command command = new Command(CommandType.DirecCommand, CommandByte.StartProgram, reply);
			command.Append(name, MonoBrick.NXT.FilSystem.MaxFileNameLength,true);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
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
			connection.Send(new Command(CommandType.DirecCommand, CommandByte.StopProgram, reply));
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}

		/// <summary>
		/// Get the name of the program that is curently running
		/// </summary>
		/// <returns>
		/// The running program.
		/// </returns>
		public string GetRunningProgram(){
			var reply = connection.SendAndReceive(new Command(CommandType.DirecCommand, CommandByte.GetCurrentProgramName,true));
			Error.CheckForError(reply,23);
			return reply.GetString(3,19);
		}

		/// <summary>
		/// Play a tone
		/// </summary>
		/// <param name='frequency'>
		/// Frequency of the tone
		/// </param>
		/// <param name='durationMs'>
		/// Duration in ms
		/// </param>
		public void PlayTone(UInt16 frequency, UInt16 durationMs){
			PlayTone(frequency,durationMs,false);
		}

		/// <summary>
		/// Play a tone
		/// </summary>
		/// <param name='frequency'>
		/// Frequency of the tone
		/// </param>
		/// <param name='durationMs'>
		/// Duration in ms
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> reply the brick will send a reply
		/// </param>
		public void PlayTone(UInt16 frequency, UInt16 durationMs, bool reply){
			Command command = new Command(CommandType.DirecCommand, CommandByte.PlayTone, reply);
			command.Append(frequency);
			command.Append(durationMs);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}

		/// <summary>
		/// Make the brick say beep
		/// </summary>
		/// <param name='durationMs'>
		/// Duration in ms
		/// </param>
		public void Beep(UInt16 durationMs){
			Beep(durationMs,false);
		}

		/// <summary>
		/// Make the brick say beep
		/// </summary>
		/// <param name='durationMs'>
		/// Duration in ms
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply.
		/// </param>
		public void Beep(UInt16 durationMs, bool reply){
			PlayTone(1000, durationMs,reply);
		}

		/// <summary>
		/// Play a sound file
		/// </summary>
		/// <param name='name'>
		/// The name of the sound file to play
		/// </param>
		/// <param name='loop'>
		/// If set to <c>true</c> the sound file will loop
		/// </param>
		public void PlaySoundFile(string name,bool loop){
			PlaySoundFile(name,loop,false);
		}

		/// <summary>
		/// Play a sound file
		/// </summary>
		/// <param name='name'>
		/// The name of the sound file to play
		/// </param>
		/// <param name='loop'>
		/// If set to <c>true</c> the sound file will loop
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply.
		/// </param>
		public void PlaySoundFile(string name,bool loop, bool reply){
			Command command = new Command(CommandType.DirecCommand, CommandByte.PlaySoundFile, reply);
			command.Append(loop);
			command.Append(name,MonoBrick.NXT.FilSystem.MaxFileNameLength,true);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}

		/// <summary>
		/// Gets information about the NXT's firmware
		/// </summary>
		/// <returns>
		/// The device firmware class 
		/// </returns>
		public DeviceFirmware GetDeviceFirmware(){
			var reply = connection.SendAndReceive(new Command(CommandType.SystemCommand,CommandByte.GetFirmware, true));
			Error.CheckForError(reply,7);
			StringBuilder protocol = new StringBuilder(10);
			protocol.Append(reply[4]);
			protocol.Append('.');
			protocol.Append(reply[3]);
			
			StringBuilder firmware = new StringBuilder(10);
			firmware.Append(reply[6]);
			firmware.Append('.');
			firmware.AppendFormat("{0:d2}",reply[5]);
			return new DeviceFirmware(firmware.ToString(), protocol.ToString());
		}

		/// <summary>
		/// Gets the firmware version.
		/// </summary>
		/// <returns>
		/// The firmware version
		/// </returns>
		public string GetFirmwareVersion(){
			return GetDeviceFirmware().FirmwareVersion;
		}

		/// <summary>
		/// Gets the protocol version.
		/// </summary>
		/// <returns>
		/// The protocol version
		/// </returns>
		public string GetProtocolVersion(){
			return GetDeviceFirmware().ProtocolVersion;
		}

		/// <summary>
		/// Get information about the keep alive settings
		/// </summary>
		/// <returns>
		/// The time in minuts that the brick will stay alive
		/// </returns>
		public Int32 KeepAlive(){
			var reply = connection.SendAndReceive(new Command(CommandType.DirecCommand, CommandByte.KeepAlive, true));
			Error.CheckForError(reply,7);
			return reply.GetInt32(3);
		}

        /// <summary>
        /// Resets the bluetooth settings
        /// </summary>
        /// <param name='reply'>
        /// If set to <c>true</c> nxt will send a reply
        /// </param>
		public void ResetBluetoothSettings(bool reply) {
            throw new NotImplementedException();
            //TODO Add implementation     
        }
		#endregion
	}
}
