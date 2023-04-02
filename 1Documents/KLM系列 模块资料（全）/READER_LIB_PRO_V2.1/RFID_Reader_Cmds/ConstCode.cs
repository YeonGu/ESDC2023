#define _FEATURE_NXP_TAG_CMDS_

using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Reader_Cmds
{
    /// <summary>
    /// defines some constant command and fail code
    /// </summary>
    public class ConstCode
    {
        /// <summary>
        /// Packet Begin Hex String
        /// </summary>
        public const string FRAME_BEGIN_HEX = "BB";
        /// <summary>
        /// Packet End Hex String
        /// </summary>
        public const string FRAME_END_HEX = "7E";
        /// <summary>
        /// Packet Begin Byte
        /// </summary>
        public const byte FRAME_BEGIN_BYTE = 0xBB;
        /// <summary>
        /// Packet End Byte
        /// </summary>
        public const byte FRAME_END_BYTE = 0x7E;

        /// <summary>
        /// Packet Type : Command
        /// </summary>
        public const string FRAME_TYPE_CMD = "00";
        /// <summary>
        /// Packet Type : Answer
        /// </summary>
        public const string FRAME_TYPE_ANS = "01";
        /// <summary>
        /// Packet Type ：Message
        /// </summary>
        public const string FRAME_TYPE_INFO = "02";

        /// <summary>
        /// Command Type : Get Module Information
        /// </summary>
        public const string CMD_GET_MODULE_INFO = "03";
        /// <summary>
        /// Command Type : Set Query Parameters
        /// </summary>
        public const string CMD_SET_QUERY = "0E";
        /// <summary>
        /// Command Type : Get Query Parameters
        /// </summary>
        public const string CMD_GET_QUERY = "0D";
        /// <summary>
        /// Command Type : Read Single Tag ID(PC + EPC)
        /// </summary>
        public const string CMD_INVENTORY = "22";
        /// <summary>
        /// Command Type : Read Multiply Tag IDs(PC + EPC)
        /// </summary>
        public const string CMD_READ_MULTI = "27";
        /// <summary>
        /// Command Type : Stop Read Multiply Tag IDs(PC + EPC)
        /// </summary>
        public const string CMD_STOP_MULTI = "28";
        /// <summary>
        /// Command Type : Read Tag Data
        /// </summary>
        public const string CMD_READ_DATA = "39";
        /// <summary>
        /// Command Type : Write Tag Data
        /// </summary>
        public const string CMD_WRITE_DATA = "49";
        /// <summary>
        /// Command Type : Lock/Unlock Tag Memory
        /// </summary>
        public const string CMD_LOCK_UNLOCK = "82";
        /// <summary>
        /// Command Type : Kill Tag
        /// </summary>
        public const string CMD_KILL = "65";

        /// <summary>
        /// Command Type : Set Reader RF Region
        /// </summary>
        public const string CMD_SET_REGION = "07";
        /// <summary>
        /// Command Type : Set Reader RF Channel
        /// </summary>
        public const string CMD_SET_RF_CHANNEL = "AB";
        /// <summary>
        /// Command Type : Get Reader RF Channel No.
        /// </summary>
        public const string CMD_GET_RF_CHANNEL = "AA";
        /// <summary>
        /// Command Type : Set Reader Power Level
        /// </summary>
        public const string CMD_SET_POWER = "B6";
        /// <summary>
        /// Command Type : Get Reader Power Level
        /// </summary>
        public const string CMD_GET_POWER = "B7";
        /// <summary>
        /// Command Type : Set Reader FHSS On/Off
        /// </summary>
        public const string CMD_SET_FHSS = "AD";
        /// <summary>
        /// Command Type : Set Reader CW On/Off
        /// </summary>
        public const string CMD_SET_CW = "B0";
        /// <summary>
        /// Command Type : Set Modem Parameter
        /// </summary>
        public const string CMD_SET_MODEM_PARA  = "F0";
        /// <summary>
        /// Command Type : Read Modem Parameter
        /// </summary>
        public const string CMD_READ_MODEM_PARA = "F1";

        /// <summary>
        /// Command Type : Set ISO18000-6C Select Command Parameters
        /// </summary>
        public const string CMD_SET_SELECT_PARA = "0C";
        /// <summary>
        /// Command Type : Get Select Command Parameters
        /// </summary>
        public const string CMD_GET_SELECT_PARA = "0B";
        /// <summary>
        /// Command Type : Set Inventory Mode 
        /// (MODE0, Send Select Command Before Each Tag Command)
        /// (MODE1, DoNot Send Select Command Before Each Tag Command)
        /// (MODE2, Send Select Command Before Tag Commands(Read, Write, Lock, Kill) Except Inventory
        /// </summary>
        public const string CMD_SET_INVENTORY_MODE = "12";
        /// <summary>
        /// Command Type : Scan Jammer
        /// </summary>
        public const string CMD_SCAN_JAMMER = "F2";
        /// <summary>
        /// Command Type : Scan RSSI
        /// </summary>
        public const string CMD_SCAN_RSSI = "F3";
        /// <summary>
        /// Command Type : Control IO
        /// </summary>
        public const string CMD_IO_CONTROL = "1A";
        /// <summary>
        /// Command Type : Restart Reader
        /// </summary>
        public const string CMD_RESTART = "19";
        /// <summary>
        /// Command Type : Set Reader Mode(Dense Reader Mode or High-sensitivity Mode)
        /// </summary>
        public const string CMD_SET_READER_ENV_MODE = "F5";
        /// <summary>
        /// Command Type : Insert RF Channel to the FHSS Frequency Look-up Table
        /// </summary>
        public const string CMD_INSERT_FHSS_CHANNEL = "A9";
        /// <summary>
        /// Command Type : Set Reader to Sleep Mode
        /// </summary>
        public const string	CMD_SLEEP_MODE = "17";
        /// <summary>
        /// Command Type : Set Reader Idle Time, after These Minutes, the Reader Will Go to Sleep Mode 
        /// </summary>
        public const string CMD_SET_SLEEP_TIME = "1D";
        /// <summary>
        /// Command Type : Load Configuration From Non-volatile Memory
        /// </summary>
	    public const string CMD_LOAD_NV_CONFIG = "0A";
        /// <summary>
        /// Command Type : Save Configuration to Non-volatile Memory
        /// </summary>
        public const string CMD_SAVE_NV_CONFIG = "09";
        /// <summary>
        /// Command Type : Change Config Command for NXP G2X Tags
        /// </summary>
        public const string CMD_NXP_CHANGE_CONFIG = "E0";
        /// <summary>
        /// Command Type : ReadProtect Command for NXP G2X Tags
        /// Reset ReadProtect Command uses the same command code but with different parameter
        /// </summary>
        public const string CMD_NXP_READPROTECT = "E1";
        /// <summary>
        ///  Command Type : Reset ReadProtect Command for NXP G2X Tags
        /// </summary>
        public const string CMD_NXP_RESET_READPROTECT = "E2";
        /// <summary>
        /// Command Type : ChangeEAS Command for NXP G2X Tags
        /// </summary>
        public const string CMD_NXP_CHANGE_EAS = "E3";
        /// <summary>
        /// Command Type : EAS_Alarm Command for NXP G2X Tags
        /// </summary>
        public const string CMD_NXP_EAS_ALARM = "E4";

        /// <summary>
        /// Command Execute Fail Type 
        /// </summary>
        public const string CMD_EXE_FAILED = "FF";

        /// <summary>
        /// Fail Type : Command Parameter Invalid
        /// </summary>
        public const string FAIL_INVALID_PARA = "0E";
        /// <summary>
        /// Fail Type : Read Tag ID Time out
        /// </summary>
        public const string FAIL_INVENTORY_TAG_TIMEOUT = "15";
        /// <summary>
        /// Fail Type : Invalid Command
        /// </summary>
        public const string FAIL_INVALID_CMD = "17";
        /// <summary>
        /// Fail Type : FHSS Failed
        /// </summary>
        public const string FAIL_FHSS_FAIL = "20";
        /// <summary>
        /// Fail Type : Access Password Error
        /// </summary>
        public const string FAIL_ACCESS_PWD_ERROR = "16";
        /// <summary>
        /// Fail Type : Read Tag Memory No Tag Response
        /// </summary>
        public const string FAIL_READ_MEMORY_NO_TAG = "09";
        /// <summary>
        /// Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Read Operation.
        /// The Error Code Will Be Added to this Code.
        /// </summary>
        public const string FAIL_READ_ERROR_CODE_BASE = "A0";
        /// <summary>
        /// Fail Type : Write Tag Memory No Tag Response
        /// </summary>
        public const string FAIL_WRITE_MEMORY_NO_TAG = "10";
        /// <summary>
        /// Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Write Operation.
        /// The Error Code Will Be Added to this Code.
        /// </summary>
        public const string FAIL_WRITE_ERROR_CODE_BASE = "B0";
        /// <summary>
        /// Fail Type : Lock Tag No Tag Response
        /// </summary>
        public const string FAIL_LOCK_NO_TAG = "13";
        /// <summary>
        /// Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Lock Operation.
        /// The Error Code Will Be Added to this Code.
        /// </summary>
        public const string FAIL_LOCK_ERROR_CODE_BASE = "C0";
        /// <summary>
        /// Fail Type : Kill Tag No Tag Response
        /// </summary>
        public const string FAIL_KILL_NO_TAG = "12";
        /// <summary>
        /// Fail Type : Error Code(defined in C1Gen2 Protocol) Caused By Kill Operation.
        /// The Error Code Will Be Added to this Code.
        /// </summary>
        public const string FAIL_KILL_ERROR_CODE_BASE = "D0";

#if _FEATURE_NXP_TAG_CMDS_
        /// <summary>
        /// Fail Type : NXP Change Config Command No Tag Response
        /// </summary>
        public const string FAIL_NXP_CHANGE_CONFIG_NO_TAG = "1A";
        /// <summary>
        /// Fail Type : NXP ReadProtect Command No Tag Response
        /// </summary>
        public const string FAIL_NXP_READPROTECT_NO_TAG = "2A";
        /// <summary>
        /// Fail Type : NXP Reset ReadProtect Command No Tag Response
        /// </summary>
        public const string FAIL_NXP_RESET_READPROTECT_NO_TAG = "2B";
        /// <summary>
        /// Fail Type : NXP Change EAS Command No Tag Response
        /// </summary>
        public const string FAIL_NXP_CHANGE_EAS_NO_TAG = "1B";
        /// <summary>
        /// Fail Type : When Executing NXP Change Config Command , Tag is Not in Secure State.
        /// NXP tag responses to Change EAS command only in secure state, so the access password must not be 0.
        /// </summary>
        public const string FAIL_NXP_CHANGE_EAS_NOT_SECURE = "1C";
        /// <summary>
        /// Fail Type : NXP EAS Alarm Command No Tag Response
        /// </summary>
        public const string FAIL_NXP_EAS_ALARM_NO_TAG = "1D";
        /// <summary>
        /// Fail Type : Error Code Caused By Custom(NXP tags or other tags) Operation.
        /// The Error Code Will Be Added to this Code.
        /// </summary>
        public const string FAIL_CUSTOM_CMD_BASE = "E0";
#endif

        /// <summary>
        /// Error Code(according to C1Gen2 Protocol) : Other Error
        /// </summary>
        public const int ERROR_CODE_OTHER_ERROR = 0x00;
        /// <summary>
        /// Error Code(according to C1Gen2 Protocol) : Memory Overrun
        /// </summary>
        public const int ERROR_CODE_MEM_OVERRUN = 0x03;
        /// <summary>
        /// Error Code(according to C1Gen2 Protocol) : Memory Locked
        /// </summary>
        public const int ERROR_CODE_MEM_LOCKED = 0x04;
        /// <summary>
        /// Error Code(according to C1Gen2 Protocol) : Insufficient Power
        /// </summary>
        public const int ERROR_CODE_INSUFFICIENT_POWER = 0x0B;
        /// <summary>
        /// Error Code(according to C1Gen2 Protocol) : Non-specific Error
        /// </summary>
        public const int ERROR_CODE_NON_SPEC_ERROR = 0x0F;

        /// <summary>
        /// Success Message Data field
        /// </summary>
        public const string SUCCESS_MSG_DATA = "00";

        /// <summary>
        /// Region Code : China 2 (920MHz - 925MHz)
        /// </summary>
        public const string REGION_CODE_CHN2 = "01";
        /// <summary>
        /// Region Code : US
        /// </summary>
        public const string REGION_CODE_US   = "02";
        /// <summary>
        /// Region Code : Europe
        /// </summary>
        public const string REGION_CODE_EUR  = "03";
        /// <summary>
        /// Region Code : China 1(840MHz - 845MHz)
        /// </summary>
        public const string REGION_CODE_CHN1 = "04";
        /// <summary>
        /// Region Code : Japan (Not Support Yet)
        /// </summary>
        public const string REGION_CODE_JAPAN = "05";
        /// <summary>
        /// Region Code : Korea
        /// </summary>
        public const string REGION_CODE_KOREA = "06";

        /// <summary>
        /// Set Function On Indicator
        /// </summary>
        public const string SET_ON = "FF";
        /// <summary>
        /// Set Function Off Indicator
        /// </summary>
        public const string SET_OFF = "00";

        /// <summary>
        /// Inventory MODE0, Send Select Command Before Each Tag Command
        /// </summary>
        public const string INVENTORY_MODE0 = "00";
        /// <summary>
        /// Inventory MODE1, DoNot Send Select Command Before Each Tag Command
        /// </summary>
        public const string INVENTORY_MODE1 = "01";
        /// <summary>
        /// Inventory MODE1, Send Select Command Before Tag Commands(Read, Write, Lock, Kill etc.) Except Inventory
        /// </summary>
        public const string INVENTORY_MODE2 = "02";

        /// <summary>
        /// Get Module Information Type : Hardware Version
        /// </summary>
        public const string MODULE_HARDWARE_VERSION_FIELD = "00";
        /// <summary>
        /// Get Module Information Type : Software Version
        /// </summary>
	    public const string MODULE_SOFTWARE_VERSION_FIELD = "01";
        /// <summary>
        /// Get Module Information Type : Manufacture Information
        /// </summary>
	    public const string MODULE_MANUFACTURE_INFO_FIELD = "02";
        /// <summary>
        /// NV Memory Configuration 
        /// </summary>
        public enum NVconfig 
        {
            /// <summary>
            /// Enable NV Configuration. The Reader Will Load the Configuration when Next Power-up.
            /// </summary>
            NVenable = 0x01,
            /// <summary>
            /// Disable NV Configuration. This Setting Will Erase the Exist Configuration!
            /// </summary>
            NVdisable = 0x00,
        };

#if INTERNAL_DEV
        
        public const string CMD_SET_SFR = "FE";
        public const string CMD_READ_SFR = "FD";
        public const string CMD_READ_MEM = "FB";
        public const string CMD_INIT_SFR = "EC";
        public const string CMD_TEST_RESET = "55";

        public const string FAIl_MEM_OVERUN = "03";
#endif
    }
}
