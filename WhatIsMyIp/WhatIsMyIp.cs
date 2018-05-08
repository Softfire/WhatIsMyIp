using System;
using System.IO;
using System.Net;
using System.Security;
using System.ServiceProcess;
using System.Timers;
using Microsoft.Win32;
using WhatIsMyIp.Modules;

namespace WhatIsMyIp
{
    public partial class WhatIsMyIp : ServiceBase
    {
        /// <summary>
        /// Watch.
        /// </summary>
        internal static Timer Watch { get; set; }

        /// <summary>
        /// Service Host.
        /// </summary>
        internal static string ServiceHost { get; set; }

        /// <summary>
        /// New External Ip Address.
        /// </summary>
        internal static IPAddress NewExternalIpAddress { get; set; }

        /// <summary>
        /// Current External Ip Address.
        /// </summary>
        internal static IPAddress CurrentExternalIp { get; set; }

        /// <summary>
        /// Web Response.
        /// </summary>
        private static string WebResponse { get; set; }

        /// <summary>
        /// Email To.
        /// </summary>
        internal static string EmailTo { get; set; } = string.Empty;

        /// <summary>
        /// Email From.
        /// </summary>
        internal static string EmailFrom { get; set; } = string.Empty;

        /// <summary>
        /// Email Host.
        /// </summary>
        internal static string EmailHost { get; set; } = string.Empty;

        /// <summary>
        /// Email Host.
        /// </summary>
        internal static int EmailPort { get; set; }

        /// <summary>
        /// Enable Ssl.
        /// </summary>
        internal static bool EnableSsl { get; set; }

        /// <summary>
        /// Log File Path.
        /// </summary>
        internal static string LogFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Watch Interval.
        /// Amount of time in milliseconds between calls to ProcessIP method.
        /// </summary>
        internal static int WatchInterval { get; set; } = 500000;

        /// <summary>
        /// What Is My Ip?
        /// Retrieves external IP address and processes any enabled modules then sends an email notifying an admin that the external IP address changed.
        /// </summary>
        public WhatIsMyIp()
        {
            InitializeComponent();

            // Instantiate timer.
            Watch = new Timer
            {
                Interval = WatchInterval,
                AutoReset = true,
                Enabled = true
            };

            // Pull registry data.
            GetRegistrySettings();

            // Add processing method to trigger.
            Watch.Elapsed += ProcessIp;
            
            // Create log directory.
            CreateDirectory(LogFilePath);
        }

        /// <summary>
        /// On Start.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // Start watch.
            Watch.Start();

            // Log action.
            File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Starting service...{Environment.NewLine}");
        }

        /// <summary>
        /// On Stop.
        /// </summary>
        protected override void OnStop()
        {
            // Log action.
            File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Stopping service...{Environment.NewLine}");

            // Stop watch.
            Watch.Stop();
        }

        /// <summary>
        /// Process Ip.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ProcessIp(object source, ElapsedEventArgs e)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    WebResponse = wc.DownloadString(ServiceHost);

                    if (string.IsNullOrWhiteSpace(WebResponse))
                    {
                        File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: The web response from {ServiceHost} was '{WebResponse}'.{Environment.NewLine}{Environment.NewLine}");
                        MailModule.Send(EmailHost, EmailPort,
                                  EmailTo, EmailFrom,
                                  "What Is My Ip - Error!", $"External IP Web Response was '{WebResponse}'.",
                                  EnableSsl);
                    }
                }

                // Check for a response.
                if (string.IsNullOrWhiteSpace(WebResponse) == false)
                {
                    if (CurrentExternalIp == null)
                    {
                        CurrentExternalIp = NewExternalIpAddress = IPAddress.Parse(WebResponse);
                    }
                    else
                    {
                        NewExternalIpAddress = IPAddress.Parse(WebResponse);
                    }

                    if (CurrentExternalIp.Equals(NewExternalIpAddress) == false)
                    {
                        CurrentExternalIp = NewExternalIpAddress;

                        if (ModulesController.IsIisEnabled)
                        {
                            // Update IIS FTP firewall settings.
                            IISModule.SetFTPExternalFirewallIp(NewExternalIpAddress);
                        }

                        // Notify admin of IP change.
                        MailModule.Send(EmailHost, EmailPort,
                                  EmailTo, EmailFrom,
                                  "What Is My Ip - IP Address Change!", $"External IP changed to: {WebResponse}",
                                  EnableSsl);
                        
                        // Write to logs.
                        File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - External Ip Address has changed to: {WebResponse}{Environment.NewLine}");
                        File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email sent out to: {EmailTo}{Environment.NewLine}{Environment.NewLine}");
                    }

                    WebResponse = null;
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (string.IsNullOrWhiteSpace(LogFilePath) == false)
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Get Registry Settings.
        /// </summary>
        private static void GetRegistrySettings()
        {
            try
            {
                // Update progress.
                Console.WriteLine(@"Reading Registry for service entry...");

                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var parameters = hklm.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{nameof(WhatIsMyIp)}\Parameters", false))
                    {
                        // Update progress.
                        Console.WriteLine($@"Registry Entry: ({parameters})");

                        if (parameters != null)
                        {
                            var serviceHost = parameters.GetValue(nameof(ServiceHost)).ToString();
                            var emailTo = parameters.GetValue(nameof(EmailTo)).ToString();
                            var emailFrom = parameters.GetValue(nameof(EmailFrom)).ToString();
                            var emailHost = parameters.GetValue(nameof(EmailHost)).ToString();
                            int.TryParse(parameters.GetValue(nameof(EmailPort)).ToString(), out var emailPort);
                            var logFilePath = parameters.GetValue(nameof(LogFilePath)).ToString();
                            int.TryParse(parameters.GetValue(nameof(WatchInterval)).ToString(), out var watchInterval);

                            if (string.IsNullOrWhiteSpace(serviceHost) == false &&
                                string.IsNullOrWhiteSpace(emailTo) == false &&
                                string.IsNullOrWhiteSpace(emailFrom) == false &&
                                string.IsNullOrWhiteSpace(emailHost) == false &&
                                emailPort > 0 &&
                                string.IsNullOrWhiteSpace(logFilePath) == false &&
                                watchInterval > 0)
                            {
                                ServiceHost = serviceHost;
                                EmailTo = emailTo;
                                EmailFrom = emailFrom;
                                EmailHost = emailHost;
                                EmailPort = emailPort;
                                LogFilePath = logFilePath;
                                WatchInterval = watchInterval;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (string.IsNullOrWhiteSpace(LogFilePath) == false)
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Set Registry Settings.
        /// </summary>
        internal static void SetRegistrySettings()
        {
            try
            {
                // Update progress.
                Console.WriteLine(@"Reading Registry for service entry...");

                // Registry setup.
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var registryEntry = hklm.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{nameof(WhatIsMyIp)}", true))
                    {
                        // Update progress.
                        Console.WriteLine($@"Registry Entry: ({registryEntry})");

                        // Confirm inputs.
                        if (registryEntry != null &&
                            string.IsNullOrWhiteSpace(ServiceHost) == false &&
                            string.IsNullOrWhiteSpace(EmailTo) == false &&
                            string.IsNullOrWhiteSpace(EmailFrom) == false &&
                            string.IsNullOrWhiteSpace(EmailHost) == false &&
                            EmailPort > 0 &&
                            string.IsNullOrWhiteSpace(LogFilePath) == false &&
                            WatchInterval >= 0)
                        {
                            // Update progress.
                            Console.WriteLine(@"Creating Parameters SubKey in Registry for service...");

                            // Set Registry Data.
                            using (var parameterSubKey = registryEntry.CreateSubKey("Parameters"))
                            {
                                // Update progress.
                                Console.WriteLine(@"Adding Parameters in Registry...");
                                
                                // Create data for the ImagePathParameters subkey.
                                parameterSubKey?.SetValue(nameof(ServiceHost), ServiceHost);
                                parameterSubKey?.SetValue(nameof(EmailTo), EmailTo);
                                parameterSubKey?.SetValue(nameof(EmailFrom), EmailFrom);
                                parameterSubKey?.SetValue(nameof(EmailHost), EmailHost);
                                parameterSubKey?.SetValue(nameof(EmailPort), EmailPort);
                                parameterSubKey?.SetValue(nameof(LogFilePath), LogFilePath);
                                parameterSubKey?.SetValue(nameof(WatchInterval), WatchInterval);

                                // Update progress.
                                Console.WriteLine(@"Parameters added successfully.");
                                File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Registry entry created and populated. Entry: {registryEntry}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (string.IsNullOrWhiteSpace(LogFilePath) == false)
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete Registry Settings.
        /// </summary>
        internal static void DeleteRegistrySettings()
        {
            try
            {
                // Update progress.
                Console.WriteLine(@"Reading Registry for service entry...");

                // Registry setup.
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    hklm.DeleteSubKeyTree($@"SYSTEM\CurrentControlSet\Services\{nameof(WhatIsMyIp)}");

                    // Update progress.
                    Console.WriteLine(@"Deleting Registry entries...");
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (string.IsNullOrWhiteSpace(LogFilePath) == false)
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Create Directory.
        /// Creates a directory to store the log files.
        /// </summary>
        /// <param name="filePath">The file path of where the log is to be created.</param>
        public static void CreateDirectory(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) == false)
                {
                    var fi = new FileInfo(filePath);

                    if (fi.Directory != null &&
                        fi.Directory.Exists == false &&
                        fi.DirectoryName != null)
                    {
                        Directory.CreateDirectory(fi.DirectoryName);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException ||
                    ex is SecurityException ||
                    ex is ArgumentException ||
                    ex is UnauthorizedAccessException ||
                    ex is PathTooLongException ||
                    ex is NotSupportedException ||
                    ex is DirectoryNotFoundException ||
                    ex is IOException)
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
            }
        }
    }
}