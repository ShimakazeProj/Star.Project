using System.CommandLine;
using System.CommandLine.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace Star.Project.GUI.Data
{
    public class RichTextBoxConsole : IConsole
    {
        public IStandardStreamWriter Error { get; }
        public IStandardStreamWriter Out { get; }

        public bool IsErrorRedirected => false;
        public bool IsOutputRedirected => false;
        public bool IsInputRedirected => false;

        public RichTextBoxConsole(RichTextBox textBox)
        {
            Out = StandardStreamWriter.Create(new RichTextBoxWriter(textBox));
            Error = StandardStreamWriter.Create(new RichTextBoxWriter(textBox, Brushes.Red));
        }
    }
}
