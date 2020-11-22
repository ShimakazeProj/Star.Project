using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using ModernWpf.Controls;

using Star.Project.GUI.Data;
using Star.Project.Tools;

namespace Star.Project.GUI
{
    /// <summary>
    /// IniMergePage.xaml 的交互逻辑
    /// </summary>
    public partial class IniMergePage : IToolPage
    {
        public IniMergePage()
        {
            InitializeComponent();
        }

        public string Help => "INI 合并工具可以通过读取INI文件中已存在的[#Include]节中定义的文件, 并将其整合成一个文件";

        public async Task ApplyTemplate(Button sender, FileInfo file)
        {
            await Task.Delay(0);
        }
        public async Task SaveTemplate(Button sender, FileInfo file)
        {
            await Task.Delay(0);
        }

        public async Task Start(object sender, StartEventArgs e)
        {
            var list = new List<string>
            {
                IniMergeTool.NAME,
                IniMergeTool.FILE_INPUT,
                $"\"{e.InputFile}\"",
                IniMergeTool.FILE_OUTPUT,
                $"\"{e.OutputFile}\""
            };

            await new ConsoleOutputDialog(new StringBuilder().AppendJoin(' ', list).ToString()).ShowAsync();

        }
    }
}
