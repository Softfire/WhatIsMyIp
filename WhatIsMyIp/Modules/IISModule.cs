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
        public static void SetFTPExternalFirewallIp(IPAddress newIpAddress)
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

                    var sites = string.Empty;

                    foreach (var site in serverManager.Sites)
                    {
                        if (site.GetChildElement("ftpServer") != null)
                        {
                            // Configure the external IP address of the firewall.
                            site.GetChildElement("ftpServer")
                                .GetChildElement("firewallSupport")
                                .SetAttributeValue("externalIp4Address", newIpAddress.ToString());

                            sites += site.Name + Environment.NewLine;

                            // Log ip address update.
                            File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Updated IIS FTP Site ({site.Name}) External IP Address to: {newIpAddress}{Environment.NewLine}");
                        }
                    }

                    //Commit changes.
                    serverManager.CommitChanges();

                    if (serverManager.Sites.Count > 0)
                    {
                        // Send out email notification.
                        MailModule.Send(WhatIsMyIp.EmailHost, WhatIsMyIp.EmailPort,
                                        WhatIsMyIp.EmailTo, WhatIsMyIp.EmailFrom,
                                        @"IIS FTP Firewall External Ip Updated!", sites,
                                        WhatIsMyIp.EnableSsl, MailModule.Templates.IIS);
                    }

                    File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Update complete!{Environment.NewLine}{Environment.NewLine}");
                }
            }
        }
    }
}