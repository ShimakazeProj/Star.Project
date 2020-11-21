using System;
using System.CommandLine;
using System.CommandLine.IO;

namespace Star.Project.Data
{
    public class SystemConsole : IConsole
    {
        public IStandardStreamWriter Error { get; }
        public IStandardStreamWriter Out { get; }

        public bool IsErrorRedirected => false;
        public bool IsOutputRedirected => false;
        public bool IsInputRedirected => false;

        public SystemConsole()
        {
            Out = StandardStreamWriter.Create(new ConsoleStdWriter());
            Error = StandardStreamWriter.Create(Console.Error);
        }
    }
}
