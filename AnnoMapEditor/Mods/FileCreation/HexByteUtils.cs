using System;
using System.Text;

namespace AnnoMapEditor.Mods.FileCreation
{
    internal static class HexByteUtils
    {
        public static string Int32ToLittleEndianHex(int input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BytesToContinuousHex(BitConverter.GetBytes(input));
            }
            else
            {
                return BytesToContinuousHex(ByteArrayReverse(BitConverter.GetBytes(input)));
            }
        }

        public static string Int16ToLittleEndianHex(short input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BytesToContinuousHex(BitConverter.GetBytes(input));
            }
            else
            {
                return BytesToContinuousHex(ByteArrayReverse(BitConverter.GetBytes(input)));
            }
        }

        public static string ByteToHex(byte input)
        {
            return BytesToContinuousHex(new byte[] { input });
        }

        public static string BoolToHex(bool input)
        {
            if (input)
                return ByteToHex((byte)1);
            return ByteToHex((byte)0);
        }

        public static string FloatToLittleEndianHex(float input)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BytesToContinuousHex(BitConverter.GetBytes(input));
            }
            else
            {
                return BytesToContinuousHex(ByteArrayReverse(BitConverter.GetBytes(input)));
            }
        }

        public static string StringToUTF16LittleEndianHex(string input)
        {
            return BytesToContinuousHex(Encoding.Unicode.GetBytes(input));
        }

        private static byte[] ByteArrayReverse(byte[] toReverse)
        {
            byte temp;
            int length = toReverse.Length;
            for (int i = 0; i < length / 2; i++)
            {
                temp = toReverse[i];
                toReverse[i] = toReverse[length - (i + 1)];
                toReverse[length - (i + 1)] = temp;
            }

            return toReverse;
        }

        private static string BytesToContinuousHex(byte[] input)
        {
            return BitConverter.ToString(input).Replace("-", string.Empty);
        }
    }
}
