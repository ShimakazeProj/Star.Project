using System.IO;

namespace Star.Project.Data.Options
{
    public struct SectionScreenOptions
    {
        public TextReader Input;
        public bool MatchCase;
        public TextWriter Output;
        public string[] Sections;
    }
}
