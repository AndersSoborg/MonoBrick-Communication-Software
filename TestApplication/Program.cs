using System;  
using MonoBrick.EV3;//use this to run the example on the EV3
//using MonoBrick.NXT;//use this to run the example on the NXT  
namespace Application  
{  
    public static class Program{  
      static void Main(string[] args)  
      {  
        try{  
            var brick = new Brick<Sensor,Sensor,Sensor,Sensor>("usb");  
            sbyte speed = 0;  
            brick.Connection.Open();  
            ConsoleKeyInfo cki;  
            Console.WriteLine("Press Q to quit");  
            do   
            {  
                cki = Console.ReadKey(true); //press a key  
                switch(cki.Key){    
                    case ConsoleKey.R:    
                        Console.WriteLine("Motor A reverse direction");    
                        brick.MotorA.Reverse = !brick.MotorA.Reverse;    
                    break;                              
                    case ConsoleKey.UpArrow:     
                        if(speed < 100)    
                            speed = (sbyte)(speed + 10);    
                        Console.WriteLine("Motor A speed set to " + speed);    
                        brick.MotorA.On(speed);    
                    break;    
                    case ConsoleKey.DownArrow:     
                        if(speed > -100)    
                            speed = (sbyte)(speed - 10);    
                        Console.WriteLine("Motor A speed set to " + speed);    
                        brick.MotorA.On(speed);    
                    break;    
                    case ConsoleKey.S:     
                        Console.WriteLine("Motor A off");    
                        speed = 0;    
                        brick.MotorA.Off();    
                    break;    
                    case ConsoleKey.B:    
                        Console.WriteLine("Motor A break");    
                        speed = 0;    
                        brick.MotorA.Brake();    
                    break;  
                     case ConsoleKey.T:    
                        int count = brick.MotorA.GetTachoCount();  
                        Console.WriteLine("Motor A tacho count:" +count);    
                    break;  
                    case ConsoleKey.C:    
                        Console.WriteLine("Clear tacho count");    
                        brick.MotorA.ResetTacho();  
                    break;  
                    case ConsoleKey.M:  
                        Console.WriteLine("Enter position to move to.");  
                        string input = Console.ReadLine();  
                        Int32 position;  
                        if(Int32.TryParse(input, out position)){  
                            Console.WriteLine("Move to " + position);  
                            brick.MotorA.MoveTo(50, position, false);  
                        }  
                        else{  
                            Console.WriteLine("Enter a valid number");  
                        }  
                    break;  
                }  
            } while (cki.Key != ConsoleKey.Q);  
        }  
        catch(Exception e){  
            Console.WriteLine("Error: " + e.Message);  
            Console.WriteLine("Press any key to end...");  
            Console.ReadKey();                
        }  
      }  
    }  
} 