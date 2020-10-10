using System;
using System.CommandLine;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Star.Project.Test
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public async Task HelpTestAsync()
        {
            Console.WriteLine("Argument: /?");
            await Program.RootCommand.InvokeAsync("/?");

            Console.WriteLine("Argument: --help");
            await Program.RootCommand.InvokeAsync("--help");

            Console.WriteLine("Argument: -h");
            await Program.RootCommand.InvokeAsync("-h");

            Console.WriteLine("Argument: -?");
            await Program.RootCommand.InvokeAsync("-?");

        }
        [TestMethod]
        public async Task VersionTestAsync()
        {
            Console.WriteLine("Argument: --version");
            await Program.RootCommand.InvokeAsync("--version");

        }
    }
}
