using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using MonoLibUsb.Transfer;

namespace MonoBrick
{    
    /// <summary>
    /// Lego usb constants.
    /// </summary>
    internal static class LegoUsbConstants{
        /// <summary>
        /// The NXT product identifier.
        /// </summary>
        public const short NXTProductId = 0x0002;
        
        /// <summary>
        /// The vendor identifier.
        /// </summary>
        public const short VendorId = 0x0694;
        
        /// <summary>
        /// The EV3 product identifier.
        /// </summary>
        public const short EV3ProductId = 0x0005;
    }
    
    
    /// <summary>
    /// Class used for USB communication on both NXT and EV3. Wraps all other USB classes
    /// </summary>
    public class USB<TBrickCommand,TBrickReply> : Connection<TBrickCommand, TBrickReply>
        where TBrickCommand : BrickCommand
        where TBrickReply : BrickReply, new()
    {
        private Connection<TBrickCommand,TBrickReply> connection = null;
        
           /// <summary>
           /// Initializes a new instance of the USB class.
           /// </summary>
           public USB()
        {
            isConnected = false;
            MonoBrickHelper.Platform platform = MonoBrickHelper.RunningPlatform();
            if(typeof(TBrickCommand) == typeof(NXT.Command)){
                if(platform == MonoBrickHelper.Platform.Mac){
                    connection = new MonoLibUsb<TBrickCommand,TBrickReply>(
                        new MonoLibUsbSettings(LegoUsbConstants.VendorId, LegoUsbConstants.NXTProductId, 0x82, 0x01,2,0), 
                        false, BrickType.NXT);
                }
                else{
                    connection = new LibUsb<TBrickCommand,TBrickReply>(LegoUsbConstants.VendorId, LegoUsbConstants.NXTProductId, ReadEndpointID.Ep02, WriteEndpointID.Ep01);    
                }
            }
            else{
                if(platform == MonoBrickHelper.Platform.Mac){
                    connection = new  HidLib<TBrickCommand,TBrickReply>(LegoUsbConstants.VendorId, LegoUsbConstants.EV3ProductId, false);
                }
                else if(platform == MonoBrickHelper.Platform.Linux){
                    connection = new MonoLibUsb<TBrickCommand,TBrickReply>(new MonoLibUsbSettings(LegoUsbConstants.VendorId, LegoUsbConstants.EV3ProductId, 0x81,0x01,0,0),true, BrickType.EV3);
                }
                else{
                    connection = new  HidLib<TBrickCommand,TBrickReply>(LegoUsbConstants.VendorId, LegoUsbConstants.EV3ProductId, true);
                }
            }
            
            connection.Connected += () => this.ConnectionWasOpened();
            connection.Disconnected += () => this.ConnectionWasClosed();
            connection.CommandSend += (TBrickCommand command) => this.CommandWasSend(command);
            connection.ReplyReceived += (TBrickReply reply) => this.ReplyWasReceived(reply);
            
        }
           
           /// <summary>
        /// Open the connection
        /// </summary>
        public override void Open(){
            connection.Open();    
        }        
        
        /// <summary>
        /// Close the connection
        /// </summary>
        public override void Close()
        {
            connection.Close();
        }
        
         /// <summary>
        /// Send the specified command.
        /// </summary>
        /// <param name='command'>
        /// Command to send
        /// </param>
        public override void Send(TBrickCommand command){
            connection.Send(command);
        }
        
        /// <summary>
        /// Receive a reply
        /// </summary>
        public override TBrickReply Receive(){
            return connection.Receive();
        }
    }
    
    /// <summary>
    /// Hid API native functions. This is placed in a separate class to satisfy Windows compiler since Dll import does not work with generics 
    /// </summary>
    internal class HidApiNative{
        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr hid_open(UInt16 vendor_id, UInt16  product_id, IntPtr serial_number);

        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hid_set_nonblocking(IntPtr device, int nonblock);

        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void hid_close(IntPtr device);

        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int hid_write(IntPtr device, IntPtr data, int length);

        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int hid_read_timeout (IntPtr device, IntPtr data, int length, int timeOutms);

        [DllImport("hidapi", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int hid_read (IntPtr device, IntPtr data, int length);
            
    }
    
    
    /// <summary>
    /// Class used for HID USB communication on MAC OS and Windows. Used for the EV3
    /// </summary>
    internal class  HidLib<TBrickCommand,TBrickReply> : Connection<TBrickCommand, TBrickReply>
        where TBrickCommand : BrickCommand
        where TBrickReply : BrickReply, new()
    {
        private IntPtr device = IntPtr.Zero;
        private const int Timeout = 4000;
        private ushort vendorId;
        private ushort productId;
        private bool addReportId;
        
        /// <summary>
        /// Initializes a new instance of the HID Lib class.
        /// </summary>
        /// <param name="vendorId">Vendor identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="addReportId">If set to <c>true</c> report id 0x00 will be added when seding data.</param>
        public  HidLib (short vendorId, short productId, bool addReportId)
        {
            this.vendorId = (ushort)vendorId;
            this.productId = (ushort)productId;
            this.addReportId = addReportId;
        }
        
        /// <summary>
        /// Open the connection
        /// </summary>
        public override void Open(){
            bool hasError = false;
            try
            {
                device = HidApiNative.hid_open(this.vendorId, this.productId, IntPtr.Zero);
                if(device != IntPtr.Zero){
                    HidApiNative.hid_set_nonblocking(device,0);
                }
                else{
                    hasError = true;
                }
            }        
            catch(Exception e) {
                throw new ConnectionException(ConnectionError.OpenError, e);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.OpenError);
            isConnected = true;
            ConnectionWasOpened();
        }        
        
        /// <summary>
        /// Close the connection
        /// </summary>
        public override void Close()
        {
            try{
                if(device != IntPtr.Zero){
                    HidApiNative.hid_close(device);    
                }
            }
            catch{}
            isConnected = false;
            ConnectionWasClosed();

        }
        
         /// <summary>
        /// Send the specified command.
        /// </summary>
        /// <param name='command'>
        /// Command to send
        /// </param>
        public override void Send(TBrickCommand command){
            Exception innerException = null;
            int bytesWritten;
            byte[] data = null;
            ushort length = (ushort) command.Length;
            if(addReportId){
                data = new byte[length+3];
                data[0] = 0x00;
                data[1] = (byte) (length & 0x00ff);
                data[2] = (byte)((length&0xff00) >> 2);
                Array.Copy(command.Data,0,data,3,command.Length);
            }
            else{
                data = new byte[length+2];
                data[0] = (byte) (length & 0x00ff);
                data[1] = (byte)((length&0xff00) >> 2);
                Array.Copy(command.Data,0,data,2,command.Length);
            }
            CommandWasSend(command);
            bool hasError = false;
            IntPtr pnt = IntPtr.Zero;
            try{
                int size = Marshal.SizeOf(data[0]) * data.Length;
                pnt = Marshal.AllocHGlobal(size);
                // Copy the array to unmanaged memory.
                Marshal.Copy(data, 0, pnt, data.Length);
                bytesWritten = HidApiNative.hid_write(device, pnt, data.Length);
                if(!addReportId){
                    if(bytesWritten != data.Length)
                        hasError = true;
                }
                else{
                    if(bytesWritten == -1)
                        hasError = true;
                }
            }
            catch(Exception e){
                innerException = e;
                hasError = true;
            }
            finally
            {
                if(pnt != IntPtr.Zero)    
                    Marshal.FreeHGlobal(pnt);
            }
            if(hasError){
                if(innerException != null){
                    throw new ConnectionException(ConnectionError.WriteError, innerException);
                }
                throw new ConnectionException(ConnectionError.WriteError);
            }

        }
        
        /// <summary>
        /// Receive a reply
        /// </summary>
        public override TBrickReply Receive(){
            byte[] reply = null;
            IntPtr pnt = IntPtr.Zero;
            int bytesRead = 0;
            int expectedlength = 0;
            bool hasError = false;
            try{
                byte[] temp = new byte[1024];
                pnt = Marshal.AllocHGlobal(Marshal.SizeOf(temp[0]) * 1024);
                bytesRead =  HidApiNative.hid_read_timeout(device, pnt, 1024, Timeout);
                if(bytesRead > 2){
                    Marshal.Copy(pnt, temp, 0, 1024); 
                    expectedlength = (ushort)(0x0000 | temp[0] | (temp[1] << 2));
                    Marshal.FreeHGlobal(pnt);
                    pnt = IntPtr.Zero;
                    reply = new byte[expectedlength];
                    Array.Copy(temp, 2,reply,0,expectedlength);
                }
                else{
                    if(pnt != IntPtr.Zero){
                        Marshal.FreeHGlobal(pnt);
                        pnt = IntPtr.Zero;
                    }
                    hasError = true;
                }
            
            }
            catch(Exception e){
                throw new ConnectionException(ConnectionError.ReadError,e);
            }
            finally{
                if(pnt != IntPtr.Zero)
                    Marshal.FreeHGlobal(pnt);    
            }
            if(hasError){
                if(typeof(TBrickCommand) == typeof(NXT.Command)){
                    throw new NXT.BrickException(NXT.BrickError.WrongNumberOfBytes);
                }
                else{
                    throw new EV3.BrickException(EV3.BrickError.WrongNumberOfBytes);
                }
            }
            var brickReply = new TBrickReply();
            brickReply.SetData(reply);
            ReplyWasReceived(brickReply);
            return brickReply;
        }
    }
    
    
    /// <summary>
    /// LibUsb is a library for WinUsb, libusb-win32, and Linux libusb v1.x developers. This is used for Linux and Windows support for the NXT
    /// </summary>
    internal class LibUsb<TBrickCommand,TBrickReply> : Connection<TBrickCommand,TBrickReply>
        where TBrickCommand : BrickCommand
        where TBrickReply : BrickReply, new()
    {
        private UsbDeviceFinder myUsbFinder = null;
        private UsbEndpointReader reader;
        private UsbEndpointWriter writer;
        private UsbDevice myUsbDevice;
        private ReadEndpointID readId;
        private WriteEndpointID writeId;
        private short vendorId;
        private short productId;
        private const int Timeout = 500;
        private bool addLength = false;
        /// <summary>
        /// Initializes a new instance of the LIB USB class.
        /// </summary>
        /// <param name="vendorId">Vendor identifier.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="readId">Read identifier.</param>
        /// <param name="writeId">Write identifier.</param>
        public LibUsb(short vendorId, short productId, ReadEndpointID readId, WriteEndpointID writeId){
            this.vendorId = vendorId;
            this.productId = productId;
            this.readId = readId;
            this.writeId = writeId;
            myUsbFinder = new UsbDeviceFinder (this.vendorId, this.productId);
        }
        
        /// <summary>
        /// Open the connection
        /// </summary>
        public override void Open(){
            bool hasError = false;
            try
            {
                myUsbDevice = UsbDevice.OpenUsbDevice(myUsbFinder);
                if (myUsbDevice == null)
                    throw new ConnectionException(ConnectionError.OpenError); 
                reader = myUsbDevice.OpenEndpointReader(readId, 1024, EndpointType.Bulk);
                writer = myUsbDevice.OpenEndpointWriter(writeId, EndpointType.Bulk);    
            }      
            catch {
                throw new ConnectionException(ConnectionError.OpenError);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.OpenError);
            isConnected = true;
            ConnectionWasOpened();
        }
        
        /// <summary>
        /// Close the connection
        /// </summary>
        public override void Close()
        {
            try{
                myUsbDevice.Close();
                myUsbDevice = null;
                UsbDevice.Exit();
            }
            catch{}
            isConnected = false;
            ConnectionWasClosed();

        }

        /// <summary>
        /// Send the specified command.
        /// </summary>
        /// <param name='command'>
        /// Command to send
        /// </param>
        public override void Send(TBrickCommand command){
            int bytesWritten;
            byte[] data = null;
            if (addLength) {
                ushort length = (ushort) command.Length;
                data = new byte[length+2];
                data[0] = (byte) (length & 0x00ff);
                data[1] = (byte)((length&0xff00) >> 2);
                Array.Copy(command.Data,0,data,2,command.Length);
            }    
            else {
                data = command.Data;
            }
            CommandWasSend(command);
            bool hasError = false;
            try{
                ErrorCode ec = ErrorCode.None;
                ec = writer.Write(data, Timeout , out bytesWritten);
                if (ec != ErrorCode.None){ 
                    hasError = true;
                }
            }
            catch(Exception e){
                throw new ConnectionException(ConnectionError.WriteError,e);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.WriteError);

        }

        /// <summary>
        /// Receive a reply
        /// </summary>
        public override TBrickReply Receive(){
            byte[] reply = new byte[1024];
            ErrorCode ec = ErrorCode.None;
            int bytesRead = 0;
            bool hasError = false;
            try{
                ec = reader.Read(reply, Timeout, out bytesRead);
                if (ec != ErrorCode.None){ 
                    hasError = true;
                }
            }
            catch(Exception e){
                throw new ConnectionException(ConnectionError.ReadError,e);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.ReadError);
            if (addLength) {
                int expectedlength = (ushort)(0x0000 | reply[0] | (reply[1] << 2));
                if(expectedlength != bytesRead -2){
                    if(typeof(TBrickCommand) == typeof(MonoBrick.NXT.Command)){
                        throw new NXT.BrickException(NXT.BrickError.WrongNumberOfBytes);
                    }
                    else{
                        throw new EV3.BrickException(EV3.BrickError.WrongNumberOfBytes);
                    }
                }
                byte[] temp = new byte[bytesRead - 2];
                Array.Copy (reply, 2, temp, 0, bytesRead - 2);
                reply = temp;
            }
            else{
                Array.Resize<byte>(ref reply,bytesRead);
            }
            var brickReply = new TBrickReply();
            brickReply.SetData(reply);
            ReplyWasReceived(brickReply);
            return brickReply;
        }

        
    }
    
    /// <summary>
    /// The MonoLibUsb is a complete implementation of Libusb-1.0. This is used on Mac OS for the NXT and on Windows and Linux for the EV3
    /// </summary>
    internal class MonoLibUsb<TBrickCommand,TBrickReply> : Connection<TBrickCommand,TBrickReply>
        where TBrickCommand : BrickCommand
        where TBrickReply : BrickReply, new()
    {
        
        private MonoLibUsbSettings settings = null;
        private static MonoLibUsb.MonoUsbSessionHandle sessionHandle  = null;
        private MonoLibUsb.MonoUsbDeviceHandle deviceHandle = null;
        private bool resetDvice = true;
        private const int Timeout = 500;
        private bool addLength;
        private BrickType type;
        /// <summary>
        /// Create a instance of MonoLibUSB
        /// </summary>
        /// <param name="settings">Settings to use</param>
        /// <param name="addLength">If set to <c>true</c> add length to the message</param>
        /// <param name="type">Brick type to use</param>
        public MonoLibUsb(MonoLibUsbSettings settings, bool addLength, BrickType type){
            this.settings = settings;
            this.addLength = addLength;
            this.type = type;
        }
        
        /// <summary>
        /// Open the connection
        /// </summary>
        public override void Open(){
            bool hasError = false;
            try
            {
                int r = 0;
                sessionHandle=new MonoLibUsb.MonoUsbSessionHandle();
                if (sessionHandle.IsInvalid){ 
                    hasError = true;
                }
                deviceHandle = MonoLibUsb.MonoUsbApi.OpenDeviceWithVidPid(sessionHandle, settings.VendorId , settings.ProductId);
                if ((deviceHandle == null) || deviceHandle.IsInvalid){ 
                    hasError = true;
                }
                if(type == BrickType.NXT){
                    if (resetDvice)
                    {
                        MonoLibUsb.MonoUsbApi.ResetDevice(deviceHandle);
                        deviceHandle.Close();
                        deviceHandle = MonoLibUsb.MonoUsbApi.OpenDeviceWithVidPid(sessionHandle, settings.VendorId , settings.ProductId);
                        if ((deviceHandle == null) || deviceHandle.IsInvalid) 
                            hasError = true;
                    }
                

                    // Set configuration
                    r = MonoLibUsb.MonoUsbApi.SetConfiguration(deviceHandle, settings.Configuration);
                    if (r != 0)
                        hasError = true;
                
                }
                else{
                    MonoLibUsb.MonoUsbApi.DetachKernelDriver(deviceHandle,settings.Interface);
                }
                
                // Claim interface
                r = MonoLibUsb.MonoUsbApi.ClaimInterface(deviceHandle, settings.Interface);
                if (r != 0)
                    hasError = true;

            }      
            catch {
                throw new ConnectionException(ConnectionError.OpenError);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.OpenError);
            isConnected = true;
            ConnectionWasOpened();
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        public override void Close()
        {
            try{
                // Free and close resources
                if (deviceHandle != null)
                {
                    if (!deviceHandle.IsInvalid)
                    {
                        MonoLibUsb.MonoUsbApi.ReleaseInterface(deviceHandle, settings.Interface);
                        deviceHandle.Close();
                    }
                }
                if (sessionHandle!=null)
                {
                    sessionHandle.Close();
                    sessionHandle = null;
                }
            
            }
            catch{}
            isConnected = false;
            ConnectionWasClosed();

        }

        /// <summary>
        /// Send the specified command.
        /// </summary>
        /// <param name='command'>
        /// Command to send
        /// </param>
        public override void Send(TBrickCommand command){
            int bytesWritten;
            byte[] data = null;
            if (addLength) {
                ushort length = (ushort) command.Length;
                data = new byte[length+2];
                data[0] = (byte) (length & 0x00ff);
                data[1] = (byte)((length&0xff00) >> 2);
                Array.Copy(command.Data,0,data,2,command.Length);
            }    
            else {
                data = command.Data;
            }
            CommandWasSend(command);
            bool hasError = false;
            try{
                if(type == BrickType.NXT){
                    MonoLibUsb.MonoUsbApi.BulkTransfer(deviceHandle, settings.WriteEndPoint, data,data.Length, out bytesWritten, Timeout);
                }
                else{
                    MonoLibUsb.MonoUsbApi.InterruptTransfer(deviceHandle, settings.WriteEndPoint, data,data.Length, out bytesWritten, Timeout);
                }
                if(bytesWritten != data.Length){
                    hasError = true;
                }
            }
            catch(Exception e){
                throw new ConnectionException(ConnectionError.WriteError,e);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.WriteError);

        }

        /// <summary>
        /// Receive a reply
        /// </summary>
        public override TBrickReply Receive(){
            byte[] reply = new byte[1024];
            int bytesRead = 0;
            bool hasError = false;
            try{
                int r = 0;
                if(type == BrickType.NXT){
                    r = MonoLibUsb.MonoUsbApi.BulkTransfer(deviceHandle, settings.ReadEndPoint, reply,reply.Length, out bytesRead, Timeout);
                }
                else{
                    r = MonoLibUsb.MonoUsbApi.InterruptTransfer(deviceHandle, settings.ReadEndPoint, reply,reply.Length, out bytesRead, Timeout);
                }
                if(r!= 0){ 
                    hasError = true;
                }
            }
            catch(Exception e){
                throw new ConnectionException(ConnectionError.ReadError,e);
            }
            if(hasError)
                throw new ConnectionException(ConnectionError.ReadError);
            if (addLength) {
                int expectedlength = (ushort)(0x0000 | reply[0] | (reply[1] << 2));
                if(type == BrickType.NXT){
                    if(expectedlength != bytesRead -2){
                        throw new NXT.BrickException(NXT.BrickError.WrongNumberOfBytes);
                    }
                }
                byte[] temp = new byte[bytesRead - 2];
                Array.Copy (reply, 2, temp, 0, bytesRead - 2);
                reply = temp;
            }
            else{
                Array.Resize<byte>(ref reply,bytesRead);
            }
            var brickReply = new TBrickReply();
            brickReply.SetData(reply);
            ReplyWasReceived(brickReply);
            return brickReply;
        }

    }
    

    /// <summary>
    /// Mono lib USB settings.
    /// </summary>
    internal class MonoLibUsbSettings{
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoBrick.MonoLibUsbSettings"/> class.
        /// </summary>
        /// <param name="vendorID">Vendor I.</param>
        /// <param name="productId">Product identifier.</param>
        /// <param name="readEndPoint">Read end point.</param>
        /// <param name="writeEndPoint">Write end point.</param>
        /// <param name="usbInterface">Usb interface.</param>
        /// <param name="usbConfiguration">Usb configuration.</param>
        public  MonoLibUsbSettings(short vendorID, short productId,byte readEndPoint, byte writeEndPoint, int usbInterface, int usbConfiguration) 
        {
            this.VendorId = vendorID;
            this.ProductId = productId;
            this.ReadEndPoint = readEndPoint;
            this.WriteEndPoint = writeEndPoint;
            this.Interface = usbInterface;
            this.Configuration = usbConfiguration;
        }
        /// <summary>
        /// Gets the read end point.
        /// </summary>
        /// <value>The read end point.</value>
        public byte ReadEndPoint {get; private set;}
        
        /// <summary>
        /// Gets the write end point.
        /// </summary>
        /// <value>The write end point.</value>
        public byte WriteEndPoint {get; private set;}
        
        /// <summary>
        /// Gets the interface.
        /// </summary>
        /// <value>The interface.</value>
        public int Interface {get; private set;}
        
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public int Configuration {get; private set;}
        
        
        /// <summary>
        /// Gets the vendor identifier.
        /// </summary>
        /// <value>The vendor identifier.</value>
        public short VendorId {get; private set;}
        
        
        /// <summary>
        /// Gets the product identifier.
        /// </summary>
        /// <value>The product identifier.</value>
        public short ProductId {get; private set;}
   }
    
    
}