using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Star.Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await RootCommand.InvokeAsync(args);
        }
        public static RootCommand RootCommand
        {
            get
            {
                var cmd = new RootCommand("=StarProject=工具集");
                cmd.Name = "StarProject";
                cmd.Add(Cleanner.GetCommand());
                return cmd;
            }
        }
    }
}
