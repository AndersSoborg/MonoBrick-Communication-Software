using System;
using MonoBrick;
namespace MonoBrick.NXT
{
	/// <summary>
	/// Motor ports
	/// </summary>
	public enum MotorPort  {
		#pragma warning disable
		OutA = 0, OutB = 1, OutC = 2
		#pragma warning restore
	};
	
	/// <summary>
	/// Motor mode
	/// </summary>
	[Flags]	public enum MotorMode {
		#pragma warning disable
		On = 0x01, Break = 0x02, Regulated = 0x04
		#pragma warning restore
	};
 	
	/// <summary>
	/// Motor regulation
	/// </summary>
	public enum MotorRegulation {
		#pragma warning disable
		Idle = 0x00, Speed = 0x01, Sync = 0x02
		#pragma warning restore
	};

	/// <summary>
	/// Motor run state
	/// </summary>
	public enum MotorRunState {
		#pragma warning disable
		Idle = 0x00, RampUp = 0x10, Running = 0x20, RampDown = 0x40
		#pragma warning restore
	};

    /// <summary>
    /// Output state of the motor
    /// </summary>
	public struct OutputState{
		/// <summary>
		/// Gets or sets the speed.
		/// </summary>
		/// <value>
		/// The speed.
		/// </value>
		public sbyte Speed{get;set;}

		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>
		/// The mode.
		/// </value>
		public MotorMode Mode{get;set;}

		/// <summary>
		/// Gets or sets the regulation.
		/// </summary>
		/// <value>
		/// The regulation.
		/// </value>
		public MotorRegulation Regulation{get;set;}

		/// <summary>
		/// Gets or sets the turn ratio.
		/// </summary>
		/// <value>
		/// The turn ratio.
		/// </value>
		public sbyte TurnRatio{get;set;}

		/// <summary>
		/// Gets or sets the state of the run.
		/// </summary>
		/// <value>
		/// The state of the run.
		/// </value>
		public MotorRunState RunState{get;set;}

		/// <summary>
		/// Gets or sets the tacho limit.
		/// </summary>
		/// <value>
		/// The tacho limit.
		/// </value>
		public UInt32 TachoLimit{get;set;}//Current limit on a movement in progres, if any

		/// <summary>
		/// Gets or sets the tacho count.
		/// </summary>
		/// <value>
		/// The tacho count.
		/// </value>
		public Int32 TachoCount{get;internal set;}//Internal count. Number of counts since last reset of motor

		/// <summary>
		/// Gets or sets the block tacho count.
		/// </summary>
		/// <value>
		/// The block tacho count.
		/// </value>
		public Int32 BlockTachoCount{get;internal set;} //Current position relative to last programmed movement
        
		/// <summary>
		/// Gets or sets the rotation count.
		/// </summary>
		/// <value>
		/// The rotation count.
		/// </value>
		public Int32 RotationCount{get;internal set;} //Current position relative to last reset of the rotation sensor for this
	}

	/// <summary>
	/// Motor class
	/// </summary>
	public class Motor : IMotor
	{
		private MotorPort port = MotorPort.OutA;
		private Connection<Command,Reply> connection = null;
		private bool reverse;
        internal MotorPort Port{
			get{ return port;}
			set{ port = value;}
		}
		internal Connection<Command,Reply> Connection{
			get{ return connection;}
			set{ connection = value;}
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Motor"/> class.
		/// </summary>
		public Motor() { Reverse = false; Sync = false;}
        
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MonoBrick.NXT.Motor"/> run in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if reverse; otherwise, <c>false</c>.
		/// </value>
		public bool Reverse {get{return reverse;} set{reverse = value;}}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MonoBrick.NXT.Motor"/> is synchronised with another motor.
		/// Two motors needs to have this to true in order to work
		/// </summary>
		/// <value>
		/// <c>true</c> if sync; otherwise, <c>false</c>.
		/// </value>
		public bool Sync{get;set;}

		/// <summary>
		/// Sets the output state of the motor
		/// </summary>
		/// <param name='state'>
		/// Outputstate
		/// </param>
		public void  SetOutputState(OutputState state){
			SetOutputState(state,false);
		}

		/// <summary>
		/// Sets the output state of the motor
		/// </summary>
		/// <param name='state'>
		/// Outputstate
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void  SetOutputState(OutputState state, bool reply){
			if(state.Speed > 100){
				 state.Speed = 100;
			}
			if(state.Speed < -100){
				state.Speed = -100;
			}
			if(state.TurnRatio > 100){
				state.TurnRatio = 100;
			}
			if(state.TurnRatio < -100){
				state.TurnRatio = 100;
			}
			if (Reverse)
            	state.Speed = (sbyte)-state.Speed;
            var command = new Command(CommandType.DirecCommand,CommandByte.SetOutputState, reply);
			command.Append((byte)port);
			command.Append(state.Speed);
			command.Append((byte)state.Mode);
			command.Append((byte)state.Regulation);
            command.Append(state.TurnRatio);
			command.Append((byte)state.RunState);
			command.Append(state.TachoLimit);
			command.Append((byte) 0x00);//why a 5th byte?
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}

		/// <summary>
		/// Move the motor
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		public void On(sbyte speed){
			On (speed,false);
		}

		/// <summary>
		/// Move the motor
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> brick will send a reply
		/// </param>
		public void On(sbyte speed, bool reply){
			On(speed, 0, reply);
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
		public void On(sbyte speed, UInt32 degrees){
			On (speed,degrees,false);
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
		public void On(sbyte speed, UInt32 degrees,bool reply){
            OutputState state = new OutputState();
			state.Speed = speed;
			state.Mode = MotorMode.Break | MotorMode.On | MotorMode.Regulated;
			state.Regulation =  MotorRegulation.Speed;
			state.TurnRatio = 100;
			state.RunState = MotorRunState.Running;
			state.TachoLimit = degrees;
			SetOutputState(state, reply);  
		}
		
		/*public void RampUp(sbyte speed, UInt32 degrees) {
        	RampUp(speed,degrees,false);
		}
		
        public void RampUp(sbyte speed, UInt32 degrees, bool reply) {
            SetOutputState(speed, degrees, MotorMode.Break | MotorMode.On | MotorMode.Regulated, MotorRegulation.Speed, MotorRunState.RampUp, 100, reply);
        }
		
		public void RampDown(sbyte speed, UInt32 degrees){
			RampDown(speed,degrees,false);
		}
		
        public void RampDown(sbyte speed, UInt32 degrees, bool reply)
        {
            SetOutputState(speed, degrees, MotorMode.Break | MotorMode.On | MotorMode.Regulated, MotorRegulation.Speed, MotorRunState.RampDown, 100, reply);
        }*/

		/// <summary>
		/// Brake the motor (is still on but does not move)
		/// </summary>
		public void Brake(){
			Brake(false);
		}

		/// <summary>
		/// Brake the motor (is still on but does not move)
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Brake(bool reply){
            OutputState state = new OutputState();
			state.Speed = 0;
			state.Mode = MotorMode.Break | MotorMode.On | MotorMode.Regulated;
			state.Regulation =  MotorRegulation.Speed;
			state.TurnRatio = 100;
			state.RunState = MotorRunState.Running;
			state.TachoLimit = 0;
			SetOutputState(state, reply); 
		}
        
		/// <summary>
		/// Turn the motor off
		/// </summary>
		public void Off(){
			Off (false);
		}
        
		/// <summary>
		/// Turn the motor off
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
        public void Off(bool reply)
        {
            OutputState state = new OutputState();
			state.Speed = 0;
			state.Mode = MotorMode.Break | MotorMode.Regulated;
			state.Regulation =  MotorRegulation.Speed;
			state.TurnRatio = 100;
			state.RunState = MotorRunState.Running;
			state.TachoLimit = 0;
			SetOutputState(state, reply);
        }

		/// <summary>
		/// Gets the output state of the motor
		/// </summary>
		/// <returns>
		/// The output state
		/// </returns>
        public OutputState GetOutputState() {
            OutputState motorOutput = new OutputState();
			var command = new Command(CommandType.DirecCommand, CommandByte.GetOutputState,true);
			command.Append((byte)port);
			connection.Send(command);
			var reply = connection.Receive();
			Error.CheckForError(reply,25);
			motorOutput.Speed = reply.GetSbyte(4);
            motorOutput.Mode = (MotorMode)reply[5];
            motorOutput.Regulation = (MotorRegulation)reply[6];
            motorOutput.TurnRatio = reply.GetSbyte(7);
            motorOutput.RunState = (MotorRunState)reply[8];
            motorOutput.TachoLimit = reply.GetUInt32(9);
            motorOutput.TachoCount = reply.GetInt32(13);
            motorOutput.BlockTachoCount = reply.GetInt32(17);
            motorOutput.RotationCount = reply.GetInt32(21);
            return motorOutput;
        }

		private void ResetMotorPosition(bool relative) {
			ResetMotorPosition(relative,false);
		}
        
		private void ResetMotorPosition(bool relative, bool reply) {
            var command = new Command(CommandType.DirecCommand, CommandByte.ResetMotorPosition, reply);
            command.Append((byte)Port);
            command.Append(relative);
            connection.Send(command);
            if (reply)
            {
                var brickReply = connection.Receive();
                Error.CheckForError(brickReply, 3);
            }
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
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void MoveTo(byte speed, Int32 position, bool reply){
			bool savedReverse = reverse;
			Int32 move = position-GetTachoCount();
			if(move == 0)
				return;
			if(move < 0){
				Reverse = true;
			}
			else{
				Reverse = false;
			}
			try{
				On((sbyte)speed, (uint) Math.Abs(move), reply);
			}
			catch(Exception e){
				Reverse = savedReverse;			
				throw(e);
			}
			Reverse = savedReverse;
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
		public void MoveTo(byte speed, Int32 position){
			MoveTo(speed,position, false);
		}

		/// <summary>
		/// Resets the tacho
		/// </summary>
		public void ResetTacho() {
			ResetTacho(false);
		}

		/// <summary>
		/// Resets the tacho
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
        public void ResetTacho(bool reply) {
            ResetMotorPosition(false, reply);
            ResetMotorPosition(true, reply);            
        }

		/// <summary>
		/// Gets the tacho count.
		/// </summary>
		/// <returns>
		/// The tacho count
		/// </returns>
        public Int32 GetTachoCount() {
            return GetOutputState().RotationCount;
        }

		/// <summary>
		/// Determines whether this motor is running.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this motor is running; otherwise, <c>false</c>.
		/// </returns>
        public bool IsRunning() {
            if (GetOutputState().RunState == MotorRunState.Idle)
                return false;
            return true;
        }
	}

	/// <summary>
	/// Vehicle class
	/// </summary>
	public class Vehicle: IVehicle{
		private Motor left = new Motor();
		private Motor right = new Motor();

		private Connection<Command,Reply> connection = null;
		internal Connection<Command,Reply> Connection{
			get{ return connection;}
			set{ 
				connection = value;
				left.Connection = value;
				right.Connection = value;			
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.Vehicle"/> class.
		/// </summary>
		/// <param name='left'>
		/// The left motor of the vehicle
		/// </param>
		/// <param name='right'>
		/// The right motor of the vehicle
		/// </param>
		public Vehicle(MotorPort left, MotorPort right){
			this.left.Port = left;
			this.right.Port = right;
			Sync = false;
		}

		/// <summary>
		/// Gets or sets the left motor
		/// </summary>
		/// <value>
		/// The left motor
		/// </value>
		public MotorPort LeftPort{
			get{
				return left.Port;
			}
			set{
				left.Port = value;
			}
		}

		/// <summary>
		/// Gets or sets the right motor
		/// </summary>
		/// <value>
		/// The right motor
		/// </value>
		public MotorPort RightPort{
			get{
				return right.Port;
			}
			set{
				right.Port = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the left motor is running in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if left motor is reverse; otherwise, <c>false</c>.
		/// </value>
		public bool ReverseLeft{
			get{
				return left.Reverse;
			}
			set{
				left.Reverse = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the right motor is running in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if right motor is reverse; otherwise, <c>false</c>.
		/// </value>
		public bool ReverseRight{
			get{
				return right.Reverse;
			}
			set{
				right.Reverse = value;
			}
		}

		private bool Sync{get;set;}

		/// <summary>
		/// Run backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		public void Backward(sbyte speed){
			Move((sbyte)-speed,0,false);
		}

		/// <summary>
		/// Run backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Backward(sbyte speed, bool reply){
			Move((sbyte)-speed,0,reply);
		}

		/*public void Backward(sbyte speed, UInt32 degrees){
			Move((sbyte)-speed,degrees,false);
		}

		public void Backward(sbyte speed, UInt32 degrees, bool reply){
			Move((sbyte)-speed,degrees, reply);
		}*/

		/// <summary>
		/// Run forward
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		public void Forward(sbyte speed){
			Move(speed,0,false);
		}

		/// <summary>
		/// Run forward
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Forward(sbyte speed, bool reply){
			Move(speed,0,reply);
		}

		/*public void Forward(sbyte speed, UInt32 degrees){
			Forward(speed,degrees,false);
		}
		public void Forward(sbyte speed, UInt32 degrees, bool reply){
			Move(speed, degrees, reply);
		}*/

		private void Move(sbyte speed, UInt32 degrees, bool reply){
			OutputState state = new OutputState();
			state.Speed = speed;
			state.Mode = MotorMode.Break | MotorMode.On | MotorMode.Regulated;
			if(Sync)
				state.Regulation =  MotorRegulation.Sync;
			else{
				state.Regulation =  MotorRegulation.Speed;
			}
			state.TurnRatio = 100;
			state.RunState = MotorRunState.Running;
			state.TachoLimit = degrees;
			left.SetOutputState(state,reply);
			right.SetOutputState(state,reply);
		}

		/// <summary>
		/// Spins the vehicle left.
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		public void SpinLeft(sbyte speed){
			SpinLeft(speed,false);
		}

		/// <summary>
		/// Spins the vehicle left.
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void SpinLeft(sbyte speed, bool reply){
			right.On(speed,reply);
			left.On((sbyte)-speed,reply);

		}

		/// <summary>
		/// Spins the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		public void SpinRight(sbyte speed){
			SpinRight(speed,false);

		}

		/// <summary>
		/// Spins the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void SpinRight(sbyte speed, bool reply){
			right.On((sbyte)-speed,reply);
			left.On(speed,reply);
		}

		/// <summary>
		/// Stop moving the vehicle
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Off(bool reply){
			left.Off(reply);
			right.Off(reply);
		}

		/// <summary>
		/// Stop moving the vehicle
		/// </summary>
		public void Off(){
			Off(false);
		}

		/// <summary>
		/// Brake the vehicle (the motor is still on but it does not move)
		/// </summary>
		public void Brake(){
			Brake(false);
		}

		/// <summary>
		/// Brake the vehicle (the motor is still on but it does not move)
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Brake(bool reply){
			OutputState state = new OutputState();
			state.Speed = 0;
			state.Mode = MotorMode.Break | MotorMode.On | MotorMode.Regulated;
			if(Sync)
				state.Regulation =  MotorRegulation.Sync;
			else{
				state.Regulation =  MotorRegulation.Speed;
			}
			state.TurnRatio = 100;
			state.RunState = MotorRunState.Running;
			state.TachoLimit = 0;
			left.SetOutputState(state,reply);
			right.SetOutputState(state,reply);
		}

		/// <summary>
		/// Turns the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent 
		/// </param>
		public void TurnRightForward(sbyte speed, sbyte turnPercent){
			TurnRightForward(speed, turnPercent, false);
		}

		/// <summary>
		/// Turns the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent 
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void TurnRightForward(sbyte speed, sbyte turnPercent, bool reply){
			left.On(speed,reply);
			right.On((sbyte)((double)speed * ((double)turnPercent/100.0)),reply);
		}

		/// <summary>
		/// Turns the vehicle right will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		public void TurnRightReverse(sbyte speed, sbyte turnPercent){
			TurnRightReverse(speed, turnPercent, false);
		}

		/// <summary>
		/// Turns the vehicle right will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void TurnRightReverse(sbyte speed, sbyte turnPercent, bool reply){
			left.On((sbyte)-speed,reply);
			right.On( (sbyte)((double)-speed * ((double)turnPercent/100.0)),reply);
		}

		/// <summary>
		/// Turns the vehicle left
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		public void TurnLeftForward(sbyte speed, sbyte turnPercent){
			TurnLeftForward(speed, turnPercent, false);
		}

		/// <summary>
		/// Turns the vehicle left
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void TurnLeftForward(sbyte speed, sbyte turnPercent, bool reply){
			right.On(speed,reply);
			left.On( (sbyte)((double)speed * ((double)turnPercent/100.0)),reply);
		}

		/// <summary>
		/// Turns the vehicle left will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		public void TurnLeftReverse(sbyte speed, sbyte turnPercent){
			TurnLeftReverse(speed, turnPercent, false);
		}

		/// <summary>
		/// Turns the vehicle left will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void TurnLeftReverse(sbyte speed, sbyte turnPercent, bool reply){
			right.On((sbyte)-speed,reply);
			left.On( (sbyte)((double)-speed * ((double)turnPercent/100.0)),reply);
		}

		/*public void ResetTacho() {
			motor1.ResetTacho();
			motor2.ResetTacho();
		}*/
        
		/*public void ResetTacho(bool reply) {
        	motor1.ResetTacho(reply);
			motor2.ResetTacho(reply);
        }*/
	}
}

