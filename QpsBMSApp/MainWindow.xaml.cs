using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BMSSerialPortLib;
using LibBMS;


namespace QpsBMSApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static BatteryPack[] BMS = new BatteryPack[definevalue.MAXBMS];
        static BmsCom BMSComStatus = new BmsCom();
        BMSSerialPort serialPortManager = new BMSSerialPort();
        SerialPortDetails serialPort = SerialPortDetails.CreateDefault();
        ComStatus comStatus;

        public MainWindow()
        {
            InitializeComponent();
            Initialization();
        }
        private void Initialization()
        {
            InitPorts();
        }
        private void InitPorts()
        {
            ComboBoxPorts.Items.Clear();
            string[] portNames = SerialPort.GetPortNames();
            // Populate the ComboBox with the port names
            foreach (string portName in portNames)
            {
                ComboBoxPorts.Items.Add(portName);
            }
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            InitPorts();
        }
        private void ButtonOpenSerialCom_Click(object sender, RoutedEventArgs e)
        {
            if (((String)ButtonOpenSerialCom.Content) == "Open")
            {
                try
                {
                    //BMSSerialPort serialPortManager = new BMSSerialPort(comboBoxComPort.Text, int.Parse(comboBoxSpeed.Text), int.Parse(comboBoxDataBit.Text), Parity.None, StopBits.One, 1000, 1000);
                    serialPortManager = new BMSSerialPort(ComboBoxPorts.Text, 9600, 8, Parity.None, StopBits.One, 1000, 1000, SynchronizationContext.Current);
                    bool SerialStatus = serialPortManager.Open();
                    if(SerialStatus) 
                    {
                        MessageBox.Show(ComboBoxPorts.Text+" Port opened Successfully","Success", MessageBoxButton.OK);
                        ButtonOpenSerialCom.Content = "Close";
                        //serialPortManager.dataReceivedHandler += BmsSerialPort_DataReceived;
                    }
                    else
                    {
                        MessageBox.Show(ComboBoxPorts.Text+" Port Not Opened Successflly", "Issue Occured", MessageBoxButton.OK);
                        InitPorts();
                    }
                }

                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                    ComboBoxPorts.Text = "";
                }
            }
            else if (((String)ButtonOpenSerialCom.Content) == "Close")
            {
                try
                {
                    bool SerialStatus = serialPortManager.Close();
                    if(SerialStatus)
                    {
                        MessageBox.Show(ComboBoxPorts.Text + " Port Closed Successfully", "Success", MessageBoxButton.OK);
                        ButtonOpenSerialCom.Content = "Open";
                    }
                    else
                    {
                        MessageBox.Show(ComboBoxPorts.Text + " Port Not Closed Successflly", "Issue Occured", MessageBoxButton.OK);
                        ButtonOpenSerialCom.Content = "Open";
                        InitPorts();
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ComboBoxPorts.Text = "";
                }
            }
        }

        private void Cell16Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
public static class definevalue
{
    public const int MAXBMS = 10;
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