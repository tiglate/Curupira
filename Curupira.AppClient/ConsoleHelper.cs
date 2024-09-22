using System;

namespace Curupira.AppClient
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// Centers the given text within the console window.
        /// </summary>
        /// <param name="text">The text to center.</param>
        public static void WriteCentered(string text, bool newLine = true)
        {
            // Get the console window width
            int consoleWidth = Console.WindowWidth;

            // Calculate the padding required on each side
            int padding = (consoleWidth - text.Length) / 2;

            // Write the padding and then the text
            Console.Write(new string(' ', padding));
            if (newLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }
        }
    }
}
