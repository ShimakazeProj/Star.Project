using System;

namespace Star.Project.Data
{
    public class ConsoleErrWrite : ConsoleWriterBase
    {
        public override void Write(string msg, ConsoleColor? color = null)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Error.Write(msg);
            Console.ResetColor();
        }
    }
}

