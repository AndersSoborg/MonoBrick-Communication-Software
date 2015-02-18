using System;

namespace MonoBrick.EV3
{
		/// <summary>
		/// Class for reading and writing to the bricks memory
		/// </summary>
		public class MemoryArray<TValue>{
			private Connection<Command, Reply> connection;
			internal Connection<Command,Reply> Connection{
				get{ return connection;}
				set{ connection = value;}
			}
			
			/// <summary>
			/// Gets or sets the handle to the memory array on the brick
			/// </summary>
			/// <value>The handle.</value>
			public byte Handle {get; internal set;}
			
			private void CreateArray (Int32 size, MemorySubCodes subcode)
			{
				var command = new Command(4,0,130, true);
				command.Append(ByteCodes.Array);
				command.Append(subcode);
				command.Append(size, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = connection.SendAndReceive(command);
				Error.CheckForError(reply,130);
				Handle = reply.GetByte(3);
			}
			
			private void Fill(){
				var command = new Command(0,0,130, false);
				command.Append(ByteCodes.Array);
				command.Append(MemorySubCodes.Fill);
				command.Append(Handle, ConstantParameterType.Value);
				connection.Send(command);
			}
			
			internal void Init(Int32 size){
				MemorySubCodes subcode = MemorySubCodes.CreateTEF;
				if (typeof(TValue) == typeof(byte)){
					subcode = MemorySubCodes.Init8;
				}
			    if (typeof(TValue) == typeof(Int16)){
					subcode = MemorySubCodes.Init16;
			    }
			    if (typeof(TValue) == typeof(Int32)){
					subcode = MemorySubCodes.Init32;
			    }
			    if(typeof(TValue) == typeof(float)){
					subcode = MemorySubCodes.InitF;
			    }
				var command = new Command(4,0,130, true);
				command.Append(ByteCodes.Array);
				command.Append(subcode);
				command.Append(Handle, ConstantParameterType.Value);
				command.Append((Int32) 0, ConstantParameterType.Value);
				command.Append(size, ConstantParameterType.Value);
				command.Append((Int32)0, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = connection.SendAndReceive(command);
				Error.CheckForError(reply, 130);	
			}
			
			internal void CreateArray(Int32 size){
				if (typeof(TValue) != typeof(byte)  && 	typeof(TValue) != typeof(Int16)  && typeof(TValue) != typeof(Int32)  && typeof(TValue) != typeof(float)){
					throw new ArgumentException(string.Format("Type '{0}' is not valid.", typeof(TValue).ToString()));
				}
				if (typeof(TValue) == typeof(byte)){
					CreateArray(size, MemorySubCodes.Create8);	
				}
			    
			    if (typeof(TValue) == typeof(Int16)){
			    	CreateArray(size, MemorySubCodes.Create16);
			    }
			    
			    if (typeof(TValue) == typeof(Int32)){
			    	CreateArray(size, MemorySubCodes.Create32);
			    }
			    
			    if(typeof(TValue) == typeof(float)){
			    	CreateArray(size, MemorySubCodes.CreateTEF);
			    }
			    //Init(size);
			}
			
			/// <summary>
			/// Gets the size of the memory array
			/// </summary>
			/// <value>The size.</value>
			public Int32 Size {
				get{
					var command = new Command(4,0,130, true);
					command.Append(ByteCodes.Array);
					command.Append(MemorySubCodes.Size);
					command.Append(Handle, ConstantParameterType.Value);
					command.Append((byte)0, VariableScope.Global);
					var reply = connection.SendAndReceive(command);
					Error.CheckForError(reply,130);
					return reply.GetInt32(3);	
				}
			}
			
			/// <summary>
			/// Delete the memory array on the brick
			/// </summary>
			public void Delete ()
			{
				var command = new Command(0,0,130, false);
				command.Append(ByteCodes.Array);
				command.Append(MemorySubCodes.Delete);
				command.Append(Handle, ConstantParameterType.Value);
				connection.Send(command);
			}
			
			/// <summary>
			/// Write a single value to the EV3's memory
			/// </summary>
			/// <param name="value">Value to write</param>
			/// <param name="memoryIndex">Memory index on the EV3</param>
			public void Write (TValue value, Int32 memoryIndex)
			{
				var command = new Command(0,0,130, true);
				command.Append(ByteCodes.ArrayWrite);
				command.Append(Handle, ConstantParameterType.Value);
				command.Append(memoryIndex, ConstantParameterType.Value);
				if (typeof(TValue) == typeof(byte)){
					command.Append((byte)Convert.ToByte(value), ConstantParameterType.Value);
				}
			    if (typeof(TValue) == typeof(Int16)){
					command.Append(Convert.ToInt16(value));
			    }
			    if (typeof(TValue) == typeof(Int32)){
					command.Append(Convert.ToInt32(value));
			    }
			    if(typeof(TValue) == typeof(float)){
					command.Append(Convert.ToSingle(value));
			    }
			    //command.Append((byte)0, VariableScope.Global);
			    command.Print();
			    connection.Send(command);
			    var reply = connection.Receive();
			    Error.CheckForError(reply, 130);
			}
			
			
			/// <summary>
			/// Write a data array to the bricks memory from index 0
			/// </summary>
			/// <param name="data">Data array to write</param>
			public void Write (TValue[] data)
			{
				Write(data, 0, 0, data.Length);
			}
			
			/// <summary>
			/// Write a data array to the bricks memory
			/// </summary>
			/// <param name="data">Data array to write</param>
			/// <param name="memoryIndex">Memory index on the EV3</param>
			public void Write (TValue[] data, Int32 memoryIndex)
			{
				Write(data, memoryIndex, 0, data.Length);
			}
			
			/// <summary>
			/// Write a data array to the bricks memory
			/// </summary>
			/// <param name="data">Data array to write</param>
			/// <param name="memoryIndex">Memory index on the EV3</param>
			/// <param name="offset">Offset of the data array</param>
			/// <param name="length">Length. Number of data elements to write</param>
			public void Write (TValue[] data, Int32 memoryIndex, Int32 offset, Int32 length)
			{
				for(int i = 0; i < length; i++){
					Write(data[i+offset], memoryIndex);
					memoryIndex++;
				}		
			}
			
			/// <summary>
			/// Read a single value from the bricks memory 
			/// </summary>
			/// <returns>A single value.</returns>
			/// <param name="memoryIndex">Memory index to read.</param>
			public TValue ReadSingleValue (Int32 memoryIndex)
			{
				TValue valueRead = default(TValue);
				var command = new Command(4,0,130, true);
				command.Append(ByteCodes.ArrayRead);
				command.Append(Handle, ConstantParameterType.Value);
				command.Append(memoryIndex, ConstantParameterType.Value);
				command.Append((byte)0, VariableScope.Global);
				var reply = connection.SendAndReceive(command);
				Error.CheckForError(reply,130);
				if (typeof(TValue) == typeof(byte)){
					valueRead =  (TValue)Convert.ChangeType(reply.GetByte(3).ToString(), typeof(TValue));
				}
			    if (typeof(TValue) == typeof(Int16)){
					valueRead =  (TValue)Convert.ChangeType(reply.GetInt16(3).ToString(), typeof(TValue));
			    }
			    if (typeof(TValue) == typeof(Int32)){
					valueRead =  (TValue)Convert.ChangeType(reply.GetInt32(3).ToString(), typeof(TValue));
			    }
			    if(typeof(TValue) == typeof(float)){
					valueRead =  (TValue)Convert.ChangeType(reply.GetFloat(3).ToString(), typeof(TValue));
			    }
				return valueRead;
			}
			
			/// <summary>
			/// Read an array from the bricks memory
			/// </summary>
			/// <param name="memoryIndex">Memory index.</param>
			/// <param name="length">Number of elements to read.</param>
			public TValue[] Read (Int32 memoryIndex, Int32 length)
			{
				TValue[] valueRead = new TValue[length];
				for(int i = 0; i < length; i++){
					valueRead[i] = ReadSingleValue(memoryIndex);
					memoryIndex++;
				}
				return valueRead;
			}
		}
		
		/// <summary>
		/// Class for reading and writing to a byte array in the bricks memory 
		/// </summary>
		public class ByteMemoryArray : MemoryArray<byte>{}
		
		/// <summary>
		/// Class for reading and writing to a int16 array in the bricks memory 
		/// </summary>
		public class Int16MemoryArray : MemoryArray<Int16>{}
		
		/// <summary>
		/// Class for reading and writing to a int32 array in the bricks memory 
		/// </summary>
		public class Int32MemoryArray : MemoryArray<Int32>{}
		
		
		/// <summary>
		/// Class for reading and writing to a float array in the bricks memory 
		/// </summary>
		public class FloatMemoryArray : MemoryArray<float>{}
		
		
		
		
		/// <summary>
		/// Memory class for EV3 brick
		/// </summary>
		public class Memory
		{
			private Connection<Command, Reply> connection;
			internal Connection<Command,Reply> Connection{
				get{ return connection;}
				set{ connection = value;}
			}
			
			/// <summary>
			/// Creates a new byte array on the brick
			/// </summary>
			/// <returns>The byte array.</returns>
			/// <param name="size">Size of the array.</param>
			public ByteMemoryArray CreateByteArray (int size)
			{
				var array = new ByteMemoryArray();
				array.Connection = this.connection;
				array.CreateArray(size);
				return array;
			}
			
			/// <summary>
			/// Creates a new int16 array on the brick
			/// </summary>
			/// <returns>The int16 array.</returns>
			/// <param name="size">Size of the array.</param>
			public Int16MemoryArray CreateInt16Array (int size)
			{
				var array = new Int16MemoryArray();
				array.Connection = this.connection;
				array.CreateArray(size);
				return array;
			}
			
			
			/// <summary>
			/// Creates a new int32 array on the brick
			/// </summary>
			/// <returns>The int32 array.</returns>
			/// <param name="size">Size of the array.</param>
			public Int32MemoryArray CreateInt32Array (int size)
			{
				var array = new Int32MemoryArray();
				array.Connection = this.connection;
				array.CreateArray(size);
				return array;
			}
			
			/// <summary>
			/// Creates a new float array on the brick
			/// </summary>
			/// <returns>The float array.</returns>
			/// <param name="size">Size of the array.</param>
			public FloatMemoryArray CreateFloatArray (int size)
			{
				var array = new FloatMemoryArray();
				array.Connection = this.connection;
				array.CreateArray(size);
				return array;
			}
			
			/// <summary>
			/// Gets the byte array on the brick
			/// </summary>
			/// <returns>The byte array.</returns>
			/// <param name="handle">Handle to the array on the Brick.</param>
			public ByteMemoryArray GetByteArray (byte handle)
			{
				var array = new ByteMemoryArray();
				array.Connection = this.connection;
				array.Handle = handle;
				return array;
			}
			
			/// <summary>
			/// Gets the int16 array on the brick
			/// </summary>
			/// <returns>The int16 array.</returns>
			/// <param name="handle">Handle to the array on the Brick.</param>
			public Int16MemoryArray GetInt16Array (byte handle)
			{
				var array = new Int16MemoryArray();
				array.Connection = this.connection;
				array.Handle = handle;
				return array;
			}
			
			/// <summary>
			/// Gets the int32 array on the Brick
			/// </summary>
			/// <returns>The int32 array.</returns>
			/// <param name="handle">Handle to the array on the Brick.</param>
			public Int32MemoryArray GetInt32Array (byte handle)
			{
				var array = new Int32MemoryArray();
				array.Connection = this.connection;
				array.Handle = handle;
				return array;
			}
			
			/// <summary>
			/// Gets the float array on the Brick
			/// </summary>
			/// <returns>The float array.</returns>
			/// <param name="handle">Handle to the array on the Brick.</param>
			public FloatMemoryArray GetFloatArray (byte handle)
			{
				var array = new FloatMemoryArray();
				array.Connection = this.connection;
				array.Handle = handle;
				return array;
			}
			
			/// <summary>
			/// Write the specified data and slot - this is not used hence private
			/// </summary>
			/// <param name="data">Data.</param>
			/// <param name="slot">Slot.</param>
			private void Write(byte[] data, ProgramSlots slot){
				var command = new Command(0,5,130, true);
				command.Append(ByteCodes.InitBytes);
				command.Append(0, VariableScope.Local);
				command.Append((byte) data.Length, ConstantParameterType.Value);//should this be a short value
				command.Append(data);
				command.Append(ByteCodes.MemoryWrite);
				command.Append((byte)slot, ParameterFormat.Short);
				command.Append(0, ParameterFormat.Short);
				command.Append(4, ParameterFormat.Short);
				command.Append(5, ParameterFormat.Short);
				command.Append(0, VariableScope.Local);
			}
			
			
			/// <summary>
			/// Read this instance - this is not used hence private
			/// </summary>
			private byte[] Read (){
				return null;
			}
			
		}
}

