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
            await Program.RootCommand.InvokeAsync($"{Formater.NAME}");
        }

        [TestMethod("ָ��Ŀ�������")]
        public async Task CleannerKeysTest()
        {
            await Program.RootCommand.InvokeAsync(
                $"{Formater.NAME} " +
                $"{Formater.FILE_INPUT} {@"C:\Users\Shimakaze\Desktop\aaa.txt"} " +
                $"{Formater.FILE_OUTPUT} {@"C:\Users\Shimakaze\Desktop\aaa.new.txt"} " +
                $"{Formater.KEEP_KEYS} q c a");
        }
    }
}
