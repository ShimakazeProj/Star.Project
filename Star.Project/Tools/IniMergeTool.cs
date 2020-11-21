using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Struct.Ini;

using Star.Project.Data.Options;
using Star.Project.Extensions;

namespace Star.Project.Tools
{
    public static class IniMergeTool
    {
        public const string NAME = "ini-merge";
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";

        public static Command Command => new Command(NAME, "INI 格式化工具")
        {
            new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件") { IsRequired = true }.SetArgumentName("文件"),
            new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件"){ IsRequired = true }.SetArgumentName("文件")
        }.SetHandler(CommandHandler.Create<ParseResult, IConsole>(ParseAsync));

        private static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();

            console.Debug($"源文件: {source}");
            console.Debug($"目标文件: {target}");

            await new IniMergeOptions
            {
                Input = new StreamReader(ifs),
                Output = new StreamWriter(ofs),
                WorkDir = source.Directory
            }.WorkAsync(console);
        }
        private static async Task WorkAsync(this IniMergeOptions options, IConsole console)
        {
            // 0. 读入ini
            var mainIni = new IniBuilder(await IniDocument.ParseAsync(options.Input));

            // 1. 获取[#Include]节
            if (mainIni.TryGetSection("#Include") is IniSectionBuilder include)
                // 2. 通过遍历节中内容来获取INI文件
                foreach (var kvp in include.Content)
                {
                    if (!kvp.HasData) continue;

                    var subIni = await IniDocument.ParseAsync(new FileInfo(Path.Combine(options.WorkDir.FullName, kvp.Value)).Open(FileMode.Open, FileAccess.Read, FileShare.Read));

                    foreach (var subSection in subIni.Sections)
                        if (mainIni.TryGetSection(subSection.Name) is IniSectionBuilder mainSection)
                            foreach (var subKvp in subSection.Content)
                                if (mainSection.TryGetKey(subKvp.Key) is IniKeyValuePair mainKvp)
                                    mainKvp.Value = subKvp.Value;
                                else mainSection.Add(subKvp);
                        else
                            mainIni.Add(subSection);
                }
            else
                console.Fatal($"不存在[#Include]节!");

            await mainIni.ToDocument().DeparseAsync(options.Output);
        }
    }
}
