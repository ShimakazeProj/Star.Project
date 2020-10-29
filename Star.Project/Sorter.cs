using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shimakaze.Struct.Ini;

namespace Star.Project
{
    public class Sorter
    {
        public const string CONSTRAINT_KEY = "--constraint-key";
        public const string CONSTRAINT_VALUE = "--constraint-value";
        public const string DIGIT = "--digit";
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string PREFIX = "--prefix";
        public const string PREFIX_KEY = "--prefix-key";
        public const string SORT = "--sort";
        public const string SORT_KEYS = "--sort-keys";
        public const string START_NUM = "--start-num";
        public const string SUMMARY_KEY = "--summary-key";
        public const string TARGET_SECTION = "--section-head";
        public const string NAME = "sort";

        public static Command GetCommand()
        {
            var file_input = new Option<FileInfo>(FILE_INPUT, "将文件作为输入源") { IsRequired = true };
            file_input.Argument.Name = "文件";
            var file_output = new Option<FileInfo>(FILE_OUTPUT, "设置输出文件");
            file_output.Argument.Name = "文件";

            var start = new Option<int>(START_NUM, () => 0, "以此数字开始计数");
            start.Argument.Name = "整数";
            var digit = new Option<int>(DIGIT, () => 0, "以此数字规范长度");
            digit.Argument.Name = "整数";
            var prefix = new Option<string>(PREFIX, "在输出的键前添加一个前缀, %s将会被替换为指定键的值");
            prefix.Argument.Name = "前缀";
            var prefixKey = new Option<string>(PREFIX_KEY, "将该键的值作为前缀输出");
            prefixKey.Argument.Name = "前缀键";

            var sort = new Option<bool>(SORT, DefaultHelper.False, "对输出进行排序");
            var sortKey = new Option<string[]>(SORT_KEYS, "根据目标键的指排序");
            sortKey.Argument.Name = "排序依赖键";
            var target = new Option<string>(TARGET_SECTION, "在输出结果前添加一个节头");
            target.Argument.Name = "节名";
            var summary = new Option<string>(SUMMARY_KEY, "将此键的值作为注释输出");
            summary.Argument.Name = "注释键";
            var constraintKey = new Option<string>(CONSTRAINT_KEY, "只有包含此键的节才会被输出");
            constraintKey.Argument.Name = "约束键";
            var constraintValue = new Option<string>(CONSTRAINT_VALUE, "只有当键约束的键值等于这里的值时才会被输出");
            constraintValue.Argument.Name = "约束值";

            var cmd = new Command(NAME, "INI 排序工具")
            {
                file_input,
                file_output,

                start,
                digit,
                prefix,
                prefixKey,

                sort,
                sortKey,
                target,
                summary,
                constraintKey,
                constraintValue
            };
            cmd.Handler = CommandHandler.Create<ParseResult, IConsole>(ParseAsync);
            return cmd;
        }

        public static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            SortSectionOptions options;

            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();
            options.Input = new StreamReader(ifs);
            options.Output = ofs is null ? null : new StreamWriter(ofs);

            options.First = parseResult.ValueForOption<int>(START_NUM);
            options.Digit = parseResult.ValueForOption<int>(DIGIT);
            options.Prefix = parseResult.ValueForOption<string>(PREFIX);
            options.PrefixKey = parseResult.ValueForOption<string>(PREFIX_KEY);
            options.Sort = parseResult.ValueForOption<bool>(SORT);
            options.SortTargetKeys = parseResult.ValueForOption<string[]>(SORT_KEYS);
            options.OutputSection = parseResult.ValueForOption<string>(TARGET_SECTION);
            options.SummaryKey = parseResult.ValueForOption<string>(SUMMARY_KEY);
            options.KeyConstraint = parseResult.ValueForOption<string>(CONSTRAINT_KEY);
            options.ValueConstraint = parseResult.ValueForOption<string>(CONSTRAINT_VALUE);

            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t源文件: {source}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t目标文件: {target}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t起始数字: {options.First}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t长度限制: {options.Digit}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t输出前缀: {options.Prefix}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t前缀键: {options.PrefixKey}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t重新排序: {options.Sort}");
            if (options.SortTargetKeys is null)
                console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t排序依据键: NULL");
            else
                console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t排序依据键: [{string.Join(", ", options.SortTargetKeys)}]");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t输出节名: {options.OutputSection}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t注释键名: {options.SummaryKey}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t键约束: {options.KeyConstraint}");
            console.Out.WriteLine($"[{DateTime.Now:O}]Debug\t值约束: {options.ValueConstraint}");


            console.Out.WriteLine($"[{DateTime.Now:O}]Info\t开始");
            await WorkAsync(options, console);
            console.Out.WriteLine($"[{DateTime.Now:O}]Info\t完成");
        }

        public static async Task WorkAsync(SortSectionOptions options, IConsole console)
        {
            var ini = await IniDocument.ParseAsync(options.Input);
            IEnumerable<IniSection> Result = ini.Sections;// 约束筛选
            List<IniKeyValuePair> result = new List<IniKeyValuePair>();// 结果
            int num = options.First;

            if (!string.IsNullOrEmpty(options.KeyConstraint))
            {
                console.Out.WriteLine($"[{DateTime.Now:O}]Info\t已启用键约束");
                if (!string.IsNullOrEmpty(options.ValueConstraint))
                {
                    console.Out.WriteLine($"[{DateTime.Now:O}]Info\t已启用值约束");
                    Result = ini.Sections.Where(i => i.TryGetKey(options.KeyConstraint)?.Value.ToString().Equals(options.ValueConstraint) ?? false);
                }
                else
                {
                    console.Out.WriteLine($"[{DateTime.Now:O}]Info\t已启用键约束, 但未启用值约束");
                    Result = ini.Sections.Where(i => i.TryGetKey(options.KeyConstraint, out _));
                }
            }
            if (options.Sort)
            {
                console.Out.WriteLine($"[{DateTime.Now:O}]Info\t已启用排序");
                if (options.SortTargetKeys is null)
                {
                    console.Out.WriteLine($"[{DateTime.Now:O}]Warn\t排序列表不存在, 将按Section名排序");
                    Result = Result.OrderBy(i => i.Name);
                }
                if (options.SortTargetKeys.Length > 0)
                {
                    console.Out.WriteLine($"[{DateTime.Now:O}]Info\t排序列表为空, 将按Section名排序");
                    Result = Result.OrderBy(i => i.Name);
                }
                else
                {
                    foreach (var key in options.SortTargetKeys)
                    {
                        console.Out.WriteLine($"[{DateTime.Now:O}]Trace\t正在根据键 {key} 的值排序");
                        Result = Result.Where(i => i.TryGetKey(key, out _)).OrderBy(i => i.TryGetKey(key)?.Value.ToString());
                    }
                }
            }
            if (string.IsNullOrEmpty(options.PrefixKey))
                if (string.IsNullOrEmpty(options.SummaryKey))
                {
                    foreach (var i in Result)
                    {
                        var key = $"{options.Prefix ?? string.Empty}{num++.ToString().PadLeft(options.Digit, '0')}";
                        var value = i.Name;
                        result.Add(new IniKeyValuePair(key, value));
                    }
                }
                else
                {
                    foreach (var i in Result)
                    {
                        var key = $"{options.Prefix ?? string.Empty}{num++.ToString().PadLeft(options.Digit, '0')}";
                        var value = i.Name;
                        var summary = i.TryGetKey(options.SummaryKey)?.Value ?? string.Empty;
                        result.Add(new IniKeyValuePair(key, value, summary));
                    }
                }
            else
                foreach (var i in Result)
                {
                    var prefix = string.Empty;
                    if (string.IsNullOrEmpty(options.Prefix))
                        prefix = i.TryGetKey(options.PrefixKey)?.Value.ToString().Trim();
                    else
                        prefix = options.Prefix.Replace("%s", i.TryGetKey(options.PrefixKey)?.Value.ToString().Trim());

                    var key = $"{prefix}{num++.ToString().PadLeft(options.Digit, '0')}";
                    var value = i.Name;
                    var summary = i.TryGetKey(options.SummaryKey)?.Value ?? string.Empty;
                    result.Add(new IniKeyValuePair(key, value, summary));
                }

            console.Out.WriteLine($"[{DateTime.Now:O}]Info\t输出结果");
            if (!string.IsNullOrEmpty(options.OutputSection))
            {
                console.Out.WriteLine($"[{options.OutputSection}]");
                await options.Output?.WriteAsync($"[{options.OutputSection}]");
            }
            foreach (var item in result)
            {
                console.Out.WriteLine(item.ToString());
                await options.Output?.WriteAsync(item.ToString());
            }
            await options.Output?.FlushAsync();
        }

        public struct SortSectionOptions
        {
            public int Digit;
            public int First;
            public TextReader Input;
            public string KeyConstraint;
            public TextWriter Output;
            public string OutputSection;
            public string Prefix;
            public string PrefixKey;

            public bool Sort;
            public string[] SortTargetKeys;
            public string SummaryKey;
            public string ValueConstraint;
        }
    }
}
