using System;
using System.IO;
using System.Diagnostics;
namespace Log
{
	public class SensorLog
	{
		StreamWriter stream = null;
		public SensorLog(){
			
		}
		public bool IsOpen{get
			{
				if(stream == null){
					return false;
				}
				return true;
			}
		}
		public void Open(string fileName)
		{
			if(stream!= null)
				Close();
			if(!File.Exists(fileName))
			   stream = new StreamWriter(fileName);
			else
			   stream = File.AppendText(fileName);
			Debug.WriteLine("Open sensor log stream");
			stream.AutoFlush = true;
			stream.WriteLine("--------------------------------------------------------------------------------------");
			stream.WriteLine("       Time          |    Sensor 1   |    Sensor 2   |    Sensor 3   |    Sensor 4   |");
			stream.WriteLine("--------------------------------------------------------------------------------------");
			
		}
		public void Write(string[] sensorValues){
			if(stream == null)
				Open("log.txt");
			stream.WriteLine(String.Format("{0,-20} | {1,-15} | {2,-15} | {3,-15} | {4,-15}",DateTime.Now ,
			                               sensorValues[0] , sensorValues[1], sensorValues[2], sensorValues[3] ));
		}
		public void Close(){
			Debug.WriteLine("Closing sensor log stream");
			stream.WriteLine("--------------------------------------------------------------------------------------");
			stream.Flush();
			stream.Close();
			stream = null;			
		}
	}
}



/*public static void function in util.cs

public static void writeToLogFile(string logMessage)
{
    string strLogMessage = string.Empty;
    string strLogFile = System.Configuration.ConfigurationManager.AppSettings["logFilePath"].ToString();
    StreamWriter swLog;
            
    strLogMessage = string.Format("{0}: {1}", DateTime.Now, logMessage);

    if (!File.Exists(strLogFile))
    {
        swLog = new StreamWriter(strLogFile);
    }
    else
    {
        swLog = File.AppendText(strLogFile);
    }

    swLog.WriteLine(strLogMessage);
    swLog.WriteLine();

    swLog.Close();

}*/
