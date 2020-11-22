using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

using Microsoft.Win32;

using ModernWpf;
using ModernWpf.Controls;

using Star.Project.GUI.Data;

namespace Star.Project.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            NavControl.SelectedItem = NavControl.MenuItems[0];
            App.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;
            ABTB_Theme.Label = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark ? "浅色主题" : "深色主题";
            ABTB_Theme.Checked += ToggleTheme;
            ABTB_Theme.Unchecked += ToggleTheme;
        }

        /// <summary>
        /// 未处理异常调度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText("exception.log", e.Exception.ToString());
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark
                ? (ApplicationTheme?)ApplicationTheme.Light
                : (ApplicationTheme?)ApplicationTheme.Dark;
        }
        private static Brush defaultASBBrush;

        /// <summary>
        /// 窗口主题被修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ActualThemeChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ThemeManager.GetActualTheme(this));
        }
        private Type lastSelected = null;
        private bool ignore = false;
        private async void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (ignore) return;
            var select = args.SelectedItem as NavigationViewItem;
            if ((select.Tag as Type) == lastSelected) return;
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(lastSelected = typeof(SettingsPage));
                commandBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                ContentFrame.Navigate(lastSelected = (select.Tag as Type));
                commandBar.Visibility = Visibility.Visible;
                await LoadTemplate((select.Tag as Type).Name);
            }
        }
        private async Task LoadTemplate(string name)
        {
            await CBF_Template.Dispatcher.InvokeAsync(CBF_Template.SecondaryCommands.Clear);
            var dir = new DirectoryInfo(Path.Combine("Template", name));

            if (!dir.Exists) goto NoTemplate;
            var files = dir.GetFiles();
            if (files.Length == 0) goto NoTemplate;

            for (int i = 0; i < files.Length; i++)
            {
                var btn = new AppBarButton
                {
                    Label = files[i].Name,
                    Icon = new SymbolIcon(Symbol.ImportAll),
                    Tag = files[i]
                };
                btn.Click += TemplateItem_Click;
                await CBF_Template.Dispatcher.InvokeAsync(() => CBF_Template.SecondaryCommands.Add(btn));
            }
            return;

        NoTemplate:
            await CBF_Template.Dispatcher.InvokeAsync(() => CBF_Template.SecondaryCommands.Add(new AppBarButton
            {
                Label = "暂无保存的模板",
                Icon = new SymbolIcon(Symbol.Cancel),
                IsEnabled = false
            }));
        }

        private async void TemplateItem_Click(object sender, RoutedEventArgs e)
        {
            switch (ContentFrame.Content)
            {
                case IToolPage page:
                    var btn = sender as Button;
                    await page.ApplyTemplate(btn, btn.Tag as FileInfo);
                    break;
            }
        }
        private async void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dir = new DirectoryInfo(Path.Combine("Template", ((NavControl.SelectedItem as NavigationViewItem).Tag as Type).Name));
            if (!dir.Exists) dir.Create();
            if (ContentFrame.Content is IToolPage page)
            {
                var tb = new AutoSuggestBox
                {
                    PlaceholderText = "请输入模板名"
                };
                var dialog = new ContentDialog
                {
                    Title = "保存模板",
                    Content = tb,
                    PrimaryButtonText = "确认",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
            ShowDialog:
                switch (await dialog.ShowAsync())
                {
                    case ContentDialogResult.None:
                        break;
                    case ContentDialogResult.Primary:
                        if (string.IsNullOrEmpty(tb.Text.Trim()))
                        {
                            tb.Header = "模板名不能为空";
                            tb.BorderBrush = Brushes.Red;
                            goto ShowDialog;
                        }
                        if (commandBar.SecondaryCommands[2] is AppBarButton btn && !btn.IsEnabled)
                            await commandBar.Dispatcher.InvokeAsync(() => commandBar.SecondaryCommands.RemoveAt(2));
                        var file = new FileInfo(Path.Combine(dir.FullName, tb.Text.Trim()));
                        await page.SaveTemplate(sender as Button, file);

                        btn = new AppBarButton
                        {
                            Label = file.Name,
                            Icon = new SymbolIcon(Symbol.ImportAll),
                            Tag = file
                        };
                        btn.Click += this.TemplateItem_Click;
                        await commandBar.Dispatcher.InvokeAsync(() => commandBar.SecondaryCommands.Insert(commandBar.SecondaryCommands.Count - 2, btn));

                        break;
                    case ContentDialogResult.Secondary:
                        break;
                }
            }

        }

        private async void TemplateManager_Click(object sender, RoutedEventArgs e)
        {
            await new TemplateManagerDialog(commandBar.SecondaryCommands.Where(i => i is AppBarButton btn && btn.Tag is FileInfo)
                                                                            .Select(i => (i as AppBarButton).Tag as FileInfo)).ShowAsync();
            await LoadTemplate(ContentFrame.Content.GetType().Name);
        }

        private async void RunButton_Click(object o, RoutedEventArgs e)
        {
            var sender = o as AppBarButton;
            switch (ContentFrame.Content)
            {
                case IToolPage page:
                    var flag_exit = false;

                    sender.Label = "正在处理";
                    sender.IsEnabled = false;
                    sender.Icon = new SymbolIcon(Symbol.Pause);
                    try
                    {
                        if (string.IsNullOrWhiteSpace(ASB_Input.Text))
                        {
                            fileExpander.IsExpanded = true;
                            ASB_IsNullWarn(ASB_Input);
                            flag_exit = true;
                        }
                        if (string.IsNullOrWhiteSpace(ASB_Output.Text))
                        {
                            fileExpander.IsExpanded = true;
                            ASB_IsNullWarn(ASB_Output);
                            flag_exit = true;
                        }
                        if (flag_exit)
                        {
                            throw new ArgumentNullException("未指定文件");
                        }

                        await page.Start(this, new StartEventArgs(sender, e, ASB_Input.Text, ASB_Output.Text));
                    }
                    catch (Exception ex)
                    {
                        await new ContentDialog
                        {
                            Title = "错误信息",
                            Content = ex.Message,
                            CloseButtonText = "关闭"
                        }.ShowAsync();
                    }
                    finally
                    {
                        sender.Label = "执行";
                        sender.IsEnabled = true;
                        sender.Icon = new SymbolIcon(Symbol.Play);
                    }

                    break;
            }
        }


        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            switch (ContentFrame.Content)
            {
                case IToolPage page:
                    await new ContentDialog
                    {
                        Title = "帮助",
                        Content = page.Help,
                        CloseButtonText = "关闭"
                    }.ShowAsync();
                    break;
            }
        }
        private void Input_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "INI文件|*.ini|所有文件|*.*",
                Title = "请选择一个源文件"
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
                Filter = "INI文件|*.ini|所有文件|*.*",
                Title = "请选择目标文件保存位置"
            };
            if (sfd.ShowDialog() ?? false)
            {
                sender.Text = sfd.FileName;
            }
        }
        internal static void ASB_IsNullWarn(AutoSuggestBox sender)
        {
            defaultASBBrush = sender.BorderBrush;
            sender.BorderBrush = Brushes.Red;
            sender.TextChanged += ASB_IsNullWarn_TextChanged;
        }
        private static void ASB_IsNullWarn_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender.BorderBrush == Brushes.Red)
            {
                sender.BorderBrush = defaultASBBrush;
                sender.TextChanged -= ASB_IsNullWarn_TextChanged;
            }
        }
    }
}
