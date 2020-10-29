﻿using System;
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
            this.NavControl.SelectedItem = this.NavControl.MenuItems[0];
            App.Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText("exception.log", e.Exception.ToString());
        }

        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
        }

        private void Window_ActualThemeChanged(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ThemeManager.GetActualTheme(this));
        }
        private Type lastSelected = null;
        private bool igone = false;
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (igone) return;
            var select = args.SelectedItem as NavigationViewItem;
            if ((select.Tag as Type) == lastSelected) return;
            if (args.IsSettingsSelected)
            {
                this.ContentFrame.Navigate(lastSelected = typeof(SettingsPage));
            }
            else
            {
                this.ContentFrame.Navigate(lastSelected = (select.Tag as Type));
                this.LoadTemplate((select.Tag as Type).Name);
            }
        }
        private async void LoadTemplate(string name)
        {
            await this.templates.Dispatcher.InvokeAsync(() =>
            {
                while (this.templates.SecondaryCommands[2] is AppBarButton)
                    this.templates.SecondaryCommands.RemoveAt(2);
            });
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
                btn.Click += this.TemplateItem_Click;
                await this.templates.Dispatcher.InvokeAsync(() => this.templates.SecondaryCommands.Insert(i + 2, btn));
            }
            return;

        NoTemplate:
            await this.templates.Dispatcher.InvokeAsync(() => this.templates.SecondaryCommands.Insert(2, new AppBarButton
            {
                Label = "暂无保存的模板",
                Icon = new SymbolIcon(Symbol.Cancel),
                IsEnabled = false
            }));
        }

        private void TemplateItem_Click(object sender, RoutedEventArgs e)
        {
            switch (ContentFrame.Content)
            {
                case IToolPage page:
                    var btn = sender as Button;
                    page.ApplyTemplate(btn, btn.Tag as FileInfo);
                    break;
            }
        }
        private async void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dir = new DirectoryInfo(Path.Combine("Template", ((this.NavControl.SelectedItem as NavigationViewItem).Tag as Type).Name));
            if (!dir.Exists) dir.Create();
            if (this.ContentFrame.Content is IToolPage page)
            {
                var tb = new AutoSuggestBox
                {
                    Header = "请输入模板名"
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
                        if (string.IsNullOrEmpty(tb.Text.Trim())) goto ShowDialog;
                        if (this.templates.SecondaryCommands[2] is AppBarButton btn && !btn.IsEnabled)
                            await this.templates.Dispatcher.InvokeAsync(() => this.templates.SecondaryCommands.RemoveAt(2));
                        var file = new FileInfo(Path.Combine(dir.FullName, tb.Text.Trim()));
                        page.SaveTemplate(sender as Button, file);

                        btn = new AppBarButton
                        {
                            Label = file.Name,
                            Icon = new SymbolIcon(Symbol.ImportAll),
                            Tag = file
                        };
                        btn.Click += this.TemplateItem_Click;
                        await this.templates.Dispatcher.InvokeAsync(() => this.templates.SecondaryCommands.Insert(this.templates.SecondaryCommands.Count - 2, btn));

                        break;
                    case ContentDialogResult.Secondary:
                        break;
                }
            }

        }

        private async void TemplateManager_Click(object sender, RoutedEventArgs e)
        {
            await new TemplateManagerDialog(this.templates.SecondaryCommands.Where(i => i is AppBarButton btn && btn.Tag is FileInfo)
                                                                            .Select(i => (i as AppBarButton).Tag as FileInfo)).ShowAsync();
            this.LoadTemplate(this.ContentFrame.Content.GetType().Name);
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            switch (ContentFrame.Content)
            {
                case IToolPage page:
                    page.Start(sender as Button, e);
                    break;
            }
        }

    }
}
