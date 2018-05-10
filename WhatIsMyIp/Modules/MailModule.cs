using System.Net.Mail;
using System.Net.Mime;

namespace WhatIsMyIp.Modules
{
    public static partial class MailModule
    {
        public enum Templates
        {
            None,
            IIS
        }

        /// <summary>
        /// Send.
        /// </summary>
        /// <param name="emailHost">The SMTP host.</param>
        /// <param name="emailHostPort">The SMTP port.</param>
        /// <param name="emailTo">The recipient's email address.</param>
        /// <param name="emailFrom">The sender's email address.</param>
        /// <param name="emailSubject">The mail's subject.</param>
        /// <param name="emailBody">The mail's body.</param>
        /// <param name="useSsl">Flag whether the email host requires SSL.</param>
        /// <param name="template">HTML template to use in email notification.</param>
        /// <returns>Returns a bool indicating whether the message was sent.</returns>
        public static bool Send(string emailHost, int emailHostPort,
                                string emailTo, string emailFrom,
                                string emailSubject, string emailBody,
                                bool useSsl, Templates template = Templates.None)
        {
            if (string.IsNullOrWhiteSpace(emailHost) == false &&
                emailHostPort > 0 &&
                string.IsNullOrWhiteSpace(emailTo) == false &&
                string.IsNullOrWhiteSpace(emailFrom) == false &&
                string.IsNullOrWhiteSpace(emailSubject) == false &&
                string.IsNullOrEmpty(emailBody) == false)
            {
                // Create mail client.
                using (var client = new SmtpClient
                {
                    Host = emailHost,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    EnableSsl = useSsl
                })
                {
                    MailMessage mail;

                    // Create message to send.
                    switch (template)
                    {
                        case Templates.IIS:
                            mail = new MailMessage(emailFrom, emailTo, emailSubject, emailBody)
                            {
                                IsBodyHtml = true,
                                AlternateViews =
                                {
                                    AlternateView.CreateAlternateViewFromString(TemplateIIS(), new ContentType("text/html"))
                                }
                            };
                            break;
                        default:
                            mail = new MailMessage(emailFrom, emailTo, emailSubject, emailBody);
                            break;
                    }

                    // Send message.
                    client.Send(mail);

                    return true;
                }
            }

            return false;
        }
    }
}