using System;
using System.Threading;
using MonoBrick;
using System.Diagnostics;
using System.Collections.Generic;
namespace MonoBrick.NXT
{
    #region Base Sensor
    /// <summary>
    /// Sensor ports
    /// </summary>
    public enum SensorPort  {
		#pragma warning disable 
		In1 = 0, In2 = 1, In3 = 2, In4 = 3 
		#pragma warning restore
	};
    
	/// <summary>
	/// Sensor types
	/// </summary>
	public enum SensorType  {
		#pragma warning disable 
		NoSensor = 0x00, Touch = 0x01, Temperature = 0x02, Reflection = 0x03, Angle = 0x04, LightActive = 0x05,
		LightInactive = 0x06, SoundDB = 0x07, SoundDBA  = 0x08, Custom = 0x09,LowSpeed = 0x0A, LowSpeed9V = 0x0B,
		HighSpeed = 0x0C, ColorFull = 0x0D, ColorRed = 0x0E, ColorGreen = 0x0F, ColorBlue = 0x10, ColorNone = 0x11,
		ColorExit = 0x12
		#pragma warning restore
	};
	/// <summary>
	/// Sensor modes
	/// </summary>
	public enum SensorMode {
		#pragma warning disable 
		Raw = 0x00, Bool = 0x20, Transition = 0x40, Period = 0x60, Percent = 0x80, 
		Celsius = 0xA0, Fahrenheit = 0xc0, Angle = 0xe0
		#pragma warning restore
	};
    
    internal class SensorReadings{
        public UInt16 Raw;//device dependent
        public UInt16 Normalized;//type dependent
        public Int16 Scaled;//mode dependent
        //public Int32 Calibrated;//not used
    }

	/// <summary>
	/// Analog Sensor class. Works as base class for all NXT sensors
	/// </summary>
    public class Sensor : ISensor
	{

		/// <summary>
		/// The sensor port to use
		/// </summary>
		protected SensorPort port;
		/// <summary>
		/// The connection to use for communication
		/// </summary>
		protected Connection<Command,Reply> connection = null;
		/// <summary>
		/// The sensor mode to use
		/// </summary>
		protected SensorMode Mode{get;private set;}
		/// <summary>
		/// The sensor type to use
		/// </summary>
		protected SensorType Type{get;private set;}
		/// <summary>
		/// True if sensor has been initialized
		/// </summary>
		protected bool hasInit;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Sensor"/> class with no sensor as type.
		/// </summary>
		public Sensor()
		{
			Mode = SensorMode.Raw;
			Type = SensorType.NoSensor;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Sensor"/> class.
		/// </summary>
		/// <param name='sensorType'>
		/// Sensor type
		/// </param>
		/// <param name='sensorMode'>
		/// Sensor mode
		/// </param>
		public Sensor(SensorType sensorType, SensorMode sensorMode)
        {
            Mode = sensorMode;
            Type = sensorType;
        }

		/// <summary>
		/// Resets the scaled value.
		/// </summary>
		protected void ResetScaledValue(){
			ResetScaledValue(false);
		}

		/// <summary>
		/// Resets the scaled value.
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> brick will send a reply
		/// </param>
  		protected void ResetScaledValue(bool reply){
            var command = new Command(CommandType.DirecCommand, CommandByte.ResetInputScaledValue, reply);
            command.Append((byte)port);
            connection.Send(command);
            if (reply) {
                var brickReply = connection.Receive();
                Error.CheckForError(brickReply, 3);
            }
        }
       	
		/// <summary>
		/// Updates the sensor type and mode.
		/// </summary>
		/// <param name='sensorType'>
		/// Sensor type.
		/// </param>
		/// <param name='sensorMode'>
		/// Sensor mode.
		/// </param>
		protected void UpdateTypeAndMode(SensorType sensorType, SensorMode sensorMode){
			Type = sensorType;
			Mode = sensorMode;
			var command = new Command(CommandType.DirecCommand, CommandByte.SetInputMode, true);
            command.Append((byte)port);
            command.Append((byte)Type);
            command.Append((byte)Mode);
            connection.Send(command);
            var reply = connection.Receive();
            Error.CheckForError(reply, 3);
        }

        internal SensorPort Port{
			get{ return port;}
			set{ port = value;}
		}

		internal Connection<Command,Reply> Connection{
			get{ return connection;}
			set{ connection = value;}
		}
    
        internal SensorReadings GetSensorReadings()
        {
            if (!hasInit)
            {
                Initialize();
            }
            SensorReadings sensorReadings = new SensorReadings();
			var command = new Command(CommandType.DirecCommand,CommandByte.GetInputValues, true);
			command.Append((byte) port);
			var reply = connection.SendAndReceive(command);
            Error.CheckForError(reply, 16);
            sensorReadings.Raw = reply.GetUInt16(8);
            sensorReadings.Normalized = reply.GetUInt16(10);
            sensorReadings.Scaled = reply.GetInt16(12);
			return sensorReadings;
        }

		/// <summary>
		/// Read mode dependent sensor value
		/// </summary>
		/// <returns>
		/// The scaled value 
		/// </returns>
  		protected Int16 GetScaledValue()
        {
            return GetSensorReadings().Scaled;
        }

		/// <summary>
		/// Read device dependent sensor value
		/// </summary>
		/// <returns>
		/// The raw value.
		/// </returns>
  		protected UInt16 GetRawValue()
        {
            return GetSensorReadings().Raw;
        }

		/// <summary>
		/// Read type dependent sensor value 
		/// </summary>
		/// <returns>
		/// The normalized value.
		/// </returns>
  		protected UInt16 GetNormalizedValue()
        {
            return GetSensorReadings().Normalized;
        }

        /*protected Int32 CalibratedValue(){
            return GetSensorValues().Scaled;
        }*/

		/// <summary>
		/// Initialize this sensor
		/// </summary>
		virtual public void Initialize()
        {
            //Console.WriteLine("Sensor " + Port + " init");
            if (connection != null && connection.IsConnected)
            {
                UpdateTypeAndMode(Type, Mode);
                Thread.Sleep(100);
                ResetScaledValue();
                Thread.Sleep(100);
                UpdateTypeAndMode(Type, Mode);
                hasInit = true;
            }
            else
            {
                hasInit = false;
            }            
        }

		/// <summary>
		/// Reset the sensor
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
  		virtual public void Reset(bool reply) {
            ResetScaledValue(reply);    
        }

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>
		/// The value as a string
		/// </returns>
        virtual public string ReadAsString()
        {
			return GetScaledValue().ToString();
        }

		/// <summary>
		/// Gets a value indicating whether the sensor has been initialized.
		/// </summary>
		/// <value>
		/// <c>true</c> if the sensor is initialized; otherwise, <c>false</c>.
		/// </value>
		public bool IsInitialized{get{return hasInit;}}

		/// <summary>
		/// Gets a dictionary of sensors that has been implemented. Can be use in a combobox or simular
		/// </summary>
		/// <value>The sensor dictionary.</value>
		public static Dictionary<string,Sensor> SensorDictionary{
			get{
				Dictionary<string,Sensor> dictionary = new Dictionary<string, Sensor>();
				dictionary.Add("None" , new NoSensor());
				dictionary.Add("HiTechnic Color",new HiTecColor());
				dictionary.Add("HiTechnic Compass", new HiTecCompass());
				dictionary.Add("HiTechnic Gyro", new HiTecGyro(0));
				dictionary.Add("HiTechnic Tilt", new HiTecTilt());
				dictionary.Add("NXT Color", new NXTColorSensor());
				dictionary.Add("NXT Color Reflection Pct", new NXTColorSensor(SensorMode.Percent));
				dictionary.Add("NXT Color Reflection Raw", new NXTColorSensor(SensorMode.Raw));
				dictionary.Add("NXT Color Inactive Pct", new NXTColorSensor(ColorMode.None,SensorMode.Percent));
				dictionary.Add("NXT Color Inactive Raw", new NXTColorSensor(ColorMode.None,SensorMode.Raw));
				dictionary.Add("NXT Light Active Pct", new NXTLightSensor(LightMode.On,SensorMode.Percent));
				dictionary.Add("NXT Light Active Raw", new NXTLightSensor(LightMode.On,SensorMode.Raw));
				dictionary.Add("NXT Light Inactive Pct", new NXTLightSensor(LightMode.Off,SensorMode.Percent));
				dictionary.Add("NXT Light Inactive Raw", new NXTLightSensor(LightMode.Off,SensorMode.Raw));
				dictionary.Add("NXT Sonar Inch", new Sonar(SonarMode.CentiInch));
				dictionary.Add("NXT Sonar Metric", new Sonar(SonarMode.Centimeter));
				dictionary.Add("NXT Sound dBA",new NXTSoundSensor(SoundMode.SoundDBA));
				dictionary.Add("NXT Sound dB", new NXTSoundSensor(SoundMode.SoundDB));
				dictionary.Add("NXT/RCX Touch Bool", new TouchSensor(SensorMode.Bool));
				dictionary.Add("NXT/RCX Touch Raw", new TouchSensor(SensorMode.Raw));
				dictionary.Add("RCX Angle", new RCXRotationSensor());
				dictionary.Add("RCX Light Pct", new RCXLightSensor(SensorMode.Percent));
				dictionary.Add("RCX Light Raw", new RCXLightSensor(SensorMode.Raw));
				dictionary.Add("RCX Temp Celcius", new RCXTemperatureSensor(TemperatureMode.Celsius));
				dictionary.Add("RCX Temp Fahrenheit", new RCXTemperatureSensor(TemperatureMode.Fahrenheit));
				return dictionary;
			}
		}
		
	}
    #endregion //Sensor

    #region No Sensor
	/// <summary>
	/// When a sensor is not connected use the class to minimize power consumption
	/// </summary>
    public class NoSensor: Sensor {
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NoSensor"/> class.
		/// </summary>
		public NoSensor(): base (SensorType.NoSensor, SensorMode.Raw) { 
            
        }
    }
    #endregion

	#region No Sensor
	/// <summary>
	/// When a sensor analog sensor is connected
	/// </summary>
	public class CustomSensor: Sensor {

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.CustomSensor"/> class  using custom sensor type and raw mode
		/// </summary>
		public CustomSensor(): base (SensorType.Custom, SensorMode.Percent) { 
			
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.CustomSensor"/> class.
		/// </summary>
		/// <param name='type'>
		/// Sensor type
		/// </param>
		/// <param name='mode'>
		/// Sensor mode
		/// </param>
		public CustomSensor(SensorType type, SensorMode mode): base (type, mode) { 
			
		}

		/// <summary>
		/// Read the sensor value
		/// </summary>
		public virtual int Read()
		{
			return GetScaledValue();
		}
	}
	#endregion

	#region NXT 2.0 Color sensor

	/// <summary>
	/// Sensor modes when using a NXT 2.0 color sensor
	/// </summary>
	public enum ColorMode { 
		#pragma warning disable 
		Full = SensorType.ColorFull, Red = SensorType.ColorRed, Green = SensorType.ColorGreen,
		Blue = SensorType.ColorBlue, None = SensorType.ColorNone
		#pragma warning restore
	};

	/// <summary>
	/// Colors that can be read from the  NXT 2.0 color sensor
	/// </summary>
	public enum Color { 
		#pragma warning disable 
		Black = 0x01, Blue = 0x02, Green = 0x03, 
		Yellow = 0x04, Red = 0x05, White = 0x06
		#pragma warning restore
	};

	/// <summary>
	/// NXT 2.0 color sensor.
	/// </summary>
	public class NXTColorSensor : Sensor {

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTColorSensor"/> class as a color sensor.
		/// </summary>
		public NXTColorSensor() : base ((SensorType)ColorMode.Full,SensorMode.Raw){
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTColorSensor"/> class as a light sensor.
		/// </summary>
		public NXTColorSensor(SensorMode mode) : base ((SensorType)ColorMode.Red,mode){
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTColorSensor"/> class.
		/// </summary>
		/// <param name="colorMode">The color mode to use</param>
		/// <param name="mode">The sensor mode to use</param>
		public NXTColorSensor(ColorMode colorMode, SensorMode mode) : base ((SensorType)colorMode,mode){
			
		}

		/// <summary>
		/// Set the sensor to be used as a color sensor
		/// </summary>
		public void UseAsColorSensor(){
			UpdateTypeAndMode(SensorType.ColorFull,SensorMode.Raw);
		}

		/// <summary>
		/// Set the sensor to be used as a light sensor
		/// </summary>
		public void UseAsLightSensor(SensorMode mode = SensorMode.Percent){
			UpdateTypeAndMode(SensorType.ColorRed,mode);
		}

		/// <summary>
		/// Gets or sets the light mode.
		/// </summary>
		/// <value>
		/// The light mode
		/// </value>
		public ColorMode ColorMode
		{
			set{
				UpdateTypeAndMode((SensorType)value, Mode);

			}
			get{
				return (ColorMode) Type;
			}
		}
		
		/// <summary>
		/// Gets or sets the sensor mode.
		/// </summary>
		/// <value>
		/// The sensor mode.
		/// </value>
		public SensorMode SensorMode
		{
			get{
				return Mode;
			}
			set{
				UpdateTypeAndMode(Type,value);
			}
		}
		
		/// <summary>
		/// Read the intensity of the reflected light
		/// </summary>
		public int ReadLightLevel()
		{
			return GetScaledValue();
		}

		/// <summary>
		/// Reads the color value
		/// </summary>
		/// <returns>The color read from the sensor</returns>
		public Color ReadColor(){
			Color color = Color.Black;
			try{
				color = (Color) (GetScaledValue());
			}
			catch(InvalidCastException){

			}
			return color;
		}

		/// <summary>
		/// Reads the sensor value as a string.
		/// </summary>
		/// <returns>The value as a string</returns>
		public override	string ReadAsString(){
			if(Type == SensorType.ColorFull && Mode == SensorMode.Raw){
				return ReadColor().ToString();
			}
			return ReadLightLevel().ToString();
		}


	}
	#endregion



    #region NXT Light Sensor
    /// <summary>
    /// Sensor modes when using a light sensor
    /// </summary>
    public enum LightMode { 
		#pragma warning disable 
		Off = SensorType.LightInactive, On = SensorType.LightActive
		#pragma warning restore
	};
    
	/// <summary>
	/// NXT light sensor.
	/// </summary>
	public class NXTLightSensor : Sensor {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTLightSensor"/> class with active light.
        /// </summary>
		public NXTLightSensor() : base ((SensorType)LightMode.On,SensorMode.Percent){
        
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTLightSensor"/> class.
		/// </summary>
		/// <param name='lightMode'>
		/// Light sensor mode
		/// </param>
		public NXTLightSensor(LightMode lightMode) : base ((SensorType)lightMode, SensorMode.Percent){
        
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTLightSensor"/> class.
		/// </summary>
		/// <param name='lightMode'>
		/// Light sensor mode
		/// </param>
		/// <param name='sensorMode'>
		/// Sensor mode. Raw, bool, percent...
		/// </param>
		public NXTLightSensor(LightMode lightMode, SensorMode sensorMode) : base ((SensorType)lightMode, sensorMode){
        
        }

		/// <summary>
		/// Gets or sets the light mode.
		/// </summary>
		/// <value>
		/// The light mode
		/// </value>
		public LightMode LightMode
		{
			set{
				UpdateTypeAndMode((SensorType)value, Mode);
			}

			get{
				return (LightMode) Type;
			}
		}

		/// <summary>
		/// Gets or sets the sensor mode.
		/// </summary>
		/// <value>
		/// The sensor mode.
		/// </value>
		public SensorMode SensorMode
		{
			get{
				return Mode;
			}
			set{
				UpdateTypeAndMode(Type,value);
			}
		}

		/// <summary>
		/// Read the intensity of the reflected light
		/// </summary>
		public int ReadLightLevel()
		{
			return GetScaledValue();
		}

    }
    #endregion

    #region RCX Light Sensor
    /// <summary>
    /// RCX light sensor.
    /// </summary>
    public class RCXLightSensor : Sensor {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrick.NXT.RCXLightSensor"/> class.
        /// </summary>
		public RCXLightSensor() : base(SensorType.Reflection, SensorMode.Percent) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.RCXLightSensor"/> class.
		/// </summary>
		/// <param name='sensorMode'>
		/// Sensor mode. Raw, bool, percent...
		/// </param>
		public RCXLightSensor(SensorMode sensorMode) : base(SensorType.Reflection, sensorMode) {}

		/// <summary>
		/// Gets or sets the sensor mode.
		/// </summary>
		/// <value>
		/// The sensor mode.
		/// </value>
		public SensorMode SensorMode
		{
			get{
				return Mode;
			}
			set{
				UpdateTypeAndMode(Type,value);
			}
		}

		/// <summary>
		/// Read the intensity of the reflected light
		/// </summary>
		public int ReadLightLevel()
		{
			return GetScaledValue();
		}


	}
    #endregion

    #region RCX Rotation Sensor
    /// <summary>
    /// RCX rotation sensor.
    /// </summary>
    public class RCXRotationSensor : Sensor {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrick.NXT.RCXRotationSensor"/> class.
        /// </summary>
		public RCXRotationSensor() : base(SensorType.Angle, SensorMode.Angle) { }

		/// <summary>
		/// Read the rotation count
		/// </summary>
		public int ReadCount()
		{
			return GetScaledValue();
		}

    }
    #endregion

    #region RCX Temperature Sensor
    /// <summary>
    /// Sensor mode when using a temperature sensor
    /// </summary>
    public enum TemperatureMode { 
		/// <summary>
		/// Result is in celsius
		/// </summary>
		Celsius = SensorMode.Celsius, 
		/// <summary>
		/// Result is in fahrenheit.
		/// </summary>
		Fahrenheit = SensorMode.Fahrenheit 
	};
    
	/// <summary>
	/// RCX temperature sensor.
	/// </summary>
	public class RCXTemperatureSensor : Sensor {
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.RCXTemperatureSensor"/> class as celsius
		/// </summary>
		public RCXTemperatureSensor() : base(SensorType.Temperature, (SensorMode)TemperatureMode.Celsius) { 
		
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.RCXTemperatureSensor"/> class.
		/// </summary>
		/// <param name='temperatureMode'>
		/// Temperature mode
		/// </param>
		public RCXTemperatureSensor(TemperatureMode temperatureMode) : base(SensorType.Temperature, (SensorMode)temperatureMode) { }

		/// <summary>
		/// Gets or sets the temperature mode.
		/// </summary>
		/// <value>
		/// The temperature mode.
		/// </value>
		public TemperatureMode TemperatureMode{
			get{return (TemperatureMode) Mode;}
			set
			{
				UpdateTypeAndMode(Type,(SensorMode)value);
			}
		}

		/// <summary>
		/// Read the temperature
		/// </summary>
		public int ReadTemperature()
		{
			return GetScaledValue();
		}



    }
    #endregion

    #region NXT Sound Sensor
    /// <summary>
	/// Sensor mode when using a sound sensor
    /// </summary>
    public enum SoundMode { 
		/// <summary>
		/// The sound level is measured in A-weighting decibel
		/// </summary>
		SoundDBA = SensorType.SoundDBA, 
		/// <summary>
		/// The sound level is measured in decibel 
		/// </summary>
		SoundDB = SensorType.SoundDB };
    
	/// <summary>
    /// NXT sound sensor.
    /// </summary>
	public class NXTSoundSensor : Sensor {
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTSoundSensor"/> class in DBA mode.
		/// </summary>
		public NXTSoundSensor() : base((SensorType)SoundMode.SoundDBA, SensorMode.Percent) { }


		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.NXTSoundSensor"/> class.
		/// </summary>
		/// <param name='soundMode'>
		/// Sound mode
		/// </param>
		public NXTSoundSensor(SoundMode soundMode) : base((SensorType)soundMode, SensorMode.Percent) { }

		/// <summary>
		/// Gets or sets the sound mode.
		/// </summary>
		/// <value>
		/// The sound mode.
		/// </value>
		public SoundMode SoundMode{
			get{return (SoundMode) Type;}
			set
			{
				UpdateTypeAndMode((SensorType)value,Mode);
			}
		}

		/// <summary>
		/// Read the sound level
		/// </summary>
		public int ReadSoundLevel()
		{
			return GetScaledValue();
		}
    }
    #endregion

    #region Touch Sensor
    
	/// <summary>
	/// Touch sensor.
	/// </summary>
    public class TouchSensor : CustomSensor {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrick.NXT.TouchSensor"/> class in bool mode
        /// </summary>
		public TouchSensor() : base(SensorType.Touch, SensorMode.Bool) { }
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.TouchSensor"/> class.
		/// </summary>
		/// <param name='sensorMode'>
		/// Sensor mode. Raw, bool, percent...
		/// </param>
		public TouchSensor(SensorMode sensorMode) : base(SensorType.Touch, sensorMode) { }

		/// <summary>
		/// Gets or sets the sensor mode.
		/// </summary>
		/// <value>
		/// The sensor mode.
		/// </value>
		public SensorMode SensorMode
		{
			get{
				return Mode;
			}
			set{
				UpdateTypeAndMode(Type,value);
			}
		}
	}
    #endregion
	
	#region HiTechnic gyro sensor
    /// <summary>
	/// HiTechnic gyro sensor
    /// </summary>
    public class HiTecGyro : Sensor {
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.HiTecGyro"/> class without offset
		/// </summary>
		public HiTecGyro() : base(SensorType.Custom , SensorMode.Raw) {
			Offset = 0; 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.HiTecGyro"/> class.
		/// </summary>
		/// <param name='offset'>
		/// Offset
		/// </param>
		public HiTecGyro(int offset) : base(SensorType.Custom , SensorMode.Raw) {
			Offset = offset; 
		}

		/// <summary>
		/// Read angular acceleration
		/// </summary>
		public int ReadAngularAcceleration()
        {
            return GetRawValue()-Offset;
        }

		/// <summary>
		/// Reads the angular acceleration as a string.
		/// </summary>
		/// <returns>
		/// The value as a string.
		/// </returns>
		public override string ReadAsString()
        {
            return this.ReadAngularAcceleration().ToString() + " deg/sec";
        }

		/// <summary>
		/// Gets or sets the offset.
		/// </summary>
		/// <value>
		/// The offset.
		/// </value>
		public int Offset{get;set;}

    }
    #endregion
	
}
