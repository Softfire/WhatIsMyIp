using System;
using WhatIsMyIp.Modules;

namespace WhatIsMyIp
{
    public static class ModulesController
    {
        /// <summary>
        /// Is IIS Enabled.
        /// </summary>
        internal static bool IsIisEnabled { get; set; }

        /// <summary>
        /// List Modules.
        /// </summary>
        public static void ListModules()
        {
            var listModules = true;

            while (listModules)
            {
                // Clear the Console.
                Console.Clear();

                // Display Modules Header.
                DisplayModulesHeader();

                // Confirm properties.
                Console.WriteLine(@"Available Modules:");
                Console.WriteLine($@"(1) IIS Module: {(IsIisEnabled ? "Enabled" : "Disabled")}");
                Console.WriteLine();
                Console.WriteLine($@"Are these settings correct?{Environment.NewLine}" +
                                  $@"Press Enter to continue.{Environment.NewLine}" +
                                  @"Press 1-1 to reset specific setting.");
                var response = Console.ReadKey();

                // Process response.
                if (response.Key == ConsoleKey.Enter)
                {
                    listModules = false;
                }
                else if (response.Key == ConsoleKey.D1 ||
                         response.Key == ConsoleKey.NumPad1)
                {
                    IsIisEnabled = !IsIisEnabled;
                }
            }
        }

        #region Support

        /// <summary>
        /// Display Modules Header.
        /// Outputs to Console the Modules Header.
        /// </summary>
        private static void DisplayModulesHeader()
        {
            Console.WriteLine(@"##############################################################");
            Console.WriteLine(@"#         Modules | What Is My Ip | Windows Service          #");
            Console.WriteLine(@"##############################################################");
            Console.WriteLine();
        }

        #endregion
    }
}