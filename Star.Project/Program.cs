using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Star.Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await RootCommand.InvokeAsync(args, new Data.SystemConsole());
        }
        public static RootCommand RootCommand
        {
            get
            {
                var cmd = new RootCommand("=StarProject= 工具集");
                cmd.Name = "StarTools";
                cmd.Add(KeyScreen.Command);
                cmd.Add(SectionScreen.GetCommand());
                cmd.Add(Sorter.GetCommand());
                return cmd;
            }
        }
    }
}
