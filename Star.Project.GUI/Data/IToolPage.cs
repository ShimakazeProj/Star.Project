using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Star.Project.GUI.Data
{
    public interface IToolPage
    {
        void Start(Button sender, RoutedEventArgs e);
        void ApplyTemplate(Button sender, FileInfo file);
        void SaveTemplate(Button sender, FileInfo file);
    }
}
