using System;

namespace Star.Project.Data
{
    public class ConsoleStdWriter : ConsoleWriterBase
    {
        public override void Write(string msg, ConsoleColor? color = null)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}

