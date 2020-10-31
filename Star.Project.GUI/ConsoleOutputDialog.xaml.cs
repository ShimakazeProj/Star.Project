using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ModernWpf.Controls;

using Star.Project.Data;
using Star.Project.GUI.Data;

namespace Star.Project.GUI
{
    /// <summary>
    /// ConsoleOutputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleOutputDialog
    {
        private readonly string command;

        public ConsoleOutputDialog()
        {
            InitializeComponent();
            this.consoleOutput.Loaded += consoleOutput_Loaded;
        }

        public ConsoleOutputDialog(string sz) : this() => this.command = sz;

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => ConsoleHelper.FreeConsole();

        private async void consoleOutput_Loaded(object sender, RoutedEventArgs e)
        {
            this.consoleOutput.Loaded -= consoleOutput_Loaded;

            var sb = new StringBuilder(1024);
            ConsoleHelper.AllocConsole();
            ConsoleHelper.GetConsoleTitle(sb, sb.Capacity);
            var handle = Win32API.FindWindow(null, sb.ToString());

            var ctrl = new Controls.WindowControl(handle);
            this.consoleOutput.Content = ctrl;
            this.SizeChanged += (o, e) => ctrl.UpdateSize(this.TransformToAncestor(App.Current.MainWindow).Transform(this.consoleOutput.TransformToAncestor(this).Transform(new Point(0, 0))));


            await Program.RootCommand.InvokeAsync(this.command, new SystemConsole());
            this.CloseButtonText = "完成";
        }
    }
}
