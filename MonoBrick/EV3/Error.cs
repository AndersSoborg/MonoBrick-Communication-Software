using System;

namespace MonoBrick.EV3
{
	/// <summary>
    /// Error coes from the EV3 brick
    /// </summary>
	public enum BrickError
	{
		#pragma warning disable 
		  UnknownHandle = 0x01, HandleNotReady = 0x02, CorruptFile = 0x03, NoHandlesAvailable = 0x04,
		  NoPermissions = 0x05, IllegalPath = 0x06, FileExits = 0x07, EndOfFile = 0x08,
		  SizeError = 0x09, UnknownError = 0x0a, WrongNumberOfBytes = 0x40, WrongSequenceNumber = 0x41
		#pragma warning restore
	};

	/// <summary>
	/// Exceptions from EV3 brick
	/// </summary>
	public class BrickException : MonoBrickException
	{
   		private static string errorToString(BrickError error){
			string errorDescription = "";
			switch(error){
                case BrickError.CorruptFile:
                    errorDescription = "File is corrupted";
                    break;
                case BrickError.EndOfFile:
				    errorDescription = "End of file";
					break;
			    case BrickError.FileExits:
				    errorDescription = "File already exists";
				    break;
				case BrickError.HandleNotReady:
					errorDescription = "File handle is not ready";
					break;
				case BrickError.IllegalPath:
					errorDescription = "Illegal file path";
					break;
				case BrickError.NoHandlesAvailable:
					errorDescription = "No more file handles";
					break;
				case BrickError.NoPermissions:
					errorDescription = "File permission denied";
					break;
				case BrickError.SizeError:
					errorDescription = "File size error";
					break;
				case BrickError.UnknownError:
					errorDescription = "Unknown error";
					break;
				case BrickError.UnknownHandle:
					errorDescription = "Unknown file handle";
					break;
				case BrickError.WrongNumberOfBytes:
					errorDescription = "Wrong number of bytes received";
					break;
				case BrickError.WrongSequenceNumber:
					errorDescription = "Reply has wrong sequence number";
					break;
				default:
	            	errorDescription = "Unknown brick error";
				    break;
			}
			return errorDescription;
		}

		/// <summary>
		/// Initializes a new instance of EV3 exception
		/// </summary>
		/// <param name='error'>
		/// Brick Error.
		/// </param>
		public BrickException(BrickError error): base(errorToString(error), (byte) error){}
        
		/// <summary>
		/// Initializes a new instance of EV3 exception
		/// </summary>
		/// <param name='error'>
		/// Brick error
		/// </param>
		/// <param name='inner'>
		/// Inner exception
		/// </param>
		public BrickException(BrickError error, Exception inner): base(errorToString(error), inner, (byte) error){}
		//public override ErrorType ErrorType{get{return ErrorType.Brick;}}
	}
	
	
	/// <summary>
	/// Class used to throw an EV3 exception from monoBrick
	/// </summary>
	class Error
	{
	   	
		public delegate void CleanUpMethod();

		/// <summary>
		/// Throws an monobrick related exception based on errorCode and errorType
		/// </summary>
		/// <param name='errorCode'>
		/// Error code
		/// </param>
		/// <param name='type'>
		/// Error type
		/// </param>
        protected static void ThrowException(byte errorCode, ErrorType type){
			if(type == ErrorType.Brick)
			  	throw new BrickException((BrickError)errorCode);
            if(type ==  ErrorType.Connection)
                throw new ConnectionException((ConnectionError)errorCode);
			if(type == ErrorType.Tunnel)
				throw new TunnelException((TunnelError)errorCode);
		}

		/// <summary>
		/// Throws a monobrick exception based on the Reply from the brick
		/// </summary>
		/// <param name='reply'>
		/// Reply to base the exception on
		/// </param>
		public static void ThrowException(Reply reply){
			ThrowException(reply.ErrorCode, reply.ErrorType);
		}

		/// <summary>
		/// Throws a monobrick exception based on the error code
		/// </summary>
		/// <param name='errorCode'>
		/// Error code to base the exception on
		/// </param>
		public static void ThrowException(byte errorCode){
			ThrowException(errorCode, ToErrorType(errorCode));
		}

		/// <summary>
		/// Convert errorCode to ErrorType
		/// </summary>
		/// <returns>
		/// The error type
		/// </returns>
		/// <param name='errorCode'>
		/// Error code
		/// </param>
		internal static ErrorType ToErrorType(byte errorCode){
			BrickError brickError = (BrickError) errorCode; 
			if(Enum.IsDefined(typeof(BrickError), brickError)){
				return ErrorType.Brick;
			}
			TunnelError serverError = (TunnelError) errorCode; 
			if(Enum.IsDefined(typeof(TunnelError), serverError)){
				return ErrorType.Tunnel;
			}
			ConnectionError connectionError = (ConnectionError) errorCode; 
			if(Enum.IsDefined(typeof(ConnectionError), connectionError)){
				return ErrorType.Connection;
			}
			
			if(errorCode != 0){
				return ErrorType.Brick;
			}
			return ErrorType.NoError;
		}

		/// <summary>
		/// Checks a reply for error. If reply has error relevant exception is thrown
		/// </summary>
		/// <param name='reply'>
		/// The reply to check
		/// </param>
		/// <param name="expectedSequenceNumber">Expected sequence number.</param>
		public static void CheckForError(Reply reply, UInt16 expectedSequenceNumber){
			CheckForError(reply,0,expectedSequenceNumber,null,true);
		}

		/// <summary>
		/// Checks a reply for error. If reply has error relevant exception is thrown
		/// </summary>
		/// <param name='reply'>
		/// The reply to check
		/// </param>
		/// <param name='expectedLength'>
		/// Expected reply length
		/// </param>
		/// <param name='expectedSequenceNumber'>
		/// Expected sequence number
		/// </param>
		public static void CheckForError(Reply reply, int expectedLength, UInt16 expectedSequenceNumber){
		   CheckForError(reply,expectedLength,expectedSequenceNumber,null, false);
		}

		/// <summary>
		/// Checks a reply for error. If reply has error relevant exception is thrown
		/// </summary>
		/// <param name='reply'>
		/// The reply to check
		/// </param>
		/// <param name='expectedLength'>
		/// Expected reply length
		/// </param>
		/// <param name='expectedSequenceNumber'>
		/// Expected sequence number
		/// </param>
		/// <param name='cleanUp'>
		/// Clean up method called before the exception is thrown
		/// </param>
		public static void CheckForError(Reply reply, int expectedLength, UInt16 expectedSequenceNumber, CleanUpMethod cleanUp){
			CheckForError (reply, expectedLength, expectedSequenceNumber, cleanUp, false);
		}

		private static void CheckForError(Reply reply, int expectedLength, UInt16 expectedSequenceNumber, CleanUpMethod cleanUp, bool ignoreLength){
			if(reply.HasError){
				if(cleanUp!= null){
					cleanUp();
				}
				ThrowException(reply);
			}
			if (!ignoreLength) {
				if (reply.Length != expectedLength) {
					if (cleanUp != null) {
							cleanUp ();
					}
					throw new BrickException (BrickError.WrongNumberOfBytes);
				}
			}
			if(reply.SequenceNumber != expectedSequenceNumber){
				if(cleanUp!= null){
					cleanUp();
				}
				throw new BrickException(BrickError.WrongSequenceNumber);
			}
		}
	}		
}

