using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Common.Emailing;
using Codeable.Foundation.Core.Caching;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stencil.Primary.Emaling
{
    public class SimpleEmailer : ChokeableClass, IEmailer
    {
        public SimpleEmailer(IFoundation foundation)
            : base(foundation)
        {
            this.API = new StencilAPI(foundation);
            this.Cache = new AspectCache("SimpleEmailer", this.IFoundation, new ContainerControlledLifetimeManager());
        }
        public virtual IFoundation Foundation
        {
            get
            {
                return base.IFoundation;
            }
        }
        public virtual AspectCache Cache { get; set; }
        public virtual StencilAPI API { get; set; }


        public virtual void SendAdminEmail(string subject, string message)
        {
            base.ExecuteMethod("SendAdminEmail", delegate ()
            {
                try
                {
                    IEmailTransport transport = this.IFoundation.SafeResolve<IEmailTransport>();
                    if (transport != null)
                    {
                        string recipientEmail = this.API.Direct.GlobalSettings.GetValueOrDefault("EmailTarget_Admin", "wmansfield@socialhaven.com");
                        string fromEmail = this.API.Direct.GlobalSettings.GetValueOrDefault("EmailFromEmail_System", "no-reply@socialhaven.com");
                        string fromName = this.API.Direct.GlobalSettings.GetValueOrDefault("EmailFromName_System", "Stencil Platform");

                        EmailRecipient recipient = new EmailRecipient(recipientEmail);
                        IEmail email = transport.CreateEmail();
                        email.FromEmail = fromEmail;
                        email.FromName = fromName;
                        email.InternalMessageType = "AdminMessage";
                        email.InternalTypeID = "Admin";
                        email.HTMLBody = message;
                        email.Subject = subject;

                        transport.SendEmail(email, recipient, false);
                    }
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendAdminEmail");
                }
            });
        }

        public virtual void SendEmail(EmailFormat format, string messageTypeID, string messageTypeCategory, string recipientEmail, Dictionary<string, string> tokenValues, string[] ccRecipients = null)
        {
            base.ExecuteMethod("PrepareAndSend", delegate ()
            {
                IEmailTransport transport = this.IFoundation.SafeResolve<IEmailTransport>();
                if (transport != null)
                {
                    string templateKey = "EmailTemplate_" + format.ToString();
                    string subjectKey = "EmailTemplate_" + format.ToString() + "_Subject";

                    string bodyTemplate = this.Cache.PerLifetime(templateKey, delegate ()
                    {
                        return this.API.Direct.GlobalSettings.GetValueOrDefault(templateKey, "");
                    });
                    string subjectTemplate = this.Cache.PerLifetime(subjectKey, delegate ()
                    {
                        return this.API.Direct.GlobalSettings.GetValueOrDefault(subjectKey, "");
                    });

                    if (string.IsNullOrEmpty(bodyTemplate) || string.IsNullOrEmpty(subjectTemplate))
                    {
                        this.SendAdminEmail("Flawed Configuration", "No Email Subject, Body Template found for: " + format.ToString());
                        return;
                    }

                    EmailRecipient recipient = new EmailRecipient(recipientEmail);
                    IEmail email = transport.CreateEmail();
                    email.FromEmail = this.Cache.PerLifetime("Email_FromEmail", delegate ()
                    {
                        return this.API.Direct.GlobalSettings.GetValueOrDefault("EmailFromEmail_Standard", "no-reply@socialhaven.com");
                    });
                    email.FromName = this.Cache.PerLifetime("Email_FromName", delegate ()
                    {
                        return this.API.Direct.GlobalSettings.GetValueOrDefault("EmailFromName_Standard", "Stencil");
                    });
                    email.InternalMessageType = messageTypeCategory;
                    email.InternalTypeID = messageTypeID;
                    email.HTMLBody = this.ProcessTemplate(bodyTemplate, recipientEmail, tokenValues);
                    email.Subject = this.ProcessTemplate(subjectTemplate, recipientEmail, tokenValues);
                    if (ccRecipients != null && ccRecipients.Length > 0)
                    {
                        if (email.ExtraData == null)
                        {
                            email.ExtraData = new Dictionary<string, string>();
                        }
                        email.ExtraData["cc"] = string.Join(";", ccRecipients);
                    }

                    if (string.IsNullOrEmpty(email.Subject))
                    {
                        email.Subject = "Information";
                    }
                    if (tokenValues.ContainsKey("FromName"))
                    {
                        email.FromName = tokenValues["FromName"];
                    }
                    if (tokenValues.ContainsKey("FromEmail"))
                    {
                        email.FromEmail = tokenValues["FromEmail"];
                    }

                    transport.SendEmail(email, recipient, false);
                }
            });
        }

        protected virtual string ProcessTemplate(string template, string recipientEmail, Dictionary<string, string> tokenValues)
        {
            return base.ExecuteFunction("ProcessTemplate", delegate ()
            {
                if (tokenValues != null)
                {
                    foreach (var item in tokenValues)
                    {
                        template = template.Replace("%" + item.Key.ToLower() + "%", item.Value);
                    }
                }
                // always after template
                template = template.Replace("%recipient%", recipientEmail);

                // remove any missing
                template = Regex.Replace(template, @"%.*?%", "", RegexOptions.IgnoreCase);

                return template;
            });
        }
    }
}
