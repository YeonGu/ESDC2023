using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string data = "12 BB 33 45 67";
            if (data == null)
            {
                return ;
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
            string strRet = checksum.ToString("X2");
           
            Console.WriteLine(strRet);

            int dataHexLen = 121;
            Console.WriteLine(dataHexLen.ToString("X4"));
            Console.WriteLine(dataHexLen.ToString("X2"));
            Console.WriteLine(dataHexLen.ToString("X6"));
            Console.WriteLine(dataHexLen.ToString("X8"));

            return;
        }
    }
}
