using System;
using Microsoft.SPOT;

namespace Roomba.Utils
{
    public static class Common
    {
        public static byte GetHighByte(short value)
        {
            return (byte)(value >> 8);
        }

        public static byte GetLowByte(short value)
        {
            // 0xFF = 00000000 11111111
            return (byte)(value & 0xFF);
        }

        public static short AssembleByte(byte high, byte low)
        {
            return (short)(high << 8 | low);
        }

        /// <summary>
        /// Convert int to the byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationArray"></param>
        /// <param name="startPos"></param>
        public static void GetByteArrayFromInt(int value, byte[] destinationArray, int startPos)
        {
            // Big Endian
            for (int i = 0; i < 4; i++)
            {
                destinationArray[startPos + i] = (byte)(value >> (8 * i));
            }
        }

        /// <summary>
        /// Convert byte array to an integer
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public static int GetIntFromByteArray(byte[] sourceArray, int startPos)
        {
            // Big Endian
            int result = 0;

            for (int i = 0; i < 4; i++)
            {
                result |= sourceArray[startPos + i] << (i * 8);
            }

            return result;
        }
    }
}
