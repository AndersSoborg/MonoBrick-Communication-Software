using System;
using MonoBrick;

namespace MonoBrick.NXT
{
	    /// <summary>
    /// <para>Used as argument for the MotorControl methods.</para>
    /// </summary>
    public enum MotorControlMotorPort
    {
        /// <summary>
        /// <para>Motor port A.</para>
        /// </summary>
        PortA = 0,

        /// <summary>
        /// <para>Motor port B.</para>
        /// </summary>
        PortB = 1,

        /// <summary>
        /// <para>Motor port C.</para>
        /// </summary>
        PortC = 2,

        /// <summary>
        /// <para>Motor ports A and B (synced).</para>
        /// </summary>
        PortsAB = 3,

        /// <summary>
        /// <para>Motor ports A and C (synced).</para>
        /// </summary>
        PortsAC = 4,

        /// <summary>
        /// <para>Motor ports B and C (synced).</para>
        /// </summary>
        PortsBC = 5,

        /// <summary>
        /// <para>Motor ports A, B and C (synced).</para>
        /// </summary>
        PortsABC = 6
    }

	/// <summary>
	/// Motor control proxy.
	/// </summary>
	public class MotorControlProxy
	{

		private Box pc_inbox = Box.Box0;
		private Box pc_outbox = Box.Box1;

		private Mailbox mailbox;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.MotorControlProxy"/> class.
		/// </summary>
		/// <param name='mailbox'>
		/// Mailbox.
		/// </param>
		public MotorControlProxy (Mailbox mailbox)
		{
			this.mailbox = mailbox;
		}

		        #region The MotorControl-commands

        private enum MotorControlCommandType
        {
            PROTO_CONTROLLED_MOTORCMD = 1,
            PROTO_RESET_ERROR_CORRECTION = 2,
            PROTO_ISMOTORREADY = 3,
            PROTO_CLASSIC_MOTORCMD = 4,
            PROTO_JUMBOPACKET = 5
        }

        /// <summary>
        /// <para>Sends a command to the NXT brick to run CONTROLLED_MOTORCMD - to run the motor(s) with the given parameters.</para>
        /// </summary>
        /// <remarks>
        /// <para>From the MotorControl documentation:</para>
        /// <para>This is the most important command. It contains a lot of parameters to tell the according motor(s) how and where to move. This is the actual reason MotorControl was developed. However, in certain situations, CLASSIC_MOTORCMD should be used instead (see below why). One of the reasons we have to decide in the high-level-client when to use CONTROLLED_MOTORCMD and when not is that in NXC, CPU-time is valuable, while on a computer we've got plenty!</para>
        /// </remarks>
        /// <param name="port">Motor port(s)</param>
        /// <param name="power">Power - 3 chars from "0" to "200". "0" to "100" = power 0 to 100; "101" to "200" = power -1 to -100.</param>
        /// <param name="tachoLimit">Tacho limit - 6 chars from "0" to "999999". "0" means driving forever, no limit (better not use it, see also CLASSIC_MOTORCMD); everything else: drive to specific position.</param>
        /// <param name="mode">Mode: 1 char, bitfield. Compose bits to integer (by OR or +), convert to char - Start with 0x00 (000); Set 0x01 (001) / add 1: Set for HoldBrake (keeps active brake on after end of movement); Set 0x02 (010) / add 2: Set to enable SpeedRegulation; Set 0x04 (100) / add 4: Set to enable SmoothStart.</param>
        /// <seealso cref="SendClassicMotorCommand"/>
        public void SendControlledMotorCommand( MotorControlMotorPort port, string power, string tachoLimit, char mode)
        {
			Console.WriteLine ("Power = " + power + ", tachoLimit = " + tachoLimit);
            string messageData = string.Format("{0}{1}{2}{3}{4}",
                (byte)MotorControlCommandType.PROTO_CONTROLLED_MOTORCMD,
                (byte)port,
                power,
                tachoLimit,
                mode
                );
			mailbox.Send(messageData, pc_outbox);
        }

        /// <summary>
        /// <para>Sends a command to the NXT brick to run RESET_ERROR_CORRECTION - to reset the NXT's internal error correction mechanism.</para>
        /// </summary>
        /// <remarks>
        /// <para>From the MotorControl documentation:</para>
        /// <para>This command can be used to reset the NXT's internal error correction mechanism (i.e. reset the TachoCount property). The same thing can be achieved using the IO map commands (with the output module).</para>
        /// </remarks>
        /// <param name="port">Motor port(s)</param>
        public void ResetErrorCorrection( MotorControlMotorPort port)
        {
            string messageData = string.Format("{0}{1}",
                (byte)MotorControlCommandType.PROTO_RESET_ERROR_CORRECTION,
                (byte)port
                );
			mailbox.Send(messageData, pc_outbox);
        }

        /// <summary>
        /// <para>Sends a command to the NXT block to run ISMOTORREADY - query if the motor is ready.</para>
        /// </summary>
        /// <remarks>
        /// <para>From the MotorControl documentation:</para>
        /// <para>This command is used to determine the state of a single motor: Is it currently executing a command (i.e. moving) or is it ready to accept new commands? Bi-directional communication is used. We send a request and keep asking for the reply (i.e. we poll the computer's inbox). We have to make sure that for each request we make, we also retrieve the answer (to leave MotorControl in a clean state). Please note that we can (in theory) make multiple requests concerning different motors at the same time, as long as we keep polling the incoming mailbox and collect all replies.</para>
        /// </remarks>
        /// <param name="port">Motor port</param>
        /// <returns>True (idle/ready); false (still busy).</returns>
        public bool IsMotorReady(MotorControlMotorPort port)
        {
            string messageData = string.Format("{0}{1}",
                (byte)MotorControlCommandType.PROTO_ISMOTORREADY,
                (byte)port
                );
			mailbox.Send(messageData, pc_outbox);

            while (true)
            {
                System.Threading.Thread.Sleep(10);

				string reply = mailbox.ReadString(pc_inbox, true);
                if (reply[0] != messageData[1]) continue;
                return (reply[1] == '1');
            }
        }

        /// <summary>
        /// <para>Sends a command to the NXT brick to run CLASSIC_MOTORCMD - to run the motor(s) with the given parameters.</para>
        /// </summary>
        /// <remarks>
        /// <para>From the MotorControl documentation:</para>
        /// <para>This command is very similar to the classic SetOutputState: Sometimes we don't need any fance position control algorith, we just want to drive the motors. So this command is used when we have a TachoLimit of 0. Actually we don't have to invoke our NXC MotorControl program at all if we just want to drive without a TachoLimit. We could use SetOutputState, but this is a bit dangerous: It could interfere with a command that's already being executed by MotorControl. For this and some other design reasons, we pass the parameters on to MotorControl just as we do for controlled operations. Another situation when this command is applicable: Stopping in COAST mode, i.e. when you set a TachoLimit, but you want the motor to coast to a very soft stop. Then the NXC program doesn't have to do anything for us. In this case, this command just does a ResetErrorCorrection and a SetOutput for us in NXC.</para>
        /// <para>Summary: When to use CLASSIC_MOTORCMD (instead of CONTROLLED_MOTORCMD)?</para>
        /// <list type="bullet">
        /// <item>When you've got a TachoLimit = 0.</item>
        /// <item>When you want your motor to coast (spin freely) after a TachoLimit has been reached (it will overshoot then).</item>
        /// <item>When you want to change the power of a currently running motor, you can use this command to overwrite the power level at runtime (only works if the operation wasn't started with CONTROLLED_MOTORCMD).</item>
        /// </list>
        /// </remarks>
        /// <param name="port">Motor port(s)</param>
        /// <param name="power">Power - 3 chars from "0" to "200". "0" to "100" = power 0 to 100; "101" to "200" = power -1 to -100.</param>
        /// <param name="tachoLimit">Tacho limit - 6 chars from "0" to "999999". "0" means driving forever (no limit); everything else: drive to specific position.</param>
        /// <param name="speedRegulation">Speed regulation. 1 char - "0" = disabled; "1" = enabled.</param>
        /// <seealso cref="SendControlledMotorCommand"/>
        public void SendClassicMotorCommand(MotorControlMotorPort port, string power, string tachoLimit, char speedRegulation)
        {
            string messageData = string.Format("{0}{1}{2}{3}{4}",
                (byte)MotorControlCommandType.PROTO_CLASSIC_MOTORCMD,
                (byte)port,
                power,
                tachoLimit,
                speedRegulation
                );
			mailbox.Send(messageData, pc_outbox);
        }

        #endregion
	}
}

