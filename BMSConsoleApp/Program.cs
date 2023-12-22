using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibBMS;
using BMSSerialPortLib;
using System.IO.Ports;
using System.Threading;
using System.Dynamic;
using System.Collections;

namespace BMSConsoleApp
{
    internal class Program
    {
        static BatteryPack[] BMS = new BatteryPack[10];
        static BmsCom BMSComStatus = new BmsCom();
        //static int BMSConnected = 0;
        static void Main(string[] args)
        {
            Program program = new Program();
            BMSSerialPort serialPortManager = new BMSSerialPort();
            SerialPortDetails serialPort = SerialPortDetails.CreateDefault();
            ComStatus comStatus;
            
            string[] BMSMenu = {
            "\nBMS Menu",
            "1.BMS Packs",
            "2.Cell Voltages",
            "3.Current",
            "4.Tempurature",
            "5.OV,UV,OC & UC",
            "6.BMS Status",
            "7.Alarms & Faults",
            "8.Exit",
            "Enter choice: "
            };
            string[] SerialMenu =
            {
                "\nSerial Communication Setup",
                "1.Setup Serial",
                "2.Open Connection",
                "3.Start Communication",
                "4.Go To BMS Setup",
                "5.Reset"
            };
            string[] BMSSetup =
            {
                "\nBMS Setup",
                "1.BMS Connect",
                "2.BMS Connected List",
                "3.BMS Data",
                "4.Exit"
            };
            string userMenuInput = string.Empty;
            comStatus = ComStatus.SERIAL_COM_SETUP;
            do
            {
                if(comStatus == ComStatus.SERIAL_COM_SETUP)
                {
                    foreach (var item in SerialMenu)
                    {
                        Console.WriteLine(item.ToString());
                    }
                    userMenuInput = Console.ReadLine();
                    switch (userMenuInput)
                    {
                        case "1":
                            {
                                if (serialPortManager.IsSerialPortSet)
                                {
                                    string[] ports = SerialPort.GetPortNames();
                                    string[] baudrates =
                                    {
                                    "9600",
                                    "115200"
                                    };
                                    serialPort.PortCount = 0;
                                    if (ports.Length != 0)
                                    {
                                        try
                                        {
                                            foreach (string port in ports)
                                            {
                                                Console.WriteLine("{0}." + port, ++serialPort.PortCount);
                                            }
                                            Console.WriteLine("Choose the port:");
                                            userMenuInput = Console.ReadLine();
                                            if(int.Parse(userMenuInput) <= serialPort.PortCount && int.Parse(userMenuInput) > 0)
                                            {
                                                serialPort.portName = ports[int.Parse(userMenuInput) - 1];
                                                int count = 0;
                                                foreach (string value in baudrates)
                                                {
                                                    count++;
                                                    Console.WriteLine("{0}." + value, count);
                                                }
                                                Console.WriteLine("Choose the Baudrate:");
                                                userMenuInput = Console.ReadLine();
                                                if(int.Parse(userMenuInput) <= count && int.Parse(userMenuInput) > 0)
                                                {
                                                    serialPort.baudRate = int.Parse(baudrates[int.Parse(userMenuInput) - 1]);
                                                    // These are kept default
                                                    serialPort.dataBits = 8;
                                                    serialPort.parity = Parity.None;
                                                    serialPort.stopBits = StopBits.One;
                                                    serialPort.readTimeout = 1000;
                                                    serialPort.writeTimeout = 1000;
                                                    serialPortManager = new BMSSerialPort(serialPort.portName,
                                                                                          serialPort.baudRate,
                                                                                          serialPort.dataBits,
                                                                                          serialPort.parity,
                                                                                          serialPort.stopBits,
                                                                                          serialPort.readTimeout,
                                                                                          serialPort.writeTimeout,
                                                                                          SynchronizationContext.Current);
                                                    Console.WriteLine("Serial Port Set Successfully");
                                                    Console.WriteLine("Port Name:{0}\nBuadrate:{1}", serialPort.portName, serialPort.baudRate);

                                                }
                                                else { Console.WriteLine("Invalid Input"); }
                                                count = 0;
                                            }
                                            else { Console.WriteLine("Invalid Input");}
                                        }catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                                    }
                                    else Console.WriteLine("No Ports Available.(Connect the device)");
                                }
                                else Console.WriteLine("Serialport Already Aet");
                            }
                            break;
                        case "2":
                            {
                                try
                                {
                                    if (serialPortManager.IsSerialPortSet)
                                    {
                                        serialPortManager.Open();
                                        Console.WriteLine("Serial Port Opened.");
                                    }
                                    else Console.WriteLine("Serial Port Not Set.");
                                }
                                catch (Exception ex) { Console.Write(ex.ToString()); }
                            }
                            break;
                        case "3":
                            {
                                try
                                {
                                    if (serialPortManager.IsOpen)
                                    {
                                        serialPortManager.SetAttempt = 0;
                                        serialPortManager.StartSerialThread();
                                        serialPortManager.dataReceivedHandler += program.BmsSerialPort_DataReceived;
                                        serialPortManager.synchronizationContext = SynchronizationContext.Current;
                                        Console.WriteLine("Communication Started.");
                                    }
                                    else Console.WriteLine("Open Serial Port and try again!");
                                }
                                catch (Exception ex) { Console.Write(ex.ToString()); }
                            }
                            break;
                        case "4":
                            {
                                try
                                {
                                    if (serialPortManager.IsThreadRunning) comStatus = ComStatus.BMS_SETUP;
                                    else Console.WriteLine("There is issue in the serial port connection Please Reset and Try Again!");
                                }catch (Exception ex) { Console.Write(ex.ToString()); }
                            }
                            break;
                        case "5":
                            {
                                serialPortManager.SetAttempt = 0;
                                serialPortManager.StopSerialThread();
                                serialPortManager.dataReceivedHandler += null;
                                serialPortManager.synchronizationContext = null;
                                serialPortManager.Close();
                                serialPortManager.Dispose();
                                program = new Program();
                                serialPortManager = new BMSSerialPort();
                                serialPort = SerialPortDetails.CreateDefault();
                                Console.WriteLine("Reset Successful.");
                            }
                            break;
                        default: Console.WriteLine("Invalid Input"); break;
                    }
                }
                if(comStatus == ComStatus.BMS_SETUP)
                {
                    foreach (var item in BMSSetup)
                    {
                        Console.WriteLine(item.ToString());
                    }
                    userMenuInput = Console.ReadLine();
                    switch (userMenuInput)
                    {
                        case "1": 
                            {
                                try
                                {
                                    int RequestCount = 0;
                                    serialPortManager.RecieveMcuIDStatusSet(true);
                                    Console.WriteLine("Waiting for BMSs to connect.......");
                                    while (serialPortManager.IsOpen && serialPortManager.IsThreadRunning && RequestCount<1)
                                    {
                                        serialPortManager.IntroductionHandler();
                                        Thread.Sleep(10000);
                                        RequestCount++;
                                    }
                                    serialPortManager.RecieveMcuIDStatusSet(false);

                                }catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                            }
                            break;
                        case "2":
                            {
                                try
                                {
                                    if(BMSComStatus.BMSConnected == 0)
                                    {
                                        Console.WriteLine($"No BMS connected.");
                                    }
                                    else
                                    {
                                        for(int i=0; i< BMSComStatus.BMSConnected; i++)
                                        {
                                            try
                                            {
                                                Console.WriteLine(value: $"BMS Number:{i + 1}, " +
                                                    $"UniqueID:{BitConverter.ToString(serialPortManager.uniqIDs[i].UID)}, " +
                                                    $"ControllerID:{BitConverter.ToString(serialPortManager.uniqIDs[i].ControllerID)}");
                                            }
                                            catch (Exception ex) { Console.WriteLine(ex); }
                                        }
                                    }
                                }
                                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                            }
                            break;
                        case "3":
                            {
                                try
                                {
                                    if (serialPortManager.IsThreadRunning) comStatus = ComStatus.BMS_LOOP;
                                    else Console.WriteLine("There is issue in the serial port connection Please Reset and Try Again!");
                                }
                                catch (Exception ex) { Console.Write(ex.ToString()); }
                                Console.Clear();
                            }
                            break;
                        case "4":
                            {
                                serialPortManager.SetAttempt = 0;
                                serialPortManager.StopSerialThread();
                                serialPortManager.dataReceivedHandler += null;
                                serialPortManager.synchronizationContext = null;
                                serialPortManager.Close();
                                serialPortManager.Dispose();
                                comStatus = ComStatus.SERIAL_COM_SETUP;
                                program = new Program();
                                serialPortManager = new BMSSerialPort();
                                serialPort = SerialPortDetails.CreateDefault();
                                BMSComStatus = new BmsCom();
                                Console.WriteLine("Exited Successfully......");
                            }
                            break;
                        default: Console.WriteLine("Invalid Input"); break;
                    }
                }
                if(comStatus == ComStatus.BMS_LOOP)
                {
                    foreach (var item in BMSMenu)
                    {
                        Console.WriteLine(item.ToString());
                    }
                    userMenuInput = Console.ReadLine();
                    switch (userMenuInput)
                    {
                        case "1":
                            {
                                try
                                {
                                    if (BMSComStatus.BMSConnected == 0)
                                    {
                                        Console.WriteLine($"No BMS connected.");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < BMSComStatus.BMSConnected; i++)
                                        {
                                            Console.WriteLine(value: $"BMS Number:{i + 1}, " +
                                                 $"UniqueID:{BitConverter.ToString(serialPortManager.uniqIDs[i].UID)}, " +
                                                 $"ControllerID:{BitConverter.ToString(serialPortManager.uniqIDs[i].ControllerID)}");
                                        }
                                    }
                                }
                                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                            }
                            break;
                        case "2":
                            {
                                try
                                {
                                    Console.WriteLine("-------------- BMS Data -----------");
                                    Console.WriteLine("-----------------------------------");
                                    int count = 0;
                                    foreach (BatteryPack BMSCurrent in BMS)
                                    {
                                        if (count < BMSComStatus.BMSConnected)
                                        {
                                            Console.WriteLine("{0},{1},{2},{3}",
                                                BitConverter.ToString(BMSCurrent.BMSPackID),
                                                BMSCurrent.CellVoltage,
                                                BMSCurrent.CellAh,
                                                BMSCurrent.CRating);
                                            for (int i = 0; i < BMSCurrent.IndCellVoltage.Length; i++)
                                                Console.Write(BMSCurrent.IndCellVoltage[i] + "V, ");
                                            Console.WriteLine();
                                        }
                                        count++;
                                    }
                                }
                                catch(Exception ex) { Console.WriteLine(ex.ToString()); }
                            }
                            break;
                        case "8":
                            {
                                comStatus = ComStatus.BMS_SETUP;
                                BMSComStatus = new BmsCom();
                                Console.WriteLine("Exited Successfully......");
                            }
                            break;
                        default: Console.WriteLine("Invalid Input"); break;
                    }
                }
            }
            while (userMenuInput?.Length > 0);
            try
            {
                serialPortManager.SetAttempt = 0;
                serialPortManager.StopSerialThread();
                serialPortManager.Close();
            }catch (Exception ex) { Console.WriteLine(ex?.ToString()+"\nCouldn't close the connection properly"); }
        }
        private void BmsSerialPort_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e == null){ //We can use if needed
            }
            // Handle the received data in the UI
            // For example, update a TextBox with the received data
            else if (e.RecieveDType == RecivedDataType.MCU_ID)
            {
                BMS[BMSComStatus.BMSConnected] = new BatteryPack(e.BMS_ID, e.ByteArray, 5, 12000, 10);
                BMSComStatus.BMSConnected++;
                Console.WriteLine($"Data received: {BitConverter.ToString(e.ByteArray)} MCU ID:{BitConverter.ToString(e.BMS_ID)}");
            }
            if(e.RecieveDType == RecivedDataType.CELLS_VOLTAGE_RECIEVED)
            {
                byte[] cellvoltages = e.ByteArray.Skip(2).ToArray();
                BMS[e.BMSIndex].IndCellVoltage = ByteArrayConverter.ConvertToFloatArray(cellvoltages);
                Console.WriteLine("{0},{1},{2},{3}",
                    BitConverter.ToString(BMS[e.BMSIndex].BMSPackID),
                    BMS[e.BMSIndex].CellVoltage,
                    BMS[e.BMSIndex].CellAh,
                    BMS[e.BMSIndex].CRating);
                for (int i = 0; i < BMS[e.BMSIndex].IndCellVoltage.Length; i++)
                    Console.Write($"Cell {i} : {BMS[e.BMSIndex].IndCellVoltage[i]} V,");
                Console.WriteLine();
            }
        }

    }

}
public static class ByteArrayConverter
{
    public static float[] ConvertToFloatArray(byte[] byteArray)
    {
        // Check if the length of the byte array is a multiple of 4
        if (byteArray.Length % 4 != 0)
        {
            throw new ArgumentException("The length of the byte array must be a multiple of 4.");
        }

        // Create a float array to store the converted values
        float[] floatArray = new float[byteArray.Length / 4];

        // Convert each set of 4 bytes to a float and store in the float array
        for (int i = 0; i < floatArray.Length; i++)
        {
            floatArray[i] = BitConverter.ToSingle(byteArray, i * 4);
        }
        return floatArray;
    }
    public static byte[] ConvertToByteArray(float[] floatArray)
    {
        // Create a byte array to store the converted values
        byte[] byteArray = new byte[floatArray.Length * 4];

        // Convert each float to a set of 4 bytes and store in the byte array
        for (int i = 0; i < floatArray.Length; i++)
        {
            byte[] tempBytes = BitConverter.GetBytes(floatArray[i]);
            Array.Copy(tempBytes, 0, byteArray, i * 4, 4);
        }
        return byteArray;
    }
}

public enum ComStatus
{
    SERIAL_COM_SETUP,
    BMS_SETUP,
    BMS_LOOP
}

public struct BmsCom
{
    public int BMSConnected;
}

public struct SerialPortDetails
{
    public int PortCount;
    public string portName;
    public int baudRate;
    public int dataBits;
    public Parity parity;
    public StopBits stopBits;
    public int readTimeout;
    public int writeTimeout;
    // A static method to create an instance with default values
    public static SerialPortDetails CreateDefault()
    {
        return new SerialPortDetails
        {
            PortCount = 0,
            portName = null, // Assuming you want to set string to null
            baudRate = 9600, // Default baud rate, you can change it to your preferred value
            dataBits = 8,
            parity = Parity.None,
            stopBits = StopBits.One,
            readTimeout = 1000, // Set to your preferred value
            writeTimeout = 1000 // Set to your preferred value
        };
    }
}


