using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
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
        public const string PREFIX_TEMPLATE = "--prefix-template";
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
            cmd.Handler = CommandHandler.Create<ParseResult>(ParseAsync);
            return cmd;
        }

        public static async Task ParseAsync(ParseResult parseResult)
        {
            await using var ifs = parseResult.ValueForOption<FileInfo>(FILE_INPUT).Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var osw = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT) is null ? Console.Out : new StreamWriter(parseResult.ValueForOption<FileInfo>(FILE_OUTPUT).OpenWrite());

            SortSectionOptions options;
            options.Input = new StreamReader(ifs);
            options.Output = osw;
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

            await WorkAsync(options);
        }

        public static async Task WorkAsync(SortSectionOptions options)
        {
            var ini = await IniDocument.ParseAsync(options.Input);
            IEnumerable<IniSection> Result;// 约束筛选
            IEnumerable<IniKeyValuePair> result;// 结果
            int num = options.First;


            Result = string.IsNullOrEmpty(options.KeyConstraint)// 是否有键约束
                ? ini.Sections// 没有键约束
                : string.IsNullOrEmpty(options.ValueConstraint)// 是否有值约束
                    ? ini.Sections.Where(i => i.TryGetKey(options.KeyConstraint, out _))// 仅键约束
                    : ini.Sections.Where(i => i.TryGetKey(options.KeyConstraint).Value.ToString().Equals(options.ValueConstraint));// 值约束

            Result = options.Sort// 是否排序
                ? options.SortTargetKeys.Length > 0// 是否按目标键的值排序
                    ? Result.OrderBy(i => i.Name)// 不按目标键的值排序
                    : options.SortTargetKeys.Select(key => Result = Result.Where(i => i.TryGetKey(key, out _)).OrderBy(i => i.TryGetKey(key)?.Value.ToString()))
                                            .Last()// 按目标值排序
                : Result;

            result = string.IsNullOrEmpty(options.PrefixKey)
                ? string.IsNullOrEmpty(options.SummaryKey)
                    ? Result.Select(i => new IniKeyValuePair($"{options.Prefix ?? string.Empty}{num++.ToString().PadLeft(options.Digit, '0')}", i.Name))
                    : Result.Select(i => new IniKeyValuePair($"{options.Prefix ?? string.Empty}{num++.ToString().PadLeft(options.Digit, '0')}", i.Name, i.TryGetKey(options.SummaryKey)?.Value ?? string.Empty))
                : Result.Select(i => new IniKeyValuePair(
                    $"{(i.TryGetKey(options.PrefixKey, out _) ? (string.IsNullOrEmpty(options.Prefix) ? "%s" : options.Prefix).Replace("%s", i.TryGetKey(options.PrefixKey)?.Value.ToString().Trim()) : string.Empty)}" +
                    $"{num++.ToString().PadLeft(options.Digit, '0')}", i.Name, i.TryGetKey(options.SummaryKey)?.Value ?? string.Empty));

            if (!string.IsNullOrEmpty(options.OutputSection))
                await options.Output.WriteLineAsync($"[{options.OutputSection}]");
            result.ToList().ForEach(async i => await options.Output.WriteLineAsync(i.ToString()));
            await options.Output.FlushAsync();
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
