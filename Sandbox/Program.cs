using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            int a = 983017;
            byte[] arr = new byte[4];

            GetByteArrayFromInt(a, arr, 0);
            int x = 4;
            int y = GetIntFromByteArray(arr, 0);
            int z = 4;

        }

        public static int GetIntFromByteArray(byte[] sourceArray, int startPos)
        {
            // Little Endian
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result |= sourceArray[startPos + i] << (i * 8);
            }
            return result;
        }

        public static void GetByteArrayFromInt(int value, byte[] destinationArray, int startPos)
        {
            // Little Endian?
            for (int i = 0; i < 4; i++)
            {
                byte dataToWrite= (byte)(value >> (8 * i));
                destinationArray[startPos + i] = dataToWrite;
            }
        }
    }
}
