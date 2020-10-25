using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Star.Project.GUI.Data
{
    public class RichTextBoxWriter : TextWriter
    {
        private readonly RichTextBox richTextBox;
        private readonly Brush foreground;

        public RichTextBoxWriter(RichTextBox textBox) : base()
        {
            this.richTextBox = textBox;
            this.foreground = textBox.Foreground;
        }

        public RichTextBoxWriter(RichTextBox textBox, Brush foreground) : this(textBox)
        {
            this.foreground = foreground;
        }

        // 使用UTF-16避免不必要的编码转换
        public override Encoding Encoding => Encoding.Unicode;

        // 最低限度需要重写的方法
        public override void Write(string value)
        {
            var paragraph = new Paragraph();
            var sb = new StringBuilder(value);
            if (sb.Length > 28
                && Regex.IsMatch(sb.ToString(0, 28), @"\[\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d.\d+"))
            {
                paragraph.Inlines.Add(new Run("[") { Foreground = Brushes.White });
                paragraph.Inlines.Add(new Run(sb.ToString(1, 10)) { Foreground = Brushes.Blue });
                paragraph.Inlines.Add(new Run("T") { Foreground = Brushes.White });
                paragraph.Inlines.Add(new Run(sb.ToString(12, 8)) { Foreground = Brushes.DarkGreen });
                paragraph.Inlines.Add(new Run(sb.ToString(20, 8)) { Foreground = Brushes.DarkGray });
                sb.Remove(0, 28);
                if (sb[0] == '+' || sb[0] == '-')
                {
                    paragraph.Inlines.Add(new Run(sb.ToString(0, 6)) { Foreground = Brushes.DarkBlue });
                    sb.Remove(0, 6);
                }
                paragraph.Inlines.Add(new Run("]") { Foreground = Brushes.White });
                sb.Remove(0, 1);
                paragraph.Inlines.Add(new Run(sb.ToString())
                {
                    Foreground = sb.ToString(0, 4).ToUpper() switch
                    {
                        "DEBU" => Brushes.Gray,
                        "TRAC" => Brushes.LightGray,
                        "INFO" => Brushes.White,
                        "WARN" => Brushes.Yellow,
                        "FAIL" => Brushes.Red,
                        _ => this.foreground,
                    }
                });
            }
            else
            {
                paragraph.Inlines.Add(new Run(sb.ToString()) { Foreground = this.foreground });
            }
            this.richTextBox.ScrollToEnd();
            this.richTextBox.Document.Blocks.Add(paragraph);
        }
    }
}

