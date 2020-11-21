using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;

using Star.Project.Extensions;

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
            console.Debug($"保留键: {(this.Keys is null ? null : '[' + string.Join(", ", this.Keys) + ']')}");
            console.Debug($"忽略节: {(this.IgnoreSections is null ? null : '[' + string.Join(", ", this.IgnoreSections) + ']')}");
            console.Debug($"区分大小写: {this.MatchCase}");
            console.Debug($"排序键: {this.SortKey}");
            return this;
        }
    }
}
