using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Shimakaze.Struct.Ini;

namespace Star.Project
{
    public class Formater
    {
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string MATCH_CASE = "--match-case";
        public const string KEEP_KEYS = "--keep-keys";
        public const string KEEP_SECTIONS = "--keep-sections";
        public const string NAME = "format";
        private static async Task ParseAsync(ParseResult parseResult)
        {
            if (parseResult.ValueForOption<FileInfo>(FILE_INPUT) is null) return;
            await using var ifs = parseResult.ValueForOption<FileInfo>(FILE_INPUT).Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var osw = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT) is null ? Console.Out : new StreamWriter(parseResult.ValueForOption<FileInfo>(FILE_OUTPUT)?.OpenWrite());

            CleannerOptions options;
            options.Input = new StreamReader(ifs);
            options.Output = osw;
            options.Keys = parseResult.ValueForOption<string[]>(KEEP_KEYS)?.Select(i => i.Trim()).ToArray();
            options.Sections = parseResult.ValueForOption<string[]>(KEEP_SECTIONS)?.Select(i => i.Trim()).ToArray();
            options.MatchCase = parseResult.ValueForOption<bool>(MATCH_CASE);
            await WorkAsync(options);
        }


        public static Command GetCommand()
        {
            var file_input = new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件") { IsRequired = true };
            file_input.Argument.Name = "文件";

            var file_output = new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件");
            file_output.Argument.Name = "文件";

            var keep_key = new Option<string[]>(KEEP_KEYS, "输入若干个需要保留的键名, 将删除所有未在列表中的键, 若经过处理后节内容为空, 则删除该节");
            keep_key.Argument.Name = "键名";

            var keep_section = new Option<string[]>(KEEP_SECTIONS, "输入若干个需要保留的键, 将删除所有未在列表中的节");
            keep_section.Argument.Name = "节名";

            var ignore_case = new Option<bool>(MATCH_CASE, DefaultHelper.False, "是否区分大小写");

            var cmd = new Command(NAME, "INI 格式化工具")
            {
                file_input,
                file_output,
                keep_key,
                keep_section,
                ignore_case
            };
            cmd.Handler = CommandHandler.Create<ParseResult>(ParseAsync);
            return cmd;
        }
        public static async Task WorkAsync(CleannerOptions options)
        {
            IniDocument iniFile, newIniFile;

            iniFile = await IniDocument.ParseAsync(options.Input);

            Debug.WriteLine(iniFile);

            newIniFile = new IniDocument
            {
                NoSectionContent = iniFile.NoSectionContent,
                Sections = iniFile.Sections
                    .Where(FilterSection)
                    .Select(SelectIniSection)
                    .ToArray()
            };
            await newIniFile.DeparseAsync(options.Output);
            await options.Output.FlushAsync();

            IniSection SelectIniSection(IniSection i) => options.Keys?.Length < 1 || i.Content.Count(FilterKey) < 1
                    ? i
                    : new IniSection
                    {
                        Name = i.Name,
                        Summary = i.Summary,
                        Content = i.Content.Where(FilterKey).ToArray()
                    };

            bool FilterSection(IniSection section) => options.Sections?.Contains(section.Name) ?? false || section.Content.Count(FilterKey) > 0;

            bool FilterKey(IniKeyValuePair i) => options.Keys?.Contains(i.Key.Trim(), options.MatchCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase) ?? true;
        }

        public struct CleannerOptions
        {
            public bool MatchCase;
            public TextReader Input;
            public string[] Keys;
            public TextWriter Output;
            public string[] Sections;
        }
    }
}
