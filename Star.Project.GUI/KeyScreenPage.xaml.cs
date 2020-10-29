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
    public partial class KeyScreenPage : IToolPage
    {
        public KeyScreenPage()
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
                    case nameof(KeyScreen.KEEP_KEYS):
                        this.ASB_Keep_Keys.Text = item.Value;
                        break;
                    case nameof(KeyScreen.MATCH_CASE):
                        bool.TryParse(item.Value, out var b);
                        this.CB_MatchCase.IsChecked = b;
                        break;
                    default:
                        break;
                }
            }
        }
        public async void SaveTemplate(Button sender, FileInfo file)
        {
            var temp = new List<(string Key, string Value)>();
            if (!string.IsNullOrWhiteSpace(this.ASB_Keep_Keys.Text))
                temp.Add((nameof(KeyScreen.KEEP_KEYS), this.ASB_Keep_Keys.Text));

            if (this.CB_MatchCase.IsChecked ?? false)
                temp.Add((nameof(KeyScreen.MATCH_CASE), true.ToString()));

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
                    KeyScreen.NAME,
                    KeyScreen.FILE_INPUT,
                    $"\"{this.ASB_Input.Text}\""
                };

                if (!string.IsNullOrWhiteSpace(this.ASB_Output.Text))
                {
                    list.Add(KeyScreen.FILE_OUTPUT);
                    list.Add($"\"{this.ASB_Output.Text}\"");
                }

                if (!string.IsNullOrWhiteSpace(this.ASB_Keep_Keys.Text))
                {
                    list.Add(KeyScreen.KEEP_KEYS);
                    list.AddRange(this.ASB_Keep_Keys.Text.Split(';'));
                }

                if (this.CB_MatchCase.IsChecked ?? false)
                {
                    list.Add(KeyScreen.MATCH_CASE);
                }

                var sb = new StringBuilder().AppendJoin(' ', list).ToString();

                _ = new ConsoleOutputDialog(sb).ShowAsync();
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
