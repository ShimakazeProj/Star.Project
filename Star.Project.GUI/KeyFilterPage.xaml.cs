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

namespace Star.Project.GUI
{
    /// <summary>
    /// FormaterPage.xaml 的交互逻辑
    /// </summary>
    public partial class KeyFilterPage : IToolPage
    {
        public KeyFilterPage()
        {
            InitializeComponent();
        }

        public string Help => "键过滤器可以帮助您更方便的过滤INI键";

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
                    case nameof(KeyScreenTool.KEEP_KEYS):
                        ASB_Keep_Keys.Text = item.Value;
                        break;
                    case nameof(KeyScreenTool.MATCH_CASE):
                        bool.TryParse(item.Value, out var b);
                        CB_MatchCase.IsOn = b;
                        break;
                    case nameof(KeyScreenTool.IGNORE_SECTIONS):
                        ASB_Ignore_Sections.Text = item.Value;
                        break;
                    case nameof(KeyScreenTool.SORT_KEY):
                        bool.TryParse(item.Value, out b);
                        CB_SortKey.IsOn = b;
                        break;
                    default:
                        break;
                }
            }
        }
        public async Task SaveTemplate(Button sender, FileInfo file)
        {
            var temp = new List<(string Key, string Value)>();
            if (!string.IsNullOrWhiteSpace(ASB_Keep_Keys.Text))
                temp.Add((nameof(KeyScreenTool.KEEP_KEYS), ASB_Keep_Keys.Text));

            if (CB_MatchCase.IsOn)
                temp.Add((nameof(KeyScreenTool.MATCH_CASE), true.ToString()));

            if (!string.IsNullOrWhiteSpace(ASB_Ignore_Sections.Text))
                temp.Add((nameof(KeyScreenTool.IGNORE_SECTIONS), ASB_Ignore_Sections.Text));

            if (CB_SortKey.IsOn)
                temp.Add((nameof(KeyScreenTool.SORT_KEY), true.ToString()));

            await using var fs = file.OpenWrite();
            await using var sw = new StreamWriter(fs);
            foreach (var (Key, Value) in temp)
                await sw.WriteLineAsync(Key + "=" + Value);
            await sw.FlushAsync();
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
                if (string.IsNullOrWhiteSpace(ASB_Keep_Keys.Text))
                    throw new ArgumentNullException("未指定将要保留的内容");

                var list = new List<string>
                {
                    KeyScreenTool.NAME,
                    KeyScreenTool.FILE_INPUT,
                    $"\"{ASB_Input.Text}\"",
                    KeyScreenTool.FILE_OUTPUT,
                    $"\"{ASB_Output.Text}\"",
                    KeyScreenTool.KEEP_KEYS,
                    ASB_Keep_Keys.Text.Replace(';','\x20')
                };

                if (!string.IsNullOrWhiteSpace(ASB_Ignore_Sections.Text))
                {
                    list.Add(KeyScreenTool.IGNORE_SECTIONS);
                    list.Add(ASB_Ignore_Sections.Text.Replace(';', '\x20'));
                }
                if (CB_MatchCase.IsOn) list.Add(KeyScreenTool.MATCH_CASE);
                if (CB_SortKey.IsOn) list.Add(KeyScreenTool.SORT_KEY);

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
                ASB_Output.Text = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length) + ".out.ini";
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
