using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Help;
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
                var cmd = new RootCommand("=StarProject= 工具集");
                cmd.Name = "StarTools";
                cmd.Add(Formater.GetCommand());
                cmd.Add(Sorter.GetCommand());
                return cmd;
            }
        }
    }
}
