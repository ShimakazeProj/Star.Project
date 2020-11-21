using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using ModernWpf.Controls;

namespace Star.Project.GUI
{
    /// <summary>
    /// TemplateManagerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateManagerDialog
    {
        public TemplateManagerDialog()
        {
            InitializeComponent();
        }

        public TemplateManagerDialog(IEnumerable<FileInfo> list) : this()
        {
            this.ListView.ItemsSource = new ObservableCollection<FileInfo>(list);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is FileInfo file)
            {
                file.Delete();
                (this.ListView.ItemsSource as ObservableCollection<FileInfo>).Remove(file);
            }
        }
    }
}
