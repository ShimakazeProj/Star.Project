using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Microsoft.Win32;

using ModernWpf.Controls;

using Shimakaze.Struct.Ini;

using Star.Project.Data;
using Star.Project.GUI.Data;

namespace Star.Project.GUI
{
    /// <summary>
    /// SorterPage.xaml 的交互逻辑
    /// </summary>
    public partial class SorterPage : IToolPage
    {
        public SorterPage()
        {
            InitializeComponent();
        }

        public async void ApplyTemplate(Button sender, FileInfo file)
        {
            await using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fs);
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();
                var dataRaw = line.Split(';', '#')[0].Split('=');

                (string Key, string Value) item = (dataRaw[0], dataRaw[1]);

                switch (item.Key.Trim().ToUpper())
                {
                    case nameof(Sorter.TARGET_SECTION):
                        this.tbTargetSectionName.Text = item.Value;
                        break;
                    case nameof(Sorter.PREFIX):
                        this.tbPrefix.Text = item.Value;
                        break;
                    case nameof(Sorter.PREFIX_KEY):
                        this.tbPrefixKey.Text = item.Value;
                        break;
                    case nameof(Sorter.START_NUM):
                        this.tbFirst.Text = item.Value;
                        break;
                    case nameof(Sorter.DIGIT):
                        this.tbDigit.Text = item.Value;
                        break;
                    case nameof(Sorter.SUMMARY_KEY):
                        this.tbSummaryKey.Text = item.Value;
                        break;
                    case nameof(Sorter.SORT):
                        bool.TryParse(item.Value, out var b);
                        this.cbSort.IsChecked = b;
                        break;
                    case nameof(Sorter.SORT_KEYS):
                        this.tbSortTargetKey.Text = item.Value;
                        break;
                    case nameof(Sorter.CONSTRAINT_KEY):
                        this.tbKeyConstraint.Text = item.Value;
                        break;
                    case nameof(Sorter.CONSTRAINT_VALUE):
                        this.tbValueConstraint.Text = item.Value;
                        break;
                    default:
                        break;
                }
            }
        }
        public async void SaveTemplate(Button sender, FileInfo file)
        {
            var temp = new List<(string Key, string Value)>();
            if (!string.IsNullOrWhiteSpace(this.tbTargetSectionName.Text))
                temp.Add((nameof(Sorter.TARGET_SECTION), this.tbTargetSectionName.Text));

            if (!string.IsNullOrWhiteSpace(this.tbPrefix.Text))
                temp.Add((nameof(Sorter.PREFIX), this.tbPrefix.Text));

            if (!string.IsNullOrWhiteSpace(this.tbPrefixKey.Text))
                temp.Add((nameof(Sorter.PREFIX_KEY), this.tbPrefixKey.Text));

            if (!string.IsNullOrWhiteSpace(this.tbFirst.Text))
                temp.Add((nameof(Sorter.START_NUM), this.tbFirst.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbDigit.Text))
                temp.Add((nameof(Sorter.DIGIT), this.tbDigit.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbSummaryKey.Text))
                temp.Add((nameof(Sorter.SUMMARY_KEY), this.tbSummaryKey.Text));

            if (this.cbSort.IsChecked ?? false)
                temp.Add((nameof(Sorter.SORT), true.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbSortTargetKey.Text))
                temp.Add((nameof(Sorter.SORT_KEYS), this.tbSortTargetKey.Text));

            if (!string.IsNullOrWhiteSpace(this.tbKeyConstraint.Text))
                temp.Add((nameof(Sorter.CONSTRAINT_KEY), this.tbKeyConstraint.Text));

            if (!string.IsNullOrWhiteSpace(this.tbValueConstraint.Text))
                temp.Add((nameof(Sorter.CONSTRAINT_VALUE), this.tbValueConstraint.Text));
            await using var fs = file.OpenWrite();
            await using var sw = new StreamWriter(fs);
            foreach (var (Key, Value) in temp)
                await sw.WriteLineAsync(Key + "=" + Value);
            await sw.FlushAsync();
        }

        public async void Start(Button sender, RoutedEventArgs e)
        {
            var btnText = sender.Content;
            (sender.IsEnabled, sender.Content) = (false, "正在处理");
            try
            {
                if (string.IsNullOrWhiteSpace(this.ASB_Input.Text)) return;
                var list = new List<string>
                {
                    Sorter.NAME,
                    Sorter.FILE_INPUT,
                    $"\"{this.ASB_Input.Text}\""
                };

                if (!string.IsNullOrWhiteSpace(this.ASB_Output.Text))
                {
                    list.Add(Sorter.FILE_OUTPUT);
                    list.Add($"\"{this.ASB_Output.Text}\"");
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

                _ = new ConsoleOutputDialog(sb).ShowAsync();

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
