using System;

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

            if (String.IsNullOrWhiteSpace(input) == false)
            {
                Console.WriteLine($@"Is ({input}) correct?{Environment.NewLine}Press Enter to continue. Any other key to reset setting.");
                var response = Console.ReadKey();

                if (response.Key == ConsoleKey.Enter)
                {
                    // Confirmation.
                    return (true, input);
                }
            }

            return (false, String.Empty);
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
        /// <returns>Returns a bool of the input.</returns>
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

                if (String.IsNullOrEmpty(input) == false)
                {
                    valid = Boolean.TryParse(input, out value);
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
    }
}