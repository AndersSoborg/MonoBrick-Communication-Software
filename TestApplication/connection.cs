using System.IO;
using System.Net.Sockets;
using System.Threading;
using System;
using System.ComponentModel;
using System.Net;

namespace MonoBrick
{
    
	/// <summary>
	/// Abstract class for a brick connection
	/// </summary>
	public abstract class Connection<TBrickCommand,TBrickReply>
		where TBrickCommand : BrickCommand
		where TBrickReply : BrickReply, new()
	{
	   	/// <summary>
	   	/// Occurs when connected.
	   	/// </summary>
		public event Action Connected;

		/// <summary>
		/// Occurs when disconnected.
		/// </summary>
		public event Action Disconnected;

		/// <summary>
		/// Occurs when command is send.
		/// </summary>
		public event Action<TBrickCommand> CommandSend;

		/// <summary>
		/// Occurs when reply is received.
		/// </summary>
		public event Action<TBrickReply> ReplyReceived;

		/// <summary></summary>
		protected bool isConnected;
        
		/// <summary>
		/// Gets a value indicating whether this instance is connected.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
		/// </value>
		public bool IsConnected {
            get { return isConnected; }
        }

		/// <summary>
		/// Send the specified command.
		/// </summary>
		/// <param name='command'>
		/// Command.
		/// </param>
        public abstract void Send(TBrickCommand command);
	   	
		/// <summary>
		/// Receive a reply
		/// </summary>
		public abstract TBrickReply Receive();
	   	
		/// <summary>
		/// Send and receive a reply
		/// </summary>
		/// <returns>
		/// The reply
		/// </returns>
		/// <param name='command'>
		/// The command to send
		/// </param>
        public TBrickReply SendAndReceive(TBrickCommand command)
        {
            Send(command);
			return Receive();
        }

		/// <summary>
		/// Fires an event when the connection is opened
		/// </summary>
		protected void ConnectionWasOpened(){
			if(Connected != null)
				Connected();
		}

		/// <summary>
		/// Fires an event when the connection is closed
		/// </summary>
		protected void ConnectionWasClosed(){
			if(Disconnected != null)
				Disconnected();
		}
		
		/// <summary>
		/// Fires an event when a reply is received
		/// </summary>
		/// <param name='reply'>
		/// Reply that was received
		/// </param>
		protected void ReplyWasReceived(TBrickReply reply){
			if(ReplyReceived != null)
				ReplyReceived(reply);
		}
		
		/// <summary>
		/// Fires the event CommandSend when a command is send
		/// </summary>
		/// <param name='command'>
		/// The command that was send
		/// </param>
		protected void CommandWasSend(TBrickCommand command){
			if(CommandSend != null)
				CommandSend(command);
		}

		/// <summary>
        /// Open the connection
        /// </summary>
		public abstract void Open();
	   	
		/// <summary>
		/// Close the connection
		/// </summary>
		public abstract void Close();
	}
}
