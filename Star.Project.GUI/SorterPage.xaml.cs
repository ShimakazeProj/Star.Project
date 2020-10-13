using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using ModernWpf.Controls;

using Star.Project.GUI.Data;

namespace Star.Project.GUI
{
    /// <summary>
    /// SorterPage.xaml 的交互逻辑
    /// </summary>
    public partial class SorterPage
    {
        public SorterPage()
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
                    Sorter.NAME,
                    Sorter.FILE_INPUT,
                    this.ASB_Input.Text
                };

                if (!string.IsNullOrWhiteSpace(this.ASB_Output.Text))
                {
                    list.Add(Sorter.FILE_OUTPUT);
                    list.Add(this.ASB_Output.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbTargetSectionName.Text))
                {
                    list.Add(Sorter.TARGET_SECTION);
                    list.Add(this.tbTargetSectionName.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbPrefix.Text))
                {
                    list.Add(Sorter.PREFIX);
                    list.Add(this.tbPrefix.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbPrefixKey.Text))
                {
                    list.Add(Sorter.PREFIX_KEY);
                    list.Add(this.tbPrefixKey.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbFirst.Text))
                {
                    list.Add(Sorter.START_NUM);
                    list.Add(this.tbFirst.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(this.tbDigit.Text))
                {
                    list.Add(Sorter.DIGIT);
                    list.Add(this.tbDigit.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(this.tbSummaryKey.Text))
                {
                    list.Add(Sorter.SUMMARY_KEY);
                    list.Add(this.tbSummaryKey.Text);
                }

                if (this.cbSort.IsChecked ?? false)
                    list.Add(Sorter.SORT);

                if (!string.IsNullOrWhiteSpace(this.tbSortTargetKey.Text))
                {
                    list.Add(Sorter.SORT_KEYS);
                    list.Add(this.tbSortTargetKey.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbKeyConstraint.Text))
                {
                    list.Add(Sorter.CONSTRAINT_KEY);
                    list.Add(this.tbKeyConstraint.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbValueConstraint.Text))
                {
                    list.Add(Sorter.CONSTRAINT_VALUE);
                    list.Add(this.tbValueConstraint.Text);
                }

                var sb = new StringBuilder().AppendJoin(' ', list).ToString();
                this.consoleOutput.Document.Blocks.Clear();
                this.E_Output.IsExpanded = true;
                await Program.RootCommand.InvokeAsync(sb, new RichTextBoxConsole(this.consoleOutput));
            }
#if !DEBUG
            catch (Exception ex)
            {
                using var logfs = new FileStream("program.err.log", FileMode.Create, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(logfs);
                await sw.WriteLineAsync(ex.ToString());
                await sw.WriteLineAsync("全部堆栈跟踪");
                await sw.WriteLineAsync(ex.StackTrace);
                await sw.FlushAsync();
                MessageBox.Show(ex.ToString(), "发生异常:");

            }
#endif
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
