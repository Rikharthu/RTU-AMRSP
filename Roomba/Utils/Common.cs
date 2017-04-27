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
    }
}
