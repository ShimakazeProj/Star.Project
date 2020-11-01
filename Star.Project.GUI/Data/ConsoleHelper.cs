using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Star.Project.GUI.Data
{
    /// <summary>
    /// 该类源于Stackoverflow 的一问题贴的回答：
    /// 问题：No output to console from a WPF application?
    /// see http://stackoverflow.com/questions/160587/no-output-to-console-from-a-wpf-application/718505
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        /// <summary>
        /// Allocates a new console for current process.
        /// </summary>
        [DllImport(Kernel32_DllName)]
        public static extern bool AllocConsole();

        /// <summary>
        /// Frees the console.
        /// </summary>
        [DllImport(Kernel32_DllName)]
        public static extern bool FreeConsole();

        /* Retrieves the title for the current console window. */
        [DllImport(Kernel32_DllName, EntryPoint = "GetConsoleTitle", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetConsoleTitle(
            StringBuilder lpConsoleTitle,
            int nSize
            );


        [DllImport(Kernel32_DllName)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        public static extern int GetConsoleOutputCP();

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            //#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
            //#endif
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            //#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
            //#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            FieldInfo _out = type.GetField("s_out",
                BindingFlags.Static | BindingFlags.NonPublic);

            FieldInfo _error = type.GetField("s_error",
                BindingFlags.Static | BindingFlags.NonPublic);

            //MethodInfo _InitializeStdOutError = type.GetMethod("EnsureInitialized",
            //    BindingFlags.Static | BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            //Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            //_InitializeStdOutError.Invoke(null, new object[] { true });
            _ = Console.Out;
            _ = Console.Error;
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
