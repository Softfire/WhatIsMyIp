using System;
using System.IO;
using System.Net;
using Microsoft.Web.Administration;

namespace WhatIsMyIp.Modules
{
    public static class IISModule
    {
        /// <summary>
        /// Set FTP External Firewall IP.
        /// </summary>
        /// <param name="newIpAddress">The new ip address to use.</param>
        public static void SetFtpExternalFirewallIp(IPAddress newIpAddress)
        {
            if (newIpAddress != null)
            {
                using (var serverManager = new ServerManager())
                {
                    // Configure the external IP address of the firewall.
                    serverManager.GetApplicationHostConfiguration()
                                 .GetSection("system.applicationHost/sites")
                                 .GetChildElement("siteDefaults")
                                 .GetChildElement("ftpServer")
                                 .GetChildElement("firewallSupport")
                                 .SetAttributeValue("externalIp4Address", newIpAddress.ToString());

                    // Record updated sites.
                    if (MailModule.TemplateIISAdditionalDetails.ContainsKey("SITES"))
                    {
                        MailModule.TemplateIISAdditionalDetails["SITES"] = new Tuple<string, string, string>[serverManager.Sites.Count];
                    }
                    else
                    {
                        MailModule.TemplateIISAdditionalDetails.Add("SITES", new Tuple<string ,string, string>[serverManager.Sites.Count]);
                    }

                    // Iterate sites and update ip.
                    for (var i = 0; i < serverManager.Sites.Count; i++)
                    {
                        // Current site.
                        var site = serverManager.Sites[i];

                        if (site.GetChildElement("ftpServer") != null)
                        {
                            // Get previous ip.
                            var previousIpAddress = site.GetChildElement("ftpServer")
                                                        .GetChildElement("firewallSupport")
                                                        .GetAttributeValue("externalIp4Address").ToString();

                            // Configure the external IP address of the firewall.
                            site.GetChildElement("ftpServer")
                                .GetChildElement("firewallSupport")
                                .SetAttributeValue("externalIp4Address", newIpAddress.ToString());

                            if (MailModule.TemplateIISAdditionalDetails.ContainsKey("SITES") &&
                                MailModule.TemplateIISAdditionalDetails["SITES"] is Tuple<string, string, string>[] sites)
                            {
                                // Record site.
                                sites[i] = new Tuple<string, string, string>(site.Name, previousIpAddress, newIpAddress.ToString());
                            }

                            // Log ip address update.
                            File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Updated IIS FTP Site ({site.Name}) External IP Address to: {newIpAddress}{Environment.NewLine}");
                        }
                    }

                    //Commit changes.
                    serverManager.CommitChanges();

                    if (serverManager.Sites.Count > 0)
                    {
                        // Send out email notification.
                        MailModule.Send(MailModule.SmtpHost, MailModule.SmtpPort,
                                        MailModule.EmailTo, MailModule.EmailFrom,
                                        @"IIS FTP Firewall External Ip Updated!", "SITES",
                                        MailModule.EnableSsl, MailModule.Templates.IIS);

                        // Send report to admin.
                        File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email sent out to: {MailModule.EmailTo}{Environment.NewLine}{Environment.NewLine}");
                    }

                    // Update progress.
                    File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Update complete!{Environment.NewLine}{Environment.NewLine}");
                }
            }
        }
    }
}