using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Star.Project.GUI.Data
{
    public interface IToolPage
    {
        Task Start(Button sender, RoutedEventArgs e);
        Task ApplyTemplate(Button sender, FileInfo file);
        Task SaveTemplate(Button sender, FileInfo file);

        string Help { get; }
    }
}
