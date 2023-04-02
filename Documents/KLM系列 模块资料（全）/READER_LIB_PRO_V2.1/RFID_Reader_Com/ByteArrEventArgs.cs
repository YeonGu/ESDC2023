using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Reader_Com
{
    /// <summary>
    /// It Extends from EventArgs, Data is byte[] type
    /// </summary>
    public class byteArrEventArgs : EventArgs
    {
        private readonly byte[] mData;

        /// <summary>
        /// Constructor, build a ByteArrEventArgs from byte[]
        /// </summary>
        /// <param name="byteArr">Data</param>
        public byteArrEventArgs(byte[] byteArr)
        {
            mData = byteArr;
        }

        /// <summary>
        /// ByteArrEventArgs Data Property
        /// </summary>
        public byte[] Data
        {
            get { return mData; }
        }
    }
}
