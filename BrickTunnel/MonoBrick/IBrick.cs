using System;

namespace MonoBrick
{
	
	/// <summary>
	/// Brick interface
	/// </summary>
	public interface IConnection
	{
		void Open();
		void Close();
		event Action Connected;
		event Action Disconnected;
	}
	
	/// <summary>
	/// Brick interface
	/// </summary>
	public interface IBrick
	{
		ISensor[] Sensors{get;}
		IMotor[] Motors{get;}
		IVehicle Vehicle{ get; }
		string BrickName{get;}
		IConnection Connection{ get; }
	}
}

