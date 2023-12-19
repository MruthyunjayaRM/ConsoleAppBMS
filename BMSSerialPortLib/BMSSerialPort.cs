using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Windows.Forms;

namespace BMSSerialPortLib
{
    public class BMSSerialPort : EventArgs
    {
        public SerialPort serialPort = new SerialPort();
        public int count = 0;
        serialComHandler serialCom = new serialComHandler();
        UniqIDTable[] uniqIDs = new UniqIDTable[10];
        byte[] buffer = new byte[DefineValue.MaxSerialBufferSize];
        byte serialDataRecieved;
        private bool isRunning;
        private bool startRecieveTransmit;
        private Thread dataReceivedThread;
        public SynchronizationContext synchronizationContext;
        private readonly object lockObject = new object();


        public event EventHandler<byte[]> dataReceivedHandler;
        private void DataReceivedThread()
        {
            while (isRunning)
            {
                try
                {
                    lock (lockObject)
                    {
                        if (serialPort.IsOpen && serialPort.BytesToRead > 0)
                        {
                            SerialPort_DataReceived();
                        }
                    }
                }
                catch (TimeoutException tex)
                {
                    Console.WriteLine($"Timeout exception in data received thread: {tex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle exceptions or log errors
                    Console.WriteLine($"Error in data received thread: {ex.Message}");
                }
                Thread.Sleep(100); // Adjust the sleep duration as needed
            }
        }
        //public BMSSerialPort()
        //{
        //    // default constructor, allowing you to set up the serial port later
        //}

        public BMSSerialPort(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, int readTimeout, int writeTimeout, SynchronizationContext context)
        {
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.Parity = parity;
            serialPort.StopBits = stopBits;
            serialPort.ReadTimeout = readTimeout;
            serialPort.WriteTimeout = writeTimeout;
            //serialPort.DataReceived += SerialPort_DataReceived;
            synchronizationContext = context;
        }

        public BMSSerialPort()
        {
        }

        public bool isOpen()
        {
            if(serialPort.IsOpen) return true; else return false;
        }
        public bool IsThreadRunning
        {
            get { return isRunning; }
        }

        //public void InitializeSerialPort(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, int readTimeout, int writeTimeout)
        //{
        //    serialPort.PortName = portName;
        //    serialPort.BaudRate = baudRate;
        //    serialPort.DataBits = dataBits;
        //    serialPort.Parity = parity;
        //    serialPort.StopBits = stopBits;
        //    serialPort.ReadTimeout = readTimeout;
        //    serialPort.WriteTimeout = writeTimeout;
        //    //serialPort.DataReceived += SerialPort_DataReceived;
        //    dataReceivedThread = new Thread(DataReceivedThread);
        //    isRunning = false;
        //}
        public void start_serialPortDataAssigning()
        {
            serialCom.dataRecieved.data = new byte[DefineValue.MaxSerialBufferSize];
            serialCom.dataRecieved.crc = new byte[DefineValue.MaxCRCBufferSize];
            serialCom.dataToSend.data = new byte[DefineValue.MaxSerialBufferSize];
            serialCom.dataToSend.crc = new byte[DefineValue.MaxCRCBufferSize];
            AddZeroValuesToArray();
            for (int i = 0; i < DefineValue.MaxMcu; i++)
            {
                uniqIDs[i].ControllerID = new byte[DefineValue.ControllerIDBufferSize];
                uniqIDs[i].UID = new byte[DefineValue.UIDBufferSize];
            }
            serialCom.ContrDisc = 0;
        }

        public void setAttempt(int attempt)
        {
            serialCom.attempt = attempt;
        }
        public void incAttempt()
        {
            serialCom.attempt++;
        }
        public void StartSerialThread()
        {
            if (!isRunning)
            {
                lock (lockObject)
                {
                    start_serialPortDataAssigning();
                    dataReceivedThread = new Thread(DataReceivedThread);
                    isRunning = true;
                    dataReceivedThread.Start();
                }
            }
        }
        public void StopSerialThread()
        {
            if (isRunning)
            {
                lock (lockObject)
                {
                    isRunning = false;
                    dataReceivedThread.Join(); // Wait for the thread to finish
                }
            }
        }
        public bool Open()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    return true;
                }
                else
                {
                    Console.WriteLine("Port is already open.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening port: {ex.Message}");
                return false;
            }
        }
        public bool Close()
        {
            try
            {
                if(serialPort.IsOpen)
                {
                    serialPort.Close();
                    isRunning = false;
                    if(dataReceivedThread != null)
                    {
                        dataReceivedThread.Join();// Wait for the thread to finish
                    }
                    Dispose();
                    return true;
                }
                else
                {
                    Console.WriteLine("Port is already closed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing port: {ex.Message}");
                return false;
            }
        }
        public void Dispose()
        {
            serialPort.Dispose();
        }
        public void Write(byte[] data)
        {
            try
            {
                lock (lockObject)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Write(data, 0, data.Length);
                    }
                    else
                    {
                        Console.WriteLine("Port is not open. Cannot write data.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing data: {ex.Message}");
            }
        }

        public byte[] Read(int length)
        {
            try
            {
                lock (lockObject)
                {
                    if (serialPort.IsOpen)
                    {
                        byte[] buffer = new byte[length];
                        serialPort.Read(buffer, 0, length);
                        return buffer;
                    }
                    else
                    {
                        Console.WriteLine("Port is not open. Cannot read data.");
                        return new byte[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");
                return new byte[0];
            }
        }
        public void tickHandler()
        {
            if (serialCom.attempt < 3)
            {
                serialCom.dataToSend.crc = CRC16Generator.GenerateCRC16(serialCom.dataToSend.data, 0);
                byte[] data = PacketCreation(Message.SYNC_BYTE, Message.CMD_PING_DISCOVERY, Message.SUB_CMD_REQUEST, serialCom.dataToSend.len, serialCom.dataToSend.data, serialCom.dataToSend.crc);
                if (serialPort.IsOpen)
                {
                    serialPort.Write(data, 0, data.Length);
                    serialCom.attempt++;
                }
            }
            else
            {
                //MessageBox.Show("NO response from the MCUs", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowMessage("NO response from the MCUs");
                Console.WriteLine($"NO response from the MCUs");
                serialCom.attempt = 0;
            }
            Console.ReadLine();
        }
        private void ShowMessage(string message)
        {
            // Use SynchronizationContext to post the message box on the UI thread for forms            
            //synchronizationContext.Post(state =>          
            //{
            //    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            Uncomment if Needed
            //}, null);

            // for console
            Console.WriteLine($"Error: {message}");
        }
        private void SerialPort_DataReceived()
        {
            try
            {
                serialDataRecieved = (byte)serialPort.ReadByte();
                if (serialCom.status == RecieveStatus.IDLE && serialDataRecieved == Message.SYNC_BYTE[0])
                {
                    serialCom.status = RecieveStatus.SYNC_FIRST_BYTE_RECIEVED;
                    serialDataRecieved = (byte)serialPort.ReadByte();
                }
                if (serialCom.status == RecieveStatus.SYNC_FIRST_BYTE_RECIEVED && serialDataRecieved == Message.SYNC_BYTE[1])
                {
                    serialCom.status = RecieveStatus.SYNC_BYTES_RECIEVED;
                    //Reset message data
                    ClearRecievedData();
                }
                if (serialCom.status == RecieveStatus.SYNC_BYTES_RECIEVED)
                {
                    serialCom.dataRecieved.cmd = (byte)serialPort.ReadByte();
                    serialCom.dataRecieved.subCmd = (byte)serialPort.ReadByte();
                    serialCom.dataRecieved.len = (byte)serialPort.ReadByte();
                    serialCom.status = RecieveStatus.SERIAL_ENUM_HEADER_RECEIVED;
                }
                if (serialCom.status == RecieveStatus.SERIAL_ENUM_HEADER_RECEIVED)
                {
                    for (int i = 0; i < serialCom.dataRecieved.len; i++)
                    {
                        serialCom.dataRecieved.data[i] = (byte)serialPort.ReadByte();
                    }
                    serialCom.dataRecieved.crc[0] = (byte)serialPort.ReadByte();
                    serialCom.dataRecieved.crc[1] = (byte)serialPort.ReadByte();
                    // Check CRC
                    serialCom.dataToSend.CRCcheck = CRCValidate(serialCom.dataRecieved.data, serialCom.dataRecieved.len);
                    if (serialCom.dataToSend.CRCcheck == 1)
                    {
                        serialCom.status = RecieveStatus.SERIAL_ENUM_MSG_RECIEVED;
                        serialCom.dataToSend.CRCcheck = 0;
                        DataToBeSent(serialCom.dataRecieved.data, serialCom.dataRecieved.len); //LEts keep it simple for now
                    }
                    else
                    {
                        SerialSendDataString("Resend The Message");
                        serialCom.status = RecieveStatus.IDLE;
                        serialPort.DiscardInBuffer();
                    }

                    // Handle the message if its recieved correctly
                    if (serialCom.dataRecieved.cmd == Message.CMD_PING_DISCOVERY && serialCom.status == RecieveStatus.SERIAL_ENUM_MSG_RECIEVED)
                    {
                        if (serialCom.dataRecieved.subCmd == Message.SUB_CMD_REQUEST)
                        {
                            if (serialPort.IsOpen)
                            {
                                //serialPort1.Write(serialCom.dataRecieved.data, 0, serialCom.dataRecieved.len);
                                //Configure as Needed

                                serialCom.dataToSend.data = serialCom.dataRecieved.data;
                                serialCom.dataToSend.len = serialCom.dataRecieved.len;
                                byte[] data = PacketCreation(Message.SYNC_BYTE, Message.CMD_PING_DISCOVERY, Message.SUB_CMD_RESPONSE, serialCom.dataToSend.len, serialCom.dataToSend.data, serialCom.dataToSend.crc);
                                serialPort.Write(data, 0, data.Length);
                                ClearSendData();
                                serialPort.DiscardOutBuffer();
                            }
                            serialCom.status = RecieveStatus.IDLE;
                            ClearRecievedData();
                            serialPort.DiscardInBuffer();
                            // Zero the Attempt value of requesting
                            serialCom.attempt = 0;
                        }
                    }
                    else if (serialCom.dataRecieved.cmd == Message.CMD_INTRO_SHORT_ADD && serialCom.status == RecieveStatus.SERIAL_ENUM_MSG_RECIEVED)
                    {
                        if (serialCom.dataRecieved.subCmd == Message.SUB_CMD_REQUEST)
                        {
                            if (serialCom.ContrDisc >= DefineValue.MaxMcu)
                            {
                                if (serialPort.IsOpen)
                                {
                                    SerialSendDataString("MCU capacity Full");
                                    serialCom.status = RecieveStatus.IDLE;
                                }
                            }
                            else if (serialCom.ContrDisc < DefineValue.MaxMcu)
                            {
                                if (serialPort.IsOpen)
                                {
                                    //serialPort1.Write(serialCom.dataRecieved.data, 0, serialCom.dataRecieved.len);
                                    //Configure as Needed
                                    for (int i = 0; i < DefineValue.ControllerIDBufferSize; i++)
                                    {
                                        uniqIDs[serialCom.ContrDisc].ControllerID[i] = serialCom.dataRecieved.data[i];
                                    }
                                    byte[] TempUID = new byte[DefineValue.UIDBufferSize];
                                    int PrevUIDIndex;
                                    if (serialCom.ContrDisc == 0) PrevUIDIndex = (int)serialCom.ContrDisc;
                                    else PrevUIDIndex = (int)(serialCom.ContrDisc - 1);
                                    Int16 TempValue = (Int16)(uniqIDs[(PrevUIDIndex)].UID[0] << 8 | uniqIDs[(PrevUIDIndex)].UID[1]);
                                    TempValue++;                            //Increae the UID short address


                                    uniqIDs[serialCom.ContrDisc].UID[0] = (byte)(TempValue << 8);
                                    uniqIDs[serialCom.ContrDisc].UID[1] = (byte)TempValue;
                                    for (int i = 0; i < uniqIDs[serialCom.ContrDisc].ControllerID.Length; i++)
                                    {
                                        serialCom.dataToSend.data[i] = uniqIDs[serialCom.ContrDisc].ControllerID[i];
                                        serialCom.dataToSend.len++;
                                    }
                                    serialCom.dataToSend.data[serialCom.dataToSend.len] = uniqIDs[serialCom.ContrDisc].UID[0];
                                    serialCom.dataToSend.len++;
                                    serialCom.dataToSend.data[serialCom.dataToSend.len] = uniqIDs[serialCom.ContrDisc].UID[1];
                                    serialCom.dataToSend.len++;
                                    byte[] data = PacketCreation(Message.SYNC_BYTE, Message.CMD_PING_DISCOVERY, Message.CMD_INTRO_SHORT_ADD, serialCom.dataToSend.len, serialCom.dataToSend.data, serialCom.dataToSend.crc);
                                    serialCom.ContrDisc++;          //Increase the Constrollers descovered index
                                    serialPort.Write(data, 0, data.Length);
                                    ClearSendData();
                                    serialPort.DiscardOutBuffer();
                                }
                                serialCom.status = RecieveStatus.IDLE;
                                ClearRecievedData();
                                serialPort.DiscardInBuffer();
                                // Zero the Attempt value of requesting
                                serialCom.attempt = 0;
                            }
                        }
                    }
                    else if (serialCom.status == RecieveStatus.SERIAL_ENUM_MSG_RECIEVED)
                    {
                        if (serialPort.IsOpen)
                        {
                            SerialSendDataString("Error!");
                            serialCom.status = RecieveStatus.IDLE;
                        }
                        serialCom.status = RecieveStatus.IDLE;
                        ClearRecievedData();
                        serialPort.DiscardInBuffer();
                        // Zero the Attempt value of requesting
                        serialCom.attempt = 0;
                    }
                }
            }
            catch (TimeoutException tex)
            {
                Console.WriteLine($"Timeout exception in SerialPort_DataReceived: {tex.Message}");
                // Handle timeout exception appropriately
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"IO exception in SerialPort_DataReceived: {ioex.Message}");
                // Handle IO exception appropriately
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SerialPort_DataReceived: {ex.Message}");
                // Handle other specific exceptions or log errors
                serialPort.Close();
                serialPort.Open();
            }

        }
        public byte[] PacketCreation(byte[] SYNC_BYTE, byte CMD, byte SUB_CMD, byte len, byte[] DataBuffer, byte[] CRC)
        {
            byte[] newArray = new byte[5 + len + DefineValue.MaxCRCBufferSize];
            byte[] TempByte = new byte[len];
            try
            {
                SYNC_BYTE.CopyTo(newArray, 0);
                newArray[2] = CMD;
                newArray[3] = SUB_CMD;
                newArray[4] = (byte)len;
                for (int i = 0; i < len; i++)
                    TempByte[i] = DataBuffer[i];
                TempByte.CopyTo(newArray, 5);
                CRC16Generator.GenerateCRC16(TempByte, len).CopyTo(newArray, 5 + len);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return newArray;
        }

        public int CRCValidate(byte[] DataBuffer, byte len)
        {
            byte[] TempByte = new byte[DefineValue.MaxCRCBufferSize];
            int CRCValidResult = 0;
            try
            {
                CRC16Generator.GenerateCRC16(DataBuffer, len).CopyTo(TempByte, 0);
                for (int i = 0; i < DefineValue.MaxCRCBufferSize; i++)
                {
                    if (TempByte[i] == serialCom.dataRecieved.crc[i]) CRCValidResult = 1;
                    else
                    {
                        CRCValidResult = 0;
                        break;
                    }
                }
                return CRCValidResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }
        public void ClearRecievedData()
        {
            serialCom.dataRecieved.CRCcheck = 0;
            serialCom.dataRecieved.cmd = 0;
            serialCom.dataRecieved.subCmd = 0;
            serialCom.dataRecieved.len = 0;
            Array.Clear(serialCom.dataRecieved.data, 0, DefineValue.MaxSerialBufferSize);
            Array.Clear(serialCom.dataRecieved.crc, 0, DefineValue.MaxCRCBufferSize);
        }
        public void ClearSendData()
        {
            serialCom.dataToSend.CRCcheck = 0;
            serialCom.dataToSend.cmd = 0;
            serialCom.dataToSend.subCmd = 0;
            serialCom.dataToSend.len = 0;
            Array.Clear(serialCom.dataToSend.data, 0, DefineValue.MaxSerialBufferSize);
            Array.Clear(serialCom.dataToSend.crc, 0, DefineValue.MaxCRCBufferSize);
        }
        public void AddZeroValuesToArray()
        {
            for (int i = 0; i < DefineValue.MaxSerialBufferSize; i++)
            {
                serialCom.dataRecieved.data[i] = 0;
                serialCom.dataToSend.data[i] = 0;
            }
            for (int i = 0; i < DefineValue.MaxCRCBufferSize; i++)
            {
                serialCom.dataRecieved.crc[i] = 0;
                serialCom.dataToSend.crc[i] = 0;
            }
        }
        //public void RefeshSerialPort()
        //{
        //    serialPort1.Close();
        //    serialPort1.PortName = portText;
        //    serialPort1.BaudRate = Convert.ToInt32(BuadRate);
        //    serialPort1.DataBits = Convert.ToInt32(Databit);
        //    serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Stopbit);
        //    serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), Paritybit);
        //    serialPort1.ReadTimeout = 2000;
        //    serialPort1.Open();
        //}
        public void SerialSendDataString(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            serialPort.Write(data, 0, data.Length);
            serialPort.DiscardOutBuffer();
            serialPort.DiscardInBuffer();
        }

        protected virtual void DataToBeSent(byte[] data, byte len)
        {
            byte[] bytesArray = new byte[len];
            Array.Copy(data, bytesArray, len);
            // Raise the DataReceived event with the received data
            try
            {
                if(synchronizationContext != null)
                {
                    synchronizationContext.Post(state =>
                    {
                        dataReceivedHandler?.Invoke(this, bytesArray);
                    }, null);
                }
                else
                {
                    dataReceivedHandler?.Invoke(this, bytesArray);
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }  
        }
    }
}



public struct MsgPayload
{
    public int CRCcheck;
    public byte cmd;
    public byte subCmd;
    public byte len;
    public byte[] data;    //512byte
    public byte[] crc;     //2-byte
}
public enum RecieveStatus
{
    IDLE,
    SYNC_FIRST_BYTE_RECIEVED,
    SYNC_BYTES_RECIEVED,
    SERIAL_ENUM_HEADER_RECEIVED,
    SERIAL_ENUM_MSG_RECIEVED,
    SERIAL_ENUM_CNT
}
public struct serialComHandler
{
    public int attempt;
    public RecieveStatus status;
    public MsgPayload dataToSend;
    public MsgPayload dataRecieved;
    public uint ContrDisc;

}
public struct UniqIDTable
{
    public byte[] UID;
    public byte[] ControllerID;
}

public static class Message
{
    public static readonly byte[] SYNC_BYTE = new byte[] { 0xde, 0xad };
    public const byte SUB_CMD_REQUEST = 0x01;
    public const byte SUB_CMD_RESPONSE = 0x02;
    public const byte CMD_PING_DISCOVERY = 0x50;
    public const byte CMD_INTRO_SHORT_ADD = 0x60;
}
public static class DefineValue
{
    public const int MaxMcu = 10;
    public const int MaxSerialBufferSize = 1024;
    public const int MaxCRCBufferSize = 2;
    public const int ControllerIDBufferSize = 12;
    public const int UIDBufferSize = 2;     //Also do changes in code
}


public class CRC16Generator
{
    private const ushort CRC16 = 0x8005;

    public static byte[] GenerateCRC16(byte[] data, int length)
    {
        ushort crc = 0;
        int bitsRead = 0, bitFlag;

        // Sanity check
        if (data == null || length <= 0 || length > data.Length)
            return BitConverter.GetBytes(0);

        for (int dataIndex = 0; dataIndex < length; dataIndex++)
        {
            byte currentByte = data[dataIndex];

            for (int i = 0; i < 8; i++)
            {
                bitFlag = crc >> 15;
                crc <<= 1;
                crc |= (byte)(currentByte >> bitsRead & 1);

                bitsRead++;

                if (bitsRead > 7)
                {
                    bitsRead = 0;
                }

                if (bitFlag == 1)
                {
                    crc ^= CRC16;
                }
            }
        }

        // "Push out" the last 16 bits
        for (int i = 0; i < 16; i++)
        {
            bitFlag = crc >> 15;
            crc <<= 1;

            if (bitFlag == 1)
            {
                crc ^= CRC16;
            }
        }

        // Reverse the bits
        ushort result = 0;
        int mask = 0x8000;
        int shift = 0x0001;

        for (; mask != 0; mask >>= 1, shift <<= 1)
        {
            if ((mask & crc) != 0)
            {
                result |= (ushort)shift;
            }
        }

        // Convert ushort to byte array
        byte[] crcBytes = BitConverter.GetBytes(result);

        // Ensure little-endian order
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(crcBytes);
        }

        return crcBytes;
    }
}


