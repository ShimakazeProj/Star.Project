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
            await Program.RootCommand.InvokeAsync($"{SectionScreen.NAME}");
            await Program.RootCommand.InvokeAsync($"{KeyScreen.NAME}");
        }

        [TestMethod("指定目标键测试")]
        public async Task CleannerKeysTest()
        {
            await Program.RootCommand.InvokeAsync(
                $"{SectionScreen.NAME} " +
                $"{SectionScreen.FILE_INPUT} {@"C:\Users\Shimakaze\Desktop\aaa.txt"} " +
                $"{SectionScreen.FILE_OUTPUT} {@"C:\Users\Shimakaze\Desktop\aaa.new.txt"} ");
        }
    }
}
