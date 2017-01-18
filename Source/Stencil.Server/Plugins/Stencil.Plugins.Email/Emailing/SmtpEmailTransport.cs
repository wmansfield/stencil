using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Emailing;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common.Integration;
using Stencil.Domain;
using Stencil.Primary;
using Stencil.Primary.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Stencil.Plugins.Emails.Emailing
{
    public class SmtpEmailTransport : ChokeableClass, IEmailTransport, INotifyAdmin
    {
        #region Constructor

        public SmtpEmailTransport(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.StaticCache = new AspectCache("SmtpEmailTransport.Cache", iFoundation, new StaticLifetimeManager("SmtpEmailTransport"));
            this.API = new StencilAPI(iFoundation);
        }

        #endregion

        #region Protected Properties

        protected virtual AspectCache StaticCache { get; set; }
        public StencilAPI API { get; set; }

        #endregion

        #region IEmailTransport Members

        public IEmail CreateEmail()
        {
            return new Codeable.Foundation.Common.Emailing.Email();
        }


        public object SendEmail(IEmail email, IEmailRecipient recipient, bool checkUserPreferences)
        {
            return base.ExecuteFunction<object>("SendEmail", delegate ()
            {
                //TODO:MUST:Email: Change this to a db driven queue system.
                // Create email table
                // Create deamon that processes that table
                // Add to the table here
                // Agitate the daemon
                try
                {
                    MailAddress fromAddress = null;
                    if (!string.IsNullOrEmpty(email.FromName) && !email.FromName.Equals(email.FromEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        fromAddress = new MailAddress(email.FromEmail, email.FromName, System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        fromAddress = new MailAddress(email.FromEmail, email.FromEmail, System.Text.Encoding.UTF8);
                    }

                    MailAddress toAddress = null;
                    if (!string.IsNullOrEmpty(recipient.DisplayName) && !recipient.DisplayName.Equals(recipient.EmailAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        toAddress = new MailAddress(recipient.EmailAddress, recipient.DisplayName, System.Text.Encoding.UTF8);
                    }
                    else
                    {
                        toAddress = new MailAddress(recipient.EmailAddress, recipient.EmailAddress, System.Text.Encoding.UTF8);
                    }


                    MailMessage message = new MailMessage(fromAddress, toAddress);

                    message.IsBodyHtml = true;
                    message.Body = email.HTMLBody;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.Subject = email.Subject;
                    message.SubjectEncoding = System.Text.Encoding.UTF8;

                    SmtpClient smtpClient = this.GenerateSmtpClient();
                    smtpClient.Send(message);

                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.EMAIL_FORMAT, email.InternalTypeID, "sent"), 0, 1);
                    return true;

                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "PerformSendEmail");
                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.EMAIL_FORMAT, email.InternalTypeID, "fail"), 0, 1);
                    return false;
                }
            });
        }


        #endregion

        private static DateTime? _lastAdminEmail;
        private static int _emailsSent;

        protected virtual SmtpClient GenerateSmtpClient()
        {
            return base.ExecuteFunction("GenerateSmtpClient", delegate ()
            {
                //TODO:MUST:Email: Cache these configurations when properly building this feature
                SmtpCredential credentials = new SmtpCredential();
                List<GlobalSetting> items = this.API.Direct.GlobalSettings.FindWithPrefix("Email_");
                GlobalSetting item = items.FirstOrDefault(x => x.name == "Email_SmtpUserName" && !string.IsNullOrEmpty(x.value));
                if (item != null)
                {
                    credentials.SmtpUserName = item.value;
                }
                item = items.FirstOrDefault(x => x.name == "Email_SmtpUseSSL" && !string.IsNullOrEmpty(x.value));
                if (item != null)
                {
                    credentials.SmtpUseSSL = bool.Parse(item.value);
                }
                item = items.FirstOrDefault(x => x.name == "Email_SmtpPort" && !string.IsNullOrEmpty(x.value));
                if (item != null)
                {
                    credentials.SmtpPort = int.Parse(item.value);
                }
                item = items.FirstOrDefault(x => x.name == "Email_SmtpPassword" && !string.IsNullOrEmpty(x.value));
                if (item != null)
                {
                    credentials.SmtpPassword = item.value;
                }
                item = items.FirstOrDefault(x => x.name == "Email_SmtpHostName" && !string.IsNullOrEmpty(x.value));
                if (item != null)
                {
                    credentials.SmtpHostName = item.value;
                }

                SmtpClient smtpClient = new SmtpClient(credentials.SmtpHostName);
                if (credentials.SmtpPort != 0)
                {
                    smtpClient.Port = credentials.SmtpPort;
                }
                smtpClient.Credentials = new System.Net.NetworkCredential(credentials.SmtpUserName, credentials.SmtpPassword);
                smtpClient.EnableSsl = credentials.SmtpUseSSL;
                return smtpClient;
            });
        }

        public virtual void SendAdminEmail(string subject, string body)
        {
            base.ExecuteMethod("SendAdminEmail", delegate ()
            {
                try
                {
                    if (_lastAdminEmail.HasValue)
                    {
                        TimeSpan sinceLastAdminEmail = DateTime.UtcNow - _lastAdminEmail.Value;
                        if (sinceLastAdminEmail.TotalSeconds < 60)
                        {
                            _emailsSent++;
                            if (_emailsSent > 2)
                            {
                                return; // we dont wanna spam our system
                            }
                        }
                        else
                        {
                            // reset, and keep going
                            _lastAdminEmail = DateTime.UtcNow;
                            _emailsSent = 0;
                        }
                    }
                    else
                    {
                        _lastAdminEmail = DateTime.UtcNow;
                    }

                    

                    MailAddress fromAddress = new MailAddress("no-reply@socialhaven.com", "Stencil");
                    MailAddress toAddress = new MailAddress("wmansfield@socialhaven.com", "wmansfield@socialhaven.com"); ;
                    MailMessage message = new MailMessage(fromAddress, toAddress);
                    message.IsBodyHtml = false;
                    message.Body = body;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.Subject = subject;
                    message.SubjectEncoding = System.Text.Encoding.UTF8;

                    SmtpClient smtpClient = this.GenerateSmtpClient();
                    smtpClient.Send(message);
                }
                catch
                {
                    //gulp, if this is happening, its already an error
                }
            });
        }

    }
}