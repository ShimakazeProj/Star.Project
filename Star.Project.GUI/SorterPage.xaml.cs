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
using Star.Project.Tools;

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
                    case nameof(IniSorterTool.TARGET_SECTION):
                        this.tbTargetSectionName.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.PREFIX):
                        this.tbPrefix.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.PREFIX_KEY):
                        this.tbPrefixKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.START_NUM):
                        this.tbFirst.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.DIGIT):
                        this.tbDigit.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.SUMMARY_KEY):
                        this.tbSummaryKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.SORT):
                        bool.TryParse(item.Value, out var b);
                        this.cbSort.IsChecked = b;
                        break;
                    case nameof(IniSorterTool.SORT_KEYS):
                        this.tbSortTargetKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.CONSTRAINT_KEY):
                        this.tbKeyConstraint.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.CONSTRAINT_VALUE):
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
                temp.Add((nameof(IniSorterTool.TARGET_SECTION), this.tbTargetSectionName.Text));

            if (!string.IsNullOrWhiteSpace(this.tbPrefix.Text))
                temp.Add((nameof(IniSorterTool.PREFIX), this.tbPrefix.Text));

            if (!string.IsNullOrWhiteSpace(this.tbPrefixKey.Text))
                temp.Add((nameof(IniSorterTool.PREFIX_KEY), this.tbPrefixKey.Text));

            if (!string.IsNullOrWhiteSpace(this.tbFirst.Text))
                temp.Add((nameof(IniSorterTool.START_NUM), this.tbFirst.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbDigit.Text))
                temp.Add((nameof(IniSorterTool.DIGIT), this.tbDigit.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbSummaryKey.Text))
                temp.Add((nameof(IniSorterTool.SUMMARY_KEY), this.tbSummaryKey.Text));

            if (this.cbSort.IsChecked ?? false)
                temp.Add((nameof(IniSorterTool.SORT), true.ToString()));

            if (!string.IsNullOrWhiteSpace(this.tbSortTargetKey.Text))
                temp.Add((nameof(IniSorterTool.SORT_KEYS), this.tbSortTargetKey.Text));

            if (!string.IsNullOrWhiteSpace(this.tbKeyConstraint.Text))
                temp.Add((nameof(IniSorterTool.CONSTRAINT_KEY), this.tbKeyConstraint.Text));

            if (!string.IsNullOrWhiteSpace(this.tbValueConstraint.Text))
                temp.Add((nameof(IniSorterTool.CONSTRAINT_VALUE), this.tbValueConstraint.Text));
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
                    IniSorterTool.NAME,
                    IniSorterTool.FILE_INPUT,
                    $"\"{this.ASB_Input.Text}\""
                };

                if (!string.IsNullOrWhiteSpace(this.ASB_Output.Text))
                {
                    list.Add(IniSorterTool.FILE_OUTPUT);
                    list.Add($"\"{this.ASB_Output.Text}\"");
                }

                if (!string.IsNullOrWhiteSpace(this.tbTargetSectionName.Text))
                {
                    list.Add(IniSorterTool.TARGET_SECTION);
                    list.Add(this.tbTargetSectionName.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbPrefix.Text))
                {
                    list.Add(IniSorterTool.PREFIX);
                    list.Add(this.tbPrefix.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbPrefixKey.Text))
                {
                    list.Add(IniSorterTool.PREFIX_KEY);
                    list.Add(this.tbPrefixKey.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbFirst.Text))
                {
                    list.Add(IniSorterTool.START_NUM);
                    list.Add(this.tbFirst.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(this.tbDigit.Text))
                {
                    list.Add(IniSorterTool.DIGIT);
                    list.Add(this.tbDigit.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(this.tbSummaryKey.Text))
                {
                    list.Add(IniSorterTool.SUMMARY_KEY);
                    list.Add(this.tbSummaryKey.Text);
                }

                if (this.cbSort.IsChecked ?? false)
                    list.Add(IniSorterTool.SORT);

                if (!string.IsNullOrWhiteSpace(this.tbSortTargetKey.Text))
                {
                    list.Add(IniSorterTool.SORT_KEYS);
                    list.Add(this.tbSortTargetKey.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbKeyConstraint.Text))
                {
                    list.Add(IniSorterTool.CONSTRAINT_KEY);
                    list.Add(this.tbKeyConstraint.Text);
                }

                if (!string.IsNullOrWhiteSpace(this.tbValueConstraint.Text))
                {
                    list.Add(IniSorterTool.CONSTRAINT_VALUE);
                    list.Add(this.tbValueConstraint.Text);
                }

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
