using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

using Star.Project.Tools;

namespace Star.Project
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await RootCommand.InvokeAsync(args, new Data.SystemConsole());
        }
        public static RootCommand RootCommand
        {
            get
            {
                var cmd = new RootCommand("=StarProject= 工具集")
                {
                    KeyScreenTool.Command,
                    SectionScreenTool.Command,
                    IniSorterTool.Command
                };
                cmd.Name = "StarTools";
                return cmd;
            }
        }
    }
}
