using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Reader_Cmds
{
    /// <summary>
    /// It Extends from EventArgs, Data is string[] type
    /// </summary>
    public class StrArrEventArgs : EventArgs
    {
        private readonly string[] mData;

        /// <summary>
        /// Constructor, build a StrArrEventArgs from string[]
        /// </summary>
        /// <param name="strArr">Data</param>
        public StrArrEventArgs(string[] strArr)
        {
            mData = strArr;
        }

        /// <summary>
        /// StrArrEventArgs Data Property
        /// </summary>
        public string[] Data
        {
            get { return mData; }
        }
    }
}
