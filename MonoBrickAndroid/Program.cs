using System;
using MonoBrick;
using MonoBrick.EV3;
using MonoLibUsb.Transfer;

namespace Application
{
	public static class Program{
		static void Main(string[] args)
        {
			
			var ev3 = new EV3Brick<ColorSensor,UltrasonicSensor,TouchSensor,IRSensor>("usb");
			ev3.Connection.Open();
			ev3.Sensor1.Mode = ColorMode.Color;
			ev3.Sensor2.Mode = UltrasonicMode.Centimeter;
			ev3.Sensor3.Mode = TouchMode.Count;
			ev3.Sensor4.Mode = IRMode.Seek;
			ConsoleKeyInfo cki;
			Console.WriteLine("Press Q to quit");
			sbyte speed = 0;
			do 
			{
				cki = Console.ReadKey(true); //press a key
				switch(cki.Key){
					case ConsoleKey.D1://1 was pressed
						Console.WriteLine("S1: " + ev3.Sensor1.ReadAsString());
					break;
					case ConsoleKey.D2://2 was pressed
						Console.WriteLine("S2: " + ev3.Sensor2.ReadAsString());
					break;
					case ConsoleKey.D3://3 was pressed
						Console.WriteLine("S3: " + ev3.Sensor2.ReadAsString());
					break;
					case ConsoleKey.D4://4 was pressed
						Console.WriteLine("S4: " + ev3.Sensor2.ReadAsString());
					break;
					case ConsoleKey.UpArrow:     
                        if(speed < 100)    
                            speed = (sbyte)(speed + 10);    
                        Console.WriteLine("Motor A speed set to " + speed);    
                        ev3.MotorA.On(speed);    
                    break;    
                    case ConsoleKey.DownArrow:     
                        if(speed > -100)    
                            speed = (sbyte)(speed - 10);    
                        Console.WriteLine("Motor A speed set to " + speed);    
                        ev3.MotorA.On(speed);    
                    break;    
                    case ConsoleKey.S:     
                        Console.WriteLine("Motor A off");    
                        speed = 0;    
                        ev3.MotorA.Off();    
                    break;    
                    case ConsoleKey.B:    
                        Console.WriteLine("Motor A break");    
                        speed = 0;    
                        ev3.MotorA.Brake();    
                    break;  
                     case ConsoleKey.T:    
                        int count = ev3.MotorA.GetTachoCount();  
                        Console.WriteLine("Motor A tacho count:" +count);    
                    break;  
                    case ConsoleKey.C:    
                        Console.WriteLine("Clear tacho count");    
                        ev3.MotorA.ResetTacho();  
                    break;  
                    case ConsoleKey.M:  
                        Console.WriteLine("Enter position to move to.");  
                        string input = Console.ReadLine();  
                        Int32 position;  
                        if(Int32.TryParse(input, out position)){  
                            Console.WriteLine("Move to " + position);  
                            ev3.MotorA.MoveTo(50, position);  
                        }  
                        else{  
                            Console.WriteLine("Enter a valid number");  
                        }  
                    break;
				}
			} while (cki.Key != ConsoleKey.Q);
        }
    
	

}
}