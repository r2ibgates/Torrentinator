using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Torrentinator.Extensions
{
    public static class DisplayExtensions
    {
        public static string ToFriendlyBytes(this long bytes)
        {
            if (bytes > Math.Pow(2, 30))
                return (((float)bytes) / ((float)Math.Pow(2, 30))).ToString("0.0") + "GB";
            else if (bytes > (2 ^ 20))
                return (((float)bytes) / ((float)Math.Pow(2, 20))).ToString("0.0") + "MB";
            else if (bytes > (2 ^ 10))
                return (((float)bytes) / ((float)Math.Pow(2, 10))).ToString("0.0") + "KB";
            else
                return bytes.ToString();
        }
    }
}
