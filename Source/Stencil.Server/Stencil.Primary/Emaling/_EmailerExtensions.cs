using Codeable.Foundation.Common;
using Codeable.Foundation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Emaling
{
    public static class _EmailerExtensions
    {
        public static void SendRequestPasswordCompleted(this IEmailer emailer, string recipientEmail, string recipientName)
        {
            try
            {
                if (emailer == null) { return; }

                Dictionary<string, string> values = new Dictionary<string, string>();
                values["recipient_name"] = recipientName;
                values["recipient_email"] = recipientEmail;
                emailer.SendEmail(EmailFormat.PasswordResetCompleted, "PasswordResetCompleted", "Password", recipientEmail, values);
            }
            catch (Exception ex)
            {
                emailer.Foundation.LogError(ex, "SendRequestPasswordCompleted");
            }
        }
        public static void SendPasswordResetInitiated(this IEmailer emailer, string recipientEmail, string recipientName, string resetToken)
        {
            try
            {
                if (emailer == null) { return; }

                Dictionary<string, string> values = new Dictionary<string, string>();
                values["token"] = resetToken;
                values["recipient_name"] = recipientName;
                values["recipient_email"] = recipientEmail;
                emailer.SendEmail(EmailFormat.PasswordResetInitiated, "PasswordResetInitiated", "Password", recipientEmail, values);
            }
            catch (Exception ex)
            {
                emailer.Foundation.LogError(ex, "SendPasswordResetInitiated");
            }
        }
    }
}
