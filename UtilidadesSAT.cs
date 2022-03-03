using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NoriCFDI
{
    class UtilidadesSAT
    {

    }

    public class Internet
    {
        [DllImport("wininet.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool InternetGetConnectedState(out int description, int reservedValue);
        public static bool IsConnectedToInternet()
        {
            int num;
            bool flag = Internet.InternetGetConnectedState(out num, 0);
            return flag;
        }
    }

    public class CookieReader
    {
        private const int InternetCookieHttponly = 8192;

        public static string GetCookie(string url)
        {
            string str;
            int num = 512;
            StringBuilder stringBuilder = new StringBuilder(num);
            if (!CookieReader.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
            {
                if (num >= 0)
                {
                    stringBuilder = new StringBuilder(num);
                    if (!CookieReader.InternetGetCookieEx(url, null, stringBuilder, ref num, 8192, IntPtr.Zero))
                    {
                        str = null;
                        return str;
                    }
                }
                else
                {
                    str = null;
                    return str;
                }
            }
            str = stringBuilder.ToString();
            return str;
        }

        [DllImport("wininet.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        private static extern bool InternetGetCookieEx(string url, string cookieName, StringBuilder cookieData, ref int size, int flags, IntPtr pReserved);
    }
}
