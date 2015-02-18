using System;
using System.Collections.Generic;

namespace MonoBrick
{
	/// <summary>
	/// Tunnel error codes
	/// </summary>
	public enum TunnelError {
		#pragma warning disable 
		UnsupportedCommand = 0x21, ErrorExecuting = 0x22
		#pragma warning restore
	};

	/// <summary>
	/// Connection error codes
	/// </summary>
	public enum ConnectionError {
		#pragma warning disable 
		OpenError = 0x30, WriteError = 0x31, ReadError = 0x32, NoReply = 0x33
		#pragma warning restore
	};

    internal enum ErrorType 		{Connection = 1, Brick = 2, NoError = 3, Tunnel = 4}
	
	/// <summary>
	/// Base class for all exceptions
	/// </summary>
	public abstract class MonoBrickException : System.Exception
	{
		private byte errorCode;
		/// <summary>
		/// Gets the error code.
		/// </summary>
		/// <value>
		/// The error code
		/// </value>
		public byte ErrorCode
		{
			get{ return errorCode;}
		}
		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.MonoBrickException"/>
		/// </summary>
		/// <param name='description'>
		/// Description of error
		/// </param>
		/// <param name='code'>
		/// Error code
		/// </param>
		public MonoBrickException(string description, byte code):base(description){ errorCode = code;}

		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.MonoBrickException"/>
		/// </summary>
		/// <param name='description'>
		/// Description of error
		/// </param>
		/// <param name='e'>
		/// Child exception
		/// </param>
		/// <param name='code'>
		/// Error code
		/// </param>
		public MonoBrickException(string description, Exception e, byte code):base(description, e){errorCode = code;}
		//public abstract ErrorType ErrorType {get;}
	}

	/// <summary>
	/// Connection exception.
	/// </summary>
    public class ConnectionException : MonoBrickException
    {
        private static string errorToString(ConnectionError error)
        {
            string errorDescription = "";
            switch (error)
            {
                case ConnectionError.OpenError:
                    errorDescription = "Failed to open connection";
                    break;
                case ConnectionError.NoReply:
                    errorDescription = "Communication error - no reply from Brick";
                    break;
                case ConnectionError.ReadError:
                    errorDescription = "Error reading Brick reply";
                    break;
                case ConnectionError.WriteError:
                    errorDescription = "Error sending Brick command";
                    break;
                default:
                    errorDescription = "Unknown communication error";
                    break;
            }
            return errorDescription;
        }

		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.ConnectionException"/>
		/// </summary>
		/// <param name='error'>
		/// A connection error
		/// </param>
        public ConnectionException(ConnectionError error) : base(errorToString(error), (byte)error) { }
        
		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.ConnectionException"/>
		/// </summary>
		/// <param name='error'>
		/// A connection error
		/// </param>
		/// <param name='inner'>
		/// Inner exception
		/// </param>
		public ConnectionException(ConnectionError error, Exception inner) : base(errorToString(error), inner, (byte)error) { }
		//public override ErrorType ErrorType{get{return ErrorType.Connection;}}
    }

	/// <summary>
	/// Exceptions from tunnel
	/// </summary>
	public class TunnelException : MonoBrickException
	{
   		private static string errorToString(TunnelError error){
			string errorDescription = "";
			switch(error){
				case TunnelError.UnsupportedCommand:
					errorDescription = "Tunnel does not support the command";
					break;
				case TunnelError.ErrorExecuting:
					errorDescription = "Tunnel failed to execute command";
					break;
				default:
	            	errorDescription = "Unknown tunnel error";
				    break;
			}
			return errorDescription;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.TunnelException"/>
		/// </summary>
		/// <param name='error'>
		/// Tunnel error
		/// </param>
		public TunnelException(TunnelError error): base(errorToString(error), (byte) error){}

		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.TunnelException"/>
		/// </summary>
		/// <param name='error'>
		/// Tunnel error
		/// </param>
		/// <param name='inner'>
		/// Inner exception
		/// </param>
		public TunnelException(TunnelError error, Exception inner): base(errorToString(error), inner, (byte) error){}
		//public override ErrorType ErrorType{get{return ErrorType.Tunnel;}}
	}
	
	
}
