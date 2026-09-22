using System;
using System.Collections.Generic;
using System.Text;

namespace PasetoApiTesterProj
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string TokenType { get; set; } = "";
        public int ExpiresInMinutes { get; set; }
    }
}
