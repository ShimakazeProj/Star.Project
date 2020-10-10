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
    public class Cleanner
    {
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string IGNORE_CASE = "--ignore-case";
        public const string KEEP_KEYS = "--keep-keys";
        public const string KEEP_SECTIONS = "--keep-sections";
        public const string NAME = "cleanner";
        private static async Task ParseAsync(ParseResult parseResult)
        {
            var input = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var output = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            var keys = parseResult.ValueForOption<string[]>(KEEP_KEYS);
            var sections = parseResult.ValueForOption<string[]>(KEEP_SECTIONS);
            var ignore = parseResult.ValueForOption<bool>(IGNORE_CASE);

            Debug.WriteLine($"input\t:{input}");
            Debug.WriteLine($"output\t:{output}");
            Debug.WriteLine($"ignore case\t:{ignore}");
            if (!(keys is null))
            {
                Debug.WriteLine("Keys:");
                Parallel.ForEach(keys, i => Debug.WriteLine($"\tkey\t:{i}"));
            }
            if (!(sections is null))
            {
                Debug.WriteLine("Sections:");
                Parallel.ForEach(sections, i => Debug.WriteLine($"\tsection\t:{i}"));
            }

            await using var ifs = input.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = output?.OpenWrite();
            await using var osw = output is null ? Console.Out : new StreamWriter(ofs);

            CleannerOptions options;
            options.Input = new StreamReader(ifs);
            options.Output = osw;
            options.Keys = keys?.Select(i => i.Trim()).ToArray();
            options.Sections = sections?.Select(i => i.Trim()).ToArray();
            options.IgnoreCase = ignore;
            await WorkAsync(options);
        }

        private static bool True() => true;

        public static Command GetCommand()
        {
            var file_input = new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件")
            {
                IsRequired = true,
            };
            file_input.Argument.Name = "文件";

            var file_output = new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件");
            file_output.Argument.Name = "文件";

            var keep_key = new Option<string[]>(KEEP_KEYS, "输入若干个需要保留的键名, 将删除所有未在列表中的键, 若经过处理后节内容为空, 则删除该节");
            keep_key.Argument.Name = "键名";

            var keep_section = new Option<string[]>(KEEP_SECTIONS, "输入若干个需要保留的键, 将删除所有未在列表中的节");
            keep_section.Argument.Name = "节名";

            var ignore_case = new Option<bool>(IGNORE_CASE, True, "是否区分大小写");

            var cmd = new Command(NAME, "这个工具可以用来过滤INI节和键")
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

            IniSection SelectIniSection(IniSection i) => options.Keys?.Length < 1 || i.Content.Count(FilterKey) < 1
                    ? i
                    : new IniSection
                    {
                        Name = i.Name,
                        Summary = i.Summary,
                        Content = i.Content.Where(FilterKey).ToArray()
                    };

            bool FilterSection(IniSection section) => options.Sections?.Contains(section.Name) ?? false || section.Content.Count(FilterKey) > 0;

            bool FilterKey(IniKeyValuePair i) => options.Keys?.Contains(i.Key.Trim(), options.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal) ?? true;
        }

        public struct CleannerOptions
        {
            public bool IgnoreCase;
            public TextReader Input;
            public string[] Keys;
            public TextWriter Output;
            public string[] Sections;
        }
    }
}
