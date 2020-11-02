using System.IO;

namespace Star.Project.Data.Options
{
    public struct IniMergeOptions
    {
        public TextReader Input;
        public TextWriter Output;
        public DirectoryInfo WorkDir;
    }
}
