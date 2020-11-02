using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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


        public void ApplyTemplate(Button sender, FileInfo file)
        {
        }
        public void SaveTemplate(Button sender, FileInfo file)
        {
        }

        public async void Start(Button sender, RoutedEventArgs e)
        {
            var btnText = sender.Content;
            (sender.IsEnabled, sender.Content) = (false, "正在处理");
            try
            {
                if (string.IsNullOrWhiteSpace(this.ASB_Input.Text)
                    || string.IsNullOrWhiteSpace(this.ASB_Output.Text)) return;

                var list = new List<string>
                {
                    IniMergeTool.NAME,
                    IniMergeTool.FILE_INPUT,
                    $"\"{this.ASB_Input.Text}\"",
                    IniMergeTool.FILE_OUTPUT,
                    $"\"{this.ASB_Output.Text}\"",
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
