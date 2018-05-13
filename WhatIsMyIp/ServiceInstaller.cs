using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.ServiceProcess;
using WhatIsMyIp.Modules;

namespace WhatIsMyIp
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        #region Install

        /// <summary>
        /// On Before Install.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            try
            {
                // Settings variables.
                var setupIsComplete = false;
                var serviceHostIsSet = false;
                var emailToIsSet = false;
                var emailFromIsSet = false;
                var emailHostIsSet = false;
                var emailPortIsSet = false;
                var emailSslIsSet = false;
                var logFilePathIsSet = false;
                var watchIntervalIsSet = false;
                var smtpClientCredentialsAreSet = true;
                var listModules = false;
                (bool, string) stringResult;
                (bool, SecureString) secureStringResult;
                (bool, int) intResult;
                (bool, bool) boolResult;

                // Setup loop.
                while (setupIsComplete == false)
                {
                    // Set Service Host loop.
                    while (serviceHostIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"Service Host: ", @"Set service host from which to retrieve the external ip.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.ServiceHost = stringResult.Item2;
                            serviceHostIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email To loop.
                    while (emailToIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"Email To: ", @"Set recipient email address for notifications.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.EmailTo = stringResult.Item2;
                            emailToIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email From loop.
                    while (emailFromIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"Email From: ", @"Set sender email address for notifications.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.EmailFrom = stringResult.Item2;
                            emailFromIsSet = stringResult.Item1;
                        }
                    }

                    // Set Smtp Host loop.
                    while (emailHostIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"SMTP Host: ", @"Set SMTP host for email notifications.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.SmtpHost = stringResult.Item2;
                            emailHostIsSet = stringResult.Item1;
                        }
                    }

                    // Set Smtp Port loop.
                    while (emailPortIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((intResult = Support.ConfirmIntInput(@"SMTP Port: ", @"Set SMTP port to use for email notifications.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.SmtpPort = intResult.Item2;
                            emailPortIsSet = intResult.Item1;
                        }
                    }

                    // Set SSL loop.
                    while (emailSslIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((boolResult = Support.ConfirmBoolInput(@"Email SSL Enabled: ", @"Set SSL for SMTP email notifications.", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.EnableSsl = boolResult.Item2;
                            emailSslIsSet = boolResult.Item1;
                        }
                    }

                    // Set Log File Path loop.
                    while (logFilePathIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"Log File Path: ", $@"Set File Path for Logs. Eg. ""C:\{nameof(WhatIsMyIp)}\Logs""", Support.DisplaySetupHeader)).Item1)
                        {
                            // Ensure a trailing slash.
                            if (!stringResult.Item2.EndsWith(Path.DirectorySeparatorChar.ToString()))
                            {
                                stringResult.Item2 += Path.DirectorySeparatorChar;
                            }

                            WhatIsMyIp.LogFilePath = stringResult.Item2;
                            logFilePathIsSet = stringResult.Item1;
                        }
                    }

                    // Set Watch Interval loop.
                    while (watchIntervalIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((intResult = Support.ConfirmIntInput(@"Interval: ", $@"Set Interval between calls for IP detection in milliseconds.{Environment.NewLine}Eg. 300000 = 5 mins.", Support.DisplaySetupHeader)).Item1)
                        {
                            WhatIsMyIp.WatchInterval = intResult.Item2;
                            watchIntervalIsSet = intResult.Item1;
                        }
                    }

                    // Set SMTP Client credentials loop.
                    while (smtpClientCredentialsAreSet == false)
                    {
                        var usernameIsSet = false;
                        var passwordIsSet = false;

                        // If input is accepted, assign to property.
                        if ((stringResult = Support.ConfirmStringInput(@"SMTP Client Username: ", $@"Set SMTP Client username. Used to authenticate teh client with the SMTP server.{Environment.NewLine}", Support.DisplaySetupHeader)).Item1)
                        {
                            MailModule.SmtpClientUsername = stringResult.Item2;
                            usernameIsSet = stringResult.Item1;

                            // If input is accepted, assign to property.
                            if ((secureStringResult = Support.ConfirmSecureStringInput(@"SMTP Client Password: ", $@"Set SMTP Client password. Used to authenticate teh client with the SMTP server.{Environment.NewLine}", Support.DisplaySetupHeader)).Item1)
                            {
                                MailModule.SmtpClientPassword = secureStringResult.Item2;
                                passwordIsSet = secureStringResult.Item1;
                            }
                        }

                        // Confirm credentials are set.
                        if (usernameIsSet &&
                            passwordIsSet)
                        {
                            smtpClientCredentialsAreSet = true;
                        }
                    }

                    // Set SSL loop.
                    while (listModules)
                    {
                        ModulesController.ListModules();
                        listModules = false;
                    }

                    // Clear the Console.
                    Console.Clear();

                    // Display Setup Header.
                    Support.DisplaySetupHeader();

                    // Confirm properties.
                    Console.WriteLine(@"Current Settings:");
                    Console.WriteLine($@"(1) Service Host: {MailModule.ServiceHost}");
                    Console.WriteLine($@"(2) Email To: {MailModule.EmailTo}");
                    Console.WriteLine($@"(3) Email From: {MailModule.EmailFrom}");
                    Console.WriteLine($@"(4) Email (SMTP) Host: {MailModule.SmtpHost}");
                    Console.WriteLine($@"(5) Email (SMTP) Port: {MailModule.SmtpPort}");
                    Console.WriteLine($@"(6) Email SSL Enabled: {MailModule.EnableSsl}");
                    Console.WriteLine($@"(7) Log File Path: {WhatIsMyIp.LogFilePath}");
                    Console.WriteLine($@"(8) Interval: {WhatIsMyIp.WatchInterval}");
                    Console.WriteLine($@"(9) List Available Modules");
                    Console.WriteLine($@"(.) Set SMTP Client credentials");
                    Console.WriteLine();
                    Console.WriteLine($@"Are these settings correct?{Environment.NewLine}" +
                                      $@"Press Enter to continue.{Environment.NewLine}" +
                                      $@"Press 1-8 to reset specific setting.{Environment.NewLine}" +
                                      $@"Press 9 to view list of modules.{Environment.NewLine}" +
                                       @"Press . to set SMTP Client credentials.");

                    // Read next key press.
                    var response = Console.ReadKey();

                    // Process response.
                    if (response.Key == ConsoleKey.Enter)
                    {
                        setupIsComplete = true;
                    }
                    else if (response.Key == ConsoleKey.D1 ||
                             response.Key == ConsoleKey.NumPad1)
                    {
                        serviceHostIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D2 ||
                             response.Key == ConsoleKey.NumPad2)
                    {
                        emailToIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D3 ||
                             response.Key == ConsoleKey.NumPad3)
                    {
                        emailFromIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D4 ||
                             response.Key == ConsoleKey.NumPad4)
                    {
                        emailHostIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D5 ||
                             response.Key == ConsoleKey.NumPad5)
                    {
                        emailPortIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D6 ||
                             response.Key == ConsoleKey.NumPad6)
                    {
                        emailSslIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D7 ||
                             response.Key == ConsoleKey.NumPad7)
                    {
                        logFilePathIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D8 ||
                             response.Key == ConsoleKey.NumPad8)
                    {
                        watchIntervalIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D9 ||
                             response.Key == ConsoleKey.NumPad9)
                    {
                        listModules = true;
                    }
                    else if (response.Key == ConsoleKey.OemPeriod)
                    {
                        smtpClientCredentialsAreSet = false;
                    }
                }

                // Create log directory.
                Support.CreateDirectory(WhatIsMyIp.LogFilePath);

                // Get service file path.
                WhatIsMyIp.ServiceFilePath = Context.Parameters["assemblypath"];
                WhatIsMyIp.ServiceFilePath = WhatIsMyIp.ServiceFilePath.Replace("\"", string.Empty);
                WhatIsMyIp.ServiceFilePath = WhatIsMyIp.ServiceFilePath.Replace(@"\" + nameof(WhatIsMyIp) + ".exe", string.Empty);
            }
            catch (Exception ex)
            {
                // Output exception details.
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Install.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Install(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Installing...");

            base.Install(savedState);
        }

        /// <summary>
        /// On After Install.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Installation complete.");

            // Push to registry.
            WhatIsMyIp.SetRegistrySettings();

            // Save settings.
            // TODO: Saving is not working.
            //WhatIsMyIp.SaveSettings();
            //MailModule.SaveSettings();
            //ModulesController.SaveSettings();

            // Get the service.
            using (var sc = new ServiceController(serviceInstaller.ServiceName))
            {
                // Update progress.
                Console.WriteLine(@"Starting...");

                // Start service.
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }
            }

            base.OnAfterInstall(savedState);
        }

        #endregion

        #region Uninstall

        /// <summary>
        /// On Before Uninstall.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            try
            {
                // Update progress.
                Console.WriteLine(@"Preparing to stop the service...");

                // Get the service.
                using (var sc = new ServiceController(serviceInstaller.ServiceName))
                {
                    // Update progress.
                    Console.WriteLine(@"Stopping...");

                    // Stop service.
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                // Output exception details.
                Console.WriteLine(ex);
            }

            base.OnBeforeUninstall(savedState);
        }

        /// <summary>
        /// Uninstall.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Uninstalling...");
            
            base.Uninstall(savedState);
        }

        /// <summary>
        /// On After Uninstall.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnAfterUninstall(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Uninstallation complete.");

            base.OnAfterUninstall(savedState);
        }

        #endregion

        #region Commit

        /// <summary>
        /// On Committing.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnCommitting(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Committing...");

            base.OnCommitting(savedState);
        }

        /// <summary>
        /// Commit.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Commit(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Committing...");

            base.Commit(savedState);
        }

        /// <summary>
        /// On Committed.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnCommitted(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Commit complete.");

            base.OnCommitted(savedState);
        }

        #endregion

        #region Rollback

        /// <summary>
        /// On Before Rollback.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnBeforeRollback(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Preparing to rollback cahnges...");

            base.OnBeforeRollback(savedState);
        }

        /// <summary>
        /// Rollback.
        /// </summary>
        /// <param name="savedState"></param>
        public override void Rollback(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Rolling back changes...");

            base.Rollback(savedState);
        }

        /// <summary>
        /// On After Rollback.
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnAfterRollback(IDictionary savedState)
        {
            // Update progress.
            Console.WriteLine(@"Rollback complete.");

            base.OnAfterRollback(savedState);
        }

        #endregion
    }
}