using System;
using MonoBrick;
using System.Collections.Generic;
namespace MonoBrick.EV3
{
		/// <summary>
	    /// Sensor ports
	    /// </summary>
	    public enum SensorPort  {
			#pragma warning disable 
			In1 = 0, In2 = 1, In3 = 2, In4 = 3 
			#pragma warning restore
		};
		
		/// <summary>
		/// Device types
		/// </summary>
		public enum SensorType  {
			#pragma warning disable 
			Unknown = 0, NXTTouch = 1, NXTLight = 2, NXTSound = 3, NXTColor = 4, NXTUltraSonic = 5, NXTTemperature = 6, LMotor = 7 , MMotor = 8,
			Touch = 16, Test = 21, I2C = 100, NXTTest = 101, Color = 29, UltraSonic = 30, Gyro = 32, IR = 33,  None = 126 
			#pragma warning restore
		};
		
		/// <summary>
		/// Sensor modes
		/// </summary>
		public enum SensorMode {
			#pragma warning disable 
			Mode0 = 0, Mode1 = 1, Mode2 = 2, Mode3 = 3, Mode4 = 4, Mode5 = 5, Mode6 = 6	
			#pragma warning restore
		};
		
		/// <summary>
		/// Class for creating a sensor 
		/// </summary>
		public class Sensor : ISensor
		{
			internal SensorPort Port{get; set;}
			
			internal Connection<Command,Reply> Connection {get;set;}
			
			/// <summary>
			/// Gets or sets the daisy chain layer.
			/// </summary>
			/// <value>The daisy chain layer.</value>
			public DaisyChainLayer DaisyChainLayer{get;set;}
			
			/// <summary>
			/// Gets the sensor type
			/// </summary>
			/// <returns>The type.</returns>
			public SensorType GetSensorType()
			{
				if(!isInitialized)
					Initialize();
				SensorType type;
				SensorMode mode;
				GetTypeAndMode(out type, out mode);
				return type;
			}
			
			/// <summary>
			/// Gets the sensor mode.
			/// </summary>
			/// <returns>The mode.</returns>
			protected SensorMode GetSensorMode ()
			{
				SensorType type;
				SensorMode mode;
				GetTypeAndMode(out type, out mode);
				return mode;
			}
			
			/// <summary>
			/// Gets the sensor type and mode.
			/// </summary>
			/// <param name="type">Type.</param>
			/// <param name="mode">Mode.</param>
			protected void GetTypeAndMode(out SensorType type, out SensorMode mode){
				if(!isInitialized)
					Initialize();
				var command = new Command(2,0,200,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetTypeMode);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				
				command.Append((byte)0, VariableScope.Global);
				command.Append((byte)1, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,200);
				type = (SensorType) reply.GetByte(3);
				mode = (SensorMode) reply.GetByte(4);
			}
			
			/// <summary>
			/// Gets the name of the sensor
			/// </summary>
			/// <returns>The name.</returns>
			public virtual string GetName ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(Command.ShortValueMax,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetName);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(Command.ShortValueMax, ConstantParameterType.Value); 
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetString(3,(byte) Command.ShortValueMax);
			}
			
			/// <summary>
			/// Get device symbol
			/// </summary>
			/// <returns>The symbole.</returns>
			public virtual string GetSymbole ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(Command.ShortValueMax,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetSymbol);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(Command.ShortValueMax, ConstantParameterType.Value); 
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetString(3,(byte) Command.ShortValueMax);
			}
			
			/// <summary>
			/// Get sensor format
			/// </summary>
			/// <returns>The format.</returns>
			protected string GetFormat ()
			{
				throw new NotSupportedException();
			}
			
			/// <summary>
			/// Read the raw sensor value
			/// </summary>
			/// <returns>The raw sensor value.</returns>
			protected Int32 GetRaw ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetRaw);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetInt32(3);
			
			}
			
			/// <summary>
			/// Get device mode name
			/// </summary>
			/// <returns>The device mode name.</returns>
			protected string GetModeName ()
			{
				return GetModeName(this.mode);
			}
			
			/// <summary>
			/// Get device mode name
			/// </summary>
			/// <returns>The device mode name.</returns>
			/// <param name="mode">Mode to get name of.</param>
			protected string GetModeName (SensorMode mode)
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(Command.ShortValueMax,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetModeName);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte) mode, ConstantParameterType.Value); 
				
				command.Append(Command.ShortValueMax, ConstantParameterType.Value); 
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetString(3,(byte) Command.ShortValueMax);
			}
			
			/// <summary>
			/// Gets figure layout.
			/// </summary>
			/// <param name="figures">Figures.</param>
			/// <param name="decimals">Decimals.</param>
			protected void GetFigures(out byte  figures, out byte decimals){
				if(!isInitialized)
					Initialize();
				var command = new Command(2,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetFigures);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)0, VariableScope.Global);
				command.Append((byte)1, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				figures = reply.GetByte(3);
				decimals = reply.GetByte(4);
			}
			
			/// <summary>
			/// Gets the min and max values that can be returned.
			/// </summary>
			/// <param name="min">Minimum.</param>
			/// <param name="max">Maxium.</param>
			protected void GetMinMax(out float  min, out float max){
				if(!isInitialized)
					Initialize();
				var command = new Command(8,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetMinMax);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)0, VariableScope.Global);
				command.Append((byte)4, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				min = reply.GetFloat(3);
				max = reply.GetFloat(7);
			}
			
			/// <summary>
			/// </summary>
			protected byte ReadyPct ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(1,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.ReadyPCT);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value); 
				command.Append(mode); 
				command.Append(1, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetByte(3);	
			}
			
			/// <summary>
			/// </summary>
			protected Int32 ReadyRaw ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.ReadyRaw);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value); 
				command.Append(mode); 
				command.Append(1, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetInt32(3);	
			}
			
			/// <summary>
			/// </summary>
			protected float ReadySi ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.ReadySI);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value); 
				command.Append(mode); 
				command.Append(1, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetFloat(3);	
			}
			
			/// <summary>
			/// Get positive changes since last clear
			/// </summary>
			/// <returns>The changes.</returns>
			protected float GetChanges ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetChanges);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetFloat(3);
			}
			
			/// <summary>
			/// Get the bolean count since the last clear
			/// </summary>
			/// <returns>The bumb count.</returns>
			protected float GetBumbs ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.GetBumps);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetFloat(3);
			}
			
			/// <summary>
			/// Clear changes and bumps
			/// </summary>
			protected void ClearChanges ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(0,0,201,false);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.ClrChanges);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				Connection.Send(command);
			}
			
			/// <summary>
			/// Apply new minimum and maximum raw value for device type to be used in scaling PCT and SI
			/// </summary>
			/// <param name="min">Minimum.</param>
			/// <param name="max">Max.</param>
			protected void CalcMinMax (UInt32 min, UInt32 max)
			{
				throw new NotImplementedException();
			}
			
			/// <summary>
			/// Apply new minimum raw value for device type to be used in scaling PCT and SI
			/// </summary>
			/// <param name="min">Minimum.</param>
			protected void CalcMin(UInt32 min)
			{
				throw new NotImplementedException();
			}
			
			/// <summary>
			/// Apply new maximum raw value for device type to be used in scaling PCT and SI
			/// </summary>
			/// <param name="max">Max.</param>
			protected void CalcMax (UInt32 max)
			{
				throw new NotImplementedException();
			}
			
			/// <summary>
			/// Apply the default minimum and maximum raw value for device type to be used in scaling PCT and SI
			/// </summary>
			protected void CalcDefault()
			{
				throw new NotImplementedException();
			}
			
			/// <summary>
			/// Generic setup/read IIC sensors
			/// </summary>
			/// <returns>DATA8 array (handle) to read into</returns>
			/// <param name="repeat">Repeat setup/read "REPEAT" times (0 = infinite)</param>
			/// <param name="reapeatTime">Time between repeats [10..1000mS] (0 = 10)</param>
			/// <param name="writeData">Byte array to write</param>
			protected byte SetUp (byte repeat, Int16 reapeatTime, byte[] writeData)
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0,201,true);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.Setup);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(repeat, ConstantParameterType.Value); 
				command.Append(repeat, ConstantParameterType.Value); 
				command.Append(writeData.Length, ConstantParameterType.Value); 
				command.Append(1, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,201);
				return reply.GetByte(3);
			
			}
			
			/// <summary>
			/// Clear all devices (e.c. counters, angle, ...)
			/// </summary>
			protected void ClearAll ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(0,0,201,false);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.ClearAll);
				command.Append(this.DaisyChainLayer);
				Connection.Send(command);
			}
			/// <summary>
			/// Stop all devices (e.c. motors, ...)
			/// </summary>
			protected void StopAll ()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(0,0,201,false);
				command.Append(ByteCodes.InputDevice);
				command.Append(InputSubCodes.StopAll);
				command.Append(this.DaisyChainLayer);
				Connection.Send(command);
			}			
			
			/// <summary>
			/// Read a sensor value
			/// </summary>
			/// <returns>The sensor value.</returns>
			protected byte GetRead(){
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0, 123, true);
				command.Append(ByteCodes.InputRead);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value);
				command.Append(mode);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,123);
				return reply[3];
			} 
			
			/// <summary>
			/// Reads the si sensor value
			/// </summary>
			/// <returns>The si sensor value.</returns>
			protected float ReadSi()
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(4,0, 123, true);
				command.Append(ByteCodes.InputReadSI);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value);
				command.Append(mode);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,123);
				return reply.GetFloat(3);
			}
			
			/// <summary>
			/// Sets the sensor mode
			/// </summary>
			/// <param name="mode">Mode to use.</param>
			protected virtual void SetMode(SensorMode mode){
				isInitialized = true;
				this.mode = mode;
				var command = new Command(4,0, 123, true);
				command.Append(ByteCodes.InputReadSI);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append(0, ConstantParameterType.Value);
				command.Append(mode);
				command.Append((byte)0, VariableScope.Global);
				var reply = Connection.SendAndReceive(command);
				Error.CheckForError(reply,123);
			}
			
			/// <summary>
			/// Wait for device ready (wait for valid data)
			/// </summary>
			/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
			protected bool IsReady ()
			{
				throw new NotImplementedException();
			}
			
			/// <summary>
			/// Write data to device (only UART devices)
			/// </summary>
			/// <param name="data">Data array to write.</param>
			protected void Write (byte[] data)
			{
				if(!isInitialized)
					Initialize();
				var command = new Command(0,0, 100, false);
				command.Append(ByteCodes.InputWrite);
				command.Append(this.DaisyChainLayer);
				command.Append(this.Port);
				command.Append((byte)data.Length, ParameterFormat.Short);
				foreach(byte b in data)
					command.Append(b);
				Connection.Send(command);	
			}
			/// <summary>
			/// If initialized has been called
			/// </summary>
			protected bool isInitialized = false;
			
			/// <summary>
			/// Holds the sensor mode that is used.
			/// </summary>
			protected SensorMode mode = SensorMode.Mode0;
			
			/// <summary>
			/// Initializes a new instance of the <see cref="MonoBrick.EV3.Sensor"/> class.
			/// </summary>
			public Sensor ()
			{	
				mode = SensorMode.Mode0;
				if(Connection != null && Connection.IsConnected)
					Initialize();				
			}
			
			/// <summary>
			/// Initializes a new instance of the <see cref="MonoBrick.EV3.Sensor"/> class.
			/// </summary>
			/// <param name="mode">Mode to use.</param>
			public Sensor (SensorMode mode)
			{
				this.mode = mode;
				if(Connection != null && Connection.IsConnected)
					Initialize();	
			}
			
			#region ISensor
			
			/// <summary>
			/// Initialize the sensor
			/// </summary>
			public void Initialize ()
			{
				SetMode(mode);
				ReadyRaw();
			}
			
			/// <summary>
			/// Reads the sensor value as a string.
			/// </summary>
			/// <returns>
			/// The value as a string
			/// </returns>
	        public virtual string ReadAsString(){
				return ReadSi().ToString();	
			}
			#endregion
	}
	
	/// <summary>
	/// Sensor mode when using a touch sensor
    /// </summary>
    public enum TouchMode { 
		/// <summary>
		/// On or off mode
		/// </summary>
		Boolean = SensorMode.Mode0, 
		/// <summary>
		/// Bump mode
		/// </summary>
		Count = SensorMode.Mode1
	};
    
	/// <summary>
	/// Class used for touch sensor. Works with both EV3 and NXT
	/// </summary>
	public class TouchSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.TouchSensor"/> class.
		/// </summary>
		public TouchSensor () : base((SensorMode)TouchMode.Boolean)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.TouchSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public TouchSensor (TouchMode mode) :  base((SensorMode)mode)
		{
		
		}
		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			if (mode == (SensorMode)TouchMode.Count) {
				s = Read() + " count";
			}
			if (mode == (SensorMode)TouchMode.Boolean) {
				if( Convert.ToBoolean(ReadSi () ) ){
					s = "On";
				}
				else{
					s = "Off";
				}
			}
			return s;
		}
		
		/// <summary>
		/// Read the value. In bump mode this will return the count
		/// </summary>
		public int Read ()
		{
			int value = 0;
			if (mode == (SensorMode)TouchMode.Count) {
				value = (int)GetBumbs();
			}
			if (mode == (SensorMode)TouchMode.Boolean) {
				if(GetRead() > 50){
					value = 1;
				}
				else{
					value = 0;
				}
			}
			return value;
		}
		
		/// <summary>
		/// Reset the bumb count
		/// </summary>
		public void Reset()
		{
			this.ClearChanges();
		}
		
		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>The mode.</value>
		public TouchMode Mode {
			get{return (TouchMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}
	}
	
	
	/// <summary>
	/// Colors that can be read from the  EV3 color sensor
	/// </summary>
	public enum Color{ 
		#pragma warning disable 
		None = 0, Black = 1, Blue = 2, Green = 3, 
		Yellow = 4, Red = 5, White = 6, Brown = 7
		#pragma warning restore
	};
	
	/// <summary>
	/// Sensor mode when using a EV3 color sensor
    /// </summary>
    public enum ColorMode { 
		/// <summary>
		/// Use the color sensor to read reflected light
		/// </summary>
		Reflection = SensorMode.Mode0, 
		
		/// <summary>
		/// Use the color sensor to detect the light intensity
		/// </summary>
		Ambient  = SensorMode.Mode1,
		
		/// <summary>
		/// Use the color sensor to distinguish between eight different colors
		/// </summary>
		Color  = SensorMode.Mode2,
		
		/// <summary>
		/// Read the raw value of the reflected light
		/// </summary>
		Raw  = SensorMode.Mode3,
		
		/// <summary>
		/// Activate the green light on the color sensor. Only works with the NXT Color sensor 
		/// </summary>
		NXTGreen = SensorMode.Mode3,
		
		/// <summary>
		/// Activate the green blue on the color sensor. Only works with the NXT Color sensor 
		/// </summary>
		NXTBlue = SensorMode.Mode4,
		
		//Raw = SensorMode.Mode5
		
		//RGBRaw  = SensorMode.Mode4,
		
		//ColorCalculated  = SensorMode.Mode5,
	};
	
	/// <summary>
	/// Class for EV3 and NXT Color sensor
	/// </summary>
	public class ColorSensor : Sensor{
		private SensorType? type = null;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.ColorSensor"/> class.
		/// </summary>
		public ColorSensor () : base((SensorMode)ColorMode.Color)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.ColorSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public ColorSensor (ColorMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Gets or sets the color mode.
		/// </summary>
		/// <value>The color mode.</value>
		public ColorMode Mode {
			get{return (ColorMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}
		
		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)ColorMode.Ambient:
			        s = Read().ToString();
			        break;
			   case (SensorMode)ColorMode.Color:
			        s = ReadColor().ToString();
			        break;
			   case (SensorMode)ColorMode.Reflection:
			        s = Read().ToString();
			        break;
			   case (SensorMode)ColorMode.Raw:
			        s = Read().ToString();
			        break;
			   	default:
			   		s = Read().ToString();
			   		break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the intensity of the reflected light
		/// </summary>
		public int Read()
		{
			int value = 0;
			switch (mode)
			{
			    case (SensorMode)ColorMode.Ambient:
			        value = GetRead();
			        break;
			   	case (SensorMode)ColorMode.Color:
			        value = GetRaw();
			        if(type == null)
			        	type = this.GetSensorType();
			        if(type == SensorType.NXTColor){
			        	NXT.Color nxtColor = (NXT.Color) value;
			        	switch(nxtColor){
			        		case MonoBrick.NXT.Color.Black:
			        			value = (int) Color.Black;
			        			break;
			        		case MonoBrick.NXT.Color.Blue:
			        			value = (int) Color.Blue;
			        			break;
			        		case MonoBrick.NXT.Color.Green:
			        			value = (int) Color.Green;
			        			break;
			        		case MonoBrick.NXT.Color.Red:
			        			value = (int) Color.Red;
			        			break;
			        		case MonoBrick.NXT.Color.White:
			        			value = (int) Color.White;
			        			break;
			        		case MonoBrick.NXT.Color.Yellow:
			        			value = (int) Color.Yellow;
			        			break;
			        	}
			        }
			        break;
			   	case (SensorMode)ColorMode.Reflection:
			        value = GetRead();
			        break;
			   	case (SensorMode)ColorMode.Raw:
			        if(type == null)
			        	type = this.GetSensorType();
			        if(type == SensorType.NXTColor){
			        	if( ((ColorMode)mode) == ColorMode.Raw){
			        		SetMode(SensorMode.Mode5);
			        	}
			        }
			        value = GetRaw();
			        break;
			   	default:
			   		value = GetRaw();
			   		break;
			}
			return value;			
		}
		
		/// <summary>
		/// Reads the color.
		/// </summary>
		/// <returns>The color.</returns>
		public Color ReadColor()
		{
			Color color = Color.None;
			if (mode == (SensorMode)ColorMode.Color) {
				color = (Color) GetRaw();
			}
			return color;
		}
	}
	
	
	/// <summary>
	/// Sensor mode when using a EV3 IR Sensor
    /// </summary>
    public enum IRMode { 
		/// <summary>
		/// Use the IR sensor as a distance sensor
		/// </summary>
		Proximity = SensorMode.Mode0, 
		
		/// <summary>
		/// Use the IR sensor to detect the location of the IR Remote
		/// </summary>
		Seek  = SensorMode.Mode1,
		
		/// <summary>
		/// Use the IR sensor to detect wich Buttons where pressed on the IR Remote
		/// </summary>
		Remote  = SensorMode.Mode2,
		
	};
	
	/// <summary>
	/// Class for the EV3 IR sensor - In seek or remote mode it only works with channel 0
	/// </summary>
	public class IRSensor : Sensor{
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.IRSensor"/> class.
		/// </summary>
		public IRSensor () : base((SensorMode)IRMode.Proximity)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.IRSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public IRSensor (IRMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Gets or sets the IR mode.
		/// </summary>
		/// <value>The mode.</value>
		public IRMode Mode {
			get{return (IRMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)IRMode.Proximity:
			        s = Read().ToString();
			        break;
			   case (SensorMode)IRMode.Remote:
			        s = Read().ToString();
			        break;
			   case (SensorMode)IRMode.Seek:
			        s = Read().ToString();
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the value of the sensor. The result will vary depending on the mode
		/// </summary>
		public int Read()
		{
			int value = 0;
			switch (mode)
			{
			    case (SensorMode)IRMode.Proximity:
			        value = GetRead();
			        break;
			   case (SensorMode)IRMode.Remote:
			        value = GetRaw();
			        break;
			   case (SensorMode)IRMode.Seek:
			        value = GetRaw();
			        break;
			}
			return value;			
		}
		
		/*public void SetChannel (int i)
		{
			throw new NotImplementedException();
			//byte[] data = { 0x12, 0x01};
			//Write(data);
		}*/ 
	}
		
	
	/// <summary>
	/// Sensor mode when using a NXT light sensor
    /// </summary>
    public enum LightMode { 
		/// <summary>
		/// Use the lgith sensor to read reflected light
		/// </summary>
		Relection = SensorMode.Mode0, 
		
		/// <summary>
		/// Use the light sensor to detect the light intensity
		/// </summary>
		Ambient  = SensorMode.Mode1,
	};
	
	/// <summary>
	/// Class for the NXT light sensor
	/// </summary>
	public class LightSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.LightSensor"/> class.
		/// </summary>
		public LightSensor () : base((SensorMode)LightMode.Relection)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.LightSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public LightSensor (LightMode mode) :  base((SensorMode)mode)
		{
		
		}
		/// <summary>
		/// Gets or sets the light mode.
		/// </summary>
		/// <value>The mode.</value>
		public LightMode Mode {
			get{return (LightMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)LightMode.Ambient:
			        s = Read().ToString();
			        break;
			   case (SensorMode)LightMode.Relection:
			        s = Read().ToString();
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read this instance.
		/// </summary>
		public int Read()
		{
			int value = 0;
			switch (mode)
			{
			    case (SensorMode)LightMode.Ambient:
			        value = GetRead();
			        break;
			   case (SensorMode)LightMode.Relection:
			        value = GetRead();
			        break;
			}
			return value;			
		}
		
		/// <summary>
		/// Reads the raw sensor value.
		/// </summary>
		/// <returns>The raw sensor value.</returns>
		public int ReadRaw ()
		{
			return GetRaw();
		}
		
	}
	
	
	/// <summary>
	/// Sensor mode when using a sound sensor
    /// </summary>
    public enum SoundMode { 
		/// <summary>
		/// The sound level is measured in A-weighting decibel
		/// </summary>
		SoundDBA = SensorMode.Mode1, 
		/// <summary>
		/// The sound level is measured in decibel 
		/// </summary>
		SoundDB = SensorMode.Mode0 };
    
	/// <summary>
	/// Class for the NXT sound sensor
	/// </summary>
	public class SoundSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.SoundSensor"/> class.
		/// </summary>
		public SoundSensor () : base((SensorMode)SoundMode.SoundDBA)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.SoundSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public SoundSensor (SoundMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)SoundMode.SoundDB:
			        s = Read().ToString();
			        break;
			   case (SensorMode)SoundMode.SoundDBA:
			        s = Read().ToString();
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the sensor value
		/// </summary>
		public int Read()
		{
			int value = 0;
			switch (mode)
			{
			    case (SensorMode)SoundMode.SoundDB:
			        value = GetRead();
			        break;
			   case (SensorMode)SoundMode.SoundDBA:
			        value = GetRead();
			        break;
			}
			return value;			
		}
		
		/// <summary>
		/// Reads the raw sensor value
		/// </summary>
		/// <returns>The raw value.</returns>
		public int ReadRaw ()
		{
			return GetRaw();
		}
		
		/// <summary>
		/// Gets or set the sound mode.
		/// </summary>
		/// <value>The mode.</value>
		public SoundMode Mode {
			get{return (SoundMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

	}
	
	/// <summary>
	/// Sensor modes when using a ultrasonic sensor
	/// </summary>
	public enum UltrasonicMode { 
		#pragma warning disable 
			/// <summary>
			/// Result will be in centimeter
			/// </summary>
			Centimeter = SensorMode.Mode0,
			/// <summary>
			/// Result will be in inch
			/// </summary>
			Inch = SensorMode.Mode1,
			
			/// <summary>
			/// Sensor is in listen mode
			/// </summary>
			Listen = SensorMode.Mode2
		#pragma warning restore
	};
	
	/// <summary>
	/// Class for the EV3 and NXT ultrasonic sensor
	/// </summary>
	public class UltrasonicSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.UltrasonicSensor"/> class.
		/// </summary>
		public UltrasonicSensor () : base((SensorMode)UltrasonicMode.Centimeter)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.UltrasonicSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public UltrasonicSensor (UltrasonicMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Gets or sets the ultrasonic mode.
		/// </summary>
		/// <value>The mode.</value>
		public UltrasonicMode Mode {
			get{return (UltrasonicMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)UltrasonicMode.Centimeter:
			        s = Read().ToString() + " cm";
			        break;
			   	case (SensorMode)UltrasonicMode.Inch:
			        s = Read().ToString() +  " inch";
			        break;
			    case (SensorMode)UltrasonicMode.Listen:
			        s = Read().ToString();
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the sensor value. Result depends on the mode
		/// </summary>
		public float Read()
		{
			return ReadSi();			
		}
	}
	
	/// <summary>
	/// Sensor modes when using a Temperature sensor
	/// </summary>
	public enum TemperatureMode { 
		#pragma warning disable 

			/// <summary>
			/// Result will be in celcius
			/// </summary>
			Celcius = SensorMode.Mode0,

			
			/// <summary>
			/// Result will be in fahrenheit 
			/// </summary>
			Fahrenheit = SensorMode.Mode1,
		#pragma warning restore
	};
	
	/// <summary>
	/// Class for the EV3 temperature sensor
	/// </summary>
	public class TemperatureSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.TemperatureSensor"/> class.
		/// </summary>
		public TemperatureSensor () : base((SensorMode)TemperatureMode.Celcius)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.TemperatureSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public TemperatureSensor (TemperatureMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Gets or sets the temperature mode.
		/// </summary>
		/// <value>The mode.</value>
		public TemperatureMode Mode {
			get{return (TemperatureMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)TemperatureMode.Celcius:
			        s = ReadTemperature().ToString() + " C";
			        break;
			   	case (SensorMode)TemperatureMode.Fahrenheit:
			        s = ReadTemperature().ToString() +  " F";
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the temperature.
		/// </summary>
		/// <returns>The temperature.</returns>
		public float ReadTemperature()
		{
			return ReadSi();			
		}
	}
	
	/// <summary>
	/// Sensor modes when using a Gyro sensor
	/// </summary>
	public enum GyroMode { 
		#pragma warning disable 
			/// <summary>
			/// Result will be in degrees
			/// </summary>
			Angle = SensorMode.Mode0,
			/// <summary>
			/// Result will be in degrees per second
			/// </summary>
			AngularVelocity = SensorMode.Mode1,
		#pragma warning restore
	};
	
	/// <summary>
	/// Class for the EV3 gyro sensor
	/// </summary>
	public class GyroSensor : Sensor{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.GyroSensor"/> class.
		/// </summary>
		public GyroSensor () : base((SensorMode)GyroMode.Angle)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.EV3.GyroSensor"/> class.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public GyroSensor (GyroMode mode) :  base((SensorMode)mode)
		{
		
		}
		
		/// <summary>
		/// Gets or sets the gyro mode.
		/// </summary>
		/// <value>The mode.</value>
		public GyroMode Mode {
			get{return (GyroMode) this.mode;}
			set{SetMode((SensorMode) value);}
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override string ReadAsString ()
		{
			string s = "";
			switch (mode)
			{
			    case (SensorMode)GyroMode.Angle:
			        s = Read().ToString() + " degree";
			        break;
			   	case (SensorMode)GyroMode.AngularVelocity:
			        s = Read().ToString() +  " deg/sec";
			        break;
			}
			return s;
		}
		
		/// <summary>
		/// Read the sensor value. The result will depend on the mode
		/// </summary>
		public float Read()
		{
			return ReadSi();			
		}
	}
	
	/// <summary>
	/// Class to help with sensor setup
	/// </summary>
	public static class SensorHelper{
	
		private const string Color = "Color";
		private const string ColorAmbient = "Color Ambient";
		private const string ColorReflection = "Color Reflection";
		private const string ColorReflectionRaw = "Color Reflection Raw";
		private const string Touch = "Touch";
		private const string TouchCount = "Touch count";
		private const string GyroAngle = "Gyro Angle";
		private const string GyroAngularVelocity = "Gyro Angular Velocity";
		private const string IrProximity = "IR Proximity";
		private const string IrRemoteMode = "IR Remote mode";
		private const string IrSeekMode = "IR Seek mode";
		private const string LightAmbient = "Light Ambient";
		private const string LightReflection = "Light Reflection";
		private const string SounddBa = "Sound dBA";
		private const string SounddB = "Sound dB";
		private const string TemperatureCelcius = "Temperature Celcius";
		private const string TemperatureFahrenheit = "Temperature Fahrenheit";
		private const string UltrasonicCentimeter = "Ultrasonic Centimeter";
		private const string UltrasonicInch = "Ultrasonic Inch";
		private const string UltrasonicListenMode = "Ultrasonic Listen mode";
		
		
		
		/// <summary>
		/// Gets a dictionary of sensors that has been implemented. Can be use in a combobox or simular
		/// </summary>
		/// <value>The sensor dictionary.</value>
		public static Dictionary<string,Sensor> SensorDictionary{
			get{
				Dictionary<string,Sensor> dictionary = new Dictionary<string, Sensor>();
				dictionary.Add(Color, new ColorSensor(ColorMode.Color));
				dictionary.Add(ColorAmbient, new ColorSensor(ColorMode.Ambient));
				dictionary.Add(ColorReflection, new ColorSensor(ColorMode.Reflection));
				dictionary.Add(ColorReflectionRaw, new ColorSensor(ColorMode.Raw));
				dictionary.Add(Touch, new TouchSensor(TouchMode.Boolean));
				dictionary.Add(TouchCount, new TouchSensor(TouchMode.Count));
				dictionary.Add(GyroAngle, new GyroSensor(GyroMode.Angle));
				dictionary.Add(GyroAngularVelocity, new GyroSensor(GyroMode.AngularVelocity));
				dictionary.Add(IrProximity, new IRSensor(IRMode.Proximity));
				dictionary.Add(IrRemoteMode, new IRSensor(IRMode.Remote));
				dictionary.Add(IrSeekMode, new IRSensor(IRMode.Seek));
				dictionary.Add(LightAmbient, new LightSensor(LightMode.Ambient));
				dictionary.Add(LightReflection, new LightSensor(LightMode.Relection));
				dictionary.Add(SounddBa, new SoundSensor(SoundMode.SoundDBA));
				dictionary.Add(SounddB, new SoundSensor(SoundMode.SoundDB));
				dictionary.Add(TemperatureCelcius, new TemperatureSensor(TemperatureMode.Celcius));
				dictionary.Add(TemperatureFahrenheit, new TemperatureSensor(TemperatureMode.Fahrenheit));
				dictionary.Add(UltrasonicCentimeter, new UltrasonicSensor(UltrasonicMode.Centimeter));
				dictionary.Add(UltrasonicInch, new UltrasonicSensor(UltrasonicMode.Inch));
				dictionary.Add(UltrasonicListenMode, new UltrasonicSensor(UltrasonicMode.Listen));
				return dictionary;
			}
		}
		
		
		
		/// <summary>
		/// Convert a sensor type a to dictionary key.
		/// </summary>
		/// <returns>A dictionary key that can be used with the sensor dictionary. If no match is found an empty string is returned</returns>
		/// <param name="type">Sensor type to convert.</param>
		public static string TypeToKey (SensorType type)
		{
			string s = "";
			switch(type){
				case SensorType.Gyro:
					s = GyroAngle;
					break;
				case SensorType.I2C:
					s = Touch;
					break;
				case SensorType.IR:
					s = IrProximity;
					break;
				case SensorType.Color:
					s = Color;
					break;
				case SensorType.LMotor:
					break;
				case SensorType.MMotor:
					break;
				case SensorType.None:
					break;
				case SensorType.NXTColor:
					s = Color;
					break;
				case SensorType.NXTLight:
					s = LightReflection;
					break;
				case SensorType.NXTSound:
					s = SounddB;
					break;
				case SensorType.NXTTemperature:
					s = TemperatureCelcius;
					break;
				case SensorType.NXTTest:
					break;
				case SensorType.NXTTouch:
					s = Touch;
					break;
				case SensorType.NXTUltraSonic:
					s = UltrasonicCentimeter;
					break;
				case SensorType.Test:
					break;
				case SensorType.Touch:
					s = Touch;
					break;
				case SensorType.UltraSonic:
					s = UltrasonicCentimeter;
					break;
			}
			return s;
			
			
		}
	
	}
	
	
	
	
	
}

