﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Star.Project.Data
{
    public class ConsoleWriter : TextWriter
    {
        // 使用UTF-16避免不必要的编码转换
        public override Encoding Encoding => Encoding.Unicode;

        // 最低限度需要重写的方法
        public override void Write(string value)
        {
            var sb = new StringBuilder(value);
            if (sb.Length > 28
                && Regex.IsMatch(sb.ToString(0, 28), @"\[\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d.\d+"))
            {
                Write("[");
                Write(sb.ToString(1, 10), ConsoleColor.Blue);
                Write("T");
                Write(sb.ToString(12, 8), ConsoleColor.DarkGreen);
                Write(sb.ToString(20, 8), ConsoleColor.DarkGray);
                sb.Remove(0, 28);
                if (sb[0] == '+' || sb[0] == '-')
                {
                    Write(sb.ToString(0, 6), ConsoleColor.DarkBlue);
                    sb.Remove(0, 6);
                }
                Write("]");
                sb.Remove(0, 1);
                Write(sb.ToString(), sb.ToString(0, 4).ToUpper() switch
                {
                    "DEBU" => ConsoleColor.Green,
                    "TRAC" => ConsoleColor.DarkGray,
                    "INFO" => ConsoleColor.White,
                    "WARN" => ConsoleColor.Yellow,
                    "FAIL" => ConsoleColor.Red,
                    _ => null,
                });

            }
            else
            {
                Write(sb.ToString());
            }
        }
        public static void Write(string msg, ConsoleColor? color = null)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}

