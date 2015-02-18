using System;
using System.Configuration;

namespace PCRemote
{
	public class RemoteSettings : ApplicationSettingsBase
	{
		#region Brickpage
		[UserScopedSetting()]
		[DefaultSettingValue("192.168.1.101")]
		public string IpAddress
		{
			get
			{
				return ((string)this["IpAddress"]);
			}
			set
			{
				this["IpAddress"] = (string)value;
				
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Comport
		{
			get
			{
				return ((string)this["Comport"]);
			}
			set
			{
				this["Comport"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("1500")]
		public string Port
		{
			get
			{
				return ((string)this["Port"]);
			}
			set
			{
				this["Port"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("usb")]
		public string ConnectionType
		{
			get
			{
				return ((string)this["ConnectionType"]);
			}
			set
			{
				this["ConnectionType"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool WaitForConnection
		{
			get
			{
				return ((bool)this["WaitForConnection"]);
			}
			set
			{
				this["WaitForConnection"] = (bool)value;
			}
		}
		#endregion

		#region SensorPage
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Sensor1
		{
			get
			{
				return ((string)this["Sensor1"]);
			}
			set
			{
				this["Sensor1"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Sensor2
		{
			get
			{
				return ((string)this["Sensor2"]);
			}
			set
			{
				this["Sensor2"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Sensor3
		{
			get
			{
				return ((string)this["Sensor3"]);
			}
			set
			{
				this["Sensor3"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Sensor4
		{
			get
			{
				return ((string)this["Sensor4"]);
			}
			set
			{
				this["Sensor4"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string PollOption
		{
			get
			{
				return ((string)this["PollOption"]);
			}
			set
			{
				this["PollOption"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool SaveLog
		{
			get
			{
				return ((bool)this["SaveLog"]);
			}
			set
			{
				this["SaveLog"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool ReadSensor1
		{
			get
			{
				return ((bool)this["ReadSensor1"]);
			}
			set
			{
				this["ReadSensor1"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool ReadSensor2
		{
			get
			{
				return ((bool)this["ReadSensor2"]);
			}
			set
			{
				this["ReadSensor2"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool ReadSensor3
		{
			get
			{
				return ((bool)this["ReadSensor3"]);
			}
			set
			{
				this["ReadSensor3"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool ReadSensor4
		{
			get
			{
				return ((bool)this["ReadSensor4"]);
			}
			set
			{
				this["ReadSensor4"] = (bool)value;
			}
		}
		#endregion

		#region MotorPage
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool MotorAStop
		{
			get
			{
				return ((bool)this["MotorAStop"]);
			}
			set
			{
				this["MotorAStop"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool MotorBStop
		{
			get
			{
				return ((bool)this["MotorBStop"]);
			}
			set
			{
				this["MotorBStop"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool MotorCStop
		{
			get
			{
				return ((bool)this["MotorCStop"]);
			}
			set
			{
				this["MotorCStop"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("50.0")]
		public float MotorASpeed
		{
			get
			{
				return ((float)this["MotorASpeed"]);
			}
			set
			{
				this["MotorASpeed"] = (float)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("50.0")]
		public float MotorBSpeed
		{
			get
			{
				return ((float)this["MotorBSpeed"]);
			}
			set
			{
				this["MotorBSpeed"] = (float)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("50.0")]
		public float MotorCSpeed
		{
			get
			{
				return ((float)this["MotorCSpeed"]);
			}
			set
			{
				this["MotorCSpeed"] = (float)value;
			}
		}
		#endregion

		#region MailboxPage
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string OutboxMessage1
		{
			get
			{
				return ((string)this["OutboxMessage1"]);
			}
			set
			{
				this["OutboxMessage1"] = (string)value;
			}
		}
		
	    [UserScopedSetting()]
		[DefaultSettingValue("Mailbox 0")]
		public string OutMailbox1
		{
			get
			{
				return ((string)this["OutMailbox1"]);
			}
			set
			{
				this["OutMailbox1"] = (string)value;
			}
		}

		[UserScopedSetting()]
		[DefaultSettingValue("String")]
		public string OutFormat1
		{
			get
			{
				return ((string)this["OutFormat1"]);
			}
			set
			{
				this["OutFormat1"] = (string)value;
			}
		}

		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string OutboxMessage2
		{
			get
			{
				return ((string)this["OutboxMessage2"]);
			}
			set
			{
				this["OutboxMessage2"] = (string)value;
			}
		}
		
		
		[UserScopedSetting()]
		[DefaultSettingValue("Mailbox 0")]
		public string OutMailbox2
		{
			get
			{
				return ((string)this["OutMailbox2"]);
			}
			set
			{
				this["OutMailbox2"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("String")]
		public string OutFormat2
		{
			get
			{
				return ((string)this["OutFormat2"]);
			}
			set
			{
				this["OutFormat2"] = (string)value;
			}
		}

		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string OutboxMessage3
		{
			get
			{
				return ((string)this["OutboxMessage3"]);
			}
			set
			{
				this["OutboxMessage3"] = (string)value;
			}
		}
		
		
		[UserScopedSetting()]
		[DefaultSettingValue("Mailbox 0")]
		public string OutMailbox3
		{
			get
			{
				return ((string)this["OutMailbox3"]);
			}
			set
			{
				this["OutMailbox3"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("String")]
		public string OutFormat3
		{
			get
			{
				return ((string)this["OutFormat3"]);
			}
			set
			{
				this["OutFormat3"] = (string)value;
			}
		}

		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string OutboxMessage4
		{
			get
			{
				return ((string)this["OutboxMessage4"]);
			}
			set
			{
				this["OutboxMessage4"] = (string)value;
			}
		}
		
		
		[UserScopedSetting()]
		[DefaultSettingValue("Mailbox 0")]
		public string OutMailbox4
		{
			get
			{
				return ((string)this["OutMailbox4"]);
			}
			set
			{
				this["OutMailbox4"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("String")]
		public string OutFormat4
		{
			get
			{
				return ((string)this["OutFormat4"]);
			}
			set
			{
				this["OutFormat4"] = (string)value;
			}
		}

		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string OutboxMessage5
		{
			get
			{
				return ((string)this["OutboxMessage5"]);
			}
			set
			{
				this["OutboxMessage5"] = (string)value;
			}
		}
		
		
		[UserScopedSetting()]
		[DefaultSettingValue("Mailbox 0")]
		public string OutMailbox5
		{
			get
			{
				return ((string)this["OutMailbox5"]);
			}
			set
			{
				this["OutMailbox5"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("String")]
		public string OutFormat5
		{
			get
			{
				return ((string)this["OutFormat5"]);
			}
			set
			{
				this["OutFormat5"] = (string)value;
			}
		}
		#endregion
		#region keyboardPage
		[UserScopedSetting()]
		[DefaultSettingValue("Out A")]
		public string LeftMotor
		{
			get
			{
				return ((string)this["LeftMotor"]);
			}
			set
			{
				this["LeftMotor"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("Out C")]
		public string RightMotor
		{
			get
			{
				return ((string)this["RightMotor"]);
			}
			set
			{
				this["RightMotor"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool LeftMotorReverse
		{
			get
			{
				return ((bool)this["LeftMotorReverse"]);
			}
			set
			{
				this["LeftMotorReverse"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool RightMotorReverse
		{
			get
			{
				return ((bool)this["RightMotorReverse"]);
			}
			set
			{
				this["RightMotorReverse"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("30.0")]
		public float TurnRatio
		{
			get
			{
				return ((float)this["TurnRatio"]);
			}
			set
			{
				this["TurnRatio"] = (float)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("75.0")]
		public float VehicleSpeed
		{
			get
			{
				return ((float)this["VehicleSpeed"]);
			}
			set
			{
				this["VehicleSpeed"] = (float)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("50.0")]
		public float SpinSpeed
		{
			get
			{
				return ((float)this["SpinSpeed"]);
			}
			set
			{
				this["SpinSpeed"] = (float)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("Out B")]
		public string Motor3
		{
			get
			{
				return ((string)this["Motor3"]);
			}
			set
			{
				this["Motor3"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("false")]
		public bool Motor3Float
		{
			get
			{
				return ((bool)this["Motor3Float"]);
			}
			set
			{
				this["Motor3Float"] = (bool)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("65.0")]
		public float Motor3Speed
		{
			get
			{
				return ((float)this["Motor3Speed"]);
			}
			set
			{
				this["Motor3Speed"] = (float)value;
			}
		}
		[UserScopedSetting()]
		[DefaultSettingValue("119")]//w
		public int Forward
		{
			get
			{
				return ((int)this["Forward"]);
			}
			set
			{
				this["Forward"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("115")]//s
		public int Backward
		{
			get
			{
				return ((int)this["Backward"]);
			}
			set
			{
				this["Backward"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("97")]//a
		public int SpinLeft
		{
			get
			{
				return ((int)this["SpinLeft"]);
			}
			set
			{
				this["SpinLeft"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("100")]//d
		public int SpinRight
		{
			get
			{
				return ((int)this["SpinRight"]);
			}
			set
			{
				this["SpinRight"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("114")]//r
		public int IncVehicleSpeed
		{
			get
			{
				return ((int)this["IncVehicleSpeed"]);
			}
			set
			{
				this["IncVehicleSpeed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("101")]//e
		public int DecVehicleSpeed
		{
			get
			{
				return ((int)this["DecVehicleSpeed"]);
			}
			set
			{
				this["DecVehicleSpeed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("102")]//f
		public int IncSpinSpeed
		{
			get
			{
				return ((int)this["IncSpinSpeed"]);
			}
			set
			{
				this["IncSpinSpeed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("103")]//g
		public int DecSpinSpeed
		{
			get
			{
				return ((int)this["DecSpinSpeed"]);
			}
			set
			{
				this["DecSpinSpeed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("99")]//c
		public int IncTurnPercent
		{
			get
			{
				return ((int)this["IncTurnPercent"]);
			}
			set
			{
				this["IncTurnPercent"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("118")]//v
		public int DecTurnPercent
		{
			get
			{
				return ((int)this["DecTurnPercent"]);
			}
			set
			{
				this["DecTurnPercent"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("117")]//u
		public int Motor3Fwd
		{
			get
			{
				return ((int)this["Motor3Fwd"]);
			}
			set
			{
				this["Motor3Fwd"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("106")]//j
		public int Motor3Rev
		{
			get
			{
				return ((int)this["Motor3Rev"]);
			}
			set
			{
				this["Motor3Rev"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("108")]//l
		public int IncMotor3Speed
		{
			get
			{
				return ((int)this["IncMotor3Speed"]);
			}
			set
			{
				this["IncMotor3Speed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("107")]//k
		public int DecMotor3Speed
		{
			get
			{
				return ((int)this["DecMotor3Speed"]);
			}
			set
			{
				this["DecMotor3Speed"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("49")]//1
		public int ReadKeySensor1
		{
			get
			{
				return ((int)this["ReadKeySensor1"]);
			}
			set
			{
				this["ReadKeySensor1"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("50")]//2
		public int ReadKeySensor2
		{
			get
			{
				return ((int)this["ReadKeySensor2"]);
			}
			set
			{
				this["ReadKeySensor2"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("51")]//3
		public int ReadKeySensor3
		{
			get
			{
				return ((int)this["ReadKeySensor3"]);
			}
			set
			{
				this["ReadKeySensor3"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("52")]//4
		public int ReadKeySensor4
		{
			get
			{
				return ((int)this["ReadKeySensor4"]);
			}
			set
			{
				this["ReadKeySensor4"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("53")]//5
		public int SendMessage1
		{
			get
			{
				return ((int)this["SendMessage1"]);
			}
			set
			{
				this["SendMessage1"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("54")]//6
		public int SendMessage2
		{
			get
			{
				return ((int)this["SendMessage2"]);
			}
			set
			{
				this["SendMessage2"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("55")]//7
		public int SendMessage3
		{
			get
			{
				return ((int)this["SendMessage3"]);
			}
			set
			{
				this["SendMessage3"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("56")]//8
		public int SendMessage4
		{
			get
			{
				return ((int)this["SendMessage4"]);
			}
			set
			{
				this["SendMessage4"] = (int)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("57")]//9
		public int SendMessage5
		{
			get
			{
				return ((int)this["SendMessage5"]);
			}
			set
			{
				this["SendMessage5"] = (int)value;
			}
		}

		#endregion

		#region Android Page
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Path
		{
			get
			{
				return ((string)this["Path"]);
			}
			set
			{
				this["Path"] = (string)value;
			}
		}
		
		[UserScopedSetting()]
		[DefaultSettingValue("")]
		public string Caching
		{
			get
			{
				return ((string)this["Caching"]);
			}
			set
			{
				this["Caching"] = (string)value;
			}
		}
		#endregion
		}
}

