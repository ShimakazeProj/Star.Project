using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;

namespace Star.Project.Data.Options
{
    internal struct KeyScreenOptions
    {
        public TextReader Input;
        public string[] Keys;
        public string[] IgnoreSections;
        public bool MatchCase;
        public TextWriter Output;
        public bool SortKey;

        public KeyScreenOptions DebugWrite(IConsole console)
        {
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t保留键: {(this.Keys is null ? null : '[' + string.Join(", ", this.Keys) + ']')}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t忽略节: {(this.IgnoreSections is null ? null : '[' + string.Join(", ", this.IgnoreSections) + ']')}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t区分大小写: {this.MatchCase}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t排序键: {this.SortKey}");
            return this;
        }
    }
}
