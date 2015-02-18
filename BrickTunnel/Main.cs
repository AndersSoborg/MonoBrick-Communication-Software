using System;
using System.Threading;
using MonoBrick;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using NetworkTunnel;

namespace NetworkTunnel
{
	#region Arguments
	public class Arguments{
        // Variables
        private StringDictionary Parameters;

        // Constructor
        public Arguments(string[] Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach(string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                Parts = Spliter.Split(Txt,3);

                switch(Parts.Length){
                // Found a value (for the last parameter 
                // found (space separator))
                case 1:
                    if(Parameter != null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                        {
                            Parts[0] = 
                                Remover.Replace(Parts[0], "$1");

                            Parameters.Add(Parameter, Parts[0]);
                        }
                        Parameter=null;
                    }
                    // else Error: no parameter waiting for a value (skipped)
                    break;

                // Found just a parameter
                case 2:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.
                    if(Parameter!=null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                            Parameters.Add(Parameter, "true");
                    }
                    Parameter=Parts[1];
                    break;

                // Parameter with enclosed value
                case 3:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.
                    if(Parameter != null)
                    {
                        if(!Parameters.ContainsKey(Parameter)) 
                            Parameters.Add(Parameter, "true");
                    }

                    Parameter = Parts[1];

                    // Remove possible enclosing characters (",')
                    if(!Parameters.ContainsKey(Parameter))
                    {
                        Parts[2] = Remover.Replace(Parts[2], "$1");
                        Parameters.Add(Parameter, Parts[2]);
                    }

                    Parameter=null;
                    break;
                }
            }
            // In case a parameter is still waiting
            if(Parameter != null)
            {
                if(!Parameters.ContainsKey(Parameter)) 
                    Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        public string this [string Param]
        {
            get
            {
                return(Parameters[Param]);
            }
        }
    }
	#endregion

	class MainClass
	{

		static void ScreenPrint(string s)
        {
        	Console.WriteLine(s);
        }
		public static void Init(){
          // This code automatically loads required dlls from embedded resources.
          AppDomain.CurrentDomain.AssemblyResolve += delegate(Object sender, ResolveEventArgs delArgs)
          {
              string assemblyName = new System.Reflection.AssemblyName(delArgs.Name).Name;
              String resourceName = assemblyName + ".dll";
              using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
              {
                  if (stream == null)
                      return null;
                  Byte[] assemblyData = new Byte[stream.Length];
                  stream.Read(assemblyData, 0, assemblyData.Length);
                  return System.Reflection.Assembly.Load(assemblyData);
              }
			};
		}

		public static void Main (string[] args)
		{
				Init();
				Main2(args);
		}

		private static Tunnel tunnel = null;

		public static void Main2 (string[] args)
		{
				string connectToClient = "";	

				ConsoleKeyInfo cki;
				
			    TunnelSettings inConfig = new TunnelSettings();
				try
    			{
					inConfig = inConfig.LoadFromXML("settings.xml");
				}
				catch(Exception){
					ScreenPrint("Failed to read settings. Using default settings");
				}
				Arguments CommandLine = new Arguments(args);
				
			    if(CommandLine["connect"] != null){ 
					connectToClient = CommandLine["connect"];
				}				
				tunnel = new Tunnel();
				tunnel.LogEvent += ScreenPrint;
				try
    			{
					if(tunnel.Start(inConfig)){
					    Console.WriteLine("Press Q to quit");						
						Console.WriteLine("Press C to connect to client");
						Console.WriteLine("Press T to throw off all clients");
						if(connectToClient != "")
							tunnel.ConnectToClient(connectToClient);
						do 
	      				{
					        cki = Console.ReadKey(true);
					        if(cki.Key == ConsoleKey.C){
								tunnel.LogEvent -= ScreenPrint;
								Console.Write("Enter client IP-Address: ");
								tunnel.ConnectToClient(Console.ReadLine());
								tunnel.LogEvent += ScreenPrint;
							}
							if(cki.Key == ConsoleKey.T){
								tunnel.ThrowOffAllClients();
							}
	       				} while (cki.Key != ConsoleKey.Q);
						tunnel.Stop();
					}
 				}
			    catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				finally
				{
					if(tunnel.IsRunning)
						tunnel.Stop();
				}
				TunnelSettings outConfig = inConfig;
				try{
					outConfig.SaveToXML("settings.xml");
				}
				catch{
				}
		}
	}
	
}

