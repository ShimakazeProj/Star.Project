using System;
using System.Collections.Generic;
using System.CommandLine;
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

namespace Star.Project.GUI
{
    /// <summary>
    /// FormaterPage.xaml 的交互逻辑
    /// </summary>
    public partial class FormaterPage
    {
        public FormaterPage()
        {
            InitializeComponent();
        }

        private async void FormatButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.ASB_Input.Text))
            {
                return;
            }
            var btn = sender as Button;
            btn.IsEnabled = false;
            btn.Content = "正在处理";
            try
            {
                var list = new List<string>
                {
                    Formater.NAME,
                    Formater.FILE_INPUT,
                    this.ASB_Input.Text
                };

                if (!string.IsNullOrWhiteSpace(this.ASB_Output.Text))
                {
                    list.Add(Formater.FILE_OUTPUT);
                    list.Add(this.ASB_Output.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.ASB_Keep_Keys.Text))
                {
                    list.Add(Formater.KEEP_KEYS);
                    list.AddRange(this.ASB_Keep_Keys.Text.Split(';'));
                }

                if (!string.IsNullOrWhiteSpace(this.ASB_Keep_Sections.Text))
                {
                    list.Add(Formater.KEEP_SECTIONS);
                    list.AddRange(this.ASB_Keep_Sections.Text.Split(';'));
                }

                if (this.CB_MatchCase.IsChecked ?? false)
                {
                    list.Add(Formater.MATCH_CASE);
                }


                if (this.CB_IntAct.IsChecked ?? false)
                {
                    list.Add(Formater.KEEP_SECTION_INTACT);
                }

                var sb = new StringBuilder().AppendJoin(' ', list).ToString();
                this.consoleOutput.Document.Blocks.Clear();
                this.E_Output.IsExpanded = true;
                await Program.RootCommand.InvokeAsync(sb, new RichTextBoxConsole(this.consoleOutput));
            }
            finally
            {
                btn.IsEnabled = true;
                btn.Content = "开始格式化";
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
