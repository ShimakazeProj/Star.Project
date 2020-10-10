using System.CommandLine;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Star.Project.Test
{
    [TestClass]
    public class CleannerTest
    {
        [TestMethod]
        public async Task CleannerCommandTest()
        {
            await Program.RootCommand.InvokeAsync($"{Cleanner.NAME}");
        }

        [TestMethod("指定目标键测试")]
        public async Task CleannerKeysTest()
        {
            await Program.RootCommand.InvokeAsync(
                $"{Cleanner.NAME} " +
                $"{Cleanner.FILE_INPUT} {@"C:\Users\Shimakaze\Desktop\aaa.txt"} " +
                $"{Cleanner.FILE_OUTPUT} {@"C:\Users\Shimakaze\Desktop\aaa.new.txt"} " +
                $"{Cleanner.KEEP_KEYS} q c a");
        }
    }
}
