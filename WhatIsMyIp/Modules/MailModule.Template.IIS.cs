using System.IO;

namespace WhatIsMyIp.Modules
{
    public static partial class MailModule
    {
        /// <summary>
        /// Template IIS.
        /// </summary>
        /// <param name="additionalDetails">Additional details that will be added wherever {ADDITIONAL_DETAILS} is located.</param>
        /// <returns>Returns the body of an email in HTML format.</returns>
        private static string TemplateIIS(string additionalDetails = "None")
        {
            using (var reader = new StreamReader(@".\Templates\Mail\IIS.html"))
            {
                var body = reader.ReadToEnd();
                return body.Replace("{ADDITIONAL_DETAILS}", additionalDetails);
            }
        }
    }
}