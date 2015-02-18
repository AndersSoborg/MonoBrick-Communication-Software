using System;

namespace MonoBrick.NXT
{
	/// <summary>
	/// Boxes used for the mailbox system
	/// </summary>
	public enum Box	{
		#pragma warning disable 
		Box0 = 0, Box1 = 1, Box2 = 2, Box3 =3 , Box4 = 4, Box5 = 5, Box6 = 6, Box7 = 7, Box8 = 8, Box9 = 9
		#pragma warning restore
	};
	
	/// <summary>
	/// Mailbox system for the LEGO Brick
	/// </summary>
	public class Mailbox
	{
		private Connection<Command,Reply> connection = null;
		internal Connection<Command,Reply> Connection{
			get{ return connection;}
			set{ connection = value;}
		}
		
		/// <summary>
		/// Send a byte array to the brick's mailbox system
		/// </summary>
		/// <param name='data'>
		/// Data array to write to the mailbox
		/// </param>
		/// <param name='inbox'>
		/// The mailbox to send to
		/// </param>
		public void Send(byte[] data, Box inbox){
			Send(data,inbox,false);
		}
		
		/// <summary>
		/// Send a byte array to the bricks mailbox system
		/// </summary>
		/// <param name='data'>
		/// Data array to write to the mailbox
		/// </param>
		/// <param name='inbox'>
		/// The mailbox to send to
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Send(byte[] data, Box inbox, bool reply){
			var command = new Command(CommandType.DirecCommand, CommandByte.MessageWrite, reply);
			if(data.Length > 57){
				Array.Resize(ref data,57);
			}
			command.Append((byte)inbox);
			command.Append(System.Convert.ToByte(data.Length+1));
			command.Append(data);
			command.Append((byte)0);
			command.Print();
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}
		
		/// <summary>
		/// Send a string to brick's mailbox system
		/// </summary>
		/// <param name='s'>
		/// string to write 
		/// </param>
		/// <param name='inbox'>
		/// The mailbox to send to
		/// </param>
		public void Send(string s, Box inbox){
			Send(s,inbox,false);
		}
		
		/// <summary>
		/// Send a string to brick's mailbox system
		/// </summary>
		/// <param name='s'>
		/// string to write 
		/// </param>
		/// <param name='inbox'>
		/// The mailbox to send to
		/// </param>
		/// <param name='reply'>
		/// If set to <c>true</c> the brick will send a reply
		/// </param>
		public void Send(string s, Box inbox, bool reply){
			var command = new Command(CommandType.DirecCommand, CommandByte.MessageWrite, reply);
			if(s.Length > 57){
				s.Remove(57);	
			}
			command.Append((byte)inbox);
			command.Append(System.Convert.ToByte(s.Length+1));
			command.Append(s);
			connection.Send(command);
			if(reply){
				var brickReply = connection.Receive();
				Error.CheckForError(brickReply,3);
			}
		}
		
		/// <summary>
		/// Read a byte array from the brick's mailbox system
		/// </summary>
		/// <returns>
		/// The message as a byte array
		/// </returns>
		/// <param name='mailbox'>
		/// The mailbox to read
		/// </param>
		/// <param name='removeMessage'>
		/// If set to <c>true</c> the message will be removed from the mailbox
		/// </param>
		public byte[] Read(Box mailbox, bool removeMessage){
			var command = new Command(CommandType.DirecCommand, CommandByte.MessageRead, true);
			command.Append((byte)((byte)mailbox + (byte)10));
			command.Append((byte)((byte)mailbox + (byte)0));
			command.Append(removeMessage);
			connection.Send(command);
			var reply = connection.Receive();
			Error.CheckForError(reply,64);
			byte size = reply[4];
			byte[] returnValue = new byte[size];
			for(int i = 0; i < size; i++){
				returnValue[i] = reply[i+5];
			}
			return returnValue;
		}
		
		/// <summary>
		/// Read a string from the brick's mailbox system
		/// </summary>
		/// <returns>
		/// The message as a string.
		/// </returns>
		/// <param name='mailbox'>
		/// The mailbox to read
		/// </param>
		/// <param name='removeMessage'>
		/// If set to <c>true</c> the message will be removed from the mailbox
		/// </param>
		public string ReadString(Box mailbox, bool removeMessage){
			byte[] data = Read(mailbox,removeMessage);
			return System.Text.ASCIIEncoding.ASCII.GetString(data, 0,data.Length);
		}
		
		
	}
}



