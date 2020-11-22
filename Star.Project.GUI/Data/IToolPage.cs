using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Star.Project.GUI.Data
{
    public class StartEventArgs
    {
        public StartEventArgs(Button startButton, RoutedEventArgs startButtonRoutedEventArgs, string inputFile, string outputFile)
        {
            StartButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
            StartButtonRoutedEventArgs = startButtonRoutedEventArgs ?? throw new ArgumentNullException(nameof(startButtonRoutedEventArgs));
            InputFile = inputFile ?? throw new ArgumentNullException(nameof(inputFile));
            OutputFile = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
        }

        public Button StartButton { get; }
        public RoutedEventArgs StartButtonRoutedEventArgs { get; }

        public string InputFile { get; }
        public string OutputFile { get; }
    }
    public interface IToolPage
    {
        Task Start(object sender, StartEventArgs e);
        Task ApplyTemplate(Button sender, FileInfo file);
        Task SaveTemplate(Button sender, FileInfo file);

        string Help { get; }
    }
}
