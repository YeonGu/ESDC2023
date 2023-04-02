#define  _FEATURE_NXP_TAG_CMDS_

using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Reader_Cmds
{
    /// <summary>
    /// This Class defines some utility to build packet according to RFID Reader Serial Port 
    /// Communication Protocol. 
    /// </summary>
    public class Commands
    {
        /// <summary>
        /// Calculate the checksum of data. It will add all hexadecimal number of data,
        /// and only uses the LSB.
        /// </summary>
        /// <param name="data">data should be hexadecimal format</param>
        /// <returns>checksum of data, described in hexadecimal string</returns>
        public static string CalcCheckSum(string data)
        {
            if (data == null)
            {
                return "";
            }
            int checksum = 0;
            string dataNoSpace = data.Replace(" ", ""); // remove all spaces
            try
            {
                for (int j = 0; j < dataNoSpace.Length; j += 2)
                {
                    checksum += Convert.ToInt32(dataNoSpace.Substring(j, 2), 16);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("do checksum error" + ex);
            }

            checksum = checksum % 256;
            return checksum.ToString("X2");
        }
        /// <summary>
        /// Build a whole frame
        /// </summary>
        /// <param name="data">this should not include checksum </param>
        /// <returns>Whole frame. Frame header, ender and checksum will be added
        /// automatically. It will insert a space between every two chars. </returns>
        public static string BuildFrame(string data)
        {
            if (data == null)
            {
                return "";
            }
            string frame = data.Replace(" ", "");

            string checkSum = CalcCheckSum(frame);

            frame = ConstCode.FRAME_BEGIN_HEX + frame + checkSum + ConstCode.FRAME_END_HEX;
            return frame;
        }

        /// <summary>
        /// Build a Whole Frame that Doesn't Have Data Field
        /// </summary>
        /// <param name="msgType">Packet Type</param>
        /// <param name="cmdCode">Command Code</param>
        /// <returns>Whole frame. Frame header, ender and checksum will be added
        /// automatically. It will insert a space between every two chars. </returns>
        public static string BuildFrame(string msgType, string cmdCode)
        {
            if (msgType == null || cmdCode == null)
            {
                return "";
            }
            msgType = msgType.Replace(" ", "");
            if (msgType.Length == 1)
            {
                msgType = "0" + msgType;
            }
            cmdCode = cmdCode.Replace(" ", "");
            if (cmdCode.Length == 1)
            {
                cmdCode = "0" + cmdCode;
            }
            string frame = msgType + cmdCode + "00" + "00";
            frame = ConstCode.FRAME_BEGIN_HEX + frame + cmdCode + ConstCode.FRAME_END_HEX;
            return AutoAddSpace(frame);
        }

        /// <summary>
        /// Build a whole frame
        /// </summary>
        /// <param name="msgType">Packet Type</param>
        /// <param name="cmdCode">Command Code</param>
        /// <param name="data">Data field</param>
        /// <returns>Whole frame. Frame header, ender and checksum will be added
        /// automatically. It will insert a space between every two chars. </returns>
        public static string BuildFrame(string msgType, string cmdCode, string data)
        {
            if (msgType == null || cmdCode == null)
            {
                return "";
            }
            msgType = msgType.Replace(" ","");
            if (msgType.Length == 1)
            {
                msgType = "0" + msgType;
            }
            cmdCode = cmdCode.Replace(" ","");
            if (cmdCode.Length == 1)
            {
                cmdCode = "0" + cmdCode;
            }
            int dataHexLen = 0;
            if (data != null)
            {
                data = data.Replace(" ", "");
                if (data.Length == 1)
                {
                    data = "0" + data;
                }
                dataHexLen = data.Length / 2;
                // if data.Length is odd, the last char of data will be discard
                data = data.Substring(0, dataHexLen * 2);
            }

            string frame = msgType + cmdCode + dataHexLen.ToString("X4") + data;
            string checkSum = CalcCheckSum(frame);

            frame = ConstCode.FRAME_BEGIN_HEX + frame + checkSum + ConstCode.FRAME_END_HEX;
            frame = AutoAddSpace(frame);
            return frame;
        }

        /// <summary>
        /// Build a whole frame
        /// </summary>
        /// <param name="msgType">Packet Type</param>
        /// <param name="cmdCode">Command Code</param>
        /// <param name="dataArr">Data field</param>
        /// <returns>Whole frame. Frame header, ender and checksum will be added
        /// automatically. It will insert a space between every two chars.</returns>
        public static string BuildFrame(string msgType, string cmdCode, string[] dataArr)
        {
            if (msgType == null || cmdCode == null)
            {
                return "";
            }
            msgType = msgType.Replace(" ", "");
            if (msgType.Length == 1)
            {
                msgType = "0" + msgType;
            }
            cmdCode = cmdCode.Replace(" ", "");
            if (cmdCode.Length == 1)
            {
                cmdCode = "0" + cmdCode;
            }

            int dataHexLen = 0;
            if (dataArr != null)
            {
                dataHexLen = dataArr.Length;
            }

            string frame = ConstCode.FRAME_BEGIN_HEX + msgType + cmdCode + dataHexLen.ToString("X4");

            int checksum = 0;
            checksum += ConstCode.FRAME_BEGIN_BYTE + ConstCode.FRAME_END_BYTE;
            try
            {
                for (int i = 0; i < dataHexLen; i++)
                {
                    dataArr[i] = dataArr[i].Replace(" ", "");
                    if (dataArr[i].Length == 1)
                    {
                        frame += "0" + dataArr[i];
                    }
                    else
                    {
                        frame += dataArr[i];
                    }
                    checksum += Convert.ToInt32(dataArr[i], 16);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("convert error " + ex);
            }
            frame = frame + (checksum % 256).ToString("X2") + ConstCode.FRAME_END_HEX;
            return frame;
        }

        /// <summary>
        /// Insert a Space Between Every Two Chars
        /// </summary>
        /// <param name="Str">String that has no spaces</param>
        /// <returns>String inserted spaces</returns>
        public static string AutoAddSpace(string Str)
        {
            String StrDone = string.Empty;
            if (Str == null || Str.Length == 0)
            {
                return StrDone;
            }
            int i;
            for (i = 0; i <= (Str.Length - 2); i = i + 2)
            {
                StrDone = StrDone + Str[i] + Str[i + 1] + " ";
            }
            if (Str.Length % 2 != 0)
            {
                StrDone = StrDone + "0" + Str[i].ToString();
            }
            return StrDone;
        }

        /// <summary>
        /// Build Get Module Information Command Packet
        /// </summary>
        /// <param name="infoType">Information Type(Hardware version, Software version or Manufacture Information</param>
        /// <returns>Get Module Information Command Packet</returns>
        public static string BuildGetModuleInfoFrame(string infoType)
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_GET_MODULE_INFO, infoType);
        }
        /// <summary>
        /// Build set region Packet
        /// </summary>
        /// <param name="region">Region Code</param>
        /// <returns>Set Region Packet</returns>
        public static string BuildSetRegionFrame(string region)
        {
            if (region == null || region.Length == 0)
            {
                return "";
            }
            string frame = BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_REGION, region);
            return frame;
        }

        /// <summary>
        /// Build Set RF Channel Packet
        /// </summary>
        /// <param name="ch">Channel Number</param>
        /// <returns>Set RF Channel Packet</returns>
        public static string BuildSetRfChannelFrame(string ch)
        {
            if (ch == null || ch.Length == 0)
            {
                return "";
            }
            string frame = BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_RF_CHANNEL, ch);
            return frame;
        }

        /// <summary>
        /// Build Get RF Channel Packet
        /// </summary>
        /// <returns>Get RF Channel Packet</returns>
        public static string BuildGetRfChannelFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_GET_RF_CHANNEL);
        }

        /// <summary>
        /// Build Set HFSS On/Off Packet
        /// </summary>
        /// <param name="OnOff">SET_ON/SET_OFF</param>
        /// <returns>Set HFSS On/Off Packet</returns>
        public static string BuildSetFhssFrame(string OnOff)
        {
            if (OnOff == null || OnOff.Replace(" ", "").Length == 0)
            {
                return "";
            }
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_FHSS, OnOff);
        }

        /// <summary>
        /// Build Set PA Power Packet
        /// </summary>
        /// <param name="powerdBm">dBm * 100, eg. 7.5dBm = 750</param>
        /// <returns>Set PA Power Packet</returns>
        public static string BuildSetPaPowerFrame(Int16 powerdBm)
        {
            string strPower = powerdBm.ToString("X4");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_POWER, strPower);
        }

        /// <summary>
        /// Build Get PA Power Packet
        /// </summary>
        /// <returns>Get PA Power Packet</returns>
        public static string BuildGetPaPowerFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_GET_POWER);
        }

        /// <summary>
        /// Build Set CW On/Off Packet
        /// </summary>
        /// <param name="OnOff">On("FF")/Off("00")</param>
        /// <returns>Set CW On/Off Packet</returns>
        public static string BuildSetCWFrame(string OnOff)
        {
            if (OnOff == null || OnOff.Replace(" ", "").Length == 0)
            {
                return "";
            }
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_CW, OnOff);
        }

        /// <summary>
        /// Build Read Single Tag ID Packet
        /// </summary>
        /// <returns>Read Single Tag ID Packet</returns>
        public static string BuildReadSingleFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_INVENTORY);
        }

        /// <summary>
        /// Build Read Multi Tag ID Packet
        /// </summary>
        /// <param name="loopNum">Loop Number</param>
        /// <returns>Read Multi Tag ID Packet</returns>
        public static string BuildReadMultiFrame(int loopNum)
        {
            if (loopNum <= 0 || loopNum > 65536)
            {
                return "";
            }
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_READ_MULTI, "22" + loopNum.ToString("X4"));
        }

        /// <summary>
        /// Build Stop Read Multi TAG ID Packet
        /// </summary>
        /// <returns>Stop Read Multi TAG ID Packet</returns>
        public static string BuildStopReadFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_STOP_MULTI);
        }

        /// <summary>
        /// Build Set Query Parameter Packet
        /// </summary>
        /// <param name="dr">DR(1 bit)     DR=8(0)、DR=64/3(1)</param>
        /// <param name="m">M(2 bit)      M=1(00)、M=2(01)、M=4(10)、M=8(11)</param>
        /// <param name="TRext">TRext(1 bit)   no pilot tone(0)、use pilot tone(1)</param>
        /// <param name="sel">Sel(2 bit)     ALL(00 01)、~SL(10)、SL(11)</param>
        /// <param name="session">Session(2 bit)  S0(00)、S1(01)、S2(10)、S3(11)</param>
        /// <param name="target">Target(1 bit)   A(0)、B(1)</param>
        /// <param name="q">Q(4 bit)       0-15</param>
        /// <returns>Set Query Parameter Packet</returns>
        public static string BuildSetQueryFrame(int dr, int m, int TRext, int sel, int session, int target,int q)
        {
            // msb contains DR, M, TRext, Sel, Session
            int msb = (dr << 7) | (m << 5) | (TRext << 4) | (sel << 2) | (session);
            // lsb contains Target, Q
            int lsb = (target << 7) | (q << 3);
            string dataField = msb.ToString("X2") + lsb.ToString("X2");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_QUERY, dataField);
        }

        /// <summary>
        /// Build Get Query Parameter Packet
        /// </summary>
        /// <returns>Get Query Parameter Packet</returns>
        public static string BuildGetQueryFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_GET_QUERY);
        }

        /// <summary>
        /// Build Set Select Parameter Packet
        /// </summary>
        /// <param name="target">Target(3 bit)     S0(000)、S1(001)、S2(010)、S3(011)、SL(100)</param>
        /// <param name="action">Action(3 bit)    Reference to ISO18000-6C</param>
        /// <param name="memBank">Memory bank(2 bit)    RFU(00)、EPC(01)、TID(10)、USR(11)</param>
        /// <param name="pointer">Pointer(32 bit)     Start Address</param>
        /// <param name="len">Length(8 bit)  </param>
        /// <param name="mask">Mask(0-255bit)   Mask Data according to Length</param>
        /// <param name="truncated">Truncate(1 bit)   Disable(0)、Enable(1)</param>
        /// <returns>Set Select Parameter Packet</returns>
        public static string BuildSetSelectFrame(int target, int action, int memBank, int pointer, int len,  int truncated, string mask)
        {
            string dataField = string.Empty;
            // this contains target, action, memBank
            int combByte = (target << 5) | (action << 2) | memBank;
            dataField = combByte.ToString("X2");
            dataField += pointer.ToString("X8") + len.ToString("X2");
            if (truncated == 0x80 || truncated == 0x01)
            {
                dataField += "80";  
            }
            else
            {
                dataField += "00";
            }
            //dataField += mask.Substring(0, (int)Math.Ceiling(len / 8.0));
            dataField += mask.Replace(" ","");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_SELECT_PARA, dataField);
        }

        /// <summary>
        /// Build Get Select Parameter Packet
        /// </summary>
        /// <returns>Get Select Parameter Packet</returns>
        public static string BuildGetSelectFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_GET_SELECT_PARA);
        }

        /// <summary>
        /// Build Set Inventory Mode Frame.
        /// When set to Mode0, RFID Reader will first send Select command, then begin an
        /// Inventory Round. If set to Mode1, it will not. If set to Mode2, it will send Select
        /// before all tag operation except single Inventory.
        /// </summary>
        /// <param name="mode">INVENTORY_MODE0("00")/INVENTORY_MODE1("01")/INVENTORY_MODE2("02")</param>
        /// <returns>Set Inventory Mode Frame</returns>
        public static string BuildSetInventoryModeFrame(string mode)
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_INVENTORY_MODE, mode);
        }

        /// <summary>
        /// Build Read Tag Data Packet.
        /// </summary>
        /// <param name="accessPwd">Access Password. If a tag NOT needs Access, it should be "00 00 00 00".</param>
        /// <param name="memBank">Memory Bank, 0x00-RFU,0x01-EPC,0x02-TID,0x03-User</param>
        /// <param name="sa">Start Address</param>
        /// <param name="dl">Data Length in Words</param>
        /// <returns>Read Tag Data Packet.</returns>
        public static string BuildReadDataFrame(string accessPwd, int memBank, int sa, int dl)
        {
            if (accessPwd.Replace(" ", "").Length != 8)
            {
                return "";
            }
            string dataField = accessPwd.Replace(" ", "");
            dataField += memBank.ToString("X2") + sa.ToString("X4") + dl.ToString("X4");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_READ_DATA, dataField);
        }

        /// <summary>
        /// Build Write Tag Data Packet
        /// </summary>
        /// <param name="accessPwd">Access Password. If a tag NOT needs Access, it should be "00 00 00 00".</param>
        /// <param name="memBank">Memory Bank, 0x00-RFU,0x01-EPC,0x02-TID,0x03-User</param>
        /// <param name="sa">Start Address</param>
        /// <param name="dl">Data Length in word</param>
        /// <param name="dt">Data to Write. It Should Be "dl" Words</param>
        /// <returns>Write Tag Data Packet</returns>
        public static string BuildWriteDataFrame(string accessPwd, int memBank, int sa, int dl, string dt)
        {
            if (accessPwd.Replace(" ", "").Length != 8)
            {
                return "";
            }
            string dataField = accessPwd.Replace(" ", "");
            dataField += memBank.ToString("X2") + sa.ToString("X4") + dl.ToString("X4") + dt.Replace(" ", "");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_WRITE_DATA, dataField);
        }

        /// <summary>
        /// lock payload type, 20 bits in total, high 4 bits are reserved.
        /// user can joint the 3 bytes to generate parameter ld for BuildLockFrame function. 
        /// </summary>
        public struct lock_payload_type
        {
            /// <summary>
            /// byte0 of lock payload, high 4 bits are reserved.
            /// </summary>
            public byte byte0;  // high 4 bits are reserved
            /// <summary>
            /// byte1 of lock payload. It is middle 8 bits.
            /// </summary>
            public byte byte1;
            /// <summary>
            /// byte2 of lock payload. It is the lowest 8 bits .
            /// </summary>
            public byte byte2;
        };

        /// <summary>
        /// Generate the lock payload.
        /// </summary>
        /// <param name="lockOpt">Value from 0 to 3 means 0:unlock, 1:lock, 2:param unlock, 3:perma lock</param>
        /// <param name="memSpace">Value from 0 to 4 means 0:Kill password, 1:Access password, 2:EPC memBank, 3:TID memBank, 4:User memBank</param>
        /// <returns>lock payload</returns>
        public static lock_payload_type genLockPayload(byte lockOpt, byte memSpace)
        {
            lock_payload_type payload;

            payload.byte0 = 0;
            payload.byte1 = 0;
            payload.byte2 = 0;

            /********
             * 			 Kill passwd , access passwd , ecp mem , TID mem , User mem
             * mask		|   0     1		 0	   1	   0   1	 0   1     0    1
             *     		| <--         byte0        --> <----             byte1
             * action 	|	0     1		 0	   1	   0   1	 0   1     0    1
             *          | --------->  <-----              byte2            ----->
             */

            switch (memSpace)
            {
            case 0 : //kill password
                if (lockOpt == 0) // unlock
                {
                    payload.byte0 |= 0x08;
                    payload.byte1 |= 0x00;
                }
                else if (lockOpt == 1) // lock
                {
                    payload.byte0 |= 0x08;
                    payload.byte1 |= 0x02;
                }
                else if (lockOpt == 2) //perma unlock
                {
                    payload.byte0 |= 0x0C;
                    payload.byte1 |= 0x01;
                }
                else if (lockOpt == 3) //perma lock
                {
                    payload.byte0 |= 0x0C;
                    payload.byte1 |= 0x03;
                }
                break;
            case 1 : //access password
                if (lockOpt == 0) // unlock
                {
                    payload.byte0 |= (0x08 >> 2);
                    payload.byte2 |= 0x00;
                }
                else if (lockOpt == 1) // lock
                {
                    payload.byte0 |= (0x08 >> 2);
                    payload.byte2 |= 0x80;
                }
                else if (lockOpt == 2) //perma unlock
                {
                    payload.byte0 |= (0x0C >> 2);
                    payload.byte2 |= 0x40;
                }
                else if (lockOpt == 3) //perma lock
                {
                    payload.byte0 |= (0x0C >> 2);
                    payload.byte2 |= 0xC0;
                }
                break;
            case 2 : // epc mem
                if (lockOpt == 0) // unlock
                {
                    payload.byte1 |= 0x80;
                    payload.byte2 |= 0x00;
                }
                else if (lockOpt == 1) // lock
                {
                    payload.byte1 |= 0x80;
                    payload.byte2 |= 0x20;
                }
                else if (lockOpt == 2) //perma unlock
                {
                    payload.byte1 |= 0xC0;
                    payload.byte2 |= 0x10;
                }
                else if (lockOpt == 3) //perma lock
                {
                    payload.byte1 |= 0xC0;
                    payload.byte2 |= 0x30;
                }
                break;
            case 3 : // TID mem
                if (lockOpt == 0) // unlock
                {
                    payload.byte1 |= (0x80 >> 2);
                   payload.byte2 |= 0x00;
                }
               else if (lockOpt == 1) // lock
                {
                    payload.byte1 |= (0x80 >> 2);
                    payload.byte2 |= 0x08;
                }
                else if (lockOpt == 2) //perma unlock
                {
                    payload.byte1 |= (0xC0 >> 2);
                    payload.byte2 |= 0x04;
                }
                else if (lockOpt == 3) //perma lock
                {
                    payload.byte1 |= (0xC0 >> 2);
                    payload.byte2 |= 0x0C;
                }
                break;
            case 4 : // User mem
                if (lockOpt == 0) // unlock
                {
                    payload.byte1 |= 0x08;
                    payload.byte2 |= 0x00;
                }
                else if (lockOpt == 1) // lock
                {
                    payload.byte1 |= 0x08;
                    payload.byte2 |= 0x02;
                }
                else if (lockOpt == 2) //perma unlock
                {
                    payload.byte1 |= 0x0C;
                    payload.byte2 |= 0x01;
                }
                else if (lockOpt == 3) //perma lock
                {
                    payload.byte1 |= 0x0C;
                    payload.byte2 |= 0x03;
                }
                break;

            default :
                break;
            }

            return payload;
        }

        /// <summary>
        /// Build Lock Tag Packet
        /// </summary>
        /// <param name="accessPwd">Access Password. If a tag NOT needs Access, it should be "00 00 00 00".</param>
        /// <param name="ld">Lock Payload.It should be 3 bytes, the first 4 bits are not used</param>
        /// <returns>Lock Tag Packet</returns>
        public static string BuildLockFrame(string accessPwd, int ld)
        {
             accessPwd = accessPwd.Replace(" ", "");
             if (accessPwd.Length != 8)
             {
                 return "";
             }
             string dataField = accessPwd.Replace(" ","");
             dataField += ld.ToString("X6");
             return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_LOCK_UNLOCK, dataField);
        }


        /// <summary>
        /// Build Kill Tag Packet
        /// </summary>
        /// <param name="killPwd">Kill Password. If Kill Password is "00 00 00 00", the Kill Operation Will Be ignored By Tag</param>
        /// <param name="rfu">RFU(000)/Recomm. If you want to kill a tag, just pass 0 by default.</param>
        /// <returns>Kill Tag Packet</returns>
        public static string BuildKillFrame(string killPwd, int rfu =0 )
        {
            killPwd = killPwd.Replace(" ", "");
            string dataField = killPwd;
            if (rfu != 0)
            {
                try
                {
                    dataField += rfu.ToString("X2");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Convert RFU Error! " + ex.Message);
                }

            }
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_KILL, dataField);
        }

        /// <summary>
        /// Build Set Modem Parameter Packet
        /// </summary>
        /// <param name="mixerGain">mixer gain(0-7)</param>
        /// <param name="IFAmpGain">IF gain(0-7)</param>
        /// <param name="signalThreshold">decode threshold</param>
        /// <returns>Set Modem Parameter Packet</returns>
        public static string BuildSetModemParaFrame(int mixerGain, int IFAmpGain, int signalThreshold)
        {
            string dataField = mixerGain.ToString("X2") + IFAmpGain.ToString("X2") + signalThreshold.ToString("X4");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_MODEM_PARA, dataField);
        }
        /// <summary>
        /// Build Read Modem Parameter Packet
        /// </summary>
        /// <returns>Read Modem Parameter Packet</returns>
        public static string BuildReadModemParaFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_READ_MODEM_PARA);
        }
        /// <summary>
        /// Build Scan Jammer Command Packet
        /// </summary>
        /// <returns>Scan Jammer Command Packet</returns>
        public static string BuildScanJammerFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SCAN_JAMMER);
        }
        /// <summary>
        /// Build Scan RSSI Command Packet
        /// </summary>
        /// <returns>Scan RSSI Command Packet</returns>
        public static string BuildScanRssiFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SCAN_RSSI);
        }
        /// <summary>
        /// Build IO Control Command Packet
        /// </summary>
        /// <param name="optType">operation type. 0x00：set IO dirction;
        /// 0x01: set IO Level;
        /// 0x02: Read IO Level.</param>
        /// <param name="ioPort">the IO which will be handled. IO1 - IO4</param>
        /// <param name="modeOrLevel">if optType=0x00, set IO mode(0x00 for Input mode, 0x01 for Output mode)
        /// if optType=0x01, set IO Level(0x00 for output Low level, 0x01 for output High level</param>
        /// <returns>IO Control Command Packet</returns>
        public static string BuildIoControlFrame(byte optType, byte ioPort, byte modeOrLevel)
        {
            string strParam0 = optType.ToString("X2");
            string strParam1 = ioPort.ToString("X2");
            string strParam2 = modeOrLevel.ToString("X2");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_IO_CONTROL, strParam0 + strParam1 + strParam2);
        }
        /// <summary>
        /// Build Set Reader Environment Mode Command Packet
        /// </summary>
        /// <param name="mode">Reader environment mode. 0x01 for Dense Reader mode, 0x00 for High Sensitivity mode</param>
        /// <returns>Set Reader Environment Mode Command Packet</returns>
        public static string BuildSetReaderEnvModeFrame(byte mode)
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_READER_ENV_MODE, mode.ToString("X2"));
        }
        /// <summary>
        /// Build Save Configuration to NV Memory Command Packet
        /// </summary>
        /// <param name="NVenable">0x01 for Enable NV Configuration. The Reader Will Load the Configuration when Next Power-up.
        /// 0x00 for Disable NV Configuration. This Setting Will Erase the Exist Configuration!</param>
        /// <returns>Save Configuration to NV Memory Command Packet</returns>
        public static string BuildSaveConfigToNvFrame(byte NVenable)
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SAVE_NV_CONFIG, NVenable.ToString("X2"));
        }
        /// <summary>
        /// Build Load Configuration From NV Memory Command Packet
        /// </summary>
        /// <returns>Load Configuration From NV Memory Command Packet</returns>
        public static string BuildLoadConfigFromNvFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_LOAD_NV_CONFIG);
        }
        /// <summary>
        /// Build Set Module to Sleep Mode Command Packet
        /// </summary>
        /// <returns>Set Module to Sleep Mode Command Packet</returns>
        public static string BuildSetModuleSleepFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SLEEP_MODE);
        }
        /// <summary>
        /// Build Set Module Sleep Time Command Packet
        /// </summary>
        /// <param name="time">Idle Time In Minutes.
        /// If the module has NO operation after the time, it will go to sleep mode.</param>
        /// <returns>Set Module Sleep Time Command Packet</returns>
        public static string BuildSetSleepTimeFrame(byte time)
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_SLEEP_TIME, time.ToString("X2"));
        }
        /// <summary>
        /// Build Insert RF Channel Command Packet
        /// </summary>
        /// <param name="channelNum">the Count of Channels Will Be Inserted</param>
        /// <param name="channelList">List of All the Channel Index</param>
        /// <returns>Insert RF Channel Command Packet</returns>
        public static string BuildInsertRfChFrame(int channelNum, byte[] channelList)
        {
            string param = channelNum.ToString("X2");
            if (channelList == null || channelList.Length == 0)
            {
                return "";
            }
            for (int i = 0; i < channelNum; i++)
            {
                param += channelList[i].ToString("X2");
            }
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_INSERT_FHSS_CHANNEL, param);
        }

#if _FEATURE_NXP_TAG_CMDS_
        /// <summary>
        /// Build Change Config Command Packet for NXP G2X Tags 
        /// </summary>
        /// <param name="accessPwd">Access Password.</param>
        /// <param name="ConfigData">16 bits Config data. The bits to be toggled in the configuration register need to be set to '1'.</param>
        /// <returns>Change Config Command Packet</returns>
        public static string BuildNXPChangeConfigFrame(string accessPwd, int ConfigData)
        {
            accessPwd = accessPwd.Replace(" ", "");
            if (accessPwd.Length != 8)
            {
                return "";
            }
            string dataField = accessPwd;
            dataField += ConfigData.ToString("X4");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_NXP_CHANGE_CONFIG, dataField);
        }

        /// <summary>
        /// Build ReadProtect/ResetReadProtect Command Packet for NXP G2X Tags 
        /// </summary>
        /// <param name="accessPwd">Access Password.</param>
        /// <param name="isReset">If it is a Reset ReadProtect command, fill true.</param>
        /// <returns>ReadProtect/ResetReadProtect Command Packet</returns>
        public static string BuildNXPReadProtectFrame(string accessPwd, bool isReset)
        {
            accessPwd = accessPwd.Replace(" ", "");
            if (accessPwd.Length != 8)
            {
                return "";
            }
            string dataField = accessPwd;
            dataField += isReset ? "01" : "00";
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_NXP_READPROTECT, dataField);
        }

        /// <summary>
        /// Build Change EAS Command Packet for NXP G2X Tags 
        /// </summary>
        /// <param name="accessPwd">Access Password.</param>
        /// <param name="isSet">If uset want to set PSF bit, fill true.</param>
        /// <returns>Change EAS Command Packet</returns>
        public static string BuildNXPChangeEasFrame(string accessPwd, bool isSet)
        {
            accessPwd = accessPwd.Replace(" ", "");
            if (accessPwd.Length != 8)
            {
                return "";
            }
            string dataField = accessPwd;
            dataField += isSet ? "01" : "00";
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_NXP_CHANGE_EAS, dataField);
        }

        /// <summary>
        /// Build EAS Alarm Command Packet for NXP G2X Tags 
        /// </summary>
        /// <returns>EAS Alarm Command Packet</returns>
        public static string BuildNXPEasAlarmFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_NXP_EAS_ALARM);
        }
#endif


#if INTERNAL_DEV
        #region 内部测试用函数
        /// <summary>
        /// build set SFR frame
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string BuildSetSfrFrame(string addr, string val)
        {
            if (addr == null || addr.Length == 0)
            {
                return "";
            }
            addr = addr.Replace(" ", "");
            if (addr.Length == 1)
            {
                addr = "0" + addr;
            }
            if (val == null || val.Length == 0)
            {
                return "";
            }
            val = val.Replace(" ", "");
            if (val.Length == 1)
            {
                val = "0" + val;
            }
            string frame = BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_SET_SFR, addr + val);
            return frame;
        }

        /// <summary>
        /// Build read SFR frame
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static string BuildReadSfrFrame(string addr)
        {
            if (addr == null || addr.Length == 0)
            {
                return "";
            }
            addr = addr.Replace(" ","");
            if (addr.Length == 1)
            {
                addr = "0" + addr;
            }
            string frame = BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_READ_SFR, addr);
            return frame;
        }

        public static string BuildReadMemFrame(int memAddr, int memLen)
        {
            if (memAddr < 0 || memLen < 0)
            {
                return "";
            }
            string dataField = memAddr.ToString("X4") + memLen.ToString("X2");
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_READ_MEM, dataField);
        }

        public static string BuildResetFwFrame()
        {
            return BuildFrame(ConstCode.FRAME_TYPE_CMD, ConstCode.CMD_TEST_RESET);
        }

        #endregion
#endif
    }
}
