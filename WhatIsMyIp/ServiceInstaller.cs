using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;

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
                var logFilePathIsSet = false;
                var watchIntervalIsSet = false;
                (bool, string) stringResult;
                (bool, int) intResult;

                // Setup loop.
                while (setupIsComplete == false)
                {
                    // Set Service Host loop.
                    while (serviceHostIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = ConfirmStringInput(@"Service Host: ", @"Set service host from which to retrieve the external ip.")).Item1)
                        {
                            WhatIsMyIp.ServiceHost = stringResult.Item2;
                            serviceHostIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email To loop.
                    while (emailToIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = ConfirmStringInput(@"Email To: ", @"Set recipient email address for notifications.")).Item1)
                        {
                            WhatIsMyIp.EmailTo = stringResult.Item2;
                            emailToIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email From loop.
                    while (emailFromIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = ConfirmStringInput(@"Email From: ", @"Set sender email address for notifications.")).Item1)
                        {
                            WhatIsMyIp.EmailFrom = stringResult.Item2;
                            emailFromIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email Host loop.
                    while (emailHostIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = ConfirmStringInput(@"Email (SMTP) Host: ", @"Set SMTP host for email notifications.")).Item1)
                        {
                            WhatIsMyIp.EmailHost = stringResult.Item2;
                            emailHostIsSet = stringResult.Item1;
                        }
                    }

                    // Set Email Port loop.
                    while (emailPortIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((intResult = ConfirmIntInput(@"Email (SMTP) Port: ", @"Set SMTP port for email notifications.")).Item1)
                        {
                            WhatIsMyIp.EmailPort = intResult.Item2;
                            emailPortIsSet = intResult.Item1;
                        }
                    }

                    // Set Log File Path loop.
                    while (logFilePathIsSet == false)
                    {
                        // If input is accepted, assign to property.
                        if ((stringResult = ConfirmStringInput(@"Log File Path: ", $@"Set File Path for Logs. Eg. ""C:\{nameof(WhatIsMyIp)}\Logs""")).Item1)
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
                        if ((intResult = ConfirmIntInput(@"Interval: ", $@"Set Interval between calls for IP detection in milliseconds.{Environment.NewLine}Eg. 500000 = 5 mins.")).Item1)
                        {
                            WhatIsMyIp.WatchInterval = intResult.Item2;
                            watchIntervalIsSet = intResult.Item1;
                        }
                    }

                    // Clear the Console.
                    Console.Clear();

                    // Display Setup Header.
                    DisplaySetupHeader();

                    // Confirm properties.
                    Console.WriteLine(@"Current Settings:");
                    Console.WriteLine($@"(1) Service Host: {WhatIsMyIp.ServiceHost}");
                    Console.WriteLine($@"(2) Email To: {WhatIsMyIp.EmailTo}");
                    Console.WriteLine($@"(3) Email From: {WhatIsMyIp.EmailFrom}");
                    Console.WriteLine($@"(4) Email (SMTP) Host: {WhatIsMyIp.EmailHost}");
                    Console.WriteLine($@"(5) Email (SMTP) Port: {WhatIsMyIp.EmailPort}");
                    Console.WriteLine($@"(6) Log File Path: {WhatIsMyIp.LogFilePath}");
                    Console.WriteLine($@"(7) Interval: {WhatIsMyIp.WatchInterval}");
                    Console.WriteLine();
                    Console.WriteLine($@"Are these settings correct?{Environment.NewLine}" +
                                      $@"Press Enter to continue.{Environment.NewLine}" +
                                       @"Press 1-7 to reset specific setting.");
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
                        logFilePathIsSet = false;
                    }
                    else if (response.Key == ConsoleKey.D7 ||
                             response.Key == ConsoleKey.NumPad7)
                    {
                        watchIntervalIsSet = false;
                    }
                }
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

            // Populate Registry.
            WhatIsMyIp.SetRegistrySettings();

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

        #region Support

        /// <summary>
        /// Display Setup Header.
        /// Outputs to Console the Setup Header.
        /// </summary>
        private void DisplaySetupHeader()
        {
            Console.WriteLine(@"##############################################################");
            Console.WriteLine(@"#          Setup | What Is My Ip | Windows Service           #");
            Console.WriteLine(@"##############################################################");
            Console.WriteLine();
        }

        /// <summary>
        /// Confirm String Input.
        /// Confirms string input by reviewing the input and asking for confirmation before continueing.
        /// Allows user to provide input again if incorrectly provided earlier.
        /// </summary>
        /// <param name="propertyTitle">The property's title. Used prior to the input on input line.</param>
        /// <param name="propertyDescription">The property's description. Defines the requirement.</param>
        /// <returns>Returns a Tuple{bool, string} defining whether the input was accepted and returns the input value as a string.</returns>
        private (bool, string) ConfirmStringInput(string propertyTitle, string propertyDescription)
        {
            // Clear the Console.
            Console.Clear();

            // Display Setup Header.
            DisplaySetupHeader();

            // Set property.
            Console.WriteLine(propertyDescription);
            Console.Write(propertyTitle);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) == false)
            {
                Console.WriteLine($@"Is ({input}) correct?{Environment.NewLine}Press Enter to continue. Any other key to reset setting.");
                var response = Console.ReadKey();

                if (response.Key == ConsoleKey.Enter)
                {
                    // Confirmation.
                    return (true, input);
                }
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Confirm Int Input.
        /// Confirms string input by reviewing the input and asking for confirmation before continueing.
        /// Allows user to provide input again if incorrectly provided earlier.
        /// </summary>
        /// <param name="propertyTitle">The property's title. Used prior to the input on input line.</param>
        /// <param name="propertyDescription">The property's description. Defines the requirement.</param>
        /// <returns>Returns a Tuple{bool, int} defining whether the input was accepted and returns the input value as an int.</returns>
        private (bool, int) ConfirmIntInput(string propertyTitle, string propertyDescription)
        {
            // Clear the Console.
            Console.Clear();

            // Display Setup Header.
            DisplaySetupHeader();

            // Set property.
            Console.WriteLine(propertyDescription);
            Console.Write(propertyTitle);

            if (int.TryParse(Console.ReadLine(), out var input))
            {
                Console.WriteLine($@"Is ({input}) correct?{Environment.NewLine}Press Enter to continue. Any other key to reset setting.");
                var response = Console.ReadKey();

                if (response.Key == ConsoleKey.Enter)
                {
                    // Confirmation.
                    return (true, input);
                }
            }

            return (false, -1);
        }

        #endregion
    }
}