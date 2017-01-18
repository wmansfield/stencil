using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models.Requests
{
    public class AuthLoginInput
    {
        public AuthLoginInput()
        {
        }
        public string user { get; set; }
        public string password { get; set; }
        public string promotion { get; set; }
        public bool persist { get; set; }
    }
}
