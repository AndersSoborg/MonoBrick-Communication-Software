using System;
using System.Threading;
using System.Text;
using MonoBrick;
using System.Reflection;

namespace MonoBrick.NXT
{

	/// <summary>
	/// Class for mindstorms brick
	/// </summary>
	public class McNXTBrick<TSensor1,TSensor2,TSensor3,TSensor4> : Brick<TSensor1,TSensor2,TSensor3,TSensor4> 
				where TSensor1 : Sensor, new()
		where TSensor2 : Sensor, new()
		where TSensor3 : Sensor, new()
		where TSensor4 : Sensor, new()
	{
		#region wrapper for motor
		private McMotor motorA = new McMotor();
		private McMotor motorB = new McMotor();
		private McMotor motorC = new McMotor();
		private McSyncMotor motorAB = new McSyncMotor();
		private McSyncMotor motorAC = new McSyncMotor();
		private McSyncMotor motorBC = new McSyncMotor();
		private McSyncMotor motorABC = new McSyncMotor();
		private MotorControlProxy mcProxy;
		private void Init ()
		{
			mcProxy = new MotorControlProxy(Mailbox);
			motorA.Connection = Connection;
			motorA.Port = MotorPort.OutA;
			motorA.MCProxy = mcProxy;
			motorA.MCPort = MotorControlMotorPort.PortA;
			motorB.Connection = Connection;
			motorB.Port = MotorPort.OutB;
			motorB.MCProxy = mcProxy;
			motorB.MCPort = MotorControlMotorPort.PortB;
			motorC.Connection = Connection;
			motorC.Port = MotorPort.OutC;
			motorC.MCProxy = mcProxy;
			motorC.MCPort = MotorControlMotorPort.PortC;

			//Synchronized Motors
			motorAB.MCProxy = mcProxy;
			motorAB.MCPort = MotorControlMotorPort.PortsAB;
			motorAC.MCProxy = mcProxy;
			motorAC.MCPort = MotorControlMotorPort.PortsAC;
			motorBC.MCProxy = mcProxy;
			motorBC.MCPort = MotorControlMotorPort.PortsBC;
			motorABC.MCProxy = mcProxy;
			motorABC.MCPort = MotorControlMotorPort.PortsABC;
		}

		/// <summary>
		/// Initialize the MotorControl program on the brick.
		/// McMotors will not work if this method was not called beforehand
		/// </summary>
		public void InitMC ()
		{
			if (!IsMotorControlOnNxt ()) {
				UploadMotorControlToNXT ();
			}
			if (!IsMotorControlRunningOnNxt ()) {
				StartMotorControl();
			}
		}

		/// <summary>
		/// Motor A (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to port A
		/// </value>
		public McMotor McMotorA{
			get{ return motorA;}
		}

		/// <summary>
		/// Motor B (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to port B
		/// </value>
		public McMotor McMotorB{
			get{ return motorB;}
		}

		/// <summary>
		/// Motor C (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to port C
		/// </value>
		public McMotor McMotorC{
			get{ return motorC;}
		}

		/// <summary>
		/// Motor AB (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to ports AB
		/// </value>
		public McSyncMotor McMotorAB {
			get{ return motorAB;}
		}

		/// <summary>
		/// Motor AC (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to ports AC
		/// </value>
		public McSyncMotor McMotorAC {
			get{ return motorAC;}
		}

		/// <summary>
		/// Motor BC (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to ports BC
		/// </value>
		public McSyncMotor McMotorBC {
			get{ return motorBC;}
		}

		/// <summary>
		/// Motor ABC (MotorControl)
		/// </summary>
		/// <value>
		/// The motor connected to ports ABC
		/// </value>
		public McSyncMotor McMotorABC {
			get{ return motorABC;}
		}

		/// <summary>
		/// Gets the motor control proxy.
		/// </summary>
		/// <value>
		/// The motor control proxy.
		/// </value>
		public MotorControlProxy MotorControlProxy {
			get{ return mcProxy;}
		}
	   	
		/// <summary>
		/// Initializes a new instance of the Brick class.
		/// </summary>
		/// <param name='connection'>
		/// Connection to use
		/// </param>
		public McNXTBrick(Connection<Command,Reply> connection) : base(connection) {
			Init();
		}
		
		/// <summary>
		/// Initializes a new instance of the Brick class with bluetooth or usb connection
		/// </summary>
		/// <param name='connection'>
		/// Can either be a serial port name for bluetooth connection or "usb" for usb connection
		/// </param>
		public McNXTBrick(string connection) : base(connection)	{
			Init();
		}

		/// <summary>
		/// Initializes a new instance of the Brick class with a network connection
		/// </summary>
		/// <param name='ipAddress'>
		/// The IP address to use
		/// </param>
		/// <param name='port'>
		/// The port number to use
		/// </param>
		public McNXTBrick(string ipAddress, ushort port) : base(ipAddress, port) {
			Init();
		}

		#endregion
		

		#region Helper utilities for MotorControl

        private static string MotorControlFile = "MotorControl22.rxe";

        /// <summary>
        /// <para>Queries if the MotorControl-program is on the NXT</para>
        /// </summary>
        /// <returns>True if the MotorControl-program is on the NXT, false if not</returns>
        public bool IsMotorControlOnNxt ()
		{
			IBrickFile[] files = FileSystem.FileList ();
			for (int i = 0; i < files.Length; i++) {
				if (files [i].Name == MotorControlFile) {
					return true;
				}
			}
            return false;
        }

		/// <summary>
        /// <para>Uploads the MotorControl program onto the NXT</para>
        /// </summary>
        public void UploadMotorControlToNXT ()
		{
			FileSystem.UploadFile(MotorControlFile, MotorControlFile);
        }

        /// <summary>
        /// <para>Queries if the MotorControl-program is currently running on the NXT.</para>
        /// </summary>
        /// <returns>True if the MotorControl-program is running, false if not</returns>
        public bool IsMotorControlRunningOnNxt ()
		{
			try {
				return (GetRunningProgram () == MotorControlFile);
			} catch (MonoBrick.NXT.BrickException) {
				return false;
			}
        }

        /// <summary>
        /// <para>Starts the MotorControl-program.</para>
        /// </summary>
        public void StartMotorControl()
        {
            StartProgram(MotorControlFile, true);
        }

        /// <summary>
        /// <para>Stops the MotorControl-program.</para>
        /// </summary>
        /// <remarks>
        /// <para>Stops any running program, even it the program is not the MotorControl-program.</para>
        /// </remarks>
        public void StopMotorControl()
        {
            StopProgram();
        }

        #endregion
	}
}
