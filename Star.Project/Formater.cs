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
    public class Formater
    {
        public const string FILE_INPUT = "--file-input";
        public const string FILE_OUTPUT = "--file-output";
        public const string KEEP_KEYS = "--keep-keys";
        public const string KEEP_SECTIONS = "--keep-sections";
        public const string MATCH_CASE = "--match-case";
        public const string KEEP_SECTION_INTACT = "--section-intact";
        public const string NAME = "format";
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
            var section_intact = new Option<bool>(KEEP_SECTION_INTACT, DefaultHelper.False, "使保留节结构完整");

            var cmd = new Command(NAME, "INI 格式化工具")
            {
                file_input,
                file_output,
                keep_key,
                keep_section,
                ignore_case
            };
            cmd.Handler = CommandHandler.Create<ParseResult, IConsole>(ParseAsync);
            return cmd;
        }

        public static async Task ParseAsync(ParseResult parseResult, IConsole console)
        {
            CleannerOptions options;

            var source = parseResult.ValueForOption<FileInfo>(FILE_INPUT);
            var target = parseResult.ValueForOption<FileInfo>(FILE_OUTPUT);
            await using var ifs = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var ofs = target?.OpenWrite();
            options.Input = new StreamReader(ifs);
            options.Output = ofs is null ? null : new StreamWriter(ofs);


            options.Keys = parseResult.ValueForOption<string[]>(KEEP_KEYS)?.Select(i => i.Trim()).ToArray();
            options.Sections = parseResult.ValueForOption<string[]>(KEEP_SECTIONS)?.Select(i => i.Trim()).ToArray();
            options.MatchCase = parseResult.ValueForOption<bool>(MATCH_CASE);
            options.IntAct = parseResult.ValueForOption<bool>(KEEP_SECTION_INTACT);


            console.Out.Write($"[{DateTime.Now:O}]Debug\t源文件: {source}");
            console.Out.Write($"[{DateTime.Now:O}]Debug\t目标文件: {target}");
            if (options.Keys is null)
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留键: NULL");
            else
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留键: [{string.Join(", ", options.Keys)}]");
            if (options.Sections is null)
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留节: NULL");
            else
                console.Out.Write($"[{DateTime.Now:O}]Debug\t保留节: [{string.Join(", ", options.Sections)}]");
            console.Out.Write($"[{DateTime.Now:O}]Debug\t区分大小写: {options.MatchCase}");
            console.Out.Write($"[{DateTime.Now:O}]Debug\t保留节结构完整: {options.IntAct}");

            await WorkAsync(options, console);
        }
        public static async Task WorkAsync(CleannerOptions options, IConsole console)
        {
            IniDocument iniFile, newIniFile;

            iniFile = await IniDocument.ParseAsync(options.Input);

            if (options.Keys is null && options.Sections is null)
            {
                console.Error.Write($"[{DateTime.Now:O}]Fail\t未指定键或值, 程序将不会继续执行");
                return;
            }
            else
            {
                newIniFile = new IniDocument
                {
                    NoSectionContent = iniFile.NoSectionContent,
                    Sections = iniFile.Sections
                        .Where(FilterSection)
                        .Select(SelectIniSection)
                        .ToArray()
                };
            }
            console.Out.Write($"[{DateTime.Now:O}]Info\t输出结果");
            console.Out.Write(newIniFile.ToString());
            await newIniFile.DeparseAsync(options.Output);
            await options.Output.FlushAsync();

            // 生成新节
            IniSection SelectIniSection(IniSection i)
            {
                if (i.Content.Count(FilterKey) < 1)
                {
                    if (options.IntAct)
                    {
                        console.Out.Write($"[{DateTime.Now:O}]Info\t该节是保留节: [{i.Name}]");
                        return i;
                    }
                    else
                    {
                        console.Out.Write($"[{DateTime.Now:O}]Info\t未找到指定键, 但该节是保留节, 将仅输出节头: [{i.Name}]");
                        return new IniSection
                        {
                            Name = i.Name,
                            Summary = i.Summary
                        };
                    }
                }
                else
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t选中节: [{i.Name}]");
                    return new IniSection
                    {
                        Name = i.Name,
                        Summary = i.Summary,
                        Content = i.Content.Where(FilterKey).ToArray()
                    };
                }
            }

            // 匹配节
            bool FilterSection(IniSection section)
            {
                console.Out.Write($"[{DateTime.Now:O}]Info\t开始匹配节: [{section.Name}]");
                //console.Out.Write($"[{DateTime.Now:O}]Info\t未找到节中键匹配且未指定保留节");
                if (!(options.Sections is null) && options.Sections.Contains(section.Name))
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t该节为保留节");
                    return true;
                }
                else if (section.Content.Count(FilterKey) > 0)
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t找到节中键匹配");
                    return true;
                }
                else
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t不匹配");
                    return false;
                }
            }

            // 匹配键值对
            bool FilterKey(IniKeyValuePair i)
            {
                console.Out.Write($"[{DateTime.Now:O}]Info\t开始匹配键: [{i.Key}]");
                if (options.Keys is null)
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t未指定键");
                    return false;
                }
                else if (options.Keys.Contains(i.Key.Trim(), options.MatchCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase))
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t找到键: {i.Key}");
                    return true;
                }
                else
                {
                    console.Out.Write($"[{DateTime.Now:O}]Info\t不匹配");
                    return false;
                }
            }
        }

        public struct CleannerOptions
        {
            public TextReader Input;
            public string[] Keys;
            public bool MatchCase;
            public bool IntAct;
            public TextWriter Output;
            public string[] Sections;
        }
    }
}
