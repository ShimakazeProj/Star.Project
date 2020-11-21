using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Shimakaze.Struct.Ini;

using Star.Project.Data;
using Star.Project.Data.Options;
using Star.Project.Extensions;

namespace Star.Project.Tools
{
    public static class SectionScreenTool
    {
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string KEEP_SECTIONS = "--keep-sections";
        public const string MATCH_CASE = "--match-case";
        public const string NAME = "section-screen";
        public static Command Command
        {
            get
            {
                var file_input = new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件") { IsRequired = true };
                file_input.Argument.Name = "文件";

                var file_output = new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件");
                file_output.Argument.Name = "文件";

                var keep_section = new Option<string[]>(KEEP_SECTIONS, "输入若干个需要保留的键, 将删除所有未在列表中的节") { IsRequired = true };
                keep_section.Argument.Name = "节名";

                var ignore_case = new Option<bool>(MATCH_CASE, OverAll.False, "是否区分大小写");

                var cmd = new Command(NAME, "INI 格式化工具")
            {
                file_input,
                file_output,
                keep_section,
                ignore_case
            };
                cmd.Handler = CommandHandler.Create<ParseResult, IConsole>(ParseAsync);
                return cmd;
            }
        }

        public static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            SectionScreenOptions options;

            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();
            options.Input = new StreamReader(ifs);
            options.Output = ofs is null ? null : new StreamWriter(ofs);


            options.Sections = parseResult.ValueForOption<string[]>(KEEP_SECTIONS)?.Select(i => i.Trim()).ToArray();
            options.MatchCase = parseResult.ValueForOption<bool>(MATCH_CASE);


            console.Debug($"源文件: {source}");
            console.Debug($"目标文件: {target}");
            if (options.Sections is null)
                console.Debug($"保留节: NULL");
            else
                console.Debug($"保留节: [{string.Join(", ", options.Sections)}]");
            console.Debug($"区分大小写: {options.MatchCase}");

            await ScreeningAsync(options, console);
        }

        public static async Task ScreeningAsync(SectionScreenOptions options, IConsole console)
        {
            IniDocument ini, result;
            try
            {
                // 0. 判断程序是否具备执行条件
                if (options.Sections is null || options.Sections.Length < 1)// 未设置目标键或目标键为空
                {
                    console.Error.WriteLine($"{nameof(options.Sections)} are NULL or Empty");
                    return;
                }

                // 1. 读入文件
                ini = await IniDocument.ParseAsync(options.Input);

                // 2. 筛选并保留目标键
                // 输出
                result.Sections = options.MatchCase
                    ? ini.Sections.Where(section => options.Sections.Contains(section.Name)).ToArray()
                    : ini.Sections.Where(section => options.Sections.Contains(section.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
                await result.DeparseAsync(options.Output);
            }
            finally
            {
                console.Out.WriteLine("All Done!");
            }
        }

    }
}
