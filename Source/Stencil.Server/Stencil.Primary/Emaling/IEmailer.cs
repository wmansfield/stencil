using Codeable.Foundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Emaling
{
    public interface IEmailer
    {
        /// <summary>
        /// For Extension method ease
        /// </summary>
        IFoundation Foundation { get; } 
        void SendAdminEmail(string subject, string message);
        void SendEmail(EmailFormat format, string messageTypeID, string messageTypeCategory, string recipientEmail, Dictionary<string, string> tokenValues, string[] ccRecipients = null);
    }
}
