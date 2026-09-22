using System;
using System.Collections.Generic;
using System.Text;

namespace PasetoApiTesterProj
{
    public static class AppSession
    {
        public static string? Token { get; set; }
        public static string? RefreshToken { get; set; }
        public static string? BaseUrl { get; set; }
        public static DateTime TokenExpiry { get; set; }

        public static void Clear()
        {
            Token = null;
            RefreshToken = null;
            BaseUrl = null;
            TokenExpiry = DateTime.MinValue;
        }
    }
}
