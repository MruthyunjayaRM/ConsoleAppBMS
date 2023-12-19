using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibBMS;
using BMSSerialPortLib;
using System.IO.Ports;
using System.Threading;

namespace BMSConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            
            BatteryPack BMS = new BatteryPack();
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
            "Enter choice(or return to quit): "
            };
            string[] SerialMenu =
            {
                "\nSerial Communication Setup",
                "1.Setup Serial",
                "2.Open Connection",
                "3.Start Communication",
                "4.Go To BMS Setup"
            };
            string[] BMSSetup =
            {
                "\nBMS Setup",
                "1.BMS Connection",
                "2.BMS Start"
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
                                string[] ports = SerialPort.GetPortNames();
                                string[] baudrates =
                                {
                                    "9600",
                                    "115200"
                                };
                                serialPort.PortCount = 0;
                                if (ports.Length != 0)
                                {
                                    foreach (string port in ports)
                                    {
                                        serialPort.PortCount++;
                                        Console.WriteLine("{0}." + port, serialPort.PortCount);
                                    }
                                    Console.WriteLine("Choose the port:");
                                    userMenuInput = Console.ReadLine();
                                    serialPort.portName = ports[int.Parse(userMenuInput) - 1];
                                    int count = 0;
                                    foreach (string value in baudrates)
                                    {
                                        count++;
                                        Console.WriteLine("{0}."+value,count);
                                    }
                                    count = 0;
                                    Console.WriteLine("Choose the Baudrate:");
                                    userMenuInput = Console.ReadLine();
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
                                    Console.WriteLine("Port Name:{0}\nBuadrate:{1}", serialPort.portName, serialPort.baudRate);
                                }
                                else Console.WriteLine("No Ports Available.(Connect the device)");
                            }
                            break;
                        case "2":
                            {
                                try
                                {
                                    if (serialPortManager != null)
                                    {
                                        serialPortManager.Open();
                                    }
                                    else Console.WriteLine("Setup Serial port and try again!");
                                }
                                catch (Exception ex) { Console.Write(ex.ToString()); }
                            }
                            break;
                        case "3":
                            {
                                try
                                {
                                    if (serialPortManager.isOpen())
                                    {
                                        serialPortManager.StartSerialThread();
                                        serialPortManager.dataReceivedHandler += program.BmsSerialPort_DataReceived;
                                        serialPortManager.synchronizationContext = SynchronizationContext.Current;
                                    }
                                    else Console.WriteLine("Open Serial port and try again!");
                                }
                                catch (Exception ex) { Console.Write(ex.ToString()); }
                            }
                            break;
                        case "4":
                            {
                                try
                                {
                                    if (serialPortManager.IsThreadRunning) comStatus = ComStatus.BMS_SETUP;
                                    else Console.WriteLine("There is issue in the serial port connection Please check");
                                }catch (Exception ex) { Console.Write(ex.ToString()); }
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
                        case "1": break;
                        case "2": break;
                        case "3": break;
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
                        case "1": break;
                        case "2": break;
                        case "3": break;
                        case "4": break;
                        case "5": break;
                        case "6": break;
                        case "7": break;
                        default: Console.WriteLine("Invalid Input"); break;
                    }
                }

            }
            while (userMenuInput?.Length > 0);
        }
        private void BmsSerialPort_DataReceived(object sender, byte[] data)
        {
            // Handle the received data in the UI
            // For example, update a TextBox with the received data
            Console.WriteLine($"Data received: {BitConverter.ToString(data)}");
        }
    }
}

public enum ComStatus
{
    SERIAL_COM_SETUP,
    BMS_SETUP,
    BMS_LOOP
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
