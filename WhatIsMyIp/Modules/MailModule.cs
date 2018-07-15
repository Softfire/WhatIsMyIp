using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security;
using System.Threading.Tasks;

namespace WhatIsMyIp.Modules
{
    public static partial class MailModule
    {
        /// <summary>
        /// Service Host.
        /// </summary>
        internal static string ServiceHost { get; set; }

        /// <summary>
        /// Email To.
        /// </summary>
        internal static string EmailTo { get; set; } = string.Empty;

        /// <summary>
        /// Email From.
        /// </summary>
        internal static string EmailFrom { get; set; } = string.Empty;

        /// <summary>
        /// SMTP Host.
        /// </summary>
        internal static string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// SMTP Port.
        /// </summary>
        internal static int SmtpPort { get; set; }

        /// <summary>
        /// Enable Ssl.
        /// </summary>
        internal static bool EnableSsl { get; set; }

        /// <summary>
        /// SMTP Client Username.
        /// </summary>
        internal static string SmtpClientUsername { get; set; }

        /// <summary>
        /// SMTP Client Password.
        /// </summary>
        internal static SecureString SmtpClientPassword { get; set; }

        /// <summary>
        /// Mail Templates.
        /// </summary>
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
        /// <param name="message">The message to send.</param>
        /// <param name="useSsl">Flag whether the email host requires SSL.</param>
        /// <param name="template">HTML template to use in email notification.</param>
        /// <returns>Returns a Task indicating whether the message was sent.</returns>
        public static List<string> Send(string emailHost, int emailHostPort, MailMessage message, bool useSsl, Templates template = Templates.None)
        {
            var addresses = new List<string>();

            // Send to all in To field.
            foreach (var recipient in message.To)
            {
                if (Send(emailHost, emailHostPort, recipient.Address, message.From.Address, message.Subject, message.Body, useSsl, template).IsFaulted)
                {
                    addresses.Add($"To: {recipient.Address}");
                }
            }

            // Send to all in Cc field.
            foreach (var recipient in message.CC)
            {
                if (Send(emailHost, emailHostPort, recipient.Address, message.From.Address, message.Subject, message.Body, useSsl, template).IsFaulted)
                {
                    addresses.Add($"Cc: {recipient.Address}");
                }
            }

            // Send to all in Bcc field.
            foreach (var recipient in message.Bcc)
            {
                if (Send(emailHost, emailHostPort, recipient.Address, message.From.Address, message.Subject, message.Body, useSsl, template).IsFaulted)
                {
                    addresses.Add($"Bcc: {recipient.Address}");
                }
            }

            return addresses;
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
        /// <returns>Returns a Task indicating whether the message was sent.</returns>
        public static Task Send(string emailHost, int emailHostPort,
                                string emailTo, string emailFrom,
                                string emailSubject, string emailBody,
                                bool useSsl, Templates template = Templates.None)
        {
            return Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(emailHost) == false &&
                    emailHostPort > 0 &&
                    string.IsNullOrWhiteSpace(emailTo) == false &&
                    string.IsNullOrWhiteSpace(emailFrom) == false &&
                    string.IsNullOrWhiteSpace(emailSubject) == false &&
                    string.IsNullOrEmpty(emailBody) == false)
                {
                    // Create mail client.
                    using (var client = new SmtpClient(emailHost, emailHostPort)
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = string.IsNullOrWhiteSpace(SmtpClientUsername) && SmtpClientPassword == null,
                        Credentials = string.IsNullOrWhiteSpace(SmtpClientUsername) && SmtpClientPassword == null ? new NetworkCredential(SmtpClientUsername, SmtpClientPassword) : null,
                        EnableSsl = useSsl
                    })
                    {
                        using (var mail = new MailMessage(emailFrom, emailTo, emailSubject, emailBody))
                        {
                            // Use mail message template.
                            switch (template)
                            {
                                case Templates.None:
                                    break;
                                case Templates.IIS:
                                    mail.IsBodyHtml = true;
                                    mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(TemplateIIS(), new ContentType("text/html")));
                                    break;
                            }

                            // Send message.
                            await client.SendMailAsync(mail);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Load Settings.
        /// </summary>
        internal static void LoadSettings()
        {
            // Mail Module settings.
            ServiceHost = Properties.Settings.Default.ServiceHost;
            EmailTo = Properties.Settings.Default.EmailTo;
            EmailFrom = Properties.Settings.Default.EmailFrom;
            SmtpHost = Properties.Settings.Default.SmtpHost;
            SmtpPort = Properties.Settings.Default.SmtpPort;
            EnableSsl = Properties.Settings.Default.EnableSsl;
            SmtpClientUsername = Properties.Settings.Default.SmtpClientUsername;

            // TODO: Decrypt password.
            //SmtpClientPassword = Decrypt(Properties.Settings.Default.SmtpClientPassword);
        }

        /// <summary>
        /// Save Settings.
        /// </summary>
        internal static void SaveSettings()
        {
            // Mail Module settings.
            Properties.Settings.Default.ServiceHost = ServiceHost;
            Properties.Settings.Default.EmailTo = EmailTo;
            Properties.Settings.Default.EmailFrom = EmailFrom;
            Properties.Settings.Default.SmtpHost = SmtpHost;
            Properties.Settings.Default.SmtpPort = SmtpPort;
            Properties.Settings.Default.EnableSsl = EnableSsl;
            Properties.Settings.Default.SmtpClientUsername = SmtpClientUsername;

            // TODO: Encrypt password.
            //Properties.Settings.Default.SmtpClientPassword = Encrypt(SmtpClientPassword);

            // Save settings.
            Properties.Settings.Default.Save();
        }
    }
}