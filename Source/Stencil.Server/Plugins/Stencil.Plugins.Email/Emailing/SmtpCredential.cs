using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stencil.Plugins.Emails.Emailing
{
    public class SmtpCredential
    {
        public string SmtpHostName { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpUserName { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpUseSSL { get; set; }
    }
}