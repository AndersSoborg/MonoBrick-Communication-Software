using System;
using Gtk;
using Gdk;
using MonoBrick;
using MonoBrick.NXT;
using System.Collections.Generic;
using System.Collections;
namespace ComboBoxExtensions
{
	
	public class Entry{
		public Entry(string name, object val){Name = name;Value = val;}
		public string Name;
		public object Value;
	} 
	
	public class EntryList : IEnumerable{
		private List<Entry> list = new List<Entry>();
		public void Add(string name, object val){
			list.Add(new Entry(name,val));
		}
		public void Add(Entry entry){
			list.Add(entry);
		}
		public Entry this[int i]{
			get{return list[i];}
		}
		public Entry[] Entries{
			get{return list.ToArray();}
		}
		public int Count{
			get{return list.Count;}
		}
		public IEnumerator GetEnumerator(){
			return list.GetEnumerator();
		}
	}
	
	public class PollOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("100 ms",100);
				list.Add("200 ms",200);
				list.Add("500 ms",500);
				list.Add("1 sec",1000);
				list.Add("2 sec",2*1000);
				list.Add("5 sec",5*1000);
				list.Add("10 sec",10*1000);
				return list;
			}
		}
	}

	public class MotorOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("Out A",MonoBrick.NXT.MotorPort.OutA);
				list.Add("Out B",MonoBrick.NXT.MotorPort.OutB);
				list.Add("Out C",MonoBrick.NXT.MotorPort.OutC);
				return list;
			}
		}
	}
	
	public class SensorList{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				foreach (KeyValuePair<string, Sensor> pair in Sensor.SensorDictionary)
				{
	    			list.Add(pair.Key,pair.Value);
				}
				return list;
			}
		}
	}

	public class NetworkCachingOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("50 ms",100);
				list.Add("100 ms",100);
				list.Add("200 ms",200);
				list.Add("300 ms",300);
				list.Add("400 ms",400);
				list.Add("500 ms",500);
				list.Add("1 sec",1000);
				return list;
			}
		}
	}


	public class MessageSettingOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("Message 1",1);
				list.Add("Message 2",2);
				list.Add("Message 3",3);
				list.Add("Message 4",4);
				list.Add("Message 5",5);
				return list;
			}
		}
	}

	public class MailboxOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("Mailbox 0",MonoBrick.NXT.Box.Box0);
				list.Add("Mailbox 1",MonoBrick.NXT.Box.Box1);
				list.Add("Mailbox 2",MonoBrick.NXT.Box.Box2);
				list.Add("Mailbox 3",MonoBrick.NXT.Box.Box3);
				list.Add("Mailbox 4",MonoBrick.NXT.Box.Box4);
				list.Add("Mailbox 5",MonoBrick.NXT.Box.Box5);
				list.Add("Mailbox 6",MonoBrick.NXT.Box.Box6);
				list.Add("Mailbox 7",MonoBrick.NXT.Box.Box7);
				list.Add("Mailbox 8",MonoBrick.NXT.Box.Box8);
				list.Add("Mailbox 9",MonoBrick.NXT.Box.Box9);
				return list;
			}
		}
	}

	public enum FormatType{Hex = 0, String = 1};
	public class FormatOption{
		public static EntryList EntryList{
			get{
				EntryList list = new EntryList();
				list.Add("String",FormatType.String);
				list.Add("Hex",FormatType.Hex);
				return list;
			}
		}
	}


	public static class ComboBoxHelper
	{
		public static void Populate(this ComboBox comboBox, string[] entries)
    	{
        	comboBox.Clear();
			CellRendererText cell = new CellRendererText();
	        comboBox.PackStart(cell, false);
	        comboBox.AddAttribute(cell, "text", 0);
	        ListStore store = new ListStore(typeof (string));
	        comboBox.Model = store;
			foreach(string val in entries)
	        {
	            store.AppendValues(val);
	        }
			comboBox.Active = 0;
    	}
		
		public static void Populate(this ComboBox comboBox, EntryList entryList)
		{
			comboBox.Clear();
			ListStore listStore = new Gtk.ListStore(
				(entryList[0].Value.GetType()),
				(entryList[0].Name.GetType())
			);
			comboBox.Model = listStore;
			CellRendererText text = new CellRendererText();
			comboBox.PackStart(text, false);
			comboBox.AddAttribute(text, "text", 1);
			foreach (Entry entry in entryList){
				listStore.AppendValues(entry.Value,entry.Name);
			}
			TreeIter iter;
			if (listStore.GetIterFirst (out iter))
			{
				comboBox.SetActiveIter (iter);
			}
		}
		
		public static object GetActiveValue(this ComboBox comboBox, int column = 0){
			TreeIter iter;
			comboBox.GetActiveIter(out iter);
			return comboBox.Model.GetValue(iter,column);
		}

		public static bool SetActiveValue(this ComboBox comboBox, string entryKey, int column = 1){
			ListStore store = (ListStore) comboBox.Model;
			int i = 0;
			bool match = false;
			foreach (object[] row in store){ 
				if((string)row[column] == entryKey){
					comboBox.Active = i;
					match = true;
				}
				if(match){
					break;
				}
				else{
					i++;
				}
			}
			return match;
		}
	}
}

