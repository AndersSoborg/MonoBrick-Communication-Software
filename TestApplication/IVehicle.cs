using System;

namespace MonoBrick
{
	/// <summary>
	/// Interface for a vehicle
	/// </summary>
	public interface IVehicle
	{
		
		/// <summary>
		/// Gets or sets a value indicating whether the left motor is running in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if left motor is reverse; otherwise, <c>false</c>.
		/// </value>
		bool ReverseLeft{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the right motor is running in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if right motor is reverse; otherwise, <c>false</c>.
		/// </value>
		bool ReverseRight{
			get;
			set;
		}

		/// <summary>
		/// Run backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		void Backward(sbyte speed);

		/// <summary>
		/// Run backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void Backward(sbyte speed, bool reply);

		/// <summary>
		/// Run forward
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		void Forward(sbyte speed);

		/// <summary>
		/// Run forward
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void Forward(sbyte speed, bool reply);

		/// <summary>
		/// Spins the vehicle left.
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		void SpinLeft(sbyte speed);

		/// <summary>
		/// Spins the vehicle left.
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void SpinLeft(sbyte speed, bool reply);

		/// <summary>
		/// Spins the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		void SpinRight(sbyte speed);

		/// <summary>
		/// Spins the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void SpinRight(sbyte speed, bool reply);

		/// <summary>
		/// Stop moving the vehicle
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void Off(bool reply);

		/// <summary>
		/// Stop moving the vehicle
		/// </summary>
		void Off();

		/// <summary>
		/// Brake the vehicle (the motor is still on but it does not move)
		/// </summary>
		void Brake();

		/// <summary>
		/// Brake the vehicle (the motor is still on but it does not move)
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void Brake(bool reply);

		/// <summary>
		/// Turns the vehicle right
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent 
		/// </param>
		void TurnRightForward(sbyte speed, sbyte turnPercent);

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
		void TurnRightForward(sbyte speed, sbyte turnPercent, bool reply);

		/// <summary>
		/// Turns the vehicle right will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		void TurnRightReverse(sbyte speed, sbyte turnPercent);

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
		void TurnRightReverse(sbyte speed, sbyte turnPercent, bool reply);

		/// <summary>
		/// Turns the vehicle left
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		void TurnLeftForward(sbyte speed, sbyte turnPercent);

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
		void TurnLeftForward(sbyte speed, sbyte turnPercent, bool reply);

		/// <summary>
		/// Turns the vehicle left will moving backwards
		/// </summary>
		/// <param name='speed'>
		/// Speed of the vehicle -100 to 100
		/// </param>
		/// <param name='turnPercent'>
		/// Turn percent.
		/// </param>
		void TurnLeftReverse(sbyte speed, sbyte turnPercent);

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
		void TurnLeftReverse(sbyte speed, sbyte turnPercent, bool reply);
	}
}

