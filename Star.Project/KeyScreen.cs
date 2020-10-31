using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
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
        public const string SORT_KEY = "--sort-key";
        public const string IGNORE_SECTIONS = "--ignore-sections";
        public const string NAME = "key-screen";
        public static Command Command => new Command(NAME, "INI 格式化工具")
        {
            new Option<FileInfo>(FILE_INPUT, "打开一个将要被处理的文件") { IsRequired = true }.SetArgumentName("文件"),
            new Option<FileInfo>(FILE_OUTPUT, "将经过处理后的数据写入目标文件").SetArgumentName("文件"),
            new Option<string[]>(KEEP_KEYS, "输入若干个需要保留的键名, 将删除所有未在列表中的键, 若经过处理后节内容为空, 则删除该节") { IsRequired = true }.SetArgumentName("键名"),
            new Option<string[]>(IGNORE_SECTIONS, "输入若干个不需要保留的节名, 将删除所有在列表中的节").SetArgumentName("节名"),
            new Option<bool>(MATCH_CASE, DefaultHelper.False, "是否区分大小写"),
            new Option<bool>(SORT_KEY, DefaultHelper.False, "根据键列表排序")
        }.SetHandler(CommandHandler.Create<ParseResult, IConsole>(ParseAsync));

        private static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();

            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t源文件: {source}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t目标文件: {target}");

            await new KeyScreenOptions
            {
                Input = new StreamReader(ifs),
                Output = ofs is null ? null : new StreamWriter(ofs),
                Keys = parseResult.ValueForOption<string[]>(KEEP_KEYS)?.Select(i => i.Trim()).ToArray(),
                IgnoreSections = parseResult.ValueForOption<string[]>(IGNORE_SECTIONS)?.Select(i => i.Trim()).ToArray(),
                MatchCase = parseResult.ValueForOption<bool>(MATCH_CASE),
                SortKey = parseResult.ValueForOption<bool>(SORT_KEY)
            }.DebugWrite(console).ScreeningAsync(console);
        }

        private static async Task ScreeningAsync(this KeyScreenOptions options, IConsole console)
        {
            // 0. 判断程序是否具备执行条件
            if (options.Keys is null || options.Keys.Length < 1)// 未设置目标节或目标节为空
            {
                console.Error.WriteLine($"{nameof(options.Keys)} are NULL or Empty");
                return;
            }

            Func<IEnumerable<IniKeyValuePair>, IniKeyValuePair[]> createSectionContant;
            if (options.SortKey)
                createSectionContant = i => (
                (Func<Dictionary<string, IniKeyValuePair>, string[], IniKeyValuePair[]>)
                ((target, list) => list.Where(i => target.ContainsKey(i)).Select(i => target[i]).ToArray())
                )(i.ToDictionary(i => i.Key), options.Keys);
            else
                createSectionContant = i => i.ToArray();

            // 输出
            await new IniDocument
            {
                Sections = (new Dictionary<(string name, string summary), List<IniKeyValuePair>>(), await IniDocument.ParseAsync(options.Input))
                    .GetSectionsDictionaryAsyncFunc(options, console,
                        // 判断是否包含的方法
                        (source, i) => source.Contains(i, options.MatchCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase))
                    .Select(i => new IniSection(i.Key.name, i.Key.summary, createSectionContant(i.Value)))
                    .ToArray()
            }.DeparseAsync(options.Output);
        }

        private static
            Dictionary<(string name, string summary), List<IniKeyValuePair>>
            GetSectionsDictionaryAsyncFunc(
            this (Dictionary<(string name, string summary), List<IniKeyValuePair>> target,
            IniDocument ini) @this,
            KeyScreenOptions options,
            IConsole console,
            Func<string[], string, bool> containsFunc
            )
        {
            // 2. 筛选并保留目标键
            foreach (var section in @this.ini.Sections.Where(i => !options.IgnoreSections.Contains(i.Name)))// 遍历节内容
            {
                console.Out.WriteLine($"[{DateTime.Now:O}]Trace\t正在遍历节[{section.Name}]");
                foreach (var keyValuePair in section.Content)// 遍历键内容
                {
                    if (keyValuePair.HasData && containsFunc(options.Keys, keyValuePair.Key))// 存在有效数据
                    {
                        console.Out.WriteLine($"[{DateTime.Now:O}]Trace\t找到键[{keyValuePair.Key}]");

                        if (@this.target.TryGetValue((section.Name, section.Summary), out var targetSection))// 尝试从字典中获取目标节
                            targetSection.Add(keyValuePair);
                        else// 找不到目标节
                            @this.target.Add((section.Name, section.Summary), new List<IniKeyValuePair> { keyValuePair });
                    }
                }
            }
            return @this.target;
        }

    }
}
