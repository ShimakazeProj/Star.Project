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

        public async Task Start(Button sender, RoutedEventArgs e)
        {
            var btnText = sender.Content;
            (sender.IsEnabled, sender.Content) = (false, "正在处理");
            try
            {
                if (string.IsNullOrWhiteSpace(ASB_Input.Text)
                    || string.IsNullOrWhiteSpace(ASB_Output.Text))
                    throw new ArgumentNullException("未指定输入或输出文件");

                var list = new List<string>
                {
                    IniMergeTool.NAME,
                    IniMergeTool.FILE_INPUT,
                    $"\"{ASB_Input.Text}\"",
                    IniMergeTool.FILE_OUTPUT,
                    $"\"{ASB_Output.Text}\"",
                };

                await new ConsoleOutputDialog(new StringBuilder().AppendJoin(' ', list).ToString()).ShowAsync();
            }
            finally
            {
                (sender.IsEnabled, sender.Content) = (true, btnText);
            }
        }
        private void Input_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "INI文件|*.ini|所有文件|*.*"
            };
            if (ofd.ShowDialog() ?? false)
            {
                var fileInfo = new FileInfo(ofd.FileName);
                sender.Text = fileInfo.FullName;
                this.ASB_Output.Text = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length) + ".out.ini";
            }
        }

        private void Output_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "INI文件|*.ini|所有文件|*.*"
            };
            if (sfd.ShowDialog() ?? false)
            {
                sender.Text = sfd.FileName;
            }
        }
    }
}
