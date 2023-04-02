using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using RFID_Reader_Com;
using System.Windows.Forms;

namespace RFID_Reader_Cmds
{
    /// <summary>
    /// Parse Serial Port Received Data to a Valid Packet
    /// </summary>
    public class ReceiveParser
    {
        static bool frameBeginFlag = false;
        static bool frameEndFlag = true;
        static long frameLength;   //实际接收到的数据长度
        static long strNum;      //实际接收到的帧个数
        //static long frameDataLen;  // 从数据帧从解析出来，即数据长度字段
        static string[] strBuff = new String[4096];
        /// <summary>
        /// When a valid Packet Got, this event will be sent
        /// </summary>
        public event EventHandler<StrArrEventArgs> PacketReceived;

        /// <summary>
        /// Try to Parse a Valid Packet from Serial Port Received Data
        /// It Will Send PacketReceived event When Got a Packet.
        /// </summary>
        /// <param name="sender">usually is a SerialPort.DataReceived</param>
        /// <param name="e">SerialDataReceivedEventArgs</param>
        public void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Sp.GetInstance().Closing) return;//如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环  
            if (!Sp.GetInstance().IsOpen()) return;
            int n = Sp.GetInstance().ComDevice.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
            try
            {
                Sp.GetInstance().Listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的。

                byte[] DataCom = new byte[n];//返回命令包
                Sp.GetInstance().ComDevice.Read(DataCom, 0, n);//读取数据
                string[] DataRX = new string[DataCom.Length];
                for (int i = 0; i < DataCom.Length; i++)
                {
                    DataRX[i] = DataCom[i].ToString("X2").ToUpper();
                }

                if (n != 0) //
                {
                    for (int j = 0; j < n; j++)
                    {
#if TRACE
                        //Console.Write(DataRX[j] + " ");
#endif
                        if (frameBeginFlag)
                        {
                            strBuff[strNum] = DataRX[j];
                            if (strNum == 4)
                            {
                                frameLength = 256 * Convert.ToInt32(strBuff[3], 16) + Convert.ToInt32(strBuff[4], 16);
                                if (frameLength > 3072)
                                {
#if TRACE
                                    Console.WriteLine("ERROR FRAME, frame Length too long!");
#endif
                                    frameBeginFlag = false;
                                    continue;
                                }
                            }
                            else if ((strNum == frameLength + 6) && (strBuff[strNum] == ConstCode.FRAME_END_HEX))
                            {
                                int checksum = 0;
                                for (int i = 1; i < strNum - 1; i++)
                                {
                                    checksum += Convert.ToInt32(strBuff[i], 16);
                                }
                                checksum = checksum % 256;
                                if (checksum != Convert.ToInt32(strBuff[strNum - 1], 16))
                                {
                                    Console.WriteLine("ERROR FRAME, checksum is not right!");
                                    frameBeginFlag = false;
                                    frameEndFlag = true;
                                    continue;
                                }
                                frameBeginFlag = false;
                                frameEndFlag = true;
                                if (PacketReceived != null)
                                {
                                    string[] packet = new string[strNum + 1];
                                    for (int i = 0; i <= strNum; i++)
                                    {
                                        packet[i] = strBuff[i];
                                    }
                                    PacketReceived(this, new StrArrEventArgs(packet));
                                }
                            }
                            else if ((strNum == frameLength + 6) && (strBuff[strNum] != ConstCode.FRAME_END_HEX))
                            {
                                Console.WriteLine("ERROR FRAME, cannot get FRAME_END when extends frameLength");
                                frameBeginFlag = false;
                                frameEndFlag = true;
                                continue;
                            }
                            strNum++;
                        }
                        else if (DataRX[j] == ConstCode.FRAME_BEGIN_HEX && frameBeginFlag != true)
                        {
                            strNum = 0;
                            strBuff[strNum] = DataRX[j];
                            frameBeginFlag = true;
                            frameEndFlag = false;
                            strNum = 1;
                        }
                    }
                }
            }
            finally
            {
                Sp.GetInstance().Listening = false;//我用完了，ui可以关闭串口了。
            }
        }
    }
}
