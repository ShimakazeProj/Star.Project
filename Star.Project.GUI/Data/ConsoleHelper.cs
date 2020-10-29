using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Star.Project.GUI.Data
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// Allocates a new console for current process.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// Frees the console.
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        /* Retrieves the title for the current console window. */
        [DllImport("kernel32.dll", EntryPoint = "GetConsoleTitle", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetConsoleTitle(
            StringBuilder lpConsoleTitle,
            int nSize
            );
    }

}
