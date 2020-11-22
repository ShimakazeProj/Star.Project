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

        public string Help => "排序工具可以更容易的处理INI文件";

        public async Task ApplyTemplate(Button sender, FileInfo file)
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
                        tbTargetSectionName.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.PREFIX):
                        tbPrefix.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.PREFIX_KEY):
                        tbPrefixKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.START_NUM):
                        tbFirst.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.DIGIT):
                        tbDigit.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.SUMMARY_KEY):
                        tbSummaryKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.SORT):
                        bool.TryParse(item.Value, out var b);
                        cbSort.IsOn = b;
                        break;
                    case nameof(IniSorterTool.SORT_KEYS):
                        tbSortTargetKey.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.CONSTRAINT_KEY):
                        tbKeyConstraint.Text = item.Value;
                        break;
                    case nameof(IniSorterTool.CONSTRAINT_VALUE):
                        tbValueConstraint.Text = item.Value;
                        break;
                    default:
                        break;
                }
            }
        }
        public async Task SaveTemplate(Button sender, FileInfo file)
        {
            var temp = new List<(string Key, string Value)>();
            if (!string.IsNullOrWhiteSpace(tbTargetSectionName.Text))
                temp.Add((nameof(IniSorterTool.TARGET_SECTION), tbTargetSectionName.Text));

            if (!string.IsNullOrWhiteSpace(tbPrefix.Text))
                temp.Add((nameof(IniSorterTool.PREFIX), tbPrefix.Text));

            if (!string.IsNullOrWhiteSpace(tbPrefixKey.Text))
                temp.Add((nameof(IniSorterTool.PREFIX_KEY), tbPrefixKey.Text));

            if (!string.IsNullOrWhiteSpace(tbFirst.Text))
                temp.Add((nameof(IniSorterTool.START_NUM), tbFirst.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(tbDigit.Text))
                temp.Add((nameof(IniSorterTool.DIGIT), tbDigit.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(tbSummaryKey.Text))
                temp.Add((nameof(IniSorterTool.SUMMARY_KEY), tbSummaryKey.Text));

            if (cbSort.IsOn)
                temp.Add((nameof(IniSorterTool.SORT), true.ToString()));

            if (!string.IsNullOrWhiteSpace(tbSortTargetKey.Text))
                temp.Add((nameof(IniSorterTool.SORT_KEYS), tbSortTargetKey.Text));

            if (!string.IsNullOrWhiteSpace(tbKeyConstraint.Text))
                temp.Add((nameof(IniSorterTool.CONSTRAINT_KEY), tbKeyConstraint.Text));

            if (!string.IsNullOrWhiteSpace(tbValueConstraint.Text))
                temp.Add((nameof(IniSorterTool.CONSTRAINT_VALUE), tbValueConstraint.Text));
            await using var fs = file.OpenWrite();
            await using var sw = new StreamWriter(fs);
            foreach (var (Key, Value) in temp)
                await sw.WriteLineAsync(Key + "=" + Value);
            await sw.FlushAsync();
        }

        public async Task Start(object sender, StartEventArgs e)
        {
            var list = new List<string>
            {
                IniSorterTool.NAME,
                IniSorterTool.FILE_INPUT,
                $"\"{e.InputFile}\"",
                IniSorterTool.FILE_OUTPUT,
                $"\"{e.OutputFile}\""
            };

            if (!string.IsNullOrWhiteSpace(tbTargetSectionName.Text))
            {
                list.Add(IniSorterTool.TARGET_SECTION);
                list.Add(tbTargetSectionName.Text);
            }

            if (!string.IsNullOrWhiteSpace(tbPrefix.Text))
            {
                list.Add(IniSorterTool.PREFIX);
                list.Add(tbPrefix.Text);
            }

            if (!string.IsNullOrWhiteSpace(tbPrefixKey.Text))
            {
                list.Add(IniSorterTool.PREFIX_KEY);
                list.Add(tbPrefixKey.Text);
            }

            if (!string.IsNullOrWhiteSpace(tbFirst.Text))
            {
                list.Add(IniSorterTool.START_NUM);
                list.Add(tbFirst.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(tbDigit.Text))
            {
                list.Add(IniSorterTool.DIGIT);
                list.Add(tbDigit.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(tbSummaryKey.Text))
            {
                list.Add(IniSorterTool.SUMMARY_KEY);
                list.Add(tbSummaryKey.Text);
            }

            if (cbSort.IsOn)
                list.Add(IniSorterTool.SORT);

            if (!string.IsNullOrWhiteSpace(tbSortTargetKey.Text))
            {
                list.Add(IniSorterTool.SORT_KEYS);
                list.Add(tbSortTargetKey.Text);
            }

            if (!string.IsNullOrWhiteSpace(tbKeyConstraint.Text))
            {
                list.Add(IniSorterTool.CONSTRAINT_KEY);
                list.Add(tbKeyConstraint.Text);
            }

            if (!string.IsNullOrWhiteSpace(tbValueConstraint.Text))
            {
                list.Add(IniSorterTool.CONSTRAINT_VALUE);
                list.Add(tbValueConstraint.Text);
            }

            await new ConsoleOutputDialog(new StringBuilder().AppendJoin(' ', list).ToString()).ShowAsync();
        }
    }
}
