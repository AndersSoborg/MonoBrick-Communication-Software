using System;
using ComboBoxExtensions;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System.Configuration;

public partial class MainWindow
{
	List<char> hexKeys = new List<char>(new char[] { '0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f' } );
	const int MaxNumberOfBytesToSend = 57;

	private string ConvertStringToHex(string asciiString)
	{
		string hex = "";
		foreach (char c in asciiString)
		{
			int tmp = Convert.ToInt32(c);
			hex += String.Format("{0:x2}", tmp);
		}
		return hex;
	}

	private string ConvertByteArrayToHexString(byte[] array){
		System.Text.StringBuilder sb = new System.Text.StringBuilder(array.Length * 2);
		foreach (byte b in array)
		{
			sb.AppendFormat("{0:x2}", b);
		}
		return sb.ToString();
	}

	public static byte[] ConvertHexStringToByteArray(string hexString)
	{
		if (hexString.Length % 2 != 0)
		{
			hexString = hexString.Remove(hexString.Length -1);		
		}
		
		byte[] hexAsBytes = new byte[hexString.Length / 2];
		for (int index = 0; index < hexAsBytes.Length; index++)
		{
			string byteValue = hexString.Substring(index * 2, 2);
			hexAsBytes[index] = byte.Parse(byteValue, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
		}
		
		return hexAsBytes; 
	}
	
	private string ConvertHexToString(string hexValue)
	{
		string StrValue = "";
		if((hexValue.Length%2) != 0){
			hexValue = hexValue.Remove(hexValue.Length -1);
		}
		while (hexValue.Length > 0)
		{
			StrValue += System.Convert.ToChar(System.Convert.ToUInt32(hexValue.Substring(0, 2), 16)).ToString();
			hexValue = hexValue.Substring(2, hexValue.Length - 2);
		}
		return StrValue;
	}

	protected void OnOutboxFormatComboboxChanged (object sender, EventArgs e)
	{
		if((FormatType) outboxFormatCombobox.GetActiveValue() == FormatType.Hex){
			Gtk.Application.Invoke (delegate {
				outboxMessage.MaxLength = MaxNumberOfBytesToSend *2;
				outboxMessage.Text = ConvertStringToHex(outboxMessage.Text);
			});
		}
		if((FormatType) outboxFormatCombobox.GetActiveValue() == FormatType.String){
			Gtk.Application.Invoke (delegate {
				outboxMessage.Text = ConvertHexToString(outboxMessage.Text);
				outboxMessage.MaxLength = MaxNumberOfBytesToSend;
			});

		}
	}

	protected void OnOutboxMessageTextInserted (object o,Gtk.TextInsertedArgs args)
	{
		if((FormatType) outboxFormatCombobox. GetActiveValue() == FormatType.Hex){
			if(outboxMessage.Text.Length >= outboxMessage.MaxLength)
				return;
			foreach(char c in args.Text){
				if(! hexKeys.Contains(char.ToLower(c))){
					outboxMessage.Text = outboxMessage.Text.Substring(0, outboxMessage.Text.LastIndexOf(args.Text));
					Gtk.Application.Invoke (delegate {
						MessageDialog md = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "\nInvalid Hex Value");
						md.Icon = global::Gdk.Pixbuf.LoadFromResource (MessageDialogIconName);
						md.Run ();
						md.WindowPosition = Gtk.WindowPosition.CenterOnParent;
						md.Destroy();
					});
					break;
				}

			}
		}
	}

	protected void OnSendMessageButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
		{
			if((FormatType) outboxFormatCombobox. GetActiveValue() == FormatType.Hex){
				brick.Mailbox.Send(ConvertHexStringToByteArray(outboxMessage.Text),(MonoBrick.NXT.Box) outboxMailboxCombobox.GetActiveValue(), false);
			}
			if((FormatType) outboxFormatCombobox. GetActiveValue() == FormatType.String){
				brick.Mailbox.Send(outboxMessage.Text, (MonoBrick.NXT.Box) outboxMailboxCombobox.GetActiveValue(), false);
			}
		});
	}

	private void SendMessageSetting(string message, string mailbox, string format){
		FormatType formatType = FormatType.Hex;
		MonoBrick.NXT.Box mailBoxEnum = MonoBrick.NXT.Box.Box0;
		foreach(ComboBoxExtensions.Entry e in MailboxOption.EntryList){
			if(e.Name == mailbox){
				mailBoxEnum = (MonoBrick.NXT.Box) e.Value;
			}
		}
		foreach(ComboBoxExtensions.Entry e in FormatOption.EntryList){
			if(e.Name == format){
				formatType = (FormatType) e.Value;
			}
		}
		if(formatType == FormatType.Hex){
			brick.Mailbox.Send(ConvertHexStringToByteArray(message), mailBoxEnum , false);
		}
		else{
			brick.Mailbox.Send(message,mailBoxEnum,false);
		}
	}

	private void SendMessageSetting(int idx){
		string message = "";
		string mailbox = "";
		string format = "";
		
		switch(idx){
		case 0:
			message = settings.OutboxMessage1;
			mailbox = settings.OutMailbox1;
			format = settings.OutFormat1;
			break;
		case 1:
			message = settings.OutboxMessage2;
			mailbox = settings.OutMailbox2;
			format = settings.OutFormat2;
			break;
		case 2:
			message = settings.OutboxMessage3;
			mailbox = settings.OutMailbox3;
			format = settings.OutFormat3;
			break;
		case 3:
			message = settings.OutboxMessage4;
			mailbox = settings.OutMailbox4;
			format = settings.OutFormat4;
			break;
		case 4:
			message = settings.OutboxMessage5;
			mailbox = settings.OutMailbox5;
			format = settings.OutFormat5;
			break;
		}

		SendMessageSetting(message,mailbox,format);
		/*string entry = MessageSettingOption.EntryList.Entries[idx].Name;
		foreach(MessageSetting setting in mailboxSettings.Message){
			if(setting.Name == entry){
				SendMessageSetting(setting);
			}
		}*/
	}

	private void EnableReadMessage(bool set){
		Gtk.Application.Invoke (delegate {
			inboxMessage.Sensitive = set;
			readMessageButton.Sensitive = set;
			inboxMailboxCombobox.Sensitive = set;
			inboxFormatCombobox.Sensitive = set;
			removeMessageCheckbutton.Sensitive = set;
		});

	}


	protected void OnReadMessageButtonClicked (object sender, EventArgs e)
	{
		SpawnThread(delegate()
		{
			string s = "";
			if((FormatType) inboxFormatCombobox. GetActiveValue() == FormatType.Hex){
				s = ConvertByteArrayToHexString(brick.Mailbox.Read((MonoBrick.NXT.Box) inboxMailboxCombobox.GetActiveValue(), removeMessageCheckbutton.Active));
			}
			if((FormatType) inboxFormatCombobox. GetActiveValue() == FormatType.String){
				s = brick.Mailbox.ReadString((MonoBrick.NXT.Box) inboxMailboxCombobox.GetActiveValue(), removeMessageCheckbutton.Active);
			}
			Gtk.Application.Invoke (delegate {
				inboxMessage.Text = s;
			});

		});
	}

	protected void OnInboxFormatComboboxChanged (object sender, EventArgs e)
	{
		if((FormatType) inboxFormatCombobox. GetActiveValue() == FormatType.Hex){
			Gtk.Application.Invoke (delegate {
				inboxMessage.MaxLength = MaxNumberOfBytesToSend *2;
				inboxMessage.Text = ConvertStringToHex(inboxMessage.Text);
			});
		}
		if((FormatType) inboxFormatCombobox. GetActiveValue() == FormatType.String){
			Gtk.Application.Invoke (delegate {
				inboxMessage.Text = ConvertHexToString(inboxMessage.Text);
				inboxMessage.MaxLength = MaxNumberOfBytesToSend;
			});
			
		}
	}

	protected void LoadMailboxPageSettings(){
		//settings.Upgrade();
		//outboxMessage.Text = mailboxSettings.OutboxMessage1;
	}

	protected void OnLoadMessageButtonClicked (object sender, EventArgs e)
	{
		outboxFormatCombobox.Changed -=  OnOutboxFormatComboboxChanged;
		outboxMessage.TextInserted -= OnOutboxMessageTextInserted;

		string message = "";
		string mailbox = "";
		string format = "";

		switch(outboxMessageSettingsCombobox.Active){
		case 0:
			message = settings.OutboxMessage1;
			mailbox = settings.OutMailbox1;
			format = settings.OutFormat1;
			break;
		case 1:
			message = settings.OutboxMessage2;
			mailbox = settings.OutMailbox2;
			format =  settings.OutFormat2;
			break;
		case 2:
			message = settings.OutboxMessage3;
			mailbox = settings.OutMailbox3;
			format = settings.OutFormat3;
			break;
		case 3:
			message = settings.OutboxMessage4;
			mailbox = settings.OutMailbox4;
			format = settings.OutFormat4;
			break;
		case 4:
			message = settings.OutboxMessage5;
			mailbox = settings.OutMailbox5;
			format = settings.OutFormat5;
			break;
		}
		outboxMailboxCombobox.SetActiveValue(mailbox);
		outboxFormatCombobox.SetActiveValue(format);
		outboxMessage.Text = message;

		outboxFormatCombobox.Changed +=  OnOutboxFormatComboboxChanged;
		outboxMessage.TextInserted += OnOutboxMessageTextInserted;

		/*foreach(MessageSetting setting in messageSetting){
			if(setting.Name == entry){
				outboxFormatCombobox.Changed -=  OnOutboxFormatComboboxChanged;
				outboxMessage.TextInserted -= OnOutboxMessageTextInserted;
				outboxMailboxCombobox.SetActiveValue(setting.OutMailbox);
				outboxFormatCombobox.SetActiveValue(setting.OutFormat);
				outboxMessage.Text = setting.OutboxMessage;
				outboxFormatCombobox.Changed +=  OnOutboxFormatComboboxChanged;
				outboxMessage.TextInserted += OnOutboxMessageTextInserted;
			}
		}*/
	}
	
	protected void OnSaveMessageButtonClicked (object sender, EventArgs e)
	{
		string message = (string) outboxMessage.Text;
		string mailbox = (string) outboxMailboxCombobox.GetActiveValue(1);
		string format =  (string) outboxFormatCombobox.GetActiveValue(1);
		switch(outboxMessageSettingsCombobox.Active){
		case 0:
			settings.OutboxMessage1 = message;
			settings.OutMailbox1 = mailbox;
			settings.OutFormat1 = format;
			break;
		case 1:
			settings.OutboxMessage2 = message;
			settings.OutMailbox2 = mailbox;
			settings.OutFormat2 = format;
			break;
		case 2:
			settings.OutboxMessage3 = message;
			settings.OutMailbox3 = mailbox;
			settings.OutFormat3 = format;
			break;
		case 3:
			settings.OutboxMessage4 = message;
			settings.OutMailbox4 = mailbox;
			settings.OutFormat4 = format;
			break;
		case 4:
			settings.OutboxMessage5 = message;
			settings.OutMailbox5 = mailbox;
			settings.OutFormat5 = format;
			break;
		}

		/*string entry = (string) outboxMessageSettingsCombobox.GetActiveValue(1);
		for(int i = 0; i < messageSetting.Count; i++){
			if(messageSetting[i].Name == entry){
				messageSetting[i].OutMailbox =(string) outboxMailboxCombobox.GetActiveValue(1);
				messageSetting[i].OutFormat =(string) outboxFormatCombobox.GetActiveValue(1);
				messageSetting[i].OutboxMessage = (string) outboxMessage.Text;
			}
		}
		mailboxSettings.Message = messageSetting;*/
		settings.Save();
	}
}

