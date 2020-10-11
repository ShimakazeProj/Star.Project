using System;
using System.CommandLine;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Star.Project.Test
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod("显示程序全部帮助文本")]
        public async Task HelpTestAsync()
        {
            await Program.RootCommand.InvokeAsync("/?");
            Console.WriteLine(new string('-', 30));
            await Program.RootCommand.InvokeAsync(Formater.NAME + " /?");
            Console.WriteLine(new string('-', 30));
            await Program.RootCommand.InvokeAsync(Sorter.NAME + " /?");
        }
        [TestMethod]
        public async Task VersionTestAsync()
        {
            Console.WriteLine("Argument: --version");
            await Program.RootCommand.InvokeAsync("--version");

        }
    }
}
