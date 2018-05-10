using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WhatIsMyIp.Modules
{
    public static partial class MailModule
    {
        internal static Dictionary<string, object> TemplateIISAdditionalDetails { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Template IIS.
        /// </summary>
        /// <param name="additionalDetails">Additional details that will be added.</param>
        /// <returns>Returns the body of an email in HTML format.</returns>
        private static string TemplateIIS()
        {
            using (var reader = new StreamReader($@"{WhatIsMyIp.ServiceFilePath + @"\Templates\Mail\IIS.html"}"))
            {

                var body = new StringBuilder(reader.ReadToEnd());

                var sitesReplacement = string.Empty;
                var siteCount = 0;

                if (TemplateIISAdditionalDetails.ContainsKey("SITES") &&
                    TemplateIISAdditionalDetails["SITES"] is Tuple<string, string, string>[] sites)
                {
                    foreach (var site in sites)
                    {
                        sitesReplacement += "<tr>" + Environment.NewLine +
                                                "<td class='siteName'>" + site.Item1 + "</td>" + Environment.NewLine +
                                                "<td class='previousIp'>" + site.Item2 + "</td>" + Environment.NewLine +
                                                "<td class='newIp'>" + site.Item3 + "</td>" + Environment.NewLine +
                                            "</tr>" + Environment.NewLine;

                        // Update count.
                        siteCount++;
                    }
                }

                body.Replace("{SITES_DATA}", sitesReplacement);
                body.Replace("{NUMBER_OF_SITES}", siteCount.ToString());

                return body.ToString();
            }
        }
    }
}