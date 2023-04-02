using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using RFID_Reader_Cmds;
using RFID_Reader_Com;
using BarChart;
using System.Globalization;

namespace RFID_Reader_Csharp
{

    public partial class Form1 : Form
    {
        private bool bAutoSend = false;

        private int LoopNum_cnt = 0;
        private bool change_q_1st = true;
        private bool change_q_message = true;

        public ReceiveParser rp;

        DataTable basic_table = new DataTable();
        DataTable advanced_table = new DataTable();
        DataSet ds_basic = null;
        DataSet ds_advanced = null;
        string pc = string.Empty;
        string epc = string.Empty;
        string crc = string.Empty;
        string rssi = string.Empty;

        int FailEPCNum = 0;
        int SucessEPCNum = 0;
        double errnum = 0;
        double db_errEPCNum = 0;
        double db_LoopNum_cnt = 0;
        string per = "0.000";

        private String timeFormat = "yyyy/MM/dd HH:mm:ss.ff";
        //private String timeFormat = System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern.ToString() + " HH:mm:ss.ff";

        static string[] strBuff = new String[4096];

        int rowIndex = 0;
        int initDataTableLen = 1;  //初始化Datatable的行数

        private static int[] mixerGainTable = {0, 3, 6, 9, 12, 15, 16};

        private static int[] IFAmpGainTable = { 12, 18, 21, 24, 27, 30, 36, 40 };

        public Form1()
        {
            InitializeComponent();
            setTip();
           // this.tabPage3.Parent = null;
            this.cbxRegion.SelectedIndex = 0;
            this.cbxChannel.SelectedIndex = 0;
            this.cbxBaudRate.SelectedIndex = 2;
            this.cbxDR.SelectedIndex = 0;
            this.cbxM.SelectedIndex = 0;
            this.cbxTRext.SelectedIndex = 1;
            this.cbxSel.SelectedIndex = 0;
            this.cbxSession.SelectedIndex = 0;
            this.cbxTarget.SelectedIndex = 0;
            this.cbxQBasic.SelectedIndex = 4;
            this.cbxQAdv.SelectedIndex = 4;
            this.cbxMemBank.SelectedIndex = 3;
            this.cbxSelTarget.SelectedIndex = 0;
            this.cbxAction.SelectedIndex = 0;
            this.cbxSelMemBank.SelectedIndex = 1;
            this.cbxPaPower.SelectedIndex = 0;
            this.cbxMixerGain.SelectedIndex = 3;
            this.cbxIFAmpGain.SelectedIndex = 6;
            this.cbxMode.SelectedIndex = 2;
            this.cbxIO.SelectedIndex = 0;
            this.cbxIoLevel.SelectedIndex = 0;
            this.cbxIoDircetion.SelectedIndex = 0;
            this.cbxLockKillAction.SelectedIndex = 0;
            this.cbxLockAccessAction.SelectedIndex = 0;
            this.cbxLockEPCAction.SelectedIndex = 0;
            this.cbxLockTIDAction.SelectedIndex = 0;
            this.cbxLockUserAction.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //lblVersion.Text += AssemblyLib.AssemblyVersion;
            //get all available serial port list
            string[] ports = Sp.GetInstance().GetPortNames();
            foreach (string port in ports)
            {
                cbxPort.Items.Add(port);
            }
            if (cbxPort.Items.Count > 0)
            {
                cbxPort.SelectedIndex = 0;
                btnConn.Enabled = true;
            }
            cbxBaudRate.SelectedIndex = 2;
            //ComDevice.DataReceived += new SerialDataReceivedEventHandler(ComDevice_DataReceived);
            rp = new ReceiveParser();
            Sp.GetInstance().ComDevice.DataReceived += new SerialDataReceivedEventHandler(rp.DataReceived);
            rp.PacketReceived +=new EventHandler<StrArrEventArgs>(rp_PaketReceived);
            Sp.GetInstance().DataSent += new EventHandler<byteArrEventArgs>(ComDataSent);
            this.dgvEpcBasic.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgvEpcBasic_DataBindingComplete);
            this.dgv_epc2.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_epc2_DataBindingComplete);
            //ComDevice.NewLine = "/r/n";
            change_q_1st = false;

            //DataGridView
            ds_basic = new DataSet();
            ds_advanced = new DataSet();
            //dt = new DataTable();

            basic_table = BasicGetEPCHead();
            advanced_table = AdvancedGetEPCHead();
            ds_basic.Tables.Add(basic_table);
            ds_advanced.Tables.Add(advanced_table);

            DataView BasicDataViewEpc = ds_basic.Tables[0].DefaultView;
            DataView AdvancedDataViewEpc = ds_advanced.Tables[0].DefaultView;
            this.dgvEpcBasic.DataSource = BasicDataViewEpc;
            this.dgv_epc2.DataSource = AdvancedDataViewEpc;
            Basic_DGV_ColumnsWidth(this.dgvEpcBasic);
            Advanced_DGV_ColumnsWidth(this.dgv_epc2);
            btnInvtBasic.Focus();

            adjustUIcomponents("M100");
        }

        private void ComDataSent(object sender, byteArrEventArgs e)
        {
            txtCOMTxCnt.Text = (Convert.ToInt32(txtCOMTxCnt.Text) + e.Data.Length).ToString();
            txtCOMTxCnt_adv.Text = txtCOMTxCnt.Text;
        }

        private void rp_PaketReceived(object sender, StrArrEventArgs e)
        {
            string[] packetRx = e.Data;
            string strPacket = string.Empty;
            for (int i = 0; i < packetRx.Length; i++)
            {
                strPacket += packetRx[i] + " ";
            }
            this.Invoke((EventHandler)(delegate
            {
                txtCOMRxCnt.Text = (Convert.ToInt32(txtCOMRxCnt.Text) + packetRx.Length).ToString();
                txtCOMRxCnt_adv.Text = txtCOMRxCnt.Text;

                //auto clear received data region
                int txtReceive_len = txtReceive.Lines.Length; //txtReceive.GetLineFromCharIndex(txtReceive.Text.Length + 1);
                if (cbxAutoClear.Checked)
                {
                    if (txtReceive_len > 9)
                    {
                        txtReceive.Text = string.Empty;
                    }
                }
                #region show received packet
                if (cbxRxVisable.Checked == true)
                {
                    this.txtReceive.Text = this.txtReceive.Text + strPacket + "\r\n";
                }
                if (packetRx[1] == ConstCode.FRAME_TYPE_INFO && packetRx[2] == ConstCode.CMD_INVENTORY)         //Succeed to Read EPC
                {
                    //Console.Beep();
                    SucessEPCNum = SucessEPCNum + 1;
                    db_errEPCNum = FailEPCNum;
                    db_LoopNum_cnt = db_LoopNum_cnt + 1;
                    errnum = (db_errEPCNum / db_LoopNum_cnt) * 100;
                    per = string.Format("{0:0.000}", errnum);

                    int rssidBm = Convert.ToInt16(packetRx[5], 16); // rssidBm is negative && in bytes
                    if (rssidBm > 127)
                    {
                        rssidBm = -((-rssidBm)&0xFF);
                    }
                    rssidBm -= Convert.ToInt32(tbxCoupling.Text, 10);
                    rssidBm -= Convert.ToInt32(tbxAntennaGain.Text, 10);
                    rssi = rssidBm.ToString();

                    int PCEPCLength = ((Convert.ToInt32((packetRx[6]), 16)) / 8 + 1) * 2;
                    pc = packetRx[6] + " " + packetRx[7];
                    epc = string.Empty;
                    for (int i = 0; i < PCEPCLength - 2; i++)
                    {
                        epc = epc + packetRx[8 + i];
                    }
                    epc = Commands.AutoAddSpace(epc);
                    crc = packetRx[6 + PCEPCLength] + " " + packetRx[7 + PCEPCLength];
                    GetEPC(pc, epc, crc, rssi, per);
                }
                else if (packetRx[1] == ConstCode.FRAME_TYPE_ANS)
                {
                    if (packetRx[2] == ConstCode.CMD_EXE_FAILED)
                    {
                        int failType = Convert.ToInt32(packetRx[5], 16);
                        if (packetRx.Length > 9) // has PC+EPC field
                        {
                            txtOperateEpc.Text = "";
                            int pcEpcLen = Convert.ToInt32(packetRx[6], 16);

                            for (int i = 0; i < pcEpcLen; i++)
                            {
                                txtOperateEpc.Text += packetRx[i + 7] + " ";
                            }
                        }
                        else
                        {
                            txtOperateEpc.Text = "";
                        }
                        if (packetRx[5] == ConstCode.FAIL_INVENTORY_TAG_TIMEOUT)
                        {
                            FailEPCNum = FailEPCNum + 1;
                            db_errEPCNum = FailEPCNum;
                            db_LoopNum_cnt = db_LoopNum_cnt + 1;
                            errnum = (db_errEPCNum / db_LoopNum_cnt) * 100;
                            per = string.Format("{0:0.000}", errnum);
                            //GetEPC(pc, epc, crc, rssi_i, rssi_q, per);
                            pbx_Inv_Indicator.Visible = false;
                        }
                        else if (packetRx[5] == ConstCode.FAIL_FHSS_FAIL)
                        {
                            //MessageBox.Show("FHSS Failed.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("FHSS Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_ACCESS_PWD_ERROR)
                        {
                            //MessageBox.Show("Access Failed, Please Check the Access Password!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Access Failed, Please Check the Access Password", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_READ_MEMORY_NO_TAG)
                        {
                            setStatus("No Tag Response, Fail to Read Tag Memory", Color.Red);
                        }
                        else if (packetRx[5].Substring(0,1) == ConstCode.FAIL_READ_ERROR_CODE_BASE.Substring(0,1))
                        {
                            //MessageBox.Show("Read Failed. Error Code: " + ParseErrCode(failType), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Read Failed. Error Code: " + ParseErrCode(failType), Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_WRITE_MEMORY_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, Fail to Write Tag Memory", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, Fail to Write Tag Memory", Color.Red);
                        }
                        else if (packetRx[5].Substring(0, 1) == ConstCode.FAIL_WRITE_ERROR_CODE_BASE.Substring(0, 1))
                        {
                            //MessageBox.Show("Write Failed. Error Code: " + ParseErrCode(failType), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Write Failed. Error Code: " + ParseErrCode(failType), Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_LOCK_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, Lock Operation Failed", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, Lock Operation Failed", Color.Red);
                        }
                        else if (packetRx[5].Substring(0, 1) == ConstCode.FAIL_LOCK_ERROR_CODE_BASE.Substring(0, 1))
                        {
                            //MessageBox.Show("Lock Failed. Error Code: " + ParseErrCode(failType), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Lock Failed. Error Code: " + ParseErrCode(failType), Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_KILL_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, Kill Operation Failed", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, Kill Operation Failed", Color.Red);
                        }
                        else if (packetRx[5].Substring(0, 1) == ConstCode.FAIL_KILL_ERROR_CODE_BASE.Substring(0, 1))
                        {
                            //MessageBox.Show("Kill Failed. Error Code: " + ParseErrCode(failType), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Kill Failed. Error Code: " + ParseErrCode(failType), Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_CHANGE_CONFIG_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, NXP Change Config Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, NXP Change Config Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_CHANGE_EAS_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, NXP Change EAS Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, NXP Change EAS Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_CHANGE_EAS_NOT_SECURE)
                        {
                            //MessageBox.Show("Tag is not in Secure State, NXP Change EAS Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Tag is not in Secure State, NXP Change EAS Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_EAS_ALARM_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, NXP EAS Alarm Operation Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtOperateEpc.Text = "";
                            setStatus("No Tag Response, NXP EAS Alarm Operation Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_READPROTECT_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, NXP ReadProtect Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, NXP ReadProtect Failed", Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_NXP_RESET_READPROTECT_NO_TAG)
                        {
                            //MessageBox.Show("No Tag Response, NXP Reset ReadProtect Failed!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("No Tag Response, NXP Reset ReadProtect Failed", Color.Red);
                        }
                        else if (packetRx[5].Substring(0, 1) == ConstCode.FAIL_CUSTOM_CMD_BASE.Substring(0, 1))
                        {
                            //MessageBox.Show("Command Executed Failed. Error Code: " + ParseErrCode(failType), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            setStatus("Command Executed Failed. Error Code: " + ParseErrCode(failType), Color.Red);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_INVALID_PARA)
                        {
                            MessageBox.Show("无效参数", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (packetRx[5] == ConstCode.FAIL_INVALID_CMD)
                        {
                            MessageBox.Show("无效命令!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (packetRx[2] == ConstCode.CMD_SET_QUERY)            //SetQuery
                    {
                        MessageBox.Show("Query Parameters is Setted up", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_GET_QUERY)            //GetQuery
                    {
                        string infoGetQuery = string.Empty;
                        string[] strMSB = String16toString2(packetRx[5]);
                        string[] strLSB = String16toString2(packetRx[6]);
                        int intQ = Convert.ToInt32(strLSB[6]) * 8 + Convert.ToInt32(strLSB[5]) * 4
                            + Convert.ToInt32(strLSB[4]) * 2 + Convert.ToInt32(strLSB[3]);
                        string strM = string.Empty;
                        if ((strMSB[6] + strMSB[5]) == "00")
                        {
                            strM = "1";
                        }
                        else if ((strMSB[6] + strMSB[5]) == "01")
                        {
                            strM = "2";
                        }
                        else if ((strMSB[6] + strMSB[5]) == "10")
                        {
                            strM = "4";
                        }
                        else if ((strMSB[6] + strMSB[5]) == "11")
                        {
                            strM = "8";
                        }
                        string strTRext = string.Empty;
                        if (strMSB[4] == "0")
                        {
                            strTRext = "NoPilot";
                        }
                        else
                        {
                            strTRext = "UsePilot";
                        }
                        string strTarget = string.Empty;
                        if (strLSB[7] == "0")
                        {
                            strTarget = "A";
                        }
                        else
                        {
                            strTarget = "B";
                        }
                        infoGetQuery = "DR=" + strMSB[7] + ", ";
                        infoGetQuery = infoGetQuery + "M=" + strM + ", ";
                        infoGetQuery = infoGetQuery + "TRext=" + strTRext + ", ";
                        infoGetQuery = infoGetQuery + "Sel=" + strMSB[3] + strMSB[2] + ", ";
                        infoGetQuery = infoGetQuery + "Session=" + strMSB[1] + strMSB[0] + ", ";
                        infoGetQuery = infoGetQuery + "Target=" + strTarget + ", ";
                        infoGetQuery = infoGetQuery + "Q=" + intQ;
                        MessageBox.Show(infoGetQuery, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_READ_DATA)         //Read Tag Memory
                    {
                        string strInvtReadData = "";
                        txtInvtRWData.Text = "";
                        txtOperateEpc.Text = "";
                        int dataLen = Convert.ToInt32(packetRx[3], 16) * 256 + Convert.ToInt32(packetRx[4], 16);
                        int pcEpcLen = Convert.ToInt32(packetRx[5], 16);

                        for (int i = 0; i < pcEpcLen; i++)
                        {
                            txtOperateEpc.Text += packetRx[i + 6] + " ";
                        }

                        for (int i = 0; i < dataLen - pcEpcLen - 1; i++)
                        {
                            strInvtReadData = strInvtReadData + packetRx[i + pcEpcLen + 6];
                        }
                        txtInvtRWData.Text = Commands.AutoAddSpace(strInvtReadData);
                        setStatus("Read Memory Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_WRITE_DATA)
                    {
                        //MessageBox.Show("Write Memory Success!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        getSuccessTagEpc(packetRx);
                        setStatus("Write Memory Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_LOCK_UNLOCK)
                    {
                        //MessageBox.Show("Lock Success!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        getSuccessTagEpc(packetRx);
                        setStatus("Lock Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_KILL)
                    {
                        //MessageBox.Show("Kill Success!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        getSuccessTagEpc(packetRx);
                        setStatus("Kill Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_NXP_CHANGE_CONFIG)
                    {
                        int pcEpcLen = getSuccessTagEpc(packetRx);
                        string configWord = packetRx[pcEpcLen + 6] + packetRx[pcEpcLen + 7];
                        setStatus("NXP Tag Change Config Success, Config Word: 0x" + configWord, Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_NXP_CHANGE_EAS)
                    {
                        getSuccessTagEpc(packetRx);
                        setStatus("NXP Tag Change EAS Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_NXP_READPROTECT)
                    {
                        getSuccessTagEpc(packetRx);
                        setStatus("NXP Tag ReadProtect Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_NXP_RESET_READPROTECT)
                    {
                        getSuccessTagEpc(packetRx);
                        setStatus("NXP Tag Reset ReadProtect Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_NXP_EAS_ALARM)
                    {
                        setStatus("NXP Tag EAS Alarm Success", Color.MediumSeaGreen);
                    }
                    else if (packetRx[2] == ConstCode.CMD_GET_SELECT_PARA)            //GetQuery
                    {
                        string infoGetSelParam = string.Empty;
                        string[] strSelCombParam = String16toString2(packetRx[5]);
                        string strSelTarget = strSelCombParam[7] + strSelCombParam[6] + strSelCombParam[5];
                        string strSelAction = strSelCombParam[4] + strSelCombParam[3] + strSelCombParam[2];
                        string strSelMemBank = strSelCombParam[1] + strSelCombParam[0];

                        string strSelTargetInfo = null;
                        if (strSelTarget == "000")
                        {
                            strSelTargetInfo = "S0";
                        }
                        else if (strSelTarget == "001")
                        {
                            strSelTargetInfo = "S1";
                        }
                        else if (strSelTarget == "010")
                        {
                            strSelTargetInfo = "S2";
                        }
                        else if (strSelTarget == "011")
                        {
                            strSelTargetInfo = "S3";
                        }
                        else if (strSelTarget == "100")
                        {
                            strSelTargetInfo = "SL";
                        }
                        else
                        {
                            strSelTargetInfo = "RFU";
                        }

                        string strSelMemBankInfo = null;
                        if (strSelMemBank == "00")
                        {
                            strSelMemBankInfo = "RFU";
                        }
                        else if (strSelMemBank == "01")
                        {
                            strSelMemBankInfo = "EPC";
                        }
                        else if (strSelMemBank == "10")
                        {
                            strSelMemBankInfo = "TID";
                        }
                        else
                        {
                            strSelMemBankInfo = "User";
                        }
                        infoGetSelParam = "Target=" + strSelTargetInfo + ", Action=" + strSelAction + ", Memory Bank=" + strSelMemBankInfo;
                        infoGetSelParam = infoGetSelParam + ", Pointer=0x" + packetRx[6] + packetRx[7] + packetRx[8] + packetRx[9];
                        infoGetSelParam = infoGetSelParam + ", Length=0x" + packetRx[10];
                        string strTruncate = null;
                        if (packetRx[11] == "00")
                        {
                            strTruncate = "Disable Truncation";
                        }
                        else
                        {
                            strTruncate = "Enable Truncation";
                        }
                        infoGetSelParam = infoGetSelParam + ", " + strTruncate;

                        this.txtGetSelLength.Text = packetRx[10];

                        string strGetSelMask = null;
                        int intGetSelMaskByte = Convert.ToInt32(packetRx[10], 16) / 8;
                        int intGetSelMaskBit = Convert.ToInt32(packetRx[10], 16) - intGetSelMaskByte * 8;
                        if (intGetSelMaskBit == 0)
                        {
                            for (int i = 0; i < intGetSelMaskByte; i++)
                            {
                                strGetSelMask = strGetSelMask + packetRx[12 + i];
                            }
                        }
                        else
                        {
                            for (int i = 0; i < intGetSelMaskByte + 1; i++)
                            {
                                strGetSelMask = strGetSelMask + packetRx[12 + i];
                            }
                        }

                        this.txtGetSelMask.Text = Commands.AutoAddSpace(strGetSelMask);
                        MessageBox.Show(infoGetSelParam, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_GET_RF_CHANNEL)
                    {
                        double curRfCh = Convert.ToInt32(packetRx[5],16);
                        switch (curRegion)
                        {
                            case ConstCode.REGION_CODE_CHN2 : // China 2
                                curRfCh = 920.125 + curRfCh * 0.25;
            	                break;
                            case ConstCode.REGION_CODE_CHN1: // China 1
                                curRfCh = 840.125 + curRfCh * 0.25;
                                break;
                            case ConstCode.REGION_CODE_US: // US
                                curRfCh = 902.25 + curRfCh * 0.5;
                                break;
                            case ConstCode.REGION_CODE_EUR: // Europe
                                curRfCh = 865.1 + curRfCh * 0.2;
                                break;
                            case ConstCode.REGION_CODE_KOREA:  // Korea
                                curRfCh = 917.1 + curRfCh * 0.2;
                                break;
                            default :
                                break;
                        }
                        MessageBox.Show("当前RF频道 " + curRfCh + " MHz", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_GET_POWER)
                    {
                        string curPower = packetRx[5] + packetRx[6];
                        MessageBox.Show("当前增益 " + (Convert.ToInt16(curPower, 16) / 100.0) + "dBm", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_READ_MODEM_PARA)
                    {
                        int mixerGain = mixerGainTable[Convert.ToInt32(packetRx[5], 16)];
                        int IFAmpGain = IFAmpGainTable[Convert.ToInt32(packetRx[6], 16)];
                        string signalTh = packetRx[7] + packetRx[8];
                        MessageBox.Show("Mixer Gain is " + mixerGain + "dB, IF AMP Gain is " + IFAmpGain + "dB, Decode Threshold is 0x" + signalTh + ".", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (packetRx[2] == ConstCode.CMD_SCAN_JAMMER)
                    {
                        int startChannel = Convert.ToInt16(packetRx[5], 16);
                        int stopChannel = Convert.ToInt16(packetRx[6], 16);
                        
                        hBarChartJammer.Items.Maximum = 40;
                        hBarChartJammer.Items.Minimum = 0;

                        hBarChartJammer.Items.Clear();

                        int[] allJammer = new int[(stopChannel - startChannel + 1)];
                        int maxJammer = -100;
                        int minJammer = 20;
                        for (int i = 0; i < (stopChannel - startChannel + 1); i++)
                        {
                            int jammer = Convert.ToInt16(packetRx[7 + i], 16);
                            if (jammer > 127)
                            {
                                jammer = -((-jammer) & 0xFF);
                            }
                            allJammer[i] = jammer;
                            if (jammer >= maxJammer)
                            {
                                maxJammer = jammer;
                            }
                            if (jammer <= minJammer)
                            {
                                minJammer = jammer;
                            }
                        }
                        int offset = -minJammer + 3;
                        for (int i = 0; i < (stopChannel - startChannel + 1); i++)
                        {
                            allJammer[i] = allJammer[i] + offset;
                            hBarChartJammer.Items.Add(new HBarItem((double)(allJammer[i]),(double)offset, (i + startChannel).ToString(), Color.FromArgb(255, 190, 200, 255)));
                        }
                        hBarChartJammer.RedrawChart();
                    }
                    else if (packetRx[2] == ConstCode.CMD_SCAN_RSSI)
                    {
                        int startChannel = Convert.ToInt16(packetRx[5], 16);
                        int stopChannel = Convert.ToInt16(packetRx[6], 16);

                        hBarChartRssi.Items.Maximum = 73;
                        hBarChartRssi.Items.Minimum = 0;

                        hBarChartRssi.Items.Clear();

                        int[] allRssi = new int[(stopChannel - startChannel + 1)];
                        int maxRssi = -100;
                        int minRssi = 20;
                        for (int i = 0; i < (stopChannel - startChannel + 1); i++)
                        {
                            int rssi = Convert.ToInt16(packetRx[7 + i], 16);
                            if (rssi > 127)
                            {
                                rssi = -((-rssi) & 0xFF);
                            }
                            allRssi[i] = rssi;
                            if (rssi >= maxRssi)
                            {
                                maxRssi = rssi;
                            }
                            if (rssi <= minRssi)
                            {
                                minRssi = rssi;
                            }
                        }
                        int offset = -minRssi + 3;
                        for (int i = 0; i < (stopChannel - startChannel + 1); i++)
                        {
                            allRssi[i] = allRssi[i] + offset;
                            hBarChartRssi.Items.Add(new HBarItem((double)(allRssi[i]), (double)offset, (i + startChannel).ToString(), Color.FromArgb(255, 190, 200, 255)));
                        }
                        hBarChartRssi.RedrawChart();
                    }
                    else if (packetRx[2] == ConstCode.CMD_GET_MODULE_INFO)
                    {
                        if (checkingReaderAvailable)
                        {
                            hardwareVersion = String.Empty;
                            if (packetRx[5] == ConstCode.MODULE_HARDWARE_VERSION_FIELD)
                            {
                                try
                                {
                                    for (int i = 0; i < Convert.ToInt32(packetRx[4], 16) - 1; i++)
                                    {
                                        hardwareVersion += (char)Convert.ToInt32(packetRx[6 + i], 16);
                                    }
                                    txtHardwareVersion.Text = hardwareVersion;
                                    adjustUIcomponents(hardwareVersion);
                                }
                                catch (System.Exception ex)
                                {
                                    hardwareVersion = packetRx[6].Substring(1, 1) + "." + packetRx[7];
                                    txtHardwareVersion.Text = hardwareVersion;
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                    else if (packetRx[2] == "1A")
                    {
                        if (packetRx[5] == "02")
                        {
                            MessageBox.Show("IO" + packetRx[6].Substring(1) + " is " + (packetRx[7] == "00" ? "Low" : "High"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
#endregion
            }));

#if TRACE
            //Console.WriteLine("a packet received!");
#endif
        }

        private int getSuccessTagEpc(string[] packetRx)
        {
            txtOperateEpc.Text = "";
            if (packetRx.Length < 9)
            {
                return 0;
            }
            int pcEpcLen = Convert.ToInt32(packetRx[5], 16);
            for (int i = 0; i < pcEpcLen; i++)
            {
                txtOperateEpc.Text += packetRx[i + 6] + " ";
            }
            return pcEpcLen;
        }

        private void setStatus(string msg, Color color)
        {
            rtbxStatus.Text = msg;
            rtbxStatus.ForeColor = color;
        }

        private void adjustUIcomponents(string hardwareVersion)
        {
            if (hardwareVersion.Length >= 10 && "M100 26dBm".Equals(hardwareVersion.Substring(0, 10)))
            {
                this.cbxPaPower.Items.Clear();
                for (int i = 26; i >= -9; i--) {
                    this.cbxPaPower.Items.Add(i.ToString() + "dBm");
                }
                this.cbxPaPower.SelectedIndex = 0;
                this.cbxMixerGain.SelectedIndex = 2;
                this.cbxIFAmpGain.SelectedIndex = 6;
                this.tbxSignalThreshold.Text = "00A0";
                this.tbxAntennaGain.Text = "3";
                this.tbxCoupling.Text = "-20";
                this.gbxIoControl.Visible = false;
            }
            else if (hardwareVersion.Length >= 10 && "M100 20dBm".Equals(hardwareVersion.Substring(0, 10)))
            {
                this.cbxPaPower.Items.Clear();
                this.cbxPaPower.Items.AddRange(new object[] {
                                    "20dBm",
                                    "18.5dBm",
                                    "17dBm",
                                    "15.5dBm",
                                    "14dBm",
                                    "12.5dBm"});
                this.cbxPaPower.SelectedIndex = 0;
                this.cbxMixerGain.SelectedIndex = 3;
                this.cbxIFAmpGain.SelectedIndex = 6;
                this.tbxSignalThreshold.Text = "01B0";
                this.tbxAntennaGain.Text = "1";
                this.tbxCoupling.Text = "-27";
                this.gbxIoControl.Visible = false;
            }
            else if (hardwareVersion.Length >= 10 && "QM100 30dBm".Equals(hardwareVersion.Substring(0, 11)))
            {
                this.cbxPaPower.Items.Clear();
                for (int i = 30; i >= 19; i--)
                {
                    this.cbxPaPower.Items.Add(i.ToString() + "dBm");
                }
                this.cbxPaPower.SelectedIndex = 0;
                this.cbxMixerGain.SelectedIndex = 4;
                this.cbxIFAmpGain.SelectedIndex = 6;
                this.tbxSignalThreshold.Text = "0120";
                this.tbxAntennaGain.Text = "3";
                this.tbxCoupling.Text = "-10";
                this.cbxQBasic.SelectedIndexChanged -= new System.EventHandler(this.cbx_q_basic_SelectedIndexChanged);
                this.cbxQBasic.SelectedIndex = 5;
                this.cbxQBasic.SelectedIndexChanged += new System.EventHandler(this.cbx_q_basic_SelectedIndexChanged);
                this.cbxQAdv.SelectedIndex = 5;
                this.gbxIoControl.Visible = true;
            }
            else if (hardwareVersion.Length >= 5 && "QM100".Equals(hardwareVersion.Substring(0, 5)))
            {
                this.cbxPaPower.Items.Clear();
                this.cbxPaPower.Items.AddRange(new object[] {
                                     "30dBm",
                                     "28.5dBm",
                                     "27dBm",
                                     "25.5dBm",
                                     "24dBm",
                                     "22.5dBm",
                                     "21dBm",
                                     "19.5dBm"});
                this.cbxPaPower.SelectedIndex = 2;
                this.cbxMixerGain.SelectedIndex = 4;
                this.cbxIFAmpGain.SelectedIndex = 6;
                this.tbxSignalThreshold.Text = "0280";
                this.tbxAntennaGain.Text = "4";
                this.tbxCoupling.Text = "-10";
                this.gbxIoControl.Visible = true;
            }
        }
        private void setTip()
        {
            toolTip1.SetToolTip(this.label1, "Available COM Port");
            toolTip1.SetToolTip(this.txtReceive, "Double Click To Select ALL");
        }

        #region Serial Port connection and download Firmware
        private void btnConn_Click(object sender, EventArgs e)
        {
            if (bAutoSend == false)
            {
                if (btnConn.Tag.ToString() == "0")
                {
                    Sp.GetInstance().Config(cbxPort.SelectedItem.ToString(), Convert.ToInt32(cbxBaudRate.SelectedItem.ToString())
                        , Parity.None, 8, StopBits.One);
                    if (!Sp.GetInstance().Open())
                    {
                        MessageBox.Show("打开串口失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }                        
                    btnConn.Text = "断开连接";
                    btnConn.Tag = "1";
                    this.btnConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
                    checkReaderAvailable();
                    //picComStatus.BackgroundImage = Properties.Resources.greenlight;
                }
                else
                {
                    if (!Sp.GetInstance().Close())
                    {
                        MessageBox.Show("关闭串口失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    btnConn.Text = "连接串口";
                    btnConn.Tag = "0";
                    this.btnConn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                    //picComStatus.BackgroundImage = Properties.Resources.redlight;
                }
            }
            else
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

         public void checkReaderAvailable()
        {
            if (Sp.GetInstance().IsOpen())
            {
                hardwareVersion = "";
                checkingReaderAvailable = true;
                readerConnected = false;
                Sp.GetInstance().Send(Commands.BuildGetModuleInfoFrame(ConstCode.MODULE_HARDWARE_VERSION_FIELD));
                
                timerCheckReader.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
        }

        #endregion
        private void cbx_dr_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbxDR.SelectedIndex == 1)
            {
                MessageBox.Show("Does Not Support DR = 64/3 In this Version", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.cbxDR.SelectedIndex = 0;
            }
        }

        private void cbx_m_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbxM.SelectedIndex == 1 || this.cbxM.SelectedIndex == 2 || this.cbxM.SelectedIndex == 3)
            {
                MessageBox.Show("Does Not Support M = 2/4/8 In this Version", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.cbxM.SelectedIndex = 0;
            }
        }

        private void cbx_trext_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbxTRext.SelectedIndex == 0)
            {
                MessageBox.Show("Does Not Support No Pilot Tone In this Version", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.cbxTRext.SelectedIndex = 1;
            }
        }

        #region send data
        private void btn_Send_Click(object sender, EventArgs e)
        {
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (Sp.GetInstance().IsOpen() == true)
            {
                bAutoSend = !bAutoSend;
                if (bAutoSend)
                {
                    timerAutoSend.Interval = Convert.ToInt32(txtSendDelay.Text);
                    timerAutoSend.Enabled = true;
                    txtSend.Text = Commands.BuildReadSingleFrame();
                    btnContinue.Text = "停止";
                    tmrCheckEpc.Enabled = true;
                }
                else
                {
                    timerAutoSend.Interval = Convert.ToInt32(txtSendDelay.Text);
                    timerAutoSend.Enabled = false;
                    btnContinue.Text = "继续";
                    tmrCheckEpc.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("请连接串口！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void timerAutoSend_Tick(object sender, EventArgs e)
        {
            LoopNum_cnt = LoopNum_cnt + 1;
            try
            {
                if (Sp.GetInstance().Send(txtSend.Text) == 0)
                {
                    bAutoSend = false;
                    timerAutoSend.Enabled = false;
                    btnContinue.Text = "Continue";
                }
            }
            catch (System.Exception ex)
            {
                bAutoSend = false;
                timerAutoSend.Enabled = false;
                btnContinue.Text = "Continue";
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //clear send text region
        private void btnClearS_Click(object sender, EventArgs e)
        {
            txtSend.Text = "";
        }

        private void btnSetFreq_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtSend.Text = Commands.BuildSetRfChannelFrame(cbxChannel.SelectedIndex.ToString("X2"));
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btn_invt_Click(object sender, EventArgs e)
        {
            LoopNum_cnt = LoopNum_cnt + 1;
            txtSend.Text = Commands.BuildReadSingleFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void cbx_q_basic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (change_q_1st == false)
            {
                if (bAutoSend == true)
                {
                    if (change_q_message == true)
                    {
                        MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        change_q_message = false;
                        this.cbxQBasic.SelectedIndex = this.cbxQAdv.SelectedIndex;
                    }
                    else
                    {
                        change_q_message = true;
                    }
                }
                else
                {
                    int intDR = this.cbxDR.SelectedIndex;
                    int intM = this.cbxM.SelectedIndex;
                    int intTRext = this.cbxTRext.SelectedIndex;
                    int intSel = this.cbxSel.SelectedIndex;
                    int intSession = this.cbxSession.SelectedIndex;

                    int intTarget = this.cbxTarget.SelectedIndex;
                    int intQ = this.cbxQBasic.SelectedIndex;

                    txtSend.Text = Commands.BuildSetQueryFrame(intDR, intM, intTRext, intSel, intSession, intTarget, intQ);
                    Sp.GetInstance().Send(txtSend.Text);
                    this.cbxQAdv.SelectedIndex = intQ;
                }
            }
        }

        private void btnSetCW_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (btnSetCW.Text == "CW ON")
            {
                txtSend.Text = Commands.BuildSetCWFrame(ConstCode.SET_ON);
            }
            else
            {
                txtSend.Text = Commands.BuildSetCWFrame(ConstCode.SET_OFF);
            }
            Sp.GetInstance().Send(txtSend.Text);

            if (btnSetCW.Text == "CW ON")
            {
                btnSetCW.Text = "CW OFF";
            }
            else
            {
                btnSetCW.Text = "CW ON";
            }
        }

        #endregion

        private void btn_clear_rx_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";
        }
        //clear EPC Table
        private void btn_clear_basictable_Click(object sender, EventArgs e)
        {
            basic_table.Clear();
            advanced_table.Clear();
            LoopNum_cnt = 0;
            FailEPCNum = 0;
            SucessEPCNum = 0;
            db_LoopNum_cnt = 0;
            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                basic_table.Rows.Add(new object[] { null });
            }
            basic_table.AcceptChanges();
            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                advanced_table.Rows.Add(new object[] { null });
            }
            advanced_table.AcceptChanges();
            rowIndex = 0;
        }

        #region DataGridView
        private void GetEPC(string pc, string epc, string crc, string rssi , string per)
        {
            this.dgv_epc2.ClearSelection();
            bool isFoundEpc = false;
            string newEpcItemCnt;
            int indexEpc = 0;

            int EpcItemCnt;
            if (rowIndex <= initDataTableLen)
            {
                EpcItemCnt = rowIndex;
            }
            else
            {
                EpcItemCnt = basic_table.Rows.Count;
                EpcItemCnt = advanced_table.Rows.Count;
            }

            for (int j = 0; j < EpcItemCnt; j++)
            {
                if (basic_table.Rows[j][2].ToString() == epc && basic_table.Rows[j][1].ToString() == pc)
                {
                    indexEpc = j;
                    isFoundEpc = true;
                    break;
                }
            }

            if (EpcItemCnt < initDataTableLen) //basic_table.Rows[EpcItemCnt][0].ToString() == ""
            {
                if (!isFoundEpc || EpcItemCnt == 0)
                {
                    if (EpcItemCnt + 1 < 10)
                    {
                        newEpcItemCnt = "0" + Convert.ToString(EpcItemCnt + 1);
                    }
                    else
                    {
                        newEpcItemCnt = Convert.ToString(EpcItemCnt + 1);
                    }
                    basic_table.Rows[EpcItemCnt][0] = newEpcItemCnt; // EpcItemCnt + 1;
                    basic_table.Rows[EpcItemCnt][1] = pc;
                    basic_table.Rows[EpcItemCnt][2] = epc;
                    basic_table.Rows[EpcItemCnt][3] = crc;
                    basic_table.Rows[EpcItemCnt][4] = rssi;
                    basic_table.Rows[EpcItemCnt][5] = 1;
                    basic_table.Rows[EpcItemCnt][6] = "0.000";
                    basic_table.Rows[EpcItemCnt][7] = System.DateTime.Now.ToString(timeFormat);

                    advanced_table.Rows[EpcItemCnt][0] = newEpcItemCnt; // EpcItemCnt + 1;
                    advanced_table.Rows[EpcItemCnt][1] = pc;
                    advanced_table.Rows[EpcItemCnt][2] = epc;
                    advanced_table.Rows[EpcItemCnt][3] = crc;
                    advanced_table.Rows[EpcItemCnt][4] = 1;

                    rowIndex++;
                }
                else
                {
                    if (indexEpc + 1 < 10)
                    {
                        newEpcItemCnt = "0" + Convert.ToString(indexEpc + 1);
                    }
                    else
                    {
                        newEpcItemCnt = Convert.ToString(indexEpc + 1);
                    }
                    basic_table.Rows[indexEpc][0] = newEpcItemCnt; // indexEpc + 1;
                    basic_table.Rows[indexEpc][4] = rssi;
                    basic_table.Rows[indexEpc][5] = Convert.ToInt32(basic_table.Rows[indexEpc][5].ToString()) + 1;
                    basic_table.Rows[indexEpc][6] = per;
                    basic_table.Rows[indexEpc][7] = System.DateTime.Now.ToString(timeFormat);

                    advanced_table.Rows[indexEpc][0] = newEpcItemCnt; // indexEpc + 1;
                    advanced_table.Rows[indexEpc][4] = Convert.ToInt32(advanced_table.Rows[indexEpc][4].ToString()) + 1;
                }
            }
            else
            {
                if (!isFoundEpc || EpcItemCnt == 0)
                {
                    if (EpcItemCnt + 1 < 10)
                    {
                        newEpcItemCnt = "0" + Convert.ToString(EpcItemCnt + 1);
                    }
                    else
                    {
                        newEpcItemCnt = Convert.ToString(EpcItemCnt + 1);
                    }
                    basic_table.Rows.Add(new object[] { newEpcItemCnt, pc, epc, crc, rssi, "1", "0.000", DateTime.Now.ToString(timeFormat) });
                    advanced_table.Rows.Add(new object[] { newEpcItemCnt, pc, epc, crc, "1" });
                    rowIndex++;
                }
                else
                {
                    if (indexEpc + 1 < 10)
                    {
                        newEpcItemCnt = "0" + Convert.ToString(indexEpc + 1);
                    }
                    else
                    {
                        newEpcItemCnt = Convert.ToString(indexEpc + 1);
                    }
                    basic_table.Rows[indexEpc][0] = newEpcItemCnt; // indexEpc + 1;
                    basic_table.Rows[indexEpc][4] = rssi;
                    basic_table.Rows[indexEpc][5] = Convert.ToInt32(basic_table.Rows[indexEpc][5].ToString()) + 1;
                    basic_table.Rows[indexEpc][6] = per;
                    basic_table.Rows[indexEpc][7] = System.DateTime.Now.ToString(timeFormat);

                    advanced_table.Rows[indexEpc][0] = newEpcItemCnt; // indexEpc + 1;
                    advanced_table.Rows[indexEpc][4] = Convert.ToInt32(advanced_table.Rows[indexEpc][4].ToString()) + 1;
                }
            }
        }
        private void dgvEpcBasic_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvEpcBasic.ClearSelection();
            //double totalCnt = 0;
            //if (e.ListChangedType == ListChangedType.ItemChanged || e.ListChangedType == ListChangedType.ItemMoved)
            {
                //for (int i = 0; i < this.dgvEpcBasic.Rows.Count; i++)
                //{
                //    string cnt = this.dgvEpcBasic.Rows[i].Cells[5].Value.ToString();
                //    if (null != cnt && !"".Equals(cnt))
                //    {
                //        totalCnt += Convert.ToInt32(cnt);
                //    }
                //}
                //for (int i = 0; i < this.dgvEpcBasic.Rows.Count; i++)
                //{
                //    string cnt = this.dgvEpcBasic.Rows[i].Cells[5].Value.ToString();
                //    if (null != cnt && !"".Equals(cnt))
                //    {
                //        int sigleCnt = Convert.ToInt32(cnt);
                //        int r = 0xFF & (int)(sigleCnt / totalCnt * 255);
                //        this.dgvEpcBasic.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(0xff,255 - r,255 - r);
                //    }
                //}
                pbx_Inv_Indicator.Visible = true;
            }
        }
        private void dgv_epc2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //if (e.ListChangedType == ListChangedType.ItemChanged || e.ListChangedType == ListChangedType.ItemAdded)
            {
                for (int i = 0; i < this.dgv_epc2.Rows.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        this.dgv_epc2.Rows[i].DefaultCellStyle.BackColor = Color.AliceBlue;
                    }
                }
            }
        }
        private DataTable BasicGetEPCHead()
        {
            basic_table.TableName = "EPC";
            basic_table.Columns.Add("No.", typeof(string)); //0
            basic_table.Columns.Add("PC", typeof(string)); //1
            basic_table.Columns.Add("EPC", typeof(string)); //2
            basic_table.Columns.Add("CRC", typeof(string)); //3
            basic_table.Columns.Add("RSSI(dBm)", typeof(string)); //4
            basic_table.Columns.Add("CNT", typeof(string)); //5
            basic_table.Columns.Add("PER(%)", typeof(string)); //6
            basic_table.Columns.Add("Time", typeof(string)); //7

            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                basic_table.Rows.Add(new object[] { null });
            }
            basic_table.AcceptChanges();

            return basic_table;
        }

        private DataTable AdvancedGetEPCHead()
        {
            advanced_table.TableName = "EPC";
            advanced_table.Columns.Add("No.", typeof(string)); //0
            advanced_table.Columns.Add("PC", typeof(string)); //1
            advanced_table.Columns.Add("EPC", typeof(string)); //2
            advanced_table.Columns.Add("CRC", typeof(string)); //3
            advanced_table.Columns.Add("CNT", typeof(string)); //4

            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                advanced_table.Rows.Add(new object[] { null });
            }
            advanced_table.AcceptChanges();

            return advanced_table;
        }
        private void Basic_DGV_ColumnsWidth(DataGridView dataGridView1)
        {
            //dataGridView1.Columns[6].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeight = 40;
            //dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[0].Width = 40;
            //dataGridView1.Columns[0].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[0].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[1].Width = 60;
            //dataGridView1.Columns[1].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[1].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[2].Width = 290;
            //dataGridView1.Columns[2].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[2].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[3].Width = 60;
            //dataGridView1.Columns[3].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[3].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].Width = 75;
            //dataGridView1.Columns[4].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[4].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dataGridView1.Columns[5].Width = 70;
            ////dataGridView1.Columns[5].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            //dataGridView1.Columns[5].Resizable = DataGridViewTriState.False;
            //dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[5].Width = 70;
            //dataGridView1.Columns[5].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[5].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[6].Width = 72;
            //dataGridView1.Columns[6].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[6].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[7].Visible = false;
            //dataGridView1.Columns[7].Width = 72;
            ////dataGridView1.Columns[7].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            //dataGridView1.Columns[7].Resizable = DataGridViewTriState.False;
            //dataGridView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void Advanced_DGV_ColumnsWidth(DataGridView dataGridView1)
        {
            //dataGridView1.Columns[6].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeight = 40;
            //dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[0].Width = 40;
            //dataGridView1.Columns[0].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[0].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[1].Width = 60;
            //dataGridView1.Columns[1].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[1].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[2].Width = 240;
            //dataGridView1.Columns[2].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[2].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[3].Width = 60;
            //dataGridView1.Columns[3].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[3].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[4].Width = 52;
            //dataGridView1.Columns[6].DefaultCellStyle.Font = new Font("Lucida Console", 10);
            dataGridView1.Columns[4].Resizable = DataGridViewTriState.False;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        #endregion

        #region others
        private void btn_clear_cnt_Click(object sender, EventArgs e)
        {
            txtCOMRxCnt.Text = "0";
            txtCOMTxCnt.Text = "0";
            txtCOMRxCnt_adv.Text = "0";
            txtCOMTxCnt_adv.Text = "0";
        }

        private void btn_clear_cnt_adv_Click(object sender, EventArgs e)
        {
            txtCOMRxCnt.Text = "0";
            txtCOMTxCnt.Text = "0";
            txtCOMRxCnt_adv.Text = "0";
            txtCOMTxCnt_adv.Text = "0";
        }

        private string[] String16toString2(string S)
        {
            string[] S_array = new string[8];
            int intS = Convert.ToInt32(S, 16);
            for (int i = 7; i >= 0; i--)
            {
                S_array[i] = "0";
                if (intS >= System.Math.Pow(2, i)) S_array[i] = "1";
                intS = intS - Convert.ToInt32(S_array[i]) * Convert.ToInt32(System.Math.Pow(2, i));
            }
            return S_array;
        }

        private string StringToString(string S)
        {
            string Str = null;

            int S_num = Convert.ToInt32(S, 16);
            if (S_num < 16)
            {
                Str = "0" + S;
            }
            else
            {
                Str = S;
            }
            return Str;
        }

        private string[] StringArrayToStringArray(string[] S)
        {
            string[] Str = new string[S.Length];
            for (int i = 0; i < S.Length; i++)
            {
                int S_num = Convert.ToInt32(S[i], 16);
                if (S_num < 16)
                {
                    Str[i] = "0" + S[i];
                }
                else
                {
                    Str[i] = S[i];
                }
            }
            return Str;
        }

        private byte[] StringsToBytes(string[] B)
        {
            byte[] BToInt32 = new byte[B.Length];
            for (int i = 0; i < B.Length; i++)
            {
                BToInt32[i] = StringToByte(B[i]);
            }
            return BToInt32;
        }

        private byte StringToByte(string Str)
        {
            for (int i = Str.Length; i < 2; i++)
            {
                Str += "0";
            }
            return (byte)(Convert.ToInt32(Str, 16));
        }

        private string AutoAddSpace(string Str)
        {
            String StrDone = string.Empty;
            int i;
            for (i = 0; i < (Str.Length - 2); i = i + 2)
            {
                StrDone = StrDone + Str[i] + Str[i + 1] + " ";
            }
            if (Str.Length % 2 == 0 && Str.Length != 0)
            {
                if (Str.Length == i + 1)
                {
                    StrDone = StrDone + Str[i];
                }
                else
                {
                    StrDone = StrDone + Str[i] + Str[i + 1];
                }
            }
            else
            {
                StrDone = StrDone + StringToString(Str[i].ToString());
            }
            return StrDone;
        }

        private void txtReceive_DoubleClick(object sender, EventArgs e)
        {
            txtReceive.SelectAll();
        }

        private void txtSelMask_DoubleClick(object sender, EventArgs e)
        {
            txtSelMask.SelectAll();
        }

        private void txtSend_DoubleClick(object sender, EventArgs e)
        {
            txtSend.SelectAll();
        }


        private void txtInvtReadData_DoubleClick(object sender, EventArgs e)
        {
            txtInvtRWData.SelectAll();
        }

        private void txtGetSelMask_DoubleClick(object sender, EventArgs e)
        {
            txtGetSelMask.SelectAll();
        }
        #endregion

        #region Advanced Tab received data display
        private void btn_clear_epc2_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";
            basic_table.Clear();
            advanced_table.Clear();
            LoopNum_cnt = 0;
            FailEPCNum = 0;
            SucessEPCNum = 0;
            db_LoopNum_cnt = 0;
            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                basic_table.Rows.Add(new object[] { null });
            }
            basic_table.AcceptChanges();
            for (int i = 0; i <= initDataTableLen - 1; i++)
            {
                advanced_table.Rows.Add(new object[] { null });
            }
            advanced_table.AcceptChanges();
            rowIndex = 0;

        }

        public void dataGrid_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int rowIndex = dgv_epc2.CurrentRow.Index;
            if (dgv_epc2.Rows[rowIndex].Cells[2].Value.ToString() != null)
            {
                txtSelMask.Text = dgv_epc2.Rows[rowIndex].Cells[2].Value.ToString();
            }
            txtSelLength.Text = (txtSelMask.Text.Replace(" ", "").Length * 4).ToString("X2");
        }

        private void btn_invt2_Click(object sender, EventArgs e)
        {
            LoopNum_cnt = LoopNum_cnt + 1;
            txtSend.Text = Commands.BuildReadSingleFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }
        #endregion

        #region Advanced Tab send data region

        private void btn_setquery_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int intDR = this.cbxDR.SelectedIndex;
            int intM = this.cbxM.SelectedIndex;
            int intTRext = this.cbxTRext.SelectedIndex;
            int intSel = this.cbxSel.SelectedIndex;
            int intSession = this.cbxSession.SelectedIndex;

            int intTarget = this.cbxTarget.SelectedIndex;
            int intQ = this.cbxQAdv.SelectedIndex;

            txtSend.Text = Commands.BuildSetQueryFrame(intDR, intM, intTRext, intSel, intSession, intTarget, intQ);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btn_getquery_Click(object sender, EventArgs e)
        {
            txtSend.Text = Commands.BuildGetQueryFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }
        #endregion

        private void btn_invt_multi_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int loopCnt = Convert.ToInt32(txtRDMultiNum.Text);
            txtSend.Text = Commands.BuildReadMultiFrame(loopCnt);
            Sp.GetInstance().Send(txtSend.Text);
            tmrCheckEpc.Enabled = true;
        }

        private void btn_stop_rd_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtSend.Text = Commands.BuildStopReadFrame();
            Sp.GetInstance().Send(txtSend.Text);
            tmrCheckEpc.Enabled = false;
        }

        private void select()
        {
            if (Sp.GetInstance().IsOpen() == false)
            {
                return;
            }
            int intSelTarget = this.cbxSelTarget.SelectedIndex;
            int intAction = this.cbxAction.SelectedIndex;
            int intSelMemBank = this.cbxSelMemBank.SelectedIndex;

            int intSelPointer = Convert.ToInt32((txtSelPrt3.Text + txtSelPrt2.Text + txtSelPrt1.Text + txtSelPrt0.Text), 16);
            int intMaskLen = Convert.ToInt32(txtSelLength.Text, 16);
            int intSelDataMSB = intSelMemBank + intAction * 4 + intSelTarget * 32;
            int intTruncate = 0;

            Sp.GetInstance().Send(Commands.BuildSetSelectFrame(intSelTarget, intAction, intSelMemBank, intSelPointer, intMaskLen, intTruncate, txtSelMask.Text));
            Thread.Sleep(50);
        }

        private void btn_invtread_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strAccessPasswd = txtRwAccPassWord.Text.Replace(" ", "");
            if (strAccessPasswd.Length != 8)
            {
                MessageBox.Show("访问密码设置为2个字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int wordPtr = Convert.ToInt32((txtWordPtr1.Text.Replace(" ","") + txtWordPtr0.Text.Replace(" ","")),16);
            int wordCnt =Convert.ToInt32((txtWordCnt1.Text.Replace(" ","") + txtWordCnt0.Text.Replace(" ","")),16);

            int intMemBank = cbxMemBank.SelectedIndex;

            select();

            txtSend.Text = Commands.BuildReadDataFrame(strAccessPasswd, intMemBank, wordPtr, wordCnt);
            Sp.GetInstance().Send(txtSend.Text);

        }

        private String int2HexString(int a)
        {
            byte byte_a = Convert.ToByte(a);
            string str = byte_a.ToString("x").ToUpper();
            str = StringToString(str);
            return str;
        }
        private void btnSetSelect_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int intSelTarget = this.cbxSelTarget.SelectedIndex;
            int intAction = this.cbxAction.SelectedIndex;
            int intSelMemBank = this.cbxSelMemBank.SelectedIndex;

            int intSelPointer = Convert.ToInt32((txtSelPrt3.Text + txtSelPrt2.Text + txtSelPrt1.Text + txtSelPrt0.Text),16);
            int intMaskLen = Convert.ToInt32(txtSelLength.Text, 16);
            int intSelDataMSB = intSelMemBank + intAction * 4 + intSelTarget * 32;
            int intTruncate = 0;
            if (this.ckxTruncated.Checked == true)
            {
                intTruncate = 0x80;
            }

            txtSend.Text = Commands.BuildSetSelectFrame(intSelTarget, intAction, intSelMemBank, intSelPointer, intMaskLen, intTruncate, txtSelMask.Text);
            Sp.GetInstance().Send(txtSend.Text);
            //inv_mode.Checked = true;
        }

        private void btnGetSelect_Click(object sender, EventArgs e)
        {
            txtSend.Text = Commands.BuildGetSelectFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnInvtWrtie_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strAccessPasswd = txtRwAccPassWord.Text.Replace(" ", "");
            if (strAccessPasswd.Length != 8)
            {
                MessageBox.Show("访问密码为二个字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string strDate4Write = txtInvtRWData.Text.Replace(" ", "");

            int intMemBank = cbxMemBank.SelectedIndex;
            int wordPtr = Convert.ToInt32((txtWordPtr1.Text.Replace(" ","") + txtWordPtr0.Text.Replace(" ","")),16);
            int wordCnt = strDate4Write.Length / 4; // in word!

            if (strDate4Write.Length % 4 != 0)
            {
                MessageBox.Show("写入的数据应该为整型的倍数", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //if (strDate4Write.Length > 16 * 4)
            //{
            //    MessageBox.Show("Write Data Length Limit is 16 Words", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            select();

            txtSend.Text = Commands.BuildWriteDataFrame(strAccessPasswd, intMemBank
                , wordPtr, wordCnt, strDate4Write);
            Sp.GetInstance().Send(txtSend.Text);

        }

        private void buttonLock_Click(object sender, EventArgs e)
        {
            if (textBoxLockAccessPwd.Text.Length == 0) return;
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            select();

            int lockPayload = buildLockPayload();
            txtSend.Text = Commands.BuildLockFrame(textBoxLockAccessPwd.Text, lockPayload);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private int buildLockPayload()
        {
            int ld = 0;
            Commands.lock_payload_type payload;
            if (checkBoxKillPwd.Checked)
            {
                payload = Commands.genLockPayload((byte)cbxLockKillAction.SelectedIndex, 0x00);
                ld |= (payload.byte0 << 16) | (payload.byte1 << 8) | (payload.byte2);
            }
            if (checkBoxAccessPwd.Checked)
            {
                payload = Commands.genLockPayload((byte)cbxLockAccessAction.SelectedIndex, 0x01);
                ld |= (payload.byte0 << 16) | (payload.byte1 << 8) | (payload.byte2);
            }
            if (checkBoxEPC.Checked)
            {
                payload = Commands.genLockPayload((byte)cbxLockEPCAction.SelectedIndex, 0x02);
                ld |= (payload.byte0 << 16) | (payload.byte1 << 8) | (payload.byte2);
            }
            if (checkBoxTID.Checked)
            {
                payload = Commands.genLockPayload((byte)cbxLockTIDAction.SelectedIndex, 0x03);
                ld |= (payload.byte0 << 16) | (payload.byte1 << 8) | (payload.byte2);
            }
            if (checkBoxUser.Checked)
            {
                payload = Commands.genLockPayload((byte)cbxLockUserAction.SelectedIndex, 0x04);
                ld |= (payload.byte0 << 16) | (payload.byte1 << 8) | (payload.byte2);
            }
            return ld;
        }

        private void buttonKill_Click(object sender, EventArgs e)
        {
            if (textBoxKillPwd.Text.Length == 0) return;

            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strKillPasswd = textBoxKillPwd.Text.Replace(" ", "");
            if (strKillPasswd.Length != 8)
            {
                MessageBox.Show("访问密码为二个字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int killRfu = 0;
            string strKillRfu = textBoxKillRFU.Text.Replace(" ", "");
            if (strKillRfu.Length == 0)
            {
                killRfu = 0;
            }
            else if (strKillRfu.Length != 3)
            {
                MessageBox.Show("清除RFU命令应该为 3 bits!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                try
                {
                    killRfu = Convert.ToInt32(strKillRfu, 2);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Convert Kill RFU fail." + ex.Message);
                    MessageBox.Show("清除 RFU 命令应该为 3 bits!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            select();

            txtSend.Text = Commands.BuildKillFrame(strKillPasswd, killRfu);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void inv_mode_CheckedChanged(object sender, EventArgs e)
        {
            if (inv_mode.Checked)
            {
                txtSend.Text = Commands.BuildSetInventoryModeFrame(ConstCode.INVENTORY_MODE0);  //INVENTORY_MODE0
            }
            else
            {
                txtSend.Text = Commands.BuildSetInventoryModeFrame(ConstCode.INVENTORY_MODE1);  //INVENTORY_MODE1
            }
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void ckxTruncated_CheckedChanged(object sender, EventArgs e)
        {
            if (ckxTruncated.Checked)
            {
                int intSelTarget = this.cbxSelTarget.SelectedIndex;
                int intSelMemBank = this.cbxSelMemBank.SelectedIndex;
                if (intSelTarget != 4 || intSelMemBank != 1)
                {
                    MessageBox.Show("Select Target should be 100 and MemBank should be EPC", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ckxTruncated.Checked = false;
                }
            }
        }

        private void btnSetFhss_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (btnSetFhss.Text == "设置跳频")
            {

                txtSend.Text = Commands.BuildSetFhssFrame(ConstCode.SET_ON);
            }
            else
            {
                txtSend.Text = Commands.BuildSetFhssFrame(ConstCode.SET_OFF);
            }
            Sp.GetInstance().Send(txtSend.Text);

            if (btnSetFhss.Text == "设置跳频")
            {
                btnSetFhss.Text = "取消跳频";
            }
            else
            {
                btnSetFhss.Text = "设置跳频";
            }
        }

        private string curRegion = ConstCode.REGION_CODE_CHN2;
        private string hardwareVersion;
        private bool checkingReaderAvailable;
        private bool readerConnected;
        private void btnSetRegion_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string frame = string.Empty;
            if (cbxRegion.SelectedIndex == 0) // China 2
            {
                frame = Commands.BuildSetRegionFrame(ConstCode.REGION_CODE_CHN2);
                curRegion = ConstCode.REGION_CODE_CHN2;
            }
            else if (cbxRegion.SelectedIndex == 1) // China 1
            {
                frame = Commands.BuildSetRegionFrame(ConstCode.REGION_CODE_CHN1);
                curRegion = ConstCode.REGION_CODE_CHN1;
            }
            else if (cbxRegion.SelectedIndex == 2) // US
            {
                frame = Commands.BuildSetRegionFrame(ConstCode.REGION_CODE_US);
                curRegion = ConstCode.REGION_CODE_US;
            }
            else if (cbxRegion.SelectedIndex == 3) // Europe
            {
                frame = Commands.BuildSetRegionFrame(ConstCode.REGION_CODE_EUR);
                curRegion = ConstCode.REGION_CODE_EUR;
            }
            else if (cbxRegion.SelectedIndex == 4) // Korea
            {
                frame = Commands.BuildSetRegionFrame(ConstCode.REGION_CODE_KOREA);
                curRegion = ConstCode.REGION_CODE_KOREA;
            }
            
            txtSend.Text = frame;
            Sp.GetInstance().Send(frame);
            cbxChannel.SelectedIndex = 0;
        }

        private void cbxRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbxChannel.Items.Clear();

            switch (cbxRegion.SelectedIndex)
            {
                case 0 : // China 2
                    for (int i = 0; i < 20; i++)
                    {
                        this.cbxChannel.Items.Add((920.125 + i * 0.25).ToString() + "MHz");
                    }
            	    break;
                case 1: // China 1
                    for (int i = 0; i < 20; i++)
                    {
                        this.cbxChannel.Items.Add((840.125 + i * 0.25).ToString() + "MHz");
                    }
                    break;
                case 2: // US
                    for (int i = 0; i < 52; i++)
                    {
                        this.cbxChannel.Items.Add((902.25 + i * 0.5).ToString() + "MHz");
                    }
                    break;
                case 3: // Europe
                    for (int i = 0; i < 15; i++)
                    {
                        this.cbxChannel.Items.Add((865.1 + i * 0.2).ToString() + "MHz");
                    }
                        break;
                case 4:  // Korea
                        for (int i = 0; i < 32; i++)
                        {
                            this.cbxChannel.Items.Add((917.1 + i * 0.2).ToString() + "MHz");
                        }
                        break;
                default :
                        break;
            }
            cbxChannel.SelectedIndex = 0;
        }

        private void btnGetChannel_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            txtSend.Text = Commands.BuildGetRfChannelFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSetPaPower_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int powerDBm = 0;
            float powerFloat = 0;
            try
            {
                powerFloat = float.Parse(cbxPaPower.SelectedItem.ToString().Replace("dBm", ""));
                powerDBm = (int)(powerFloat * 100);
            }
            catch (Exception formatException)
            {
                Console.WriteLine(formatException.ToString());
                switch (cbxPaPower.SelectedIndex)
                {
                    case 5:
                        powerDBm = 1250;
                        break;
                    case 4:
                        powerDBm = 1400;
                        break;
                    case 3:
                        powerDBm = 1550;
                        break;
                    case 2:
                        powerDBm = 1700;
                        break;
                    case 1:
                        powerDBm = 1850;
                        break;
                    case 0:
                        powerDBm = 2000;
                        break;
                    default:
                        powerDBm = 2000;
                        break;
                }
            }
            txtSend.Text = Commands.BuildSetPaPowerFrame((Int16)powerDBm);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnGetPaPower_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            txtSend.Text = Commands.BuildGetPaPowerFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSetModemPara_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            int mixerGain = cbxMixerGain.SelectedIndex;
            int IFAmpGain = cbxIFAmpGain.SelectedIndex;
            int signalTh = Convert.ToInt32(tbxSignalThreshold.Text,16);
            txtSend.Text = Commands.BuildSetModemParaFrame(mixerGain, IFAmpGain, signalTh);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnGetModemPara_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            txtSend.Text = Commands.BuildReadModemParaFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private string ParseErrCode(int errorCode)
        {
            switch (errorCode & 0x0F)
            {
                case ConstCode.ERROR_CODE_OTHER_ERROR :
                    return "Other Error";
                case ConstCode.ERROR_CODE_MEM_OVERRUN:
                    return "Memory Overrun";
                case ConstCode.ERROR_CODE_MEM_LOCKED:
                    return "Memory Locked";
                case ConstCode.ERROR_CODE_INSUFFICIENT_POWER:
                    return "Insufficient Power";
                case ConstCode.ERROR_CODE_NON_SPEC_ERROR:
                    return "Non-specific Error";
                default :
                    return "Non-specific Error";
            }
        }

        private void btnScanJammer_Click(object sender, EventArgs e)
        {
            txtSend.Text = Commands.BuildScanJammerFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void saveAsTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();

            //File type filter
            save.Filter = "*.csv|*.CSV|*.*|(*.*)";

            if (save.ShowDialog() == DialogResult.OK)
            {
                string name = save.FileName;
                FileInfo info = new FileInfo(name);
                //info.Delete();
                StreamWriter writer = null;
                try
                {
                    writer = info.CreateText();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    writer.Write("No.,PC,EPC,CRC,RSSI(dBm),CNT,PER(%),");
                    writer.WriteLine();
                    for (int i = 0; i < basic_table.Rows.Count; i++)
                    {
                        for(int j = 0; j < basic_table.Columns.Count; j++)
                        {
                            writer.Write(basic_table.Rows[i][j].ToString()+",");
                        }
                        writer.WriteLine();
                        //writer.Write(richTextBox1.Text);
                    }
                    writer.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnScanRssi_Click(object sender, EventArgs e)
        {
            txtSend.Text = Commands.BuildScanRssiFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void timerCheckReader_Tick(object sender, EventArgs e)
        {
            timerCheckReader.Enabled = false;
            if (hardwareVersion == "")
            {
                MessageBox.Show("连接读写器失败, 请检查固件是否下载!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                readerConnected = false;
            }
            else
            {
                //MessageBox.Show("Connect Success! Hardware version: " + hardwareVersion, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                readerConnected = true;
            }
        }

        private void Reset_FW_Click(object sender, EventArgs e)
        {
            txtSend.Text = "BB 00 55 00 00 55 7E";
            Sp.GetInstance().Send(txtSend.Text);
        }

        int lastRecCnt = 0;
        private void tmrCheckEpc_Tick(object sender, EventArgs e)
        {
            if (lastRecCnt == Convert.ToInt32(txtCOMRxCnt.Text)) // no data received during last Tick, it may mean the Read Continue stoped
            {
                tmrCheckEpc.Enabled = false;
                return;
            }
            lastRecCnt = Convert.ToInt32(txtCOMRxCnt.Text);
            DateTime now = System.DateTime.Now;
            DateTime dt;
            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();

            dtFormat.LongDatePattern = timeFormat;

            int timeout = (10 * tmrCheckEpc.Interval);
            for (int i = 0; i < this.dgvEpcBasic.Rows.Count; i++)
            {
                string time = this.dgvEpcBasic.Rows[i].Cells[7].Value.ToString();
                if (null != time && !"".Equals(time))
                {
                    //dt = Convert.ToDateTime(time, dtFormat);
                    //dt = DateTime.ParseExact(time, timeFormat, CultureInfo.InvariantCulture);
                    if (DateTime.TryParse(time,out dt))
                    {
                        TimeSpan sub = now.Subtract(dt);
                        if (sub.TotalMilliseconds > timeout)
                        {
                            this.dgvEpcBasic.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        }
                        //else if ((sub.TotalMilliseconds > (tmrCheckEpc.Interval + 100)))
                        //{
                        //    this.dgvEpcBasic.Rows[i].DefaultCellStyle.BackColor = Color.Pink;
                        //}
                        else
                        {
                            int r = 0xFF & (int)(sub.TotalMilliseconds / timeout * 255);
                            //this.dgvEpcBasic.Rows[i].DefaultCellStyle.BackColor = Color.White;
                            this.dgvEpcBasic.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(0xff,255 - r ,255 - r);

                        }
                    }

                }
            }

        }

        private void btnSetIO_Click(object sender, EventArgs e)
        {
            byte para0 = 0x01;
            byte para1 = (byte)(cbxIO.SelectedIndex + 1);
            byte para2 = (byte)cbxIoLevel.SelectedIndex;
            txtSend.Text = Commands.BuildIoControlFrame(para0, para1, para2);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSetIoDirection_Click(object sender, EventArgs e)
        {
            byte para0 = 0x00;
            byte para1 = (byte)(cbxIO.SelectedIndex + 1);
            byte para2 = (byte)cbxIoDircetion.SelectedIndex;
            txtSend.Text = Commands.BuildIoControlFrame(para0, para1, para2);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnGetIO_Click(object sender, EventArgs e)
        {
            byte para0 = 0x02;
            byte para1 = (byte)(cbxIO.SelectedIndex + 1);
            byte para2 = 0x00;
            txtSend.Text = Commands.BuildIoControlFrame(para0, para1, para2);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSetMode_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            int mixerGain = cbxMode.SelectedIndex;
            int IFAmpGain = 6;
            int signalTh = Convert.ToInt32("00A0", 16);
            txtSend.Text = Commands.BuildSetModemParaFrame(mixerGain, IFAmpGain, signalTh);
            Sp.GetInstance().Send(txtSend.Text);
            //txtSend.Text = Commands.BuildSetReaderEnvModeFrame((byte)cbxMode.SelectedIndex);
            //Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSaveConfigToNv_Click(object sender, EventArgs e)
        {
            byte NV_enable = cbxSaveNvConfig.Checked ? (byte)0x01 : (byte)0x00;
            txtSend.Text = Commands.BuildSaveConfigToNvFrame(NV_enable);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnSetModuleSleep_Click(object sender, EventArgs e)
        {
            txtSend.Text = Commands.BuildSetModuleSleepFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnInsertRfCh_Click(object sender, EventArgs e)
        {
            byte[] channelList;
            int chIndexBegin = Convert.ToInt32(txtChIndexBegin.Text);
            int chIndexEnd = Convert.ToInt32(txtChIndexEnd.Text);
            byte channelNum = (byte)(chIndexEnd - chIndexBegin + 1);
            channelList = new byte[channelNum];
            for (int i = chIndexBegin; i <= chIndexEnd; i++)
            {
                channelList[i - chIndexBegin] = (byte)i;
            }
            txtSend.Text = Commands.BuildInsertRfChFrame(channelNum, channelList);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnChangeConfig_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strAccessPasswd = tbxNxpCmdAccessPwd.Text.Replace(" ", "");
            if (strAccessPasswd.Length != 8)
            {
                MessageBox.Show("访问密码为二字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            select();

            txtSend.Text = Commands.BuildNXPChangeConfigFrame(strAccessPasswd, Convert.ToInt32(txtConfigData.Text.Replace(" ",""), 16));
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnChangeEas_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strAccessPasswd = tbxNxpCmdAccessPwd.Text.Replace(" ", "");
            if (strAccessPasswd.Length != 8)
            {
                MessageBox.Show("访问密码为二字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            select();

            txtSend.Text = Commands.BuildNXPChangeEasFrame(strAccessPasswd, cbxSetEas.Checked);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnEasAlarm_Click(object sender, EventArgs e)
        {
            //txtSend.Text = Commands.BuildFrame(ConstCode.FRAME_TYPE_CMD, "E4");
            txtSend.Text = Commands.BuildNXPEasAlarmFrame();
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnReadProtect_Click(object sender, EventArgs e)
        {
            if (bAutoSend == true)
            {
                MessageBox.Show("请停止连续盘存", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string strAccessPasswd = tbxNxpCmdAccessPwd.Text.Replace(" ", "");
            if (strAccessPasswd.Length != 8)
            {
                MessageBox.Show("访问密码为二字节!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            select();

            txtSend.Text = Commands.BuildNXPReadProtectFrame(strAccessPasswd, cbxReadProtectReset.Checked);
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void btnChangeBaudrate_Click(object sender, EventArgs e)
        {
            int baudrate = Convert.ToInt32(cbxBaudRate.SelectedItem.ToString(), 10) / 100;
            txtSend.Text = Commands.BuildFrame(ConstCode.FRAME_TYPE_CMD, "11", baudrate.ToString("X4"));
            Sp.GetInstance().Send(txtSend.Text);
        }

        private void txtOperateEpc_DoubleClick(object sender, EventArgs e)
        {
            txtOperateEpc.SelectAll();
        }

    }
}
