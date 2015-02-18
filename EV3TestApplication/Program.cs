using System;
using MonoBrick.EV3;
using System.Threading;
using System.Diagnostics;

public static class Program{
  static void Main(string[] args)
  {
	      var ev3 = new Brick<Sensor,Sensor,Sensor,Sensor>("usb");
	      ev3.Connection.Open();
	      Stopwatch swouter = new Stopwatch();
		Stopwatch swinner = new Stopwatch();
		int steptime = 100;
		swouter.Start();
				//ev3.MotorA.On(40);
				//ev3.MotorA.On(10);
				ev3.MotorA.SetPower(80);
				
				System.Threading.Thread.Sleep(5000);
				ev3.MotorA.SetPower(0);
		
		//ev3.MotorA.Off(); 
		      ev3.Connection.Close();
	}
}