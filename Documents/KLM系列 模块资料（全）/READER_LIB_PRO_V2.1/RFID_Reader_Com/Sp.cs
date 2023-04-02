using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace RFID_Reader_Com
{
    /// <summary>
    /// Serial Port Class.
    /// </summary>
    public class Sp
    {
        /// <summary>
        /// A Instance Of SerialPort Class.
        /// </summary>
        public SerialPort ComDevice = new SerialPort();

        private static readonly Sp instance = new Sp();

        private int communicatBaudrate = 115200;

        /// <summary>
        /// Indicate if the Serial Port is doing some invoked operation
        /// </summary>
        public bool Listening = false; //是否没有执行完invoke相关操作  
        /// <summary>
        /// Is the Port Closing
        /// </summary>
        public bool Closing = false;   //是否正在关闭串口，执行Application.DoEvents，并阻止再次invoke

        /// <summary>
        /// When a valid Data Sent, this event will be sent.
        /// It has a Data Property Contains the Send Byte[].  
        /// </summary>
        public event EventHandler<byteArrEventArgs> DataSent;

        private Sp()
        {
            ComDevice.PortName = "COM1";
            ComDevice.BaudRate = 9600;
            ComDevice.Parity = Parity.None;
            ComDevice.DataBits = 8;
            ComDevice.StopBits = StopBits.One;
            ComDevice.NewLine = "/r/n";
            //ComDevice.DataReceived += new SerialDataReceivedEventHandler(ComDataReceived);
        }

        /// <summary>
        /// Return a Single Instance of Sp.
        /// </summary>
        /// <returns>Instance of Sp</returns>
        public static Sp GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Get all valid Serial Ports Names in System
        /// </summary>
        /// <returns>all valid Serial Ports Names</returns>
        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Config the Serial Port
        /// </summary>
        /// <param name="port">Port Name(COM1,COM2...)</param>
        /// <param name="baudrate">Baudrate</param>
        /// <param name="p">Parity</param>
        /// <param name="databits">databits(8,9,10)</param>
        /// <param name="s">StopBits</param>
        public void Config(string port, int baudrate,Parity p, int databits, StopBits s)
        {
            ComDevice.PortName = port;
            ComDevice.BaudRate = baudrate;
            ComDevice.Parity = p;
            ComDevice.DataBits = 8;
            ComDevice.StopBits = s;
        }

        /// <summary>
        /// Open the Serial Port
        /// </summary>
        /// <returns>return false if Open Failed, else true</returns>
        public bool Open()
        {
            if (ComDevice.IsOpen)
            {
                return true;
            }
            try
            {
                ComDevice.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Open Port Fail, " + ex.Message);
                return false;
            }
#if TRACE
            Console.WriteLine("Port Opened, ");
#endif
            return true;
        }

        /// <summary>
        /// Close Serial Port
        /// </summary>
        /// <returns>return false if Close Failed, else true</returns>
        public bool Close()
        {
            Closing = true;
            if (!ComDevice.IsOpen)
            {
                Closing = false;
                return true;
            }
            try
            {
                //while serial port is receiving data, wait it finished
                while (Listening) Application.DoEvents();
                ComDevice.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Close Port Fail, " + ex.Message);
                Closing = false;
                return false;
            }
            Closing = false;
#if TRACE
            Console.WriteLine("Closed Port, ");
#endif
            return true;
        }

        /// <summary>
        /// Return if the Serial Port Opened
        /// </summary>
        /// <returns>true if Serial Port Opened, else false</returns>
        public bool IsOpen()
        {
            return ComDevice.IsOpen;
        }

        /// <summary>
        /// Set the SerialPort BaudRate. This BaudRate will take effect after the 
        /// handshake.
        /// </summary>
        /// <param name="baudrate">baudrate</param>
        public void SetCommunicatBaudRate(int baudrate)
        {
            this.communicatBaudrate = baudrate;
        }

        /// <summary>
        /// Get the Serial Port BaudRate
        /// </summary>
        /// <returns>the BaudRate Currently in use</returns>
        public int GetCommunicateBaudRate()
        {
            return this.communicatBaudrate;
        }

        private void ComDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //if (DataReceived != null)
            //{
            //    DataReceived(sender, )
            //}
        }

        /// <summary>
        /// Send Hex Text
        /// </summary>
        /// <param name="hexText">hexadecimal string</param>
        /// <returns>Bytes Number has Sent</returns>
        public int Send(string hexText)
        {
            if (hexText.Length > 0)
            {
                if (ComDevice.IsOpen == true)
                {
                    byte[] SendBytes = null;
                    string SendData = hexText;
                    try
                    {
                        //剔除所有空格
                        SendData = SendData.Replace(" ", "");
                        if (SendData.Length % 2 == 1)
                        {//奇数个字符
                            SendData = SendData.Remove(SendData.Length - 1, 1);//去除末位字符
                        }
                        List<string> SendDataList = new List<string>();
                        for (int i = 0; i < SendData.Length; i = i + 2)
                        {
                            SendDataList.Add(SendData.Substring(i, 2));
                        }
                        SendBytes = new byte[SendDataList.Count];
                        for (int j = 0; j < SendBytes.Length; j++)
                        {
                            SendBytes[j] = (byte)(Convert.ToInt32(SendDataList[j], 16));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Please Use HEX words!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return 0;
                    }
                    ComDevice.Write(SendBytes, 0, SendBytes.Length);//发送数据
                    if (DataSent != null)
                    {
                        DataSent(this, new byteArrEventArgs(SendBytes));
                    }
                    return SendBytes.Length;
                }
                else
                {
                    MessageBox.Show("Please Connect Serial Port!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return 0;
                }
            }
            else
            {
                MessageBox.Show("Please Write Send Data!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
        }

        /// <summary>
        /// Download the Firmware of the RFID Chip
        /// </summary>
        /// <param name="fileName">Firware File Path</param>
        public void DownLoadFW(string fileName)
        {
            if (ComDevice.IsOpen == true)
            {
                Send("FE");
                Thread.Sleep(10);

                switch (this.communicatBaudrate)
                {
                    case 115200 :
                        Send("B5");
                        break;
                    case 57600 :
                        Send("B4");
                        break;
                    case 38400 :
                        Send("B3");
                        break;
                    case 28800 :
                        Send("B2");
                        break;
                    case 19200 :
                        Send("B1");
                        break;
                    case 9600 :
                        Send("B0");
                        break;
                    default :
                        Send("B5");
                        break;
                }
                Thread.Sleep(10);

                ComDevice.DiscardInBuffer();
                ComDevice.DiscardOutBuffer();

                Closing = true;
                while (Listening) Application.DoEvents();
                //打开时点击，则关闭串口  
                ComDevice.Close();
                Closing = false;

                ComDevice.BaudRate = this.communicatBaudrate;
                ComDevice.Open();

                //Send FW by Serial Port
                Send("DB");
                Thread.Sleep(10);
                //Begin Download FW handshake
                Send("FD");
                Thread.Sleep(10);

                FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader sr = new BinaryReader(file);
                int count = (int)file.Length;
                byte[] buffer = new byte[count];
                sr.Read(buffer, 0, buffer.Length);
                ComDevice.Write(buffer, 0, buffer.Length);
                //Thread.Sleep(100);

                Send("D3 D3 D3 D3 D3 D3");
                Thread.Sleep(10);

                sr.Close();
                file.Close();
                // send command to initialize some parameters
                //Send("BB 00 EC 00 00 EC 7E");
            }
            else
            {
                MessageBox.Show("Please Open COM Port First!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

}
