using System;
using System.IO;
using System.Security;

namespace WhatIsMyIp
{
    public static class Support
    {
        /// <summary>
        /// Display Setup Header.
        /// Outputs to Console the Setup Header.
        /// </summary>
        internal static void DisplaySetupHeader()
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
        /// <param name="displayHeader">The Display Header method unique to the class calling it.</param>
        /// <returns>Returns a Tuple{bool, string} defining whether the input was accepted and returns the input value as a string.</returns>
        internal static (bool, string) ConfirmStringInput(string propertyTitle, string propertyDescription, Action displayHeader)
        {
            // Clear the Console.
            Console.Clear();

            // Display Header.
            displayHeader();

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
        /// Confirms int input by reviewing the input and asking for confirmation before continueing.
        /// Allows user to provide input again if incorrectly provided earlier.
        /// </summary>
        /// <param name="propertyTitle">The property's title. Used prior to the input on input line.</param>
        /// <param name="propertyDescription">The property's description. Defines the requirement.</param>
        /// <param name="displayHeader">The Display Header method unique to the class calling it.</param>
        /// <returns>Returns a Tuple{bool, int} defining whether the input was accepted and returns the input value as an int.</returns>
        internal static (bool, int) ConfirmIntInput(string propertyTitle, string propertyDescription, Action displayHeader)
        {
            // Clear the Console.
            Console.Clear();

            // Display Header.
            displayHeader();

            // Set property.
            Console.WriteLine(propertyDescription);
            Console.Write(propertyTitle);

            if (Int32.TryParse(Console.ReadLine(), out var input))
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

        /// <summary>
        /// Confirm Bool Input.
        /// Confirms boolean input by reviewing the input and asking for confirmation before continuing.
        /// Allows user to provide input again if incorrectly provided earlier.
        /// </summary>
        /// <param name="propertyTitle">The property's title. Used prior to the input on input line.</param>
        /// <param name="propertyDescription">The property's description. Defines the requirement.</param>
        /// <param name="displayHeader">The Display Header method unique to the class calling it.</param>
        /// <returns>Returns a Tuple{bool, bool} defining whether the input was accepted and returns the input value as an bool.</returns>
        internal static (bool, bool) ConfirmBoolInput(string propertyTitle, string propertyDescription, Action displayHeader)
        {
            // Clear the Console.
            Console.Clear();

            // Display Header.
            displayHeader();

            // Set property.
            Console.WriteLine(propertyDescription);
            Console.Write(propertyTitle);

            var valid = false;
            var value = false;

            // Process boolean response.
            do
            {
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) == false)
                {
                    valid = bool.TryParse(input, out value);
                }
            } while (!valid);

            Console.WriteLine($@"Is ({value}) correct?{Environment.NewLine}Press Enter to continue. Any other key to reset setting.");
            var response = Console.ReadKey();

            if (response.Key == ConsoleKey.Enter)
            {
                // Confirmation.
                return (true, value);
            }

            return (false, value);
        }

        /// <summary>
        /// Confirm Secure String Input.
        /// Confirms SecureString input by reviewing the input and asking for confirmation before continuing.
        /// Allows user to provide input again if incorrectly provided earlier.
        /// </summary>
        /// <param name="propertyTitle">The property's title. Used prior to the input on input line.</param>
        /// <param name="propertyDescription">The property's description. Defines the requirement.</param>
        /// <param name="displayHeader">The Display Header method unique to the class calling it.</param>
        /// <returns>Returns a Tuple{bool, SecureString} defining whether the input was accepted and returns the input value as an SecureString.</returns>
        /// <remarks>SecureStrings are not stored in Managed Memory and are ideal for storing sensitive data.</remarks>
        internal static (bool, SecureString) ConfirmSecureStringInput(string propertyTitle, string propertyDescription, Action displayHeader)
        {
            // Clear the Console.
            Console.Clear();

            // Display Header.
            displayHeader();

            // Instatiate new SecureString.
            var password = new SecureString();

            // Set property.
            Console.WriteLine(propertyDescription);
            Console.Write(propertyTitle);
            var input = Console.ReadKey(true);

            // Check for Enter key.
            while (input.Key != ConsoleKey.Enter)
            {
                if (input.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        // remove last entry in password.
                        password.RemoveAt(password.Length - 1);
                        
                        // Clear * from last input.
                        Console.Write(input.KeyChar);
                        Console.Write(@" ");
                        Console.Write(input.KeyChar);
                    }
                }
                else
                {
                    // Add character to password.
                    password.AppendChar(input.KeyChar);

                    // Output an * to the screen.
                    Console.Write(@"*");
                }

                input = Console.ReadKey(true);
            }

            // Secure password by making it readonly.
            password.MakeReadOnly();

            // Confirmation.
            return (true, password);
        }

        /// <summary>
        /// Create Directory.
        /// </summary>
        /// <param name="filePath">The file path of where the directory is to be created.</param>
        internal static void CreateDirectory(string filePath)
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
                    File.AppendAllText(WhatIsMyIp.LogFilePath + $@"{ DateTime.Now:(yyyy-MM-dd)}.log", $@"{DateTime.Now} - An error occured: {ex}{Environment.NewLine}{Environment.NewLine}");
                }
            }
        }
    }
}