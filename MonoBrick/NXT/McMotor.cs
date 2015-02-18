using System;
namespace MonoBrick.NXT
{
 	

	/// <summary>
	/// McMotor class
	/// </summary>
	public class McMotor : Motor
	{
		private enum MotorControlMode{
			HoldBrake = 0x01, SpeedRegulation = 0x02, SmoothStart = 0x04
		}

		private MotorControlMotorPort mcPort = MotorControlMotorPort.PortA;
		private MotorControlProxy mcProxy = null;

        internal MotorControlMotorPort MCPort{
			get{ return mcPort;}
			set{ mcPort = value;}
		}
		internal MotorControlProxy MCProxy{
			get{ return mcProxy;}
			set{ mcProxy = value;}
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Motor"/> class.
		/// </summary>
		public McMotor() {}
        
		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		new public void On(sbyte speed, UInt32 degrees){
			On (speed,degrees,false, false);
		}

		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		new public void On(sbyte speed, UInt32 degrees, bool reply){
			On (speed,degrees,false, reply);
		}

		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		/// <param name='hold'>
		/// If set to <c>true</c> the motor will actively 
		/// hold the position after finishing.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void On (sbyte speed, UInt32 degrees, bool hold, bool reply)
		{
			if (degrees == 0) {
				base.On (speed, degrees, reply);
			}
                        
			if (speed > 100)
				speed = 100;
			if (speed < -100)
				speed = -100;

			int speedTemp = speed;
			if (speedTemp < 0)
				speedTemp = 100 - speedTemp;

			string powerMotorControl = speedTemp.ToString ().PadLeft (3, '0');

			// Translate tachoLimit to MotorControl-format.
			long tachoLimit = System.Math.Abs(degrees);

			string tachoLimitMotorControl = tachoLimit.ToString ().PadLeft (6, '0');

			// Call MotorControl.
			char mode = '6';
			if (hold) {
				mode = '7';
			}
			mcProxy.SendControlledMotorCommand(mcPort, powerMotorControl, tachoLimitMotorControl, mode);

		}

		/// <summary>
		/// Moves the motor to an absolute position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor 0 to 100
		/// </param>
		/// <param name='position'>
		/// Absolute position
		/// </param>
		/// <param name='hold'>
		/// If set to <c>true</c> the motor will actively 
		/// hold the position after finishing.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void MoveTo(byte speed, Int32 position, bool hold, bool reply){
			Int32 move = position - GetTachoCount();
			if (speed > 100) speed = 100;
            //if (speed < -100) speed = -100;

            int speedTemp = (move > 0 ? speed : -speed);
            if (speedTemp < 0) speedTemp = 100 - speedTemp;

            string powerMotorControl = speedTemp.ToString().PadLeft(3, '0');

            // Translate position to MotorControl-format.
            string tachoLimitMotorControl = System.Math.Abs(move).ToString().PadLeft(6, '0');

            // Call MotorControl.
			char mode = '6';
			if (hold) {
				mode = '7';
			}
			mcProxy.SendControlledMotorCommand(mcPort, powerMotorControl, tachoLimitMotorControl, mode);
		}

		/// <summary>
		/// Moves the motor to an absolute position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='position'>
		/// Absolute position
		/// </param>
		new public void MoveTo(byte speed, Int32 position){
			MoveTo(speed,position, false, false);
		}

				/// <summary>
		/// Moves the motor to an absolute position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='position'>
		/// Absolute position
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		new public void MoveTo(byte speed, Int32 position, bool reply){
			MoveTo(speed,position, false, reply);
		}

		/// <summary>
		/// Determines whether this motor is running.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this motor is running; otherwise, <c>false</c>.
		/// </returns>
        new public bool IsRunning() {
			return !mcProxy.IsMotorReady(mcPort);
        }
	}

	/// <summary>
	/// Mc sync motor.
	/// </summary>
	public class McSyncMotor
	{
		private enum MotorControlMode{
			HoldBrake = 0x01, SpeedRegulation = 0x02, SmoothStart = 0x04
		}

		private MotorControlMotorPort mcPort = MotorControlMotorPort.PortsAB;
		private MotorControlProxy mcProxy = null;

        internal MotorControlMotorPort MCPort{
			get{ return mcPort;}
			set{ mcPort = value;}
		}
		internal MotorControlProxy MCProxy{
			get{ return mcProxy;}
			set{ mcProxy = value;}
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Motor"/> class.
		/// </summary>
		public McSyncMotor() {}
        
		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		public void On(sbyte speed, UInt32 degrees){
			On (speed,degrees,false, false);
		}

		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void On(sbyte speed, UInt32 degrees, bool reply){
			On (speed,degrees,false, reply);
		}

		/// <summary>
		/// Move the motor to a relative position
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='degrees'>
		/// The relative position of the motor
		/// </param>
		/// <param name='hold'>
		/// If set to <c>true</c> the motor will actively 
		/// hold the position after finishing.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void On (sbyte speed, UInt32 degrees, bool hold, bool reply)
		{
			if (degrees == 0) {
				//TODO: Throw exception
			}
                        
			if (speed > 100)
				speed = 100;
			if (speed < -100)
				speed = -100;

			int speedTemp = speed;
			if (speedTemp < 0)
				speedTemp = 100 - speedTemp;

			string powerMotorControl = speedTemp.ToString ().PadLeft (3, '0');

			// Translate tachoLimit to MotorControl-format.
			long tachoLimit = System.Math.Abs(degrees);

			string tachoLimitMotorControl = tachoLimit.ToString ().PadLeft (6, '0');

			// Call MotorControl.
			char mode = '6';
			if (hold) {
				mode = '7';
			}
			mcProxy.SendControlledMotorCommand(mcPort, powerMotorControl, tachoLimitMotorControl, mode);

		}

		/// <summary>
		/// Determines whether this motor is running.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this motor is running; otherwise, <c>false</c>.
		/// </returns>
        public bool IsRunning() {
			return !mcProxy.IsMotorReady(mcPort);
        }
	}
}

