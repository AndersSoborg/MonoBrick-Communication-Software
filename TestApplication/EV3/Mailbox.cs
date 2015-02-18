using System;

namespace MonoBrick.EV3
{
		/// <summary>
		/// Class for EV3's mailbox brick.
		/// </summary>
		public class Mailbox
		{
				private Connection<Command, Reply> connection;
				internal Connection<Command,Reply> Connection{
					get{ return connection;}
					set{ connection = value;}
				}

				/// <summary>
				/// Initializes a new instance of the <see cref="MonoBrick.EV3.Mailbox"/> class.
				/// </summary>
				public Mailbox ()
				{
					
				}
				
				/// <summary>
				/// Send a byte array to the mailbox
				/// </summary>
				/// <param name="mailboxName">Mailbox name to send to.</param>
				/// <param name="data">Data to send.</param>
				/// <param name="reply">If set to <c>true</c> reply from the brick will be send.</param>
				public void Send (string mailboxName, byte[] data, bool reply = false)
				{
					var command = new Command (SystemCommand.WriteMailbox, 100, reply);
					Int16 payloadLength = (Int16)data.Length;
					byte nameLength = (byte)(mailboxName.Length + 1);
					command.Append (nameLength);
					command.Append (mailboxName);
					command.Append(payloadLength);
					command.Append(data);
					connection.Send(command);
					if(reply){
						var brickReply = connection.Receive();
						Error.CheckForError(brickReply,100);
					}
					command.Print();
				}
				
				/// <summary>
				/// Send a string message to the mailbox
				/// </summary>
				/// <param name="mailboxName">Mailbox name to send to.</param>
				/// <param name="message">Message to send.</param>
				/// <param name="reply">If set to <c>true</c> reply from brick will be send.</param>
				public void Send (string mailboxName, string message, bool reply = false)
				{
					byte[] data = new byte[message.Length+1];
					int i = 0;
					while(i < message.Length)
					{
	    				data[i] = (byte)message[i];
	    				i++;
					}
					data[i] = 0;
					Send(mailboxName,data,reply);
				}
				
				public int Open (byte number, string mailboxName)
				{
					var command = new Command(0, 0,100,true);
					command.Append(ByteCodes.MailboxOpen);
					//command.Append(number)
					command.Append(number, ParameterFormat.Short);
					
					//command.Append(0);
					command.Append(0,ParameterFormat.Short);
					
					//command.Append(0);
					command.Append(0,ParameterFormat.Short);
					
					//command.Append(0);
					command.Append(0,ParameterFormat.Short);
					
					//command.Append(30);
					command.Append(10,ParameterFormat.Short);
					
					var reply =	connection.SendAndReceive(command);
					Error.CheckForError(reply,100);
					reply.print();
					return 0;
				}
				
				public void Close (byte number)
				{
					var command = new Command(0, 0,101,true);
					command.Append(ByteCodes.MailboxClose);
					
					//command.Append(number)
					command.Append(number, ParameterFormat.Short);
					
					var reply =	connection.SendAndReceive(command);
					Error.CheckForError(reply,100);
					reply.print();
				}
				
				public byte[] Read (byte number, int bytesToRead)
				{
					var command = new Command(bytesToRead, 0,102,true);
					command.Append(ByteCodes.MailboxClose);
					
					//command.Append(number)
					command.Append(number, ParameterFormat.Short);
					
					//command.Append(number)
					command.Append(bytesToRead, ConstantParameterType.Value);
					
					//command.Append(number)
					command.Append(bytesToRead, ConstantParameterType.Value);
					
					command.Append((byte)0, VariableScope.Global);
					
					var brickReply = connection.SendAndReceive(command);
					Error.CheckForError(brickReply,102);
					brickReply.print();
					return brickReply.GetData(3);
				}
				
				
		}
}

