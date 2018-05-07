using System.Net.Mail;

namespace WhatIsMyIp.Modules
{
    public static class Mail
    {
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
        /// <returns>Returns a bool indicating whether the message was sent.</returns>
        public static bool Send(string emailHost, int emailHostPort,
                                 string emailTo, string emailFrom,
                                 string emailSubject, string emailBody,
                                 bool useSsl)
        {
            if (string.IsNullOrWhiteSpace(emailHost) == false &&
                emailHostPort > 0 &&
                string.IsNullOrWhiteSpace(emailTo) == false &&
                string.IsNullOrWhiteSpace(emailFrom) == false &&
                string.IsNullOrWhiteSpace(emailSubject) == false &&
                string.IsNullOrWhiteSpace(emailBody) == false)
            {
                // Create mail client.
                using (var client = new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    EnableSsl = useSsl
                }) {
                    // Create message to send.
                    var mail = new MailMessage(emailFrom, emailTo, emailSubject, emailBody);

                    // Send message.
                    client.Send(mail);

                    return true;
                }
            }

            return false;
        }
    }
}
