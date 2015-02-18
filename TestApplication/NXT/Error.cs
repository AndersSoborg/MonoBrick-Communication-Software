using System;

namespace MonoBrick.NXT
{
	
	/// <summary>
    /// Error coes from the NXT brick
    /// </summary>
	public enum BrickError
	{

		#pragma warning disable 
		WrongNumberOfBytes = 0x78, UnknownErrorCode = 0x79, I2CTimeOut = 0x80, NoMoreHandles = 0x81, NoSpace = 0x82, NoMoreFiles = 0x83, 
		EndOfFileExpected = 0x84, EndOfFile = 0x85, NotALinearFile = 0x86, FileNotFound = 0x87, 
		HandleAlreadyClosed = 0x88, NolinearSpace = 0x89, UndefinedFileError = 0x8a, FileBussy = 0x8b, 
		NoWriteBuffers = 0x8c, AppendNotPossible = 0x8d, FileIsFull = 0x8e, FileAlreadyExists = 0x8f, 
		ModuleNotFound = 0x90, OutOfBoundary = 0x91, IllegalFileName = 0x92, IllegalHandle = 0x93, 
		PendingCommunication = 0x20, MailboxQueueEmpty = 0x40, RequestFailed = 0xbd, UnknownCommand = 0xbe, 
		InsanePacket = 0xbf,DataOutOfRange = 0xc0, CommunicationBusError = 0xdd, BufferFull = 0xde, 
		InvalidChannel = 0xdf, ChannelBusy = 0xe0, NoActiveProgram = 0xec, IllegalSize = 0xed, 
		InvalidMailboxQueue = 0xee, InvalidField = 0xef, BadIO = 0xf0, OutOfMemory = 0xfb, BadArguments = 0xff
		#pragma warning restore
	};
	
	
	/// <summary>
	/// Exceptions from NXT brick
	/// </summary>
	public class BrickException : MonoBrickException
	{
   		private static string errorToString(BrickError error){
			string errorDescription = "";
			switch(error){
                case BrickError.WrongNumberOfBytes:
                    errorDescription = "Wrong number of bytes received";
                    break;
                case BrickError.UnknownErrorCode:
				    errorDescription = "ErrorCode is unknown";
					break;
			    case BrickError.I2CTimeOut:
				    errorDescription = "I2C sensor timeout";
				    break;
				case BrickError.NoMoreHandles:
					errorDescription = "No more file handles";
					break;
				case BrickError.NoSpace:
					errorDescription = "No more space for file";
					break;
				case BrickError.NoMoreFiles:
					errorDescription = "No more files";
					break;
				case BrickError.EndOfFileExpected:
					errorDescription = "End of file expected";
					break;
				case BrickError.EndOfFile:
					errorDescription = "End of file";
					break;
				case BrickError.NotALinearFile:
					errorDescription = "Not a none-fragmented file";
					break;
				case BrickError.FileNotFound:
					errorDescription = "File not found";
					break;
				case BrickError.HandleAlreadyClosed:
					errorDescription = "Handle already closed";
					break;
				case BrickError.NolinearSpace:
					errorDescription = "No space for none-fragmented file";
					break;
				case BrickError.UndefinedFileError:
					errorDescription = "Undefined file error";
					break;
				case BrickError.FileBussy:
					errorDescription = "File is busy";
					break;
				case BrickError.NoWriteBuffers:
					errorDescription = "No write buffers";
					break;
				case BrickError.AppendNotPossible:
					errorDescription = "Append to file is not possible";	
					break;
				case BrickError.FileIsFull:
					errorDescription = "File is full";
					break;
				case BrickError.FileAlreadyExists:
					errorDescription = "File already exists";
					break;
				case BrickError.ModuleNotFound:
					errorDescription = "Module not found";
					break;
				case BrickError.OutOfBoundary:
					errorDescription = "Out of boundary";
					break;
				case BrickError.IllegalFileName:
					errorDescription = "Illegal filename";
					break;
				case BrickError.IllegalHandle:
					errorDescription = "Illegal file handle";
					break;
				case BrickError.PendingCommunication:
					errorDescription = "Pending communication transaction in progress";
					break;
				case BrickError.MailboxQueueEmpty:
					errorDescription = "Specified mailbox queue is empty";
					break;
				case BrickError.RequestFailed:
					errorDescription = "Request failed (specified file not found)";
					break;
				case BrickError.UnknownCommand:
					errorDescription = "Unknown command opcode";
					break;
				case BrickError.InsanePacket:
					errorDescription = "Insane packet";
					break;
				case BrickError.DataOutOfRange:
					errorDescription = "Data contains out-of-range values";
					break;
				case BrickError.CommunicationBusError:
					errorDescription = "Communication bus error";
					break;
				case BrickError.BufferFull:
					errorDescription = "No free memory in communication buffer";
					break;
				case BrickError.InvalidChannel:
					errorDescription = "Pecified channel/connection is not valid";
					break;
				case BrickError.NoActiveProgram:
					errorDescription = "No active program running";
					break;
				case BrickError.IllegalSize:
					errorDescription = "Illegal size specified";
					break;
				case BrickError.InvalidMailboxQueue:
					errorDescription = "Illegal mailbox queue ID specified";
					break;
				case BrickError.InvalidField:
					errorDescription = "Attemped to access invalid field of a structure";
					break;
				case BrickError.BadIO:
					errorDescription = "Bad input or output specified";
					break;
				case BrickError.OutOfMemory:
					errorDescription = "Insufficient memory available";
					break;
				case BrickError.BadArguments:
					errorDescription = "Bad arguments";
					break;
				default:
	            	errorDescription = "Unknown brick error";
				    break;
			}
			return errorDescription;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrick.NXT.BrickException"/> class.
		/// </summary>
		/// <param name='error'>
		/// Brick Error.
		/// </param>
		public BrickException(BrickError error): base(errorToString(error), (byte) error){}
        
		/// <summary>
		/// Initializes a new instance of <see cref="MonoBrick.NXT.BrickException"/>
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
	/// Class used to throw an NXT exception from monoBrick
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
			ThrowException(errorCode, ToErrorType(ref errorCode));
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
		internal static ErrorType ToErrorType(ref byte errorCode){
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
				errorCode = (byte) BrickError.UnknownErrorCode;
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
		/// <param name='expectedLength'>
		/// Expected reply length
		/// </param>
		public static void CheckForError(Reply reply, byte expectedLength){
		   CheckForError(reply,expectedLength,null);
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
		/// <param name='cleanUp'>
		/// Clean up method called before the exception is thrown
		/// </param>
		public static void CheckForError(Reply reply, byte expectedLength, CleanUpMethod cleanUp){
			if(reply.HasError){
				if(cleanUp!= null){
					cleanUp();
				}
				ThrowException(reply);
			}
			if(reply.Length != expectedLength){
				if(cleanUp!= null){
					cleanUp();
				}
				throw new BrickException(BrickError.WrongNumberOfBytes);
			}
		}
	}
		
}

