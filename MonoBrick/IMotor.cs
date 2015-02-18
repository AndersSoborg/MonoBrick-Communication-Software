using System;

namespace MonoBrick
{
	
	/// <summary>
	/// Motor class
	/// </summary>
	public interface IMotor
	{
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MonoBrick.NXT.Motor"/> run in reverse direction
		/// </summary>
		/// <value>
		/// <c>true</c> if reverse; otherwise, <c>false</c>.
		/// </value>
		bool Reverse {get; set;}

		/// <summary>
		/// Move the motor
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		void On(sbyte speed);

		/// <summary>
		/// Move the motor
		/// </summary>
		/// <param name='speed'>
		/// Speed of the motor -100 to 100
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> brick will send a reply
		/// </param>
		void On(sbyte speed, bool reply);

		/// <summary>
		/// Brake the motor (is still on but does not move)
		/// </summary>
		void Brake();

		/// <summary>
		/// Brake the motor (is still on but does not move)
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		void Brake(bool reply);
        
		/// <summary>
		/// Turn the motor off
		/// </summary>
		void Off();
        
		/// <summary>
		/// Turn the motor off
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
        void Off(bool reply);

		/// <summary>
		/// Resets the tacho
		/// </summary>
		void ResetTacho();

		/// <summary>
		/// Resets the tacho
		/// </summary>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
        void ResetTacho(bool reply);

		/// <summary>
		/// Gets the tacho count.
		/// </summary>
		/// <returns>
		/// The tacho count
		/// </returns>
        Int32 GetTachoCount();
        
        /// <summary>
		/// Determines whether this motor is running.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this motor is running; otherwise, <c>false</c>.
		/// </returns>
        bool IsRunning();
	}
	
	
	
}

