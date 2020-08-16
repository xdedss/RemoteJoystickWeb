using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace RemoteJoystickWeb
{
    static class Ext
    {

        public static void Swap<T>(ref T a, ref T b)
        {
            var c = a;
            a = b;
            b = c;
        }

        public static T Clamp<T>(T num, T min, T max) where T : IComparable
        {
            if (min.CompareTo(max) == 1) return Clamp(num, max, min);
            if (num.CompareTo(max) == 1) return max;
            if (num.CompareTo(min) == -1) return min;
            return num;
        }

        public static void SetAxisf(this vJoy joy, float v, uint rID, HID_USAGES axis)
        {
            v = Clamp(v, -1, 1);
            joy.SetAxis((int)Math.Floor(16384 + 16384 * v), rID, axis);
        }

        /// <summary>
        /// ordinary float
        /// </summary>
        public static float[] ToFloatArray(this byte[] bytes, int startPos, int num)
        {
            float[] floats = new float[num];
            for (int i = 0; i < num; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i * 4 + startPos);
            }
            return floats;
        }

        /// <summary>
        /// short encoded float -1~1
        /// </summary>
        public static float[] ShortToFloatArray(this byte[] bytes, int startPos, int num)
        {
            float[] floats = new float[num];
            for (int i = 0; i < num; i++)
            {
                short s = BitConverter.ToInt16(bytes, i * 2 + startPos);
                floats[i] = s / 32767f;
            }
            return floats;
        }

        public static int[] ToIntArray(this byte[] bytes, int startPos, int num)
        {
            int[] ints = new int[num];
            for (int i = 0; i < num; i++)
            {
                ints[i] = BitConverter.ToInt32(bytes, i * 4 + startPos);
            }
            return ints;
        }

        public static bool GetBit(this byte b, int posR)
        {
            return ((b >> posR) & 1) != 0;
        }

        public static bool GetBit(this int i, int posR)
        {
            return ((i >> posR) & 1) != 0;
        }

        public static string FormatSelf(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    }
}
