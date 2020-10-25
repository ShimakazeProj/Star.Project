using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Shimakaze.Struct.Ini;

namespace Star.Project
{
    public static class KeyScreen
    {
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string KEEP_KEYS = "--target";
        public const string MATCH_CASE = "--match-case";
        public const string NAME = "key-screen";
        public static Command GetCommand()
        {
            var file_input = new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件") { IsRequired = true };
            file_input.Argument.Name = "文件";

            var file_output = new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件");
            file_output.Argument.Name = "文件";

            var keep_key = new Option<string[]>(KEEP_KEYS, "输入若干个需要保留的键名, 将删除所有未在列表中的键, 若经过处理后节内容为空, 则删除该节") { IsRequired = true };
            keep_key.Argument.Name = "键名";

            var ignore_case = new Option<bool>(MATCH_CASE, DefaultHelper.False, "是否区分大小写");

            var cmd = new Command(NAME, "INI 格式化工具")
            {
                file_input,
                file_output,
                keep_key,
                ignore_case
            };
            cmd.Handler = CommandHandler.Create<ParseResult, IConsole>(ParseAsync);
            return cmd;
        }

        public static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            KeyScreenOptions options;

            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();
            options.Input = new StreamReader(ifs);
            options.Output = ofs is null ? null : new StreamWriter(ofs);


            options.Keys = parseResult.ValueForOption<string[]>(KEEP_KEYS)?.Select(i => i.Trim()).ToArray();
            options.MatchCase = parseResult.ValueForOption<bool>(MATCH_CASE);


            console.Out.Write($"[{DateTime.Now:O}]Debug\t源文件: {source}");
            console.Out.Write($"[{DateTime.Now:O}]Debug\t目标文件: {target}");
            if (options.Keys is null)
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留键: NULL");
            else
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留键: [{string.Join(", ", options.Keys)}]");

            console.Out.Write($"[{DateTime.Now:O}]Debug\t区分大小写: {options.MatchCase}");

            await ScreeningAsync(options, console);
        }

        public static async Task ScreeningAsync(KeyScreenOptions options, IConsole console)
        {
            IniDocument ini, result;
            try
            {
                // 0. 判断程序是否具备执行条件
                if (options.Keys is null || options.Keys.Length < 1)// 未设置目标节或目标节为空
                {
                    console.Error.Write($"{nameof(options.Keys)} are NULL or Empty");
                    return;
                }

                // 1. 读入文件
                ini = await IniDocument.ParseAsync(options.Input);
                var comparer = options.MatchCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

                // 2. 筛选并保留目标键
                var targetSectionsDictionary = new Dictionary<(string name, string summary), List<IniKeyValuePair>>();
                Parallel.ForEach(ini.Sections, section =>// 遍历节内容
                {
                    Parallel.ForEach(section.Content, keyValuePair => // 遍历键内容
                    {
                        if (keyValuePair.HasData && options.Keys.Contains(keyValuePair.Key, comparer))// 存在有效数据
                        {
                            lock (targetSectionsDictionary)
                            {
                                if (targetSectionsDictionary.TryGetValue((section.Name, section.Summary), out var targetSection))// 尝试从字典中获取目标节
                                    targetSection.Add(keyValuePair);
                                else// 找不到目标节
                                    targetSectionsDictionary.Add((section.Name, section.Summary), new List<IniKeyValuePair> { keyValuePair });
                            }
                        }
                    });
                });
                var targetSections = new List<IniSection>(targetSectionsDictionary.Count);
                Parallel.ForEach(targetSectionsDictionary, kvp => targetSections.Add(new IniSection(kvp.Key.name, kvp.Key.summary, kvp.Value.ToArray())));
                // 输出
                result.Sections = targetSections.ToArray();
                await result.DeparseAsync(options.Output);
            }
            finally
            {
                console.Out.Write("All Done!");
            }
        }
        public struct KeyScreenOptions
        {
            public TextReader Input;
            public string[] Keys;
            public bool MatchCase;
            public TextWriter Output;
        }
    }
}
