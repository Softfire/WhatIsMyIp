using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
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
        private static WebResponse WebResponse { get; set; }

        /// <summary>
        /// Log File Path.
        /// </summary>
        internal static string LogFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Service File Path.
        /// </summary>
        internal static string ServiceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Watch Interval.
        /// Amount of time in milliseconds between calls to ProcessIP method.
        /// </summary>
        internal static int WatchInterval { get; set; } = 300000;

        /// <summary>
        /// Resend Mail.
        /// </summary>
        internal static bool ResendMail { get; set; }

        /// <summary>
        /// Resend To Addresses.
        /// </summary>
        internal static List<string> ResendToAddresses { get; set; }

        /// <summary>
        /// Mail Messages.
        /// </summary>
        internal static List<MailMessage> MailMessages { get; set; }

        /// <summary>
        /// Commands.
        /// Call through a command prompt to activate special commands. Eg. "sc control WhatIsMyIp 1'.
        /// </summary>
        private enum Commands
        {
            SaveSettings = 128,
            LoadSettings = 129
        }

        /// <summary>
        /// What Is My Ip?
        /// Retrieves external IP address and processes any enabled modules then sends an email notifying an admin that the external IP address changed.
        /// </summary>
        public WhatIsMyIp()
        {
            InitializeComponent();
            
            // Allow pausing and continuing.
            CanPauseAndContinue = true;

            // Instantiate timer.
            Watch = new Timer
            {
                Interval = WatchInterval,
                AutoReset = true,
                Enabled = true
            };

            // Create message storage for unsent mail.
            MailMessages = new List<MailMessage>();

            // Add processing method to trigger.
            Watch.Elapsed += ProcessIp;
        }

        /// <summary>
        /// On Start.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // Load settings.
            // TODO: Saving is not working.
            //LoadSettings();
            //MailModule.LoadSettings();
            //ModulesController.LoadSettings();

            // Get Registry data.
            GetRegistrySettings();

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
        /// On Shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        /// <summary>
        /// On Pause.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
        }

        /// <summary>
        /// On Continue.
        /// </summary>
        protected override void OnContinue()
        {
            base.OnContinue();
        }

        /// <summary>
        /// On Custom Command.
        /// </summary>
        /// <param name="command">Intakes an int to identify a unique command to run.</param>
        protected override void OnCustomCommand(int command)
        {
            switch ((Commands)command)
            {
                case Commands.LoadSettings:

                    // Stop watch.
                    Watch.Stop();

                    // Pull registry data.
                    GetRegistrySettings();

                    // Load settings.
                    // TODO: Saving is not working.
                    //LoadSettings();
                    //MailModule.LoadSettings();
                    //ModulesController.LoadSettings();

                    // Set watch interval.
                    Watch.Interval = WatchInterval;

                    // Start watch.
                    Watch.Start();

                    // Log action.
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Loading settings...{Environment.NewLine}");
                    break;
                case Commands.SaveSettings:
                    // Stop watch.
                    Watch.Stop();

                    // Push to registry.
                    SetRegistrySettings();

                    // Save settings.
                    // TODO: Saving in not working.
                    //SaveSettings();
                    //MailModule.SaveSettings();
                    //ModulesController.SaveSettings();

                    // Set watch interval.
                    Watch.Interval = WatchInterval;

                    // Start watch.
                    Watch.Start();

                    // Log action.
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Saving settings...{Environment.NewLine}");
                    break;
                default:
                    // Log action.
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Unknwon command received: ({command}){Environment.NewLine}");
                    break;
            }

            base.OnCustomCommand(command);
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
                // Send any unsent mail.
                for (var i = 0; i < MailMessages.Count; i++)
                {
                    var message = MailMessages[i];
                    if (MailModule.Send(MailModule.SmtpHost, MailModule.SmtpPort, message, MailModule.EnableSsl).Count == 0)
                    {
                        MailMessages.Remove(message);
                    }
                }

                // Create a web client to gather data.
                using (var wc = new WebClient())
                {
                    // Call out to the Service Host for the external ip being used by the current network.
                    WebResponse = Support.GetJsonContent<WebResponse>(wc.DownloadString(MailModule.ServiceHost));

                    // If the response is null or empty send an email to the admin(s) to check for errors.
                    if (WebResponse == null)
                    {
                        File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: The web response from {MailModule.ServiceHost} was '{null}'.{Environment.NewLine}{Environment.NewLine}");

                        // Prepare mail message to send.
                        var message = new MailMessage(MailModule.EmailFrom, MailModule.EmailTo, "What Is My Ip - Error!", $"External IP Web Response was '{null}'.");

                        // Send mail.
                        // Flag service to resend if an error occured when sending mail.
                        ResendToAddresses = MailModule.Send(MailModule.SmtpHost, MailModule.SmtpPort, message, MailModule.EnableSsl);
                        
                        // Queue previously unsent mail.
                        if (ResendToAddresses.Count > 0)
                        {
                            MailMessages.Add(message);
                        }
                        else
                        {
                            // Write to logs.
                            File.AppendAllText(LogFilePath + $@"{DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email sent out to: {MailModule.EmailTo}{Environment.NewLine}");
                        }

                        foreach (var address in ResendToAddresses)
                        {
                            File.AppendAllText(LogFilePath + $@"{DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email failed to send out to: {address}{Environment.NewLine}");
                        }

                        // Clear Addresses.
                        ResendToAddresses.Clear();
                    }
                }

                // Check for a response.
                if (WebResponse != null)
                {
                    // If current ip is null, which should not happen unless the registry entry is not set properly,
                    // then parse the web response and update the registry.
                    if (CurrentExternalIp == null)
                    {
                        CurrentExternalIp = NewExternalIpAddress = IPAddress.Parse(WebResponse.IpAddress);

                        // Push to registry.
                        UpdateCurrentExternalIpRegistryEntry();

                        // TODO: Saving in not working.
                        //SaveSettings();
                    }
                    else
                    {
                        NewExternalIpAddress = IPAddress.Parse(WebResponse.IpAddress);
                    }

                    // Process new ip, if changed.
                    if (!CurrentExternalIp.Equals(NewExternalIpAddress))
                    {
                        CurrentExternalIp = NewExternalIpAddress;

                        // Push to registry.
                        UpdateCurrentExternalIpRegistryEntry();

                        // TODO: Saving in not working.
                        //SaveSettings();

                        if (ModulesController.IsIisEnabled)
                        {
                            // Update IIS FTP firewall settings.
                            IISModule.SetFtpExternalFirewallIp(NewExternalIpAddress);
                        }

                        // Prepare mail message to send.
                        var message = new MailMessage(MailModule.EmailFrom, MailModule.EmailTo, "What Is My Ip - IP Address Change!", $"External IP changed to: {WebResponse.IpAddress}");

                        // Send Mail.
                        // Flag service to resend if an error occured when sending mail.
                        ResendToAddresses = MailModule.Send(MailModule.SmtpHost, MailModule.SmtpPort, message, MailModule.EnableSsl);
                        
                        // Queue previously unsent mail.
                        if (ResendToAddresses.Count > 0)
                        {
                            MailMessages.Add(message);
                        }
                        else
                        {
                            // Write to logs.
                            File.AppendAllText(LogFilePath + $@"{DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email sent out to: {MailModule.EmailTo}{Environment.NewLine}");
                        }

                        foreach (var address in ResendToAddresses)
                        {
                            File.AppendAllText(LogFilePath + $@"{DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Email failed to send out to: {address}{Environment.NewLine}");
                        }

                        // Clear Addresses.
                        ResendToAddresses.Clear();
                    }

                    WebResponse = null;
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (!string.IsNullOrWhiteSpace(LogFilePath))
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
        /// Get Settings.
        /// </summary>
        internal static void LoadSettings()
        {
            // Core Settings.
            ServiceFilePath = Properties.Settings.Default.ServiceFilePath;
            LogFilePath = Properties.Settings.Default.LogFilePath;
            CurrentExternalIp = IPAddress.Parse(Properties.Settings.Default.CurrentExternalIp);
            WatchInterval = Properties.Settings.Default.WatchInterval;
        }

        /// <summary>
        /// Save Settings.
        /// </summary>
        internal static void SaveSettings()
        {
            // Core settings.
            Properties.Settings.Default.ServiceFilePath = ServiceFilePath;
            Properties.Settings.Default.LogFilePath = LogFilePath;
            Properties.Settings.Default.CurrentExternalIp = CurrentExternalIp.ToString();
            Properties.Settings.Default.WatchInterval = WatchInterval;

            // Save settings.
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Get Registry Settings.
        /// </summary>
        internal static void GetRegistrySettings()
        {
            try
            {
                // Update progress.
                Console.WriteLine(@"Reading Registry for service entry...");

                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    // Retrieve Parameters.
                    using (var parametersSubKey = hklm.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{nameof(WhatIsMyIp)}\Parameters", false))
                    {
                        // Update progress.
                        Console.WriteLine($@"Registry Entry: ({parametersSubKey})");

                        if (parametersSubKey != null)
                        {
                            var serviceHost = parametersSubKey.GetValue(nameof(MailModule.ServiceHost)).ToString();
                            var emailTo = parametersSubKey.GetValue(nameof(MailModule.EmailTo)).ToString();
                            var emailFrom = parametersSubKey.GetValue(nameof(MailModule.EmailFrom)).ToString();
                            var emailHost = parametersSubKey.GetValue(nameof(MailModule.SmtpHost)).ToString();
                            int.TryParse(parametersSubKey.GetValue(nameof(MailModule.SmtpPort)).ToString(), out var emailPort);
                            var logFilePath = parametersSubKey.GetValue(nameof(LogFilePath)).ToString();
                            int.TryParse(parametersSubKey.GetValue(nameof(WatchInterval)).ToString(), out var watchInterval);
                            IPAddress.TryParse(parametersSubKey.GetValue(nameof(CurrentExternalIp)).ToString(), out var currentExternalIp);
                            var serviceFilePath = parametersSubKey.GetValue(nameof(ServiceFilePath)).ToString();

                            if (!string.IsNullOrWhiteSpace(serviceHost) &&
                                !string.IsNullOrWhiteSpace(emailTo) &&
                                !string.IsNullOrWhiteSpace(emailFrom) &&
                                !string.IsNullOrWhiteSpace(emailHost) &&
                                emailPort > 0 &&
                                !string.IsNullOrWhiteSpace(logFilePath) &&
                                watchInterval > 0 &&
                                currentExternalIp != null &&
                                !string.IsNullOrWhiteSpace(serviceFilePath) )
                            {
                                MailModule.ServiceHost = serviceHost;
                                MailModule.EmailTo = emailTo;
                                MailModule.EmailFrom = emailFrom;
                                MailModule.SmtpHost = emailHost;
                                MailModule.SmtpPort = emailPort;
                                LogFilePath = logFilePath;
                                WatchInterval = watchInterval;
                                CurrentExternalIp = currentExternalIp;
                                ServiceFilePath = serviceFilePath;
                            }
                        }
                    }

                    // Retrieve Modules.
                    using (var modulesSubKey = hklm.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{nameof(WhatIsMyIp)}\Modules", false))
                    {
                        // Update progress.
                        Console.WriteLine($@"Registry Entry: ({modulesSubKey})");

                        if (modulesSubKey != null)
                        {
                            if (bool.TryParse(modulesSubKey.GetValue(nameof(IISModule)).ToString(), out var result))
                            {
                                ModulesController.IsIisEnabled = result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (!string.IsNullOrWhiteSpace(LogFilePath) )
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
                            !string.IsNullOrWhiteSpace(MailModule.ServiceHost) &&
                            !string.IsNullOrWhiteSpace(MailModule.EmailTo) &&
                            !string.IsNullOrWhiteSpace(MailModule.EmailFrom) &&
                            !string.IsNullOrWhiteSpace(MailModule.SmtpHost) && MailModule.SmtpPort > 0 &&
                            !string.IsNullOrWhiteSpace(LogFilePath) &&
                            WatchInterval >= 0 &&
                            !string.IsNullOrWhiteSpace(ServiceFilePath))
                        {
                            // Update progress.
                            Console.WriteLine(@"Creating Parameters SubKey in Registry for service...");

                            // Set Registry Data.
                            using (var parameterSubKey = registryEntry.CreateSubKey("Parameters"))
                            {
                                // Update progress.
                                Console.WriteLine(@"Adding Parameters in Registry...");
                                
                                // Create data for the Parameters subkey.
                                parameterSubKey?.SetValue(nameof(MailModule.ServiceHost), MailModule.ServiceHost);
                                parameterSubKey?.SetValue(nameof(MailModule.EmailTo), MailModule.EmailTo);
                                parameterSubKey?.SetValue(nameof(MailModule.EmailFrom), MailModule.EmailFrom);
                                parameterSubKey?.SetValue(nameof(MailModule.SmtpHost), MailModule.SmtpHost);
                                parameterSubKey?.SetValue(nameof(MailModule.SmtpPort), MailModule.SmtpPort);
                                parameterSubKey?.SetValue(nameof(LogFilePath), LogFilePath);
                                parameterSubKey?.SetValue(nameof(WatchInterval), WatchInterval);
                                parameterSubKey?.SetValue(nameof(CurrentExternalIp), IPAddress.None);
                                parameterSubKey?.SetValue(nameof(ServiceFilePath), ServiceFilePath);

                                // Update progress.
                                Console.WriteLine(@"Parameters added successfully.");
                                File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Registry entry created and populated. Entry: {parameterSubKey}{Environment.NewLine}");
                            }

                            // Set Registry Data.
                            using (var modulesSubKey = registryEntry.CreateSubKey("Modules"))
                            {
                                // Update progress.
                                Console.WriteLine(@"Adding Modules in Registry...");

                                // Create data for the Modules subkey.
                                modulesSubKey?.SetValue(nameof(IISModule), ModulesController.IsIisEnabled);

                                // Update progress.
                                Console.WriteLine(@"Modules added successfully.");
                                File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Registry entry created and populated. Entry: {modulesSubKey}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (!string.IsNullOrWhiteSpace(LogFilePath))
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
                if (!string.IsNullOrWhiteSpace(LogFilePath))
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
        /// Update Current External Ip Registry Entry.
        /// </summary>
        internal static void UpdateCurrentExternalIpRegistryEntry()
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
                            CurrentExternalIp != null)
                        {
                            // Set Registry Data.
                            using (var parameterSubKey = registryEntry.OpenSubKey("Parameters", true))
                            {
                                // Update progress.
                                Console.WriteLine(@"Updating parameter in Registry for service...");

                                // Update current external ip data in the Parameters subkey.
                                parameterSubKey?.SetValue(nameof(CurrentExternalIp), CurrentExternalIp);

                                // Update progress.
                                Console.WriteLine(@"Parameter updated successfully.");
                                File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - Registry entry for current external ip was updated. {Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                if (!string.IsNullOrWhiteSpace(LogFilePath))
                {
                    File.AppendAllText(LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}