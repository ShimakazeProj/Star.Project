using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Text;

namespace Star.Project.Extensions
{
    public static class LogExtension
    {
        public static void Debug(this IConsole console, string msg) => console.Out.WriteLine($"[{DateTime.Now:O}]DEBUG {msg}");
        public static void Debug(this IConsole console, object msg) => console.Debug(msg.ToString());

        public static void Trace(this IConsole console, string msg) => console.Out.WriteLine($"[{DateTime.Now:O}]TRACE {msg}");
        public static void Trace(this IConsole console, object msg) => console.Trace(msg.ToString());

        public static void Info(this IConsole console, string msg) => console.Out.WriteLine($"[{DateTime.Now:O}]INFO  {msg}");
        public static void Info(this IConsole console, object msg) => console.Info(msg.ToString());

        public static void Warn(this IConsole console, string msg) => console.Out.WriteLine($"[{DateTime.Now:O}]WARN {msg}");
        public static void Warn(this IConsole console, object msg) => console.Warn(msg.ToString());

        public static void Fatal(this IConsole console, string msg) => console.Out.WriteLine($"[{DateTime.Now:O}]FATAL {msg}");
        public static void Fatal(this IConsole console, object msg) => console.Fatal(msg.ToString());
    }
}
