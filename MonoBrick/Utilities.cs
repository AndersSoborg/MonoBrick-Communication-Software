using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MonoBrick
{
		/// <summary>
		/// Brick type 
		/// </summary>
		public enum BrickType {
			/// <summary>
			/// NXT Brick.
			/// </summary>
			NXT = 1, 
			
			/// <summary>
			/// EV3 brick.
			/// </summary>
			EV3 = 2
		};
		
		/// <summary>
		/// Mono brick helper.
		/// </summary>
		public class MonoBrickHelper{
			
			/// <summary>
			/// Platform enumeration
			/// </summary>
			public enum Platform
			{
				/// <summary>
				/// Windows.
				/// </summary>
				Windows,
				
				/// <summary>
				/// Linux.
				/// </summary>
				Linux,
				
				/// <summary>
				/// Mac OS.
				/// </summary>
				Mac
			}
			/// <summary>
			/// Get the operating system
			/// </summary>
			/// <returns>The platform.</returns>
			public static Platform RunningPlatform()
			{
				switch (Environment.OSVersion.Platform)
				{
				case PlatformID.Unix:
					if (Directory.Exists("/Applications")
					    & Directory.Exists("/System")
					    & Directory.Exists("/Users")
					    & Directory.Exists("/Volumes"))
						return Platform.Mac;
					else
						return Platform.Linux;
					
				case PlatformID.MacOSX:
					return Platform.Mac;
					
				default:
					return Platform.Windows;
				}
			}
		}

		/// <summary>
		/// Network extensions
		/// </summary>
		public static class NetworkExtensions
    	{
	        /// <summary>
	        /// Wait for all bytes to be read
	        /// </summary>
	        /// <returns>
	        /// The number of bytes read
	        /// </returns>
	        /// <param name='stream'>
	        /// A network stream
	        /// </param>
	        /// <param name='buf'>
	        /// Buffer to store bytes that was read in
	        /// </param>
	        /// <param name='offset'>
	        /// Byte array offset 
	        /// </param>
	        /// <param name='length'>
	        /// Length of bytes to read
	        /// </param>
			public static int ReadAll(this NetworkStream stream, byte[] buf, int offset, int length)
	        {
	            int bytesRead = 0;
	            do
	            {
	                int br = stream.Read(buf, offset+bytesRead, length-bytesRead);
	                if (br <= 0)
						break;
	                bytesRead += br;
	            } while (bytesRead < length);
				return bytesRead;
	        }

			/// <summary>
			/// Wait for all bytes to be read
			/// </summary>
			/// <returns>
			/// The number of bytes read
			/// </returns>
			/// <param name='stream'>
			/// A network stream
			/// </param>
			/// <param name='buf'>
			/// Buffer to store bytes that was read in the number of bytes to read is determined by the length of the buffer
			/// </param>
			public static int ReadAll(this NetworkStream stream, byte[] buf)
	        {
	            return ReadAll(stream, buf, 0, buf.Length);
	        }
			
			/// <summary>
			/// Reads a line from a network stream
			/// </summary>
			/// <returns>
			/// The line that was read
			/// </returns>
			/// <param name='stream'>
			/// Stream to read from
			/// </param>
			public static string ReadLine(this NetworkStream stream){
				const int maxSize = 100;
				string s = "";
				bool run = true;	
				byte[] buf = new byte[maxSize];
				do
				{
					int br = stream.Read(buf, 0, maxSize);
					if (br <= 0){
						run = false;
						s = "";
					}
					byte size = 0;
					while(buf[size] != 0 && size < maxSize){
						size++;
					}
					s = s + System.Text.ASCIIEncoding.ASCII.GetString(buf, 0, size);
					if(size < maxSize)
						run = false;
				} while (run);
				return s;	
			}


    	}
}

