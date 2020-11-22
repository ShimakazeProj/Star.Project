using System;
using System.Collections.Generic;
using System.CommandLine;
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
    /// FormaterPage.xaml 的交互逻辑
    /// </summary>
    public partial class SectionFilterPage : IToolPage
    {
        public SectionFilterPage()
        {
            InitializeComponent();
        }
        public string Help => "节过滤器可以帮助您更方便的过滤INI节";
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
                    case nameof(SectionScreenTool.KEEP_SECTIONS):
                        ASB_Keep_Sections.Text = item.Value;
                        break;
                    case nameof(SectionScreenTool.MATCH_CASE):
                        bool.TryParse(item.Value, out var b);
                        CB_MatchCase.IsOn = b;
                        break;
                    default:
                        break;
                }
            }
        }
        public async Task SaveTemplate(Button sender, FileInfo file)
        {
            var temp = new List<(string Key, string Value)>();

            if (!string.IsNullOrWhiteSpace(ASB_Keep_Sections.Text))
                temp.Add((nameof(SectionScreenTool.KEEP_SECTIONS), ASB_Keep_Sections.Text));

            if (CB_MatchCase.IsOn)
                temp.Add((nameof(SectionScreenTool.MATCH_CASE), true.ToString()));

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
                SectionScreenTool.NAME,
                SectionScreenTool.FILE_INPUT,
                $"\"{e.InputFile}\"",
                SectionScreenTool.FILE_OUTPUT,
                $"\"{e.OutputFile}\""
            };

            if (!string.IsNullOrWhiteSpace(ASB_Keep_Sections.Text))
            {
                list.Add(SectionScreenTool.KEEP_SECTIONS);
                list.AddRange(ASB_Keep_Sections.Text.Split(';'));
            }

            if (CB_MatchCase.IsOn)
            {
                list.Add(SectionScreenTool.MATCH_CASE);
            }

            await new ConsoleOutputDialog(new StringBuilder().AppendJoin(' ', list).ToString()).ShowAsync();

        }
    }
}
