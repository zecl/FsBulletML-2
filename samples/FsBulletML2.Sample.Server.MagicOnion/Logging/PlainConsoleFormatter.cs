using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// Console へ 1 行 で出す形。
    /// </summary>
    public sealed class PlainConsoleFormatter : ConsoleFormatter
    {
        public const string FormatterName = "plain";

        public PlainConsoleFormatter()
            : base(FormatterName)
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> entry, IExternalScopeProvider scopes, TextWriter writer)
        {
            string text = entry.Formatter?.Invoke(entry.State, entry.Exception);
            if (string.IsNullOrEmpty(text) && entry.Exception == null)
            {
                return;
            }

            writer.Write(DateTime.Now.ToString("HH:mm:ss"));
            writer.Write("  ");
            writer.Write(Level(entry.LogLevel));
            writer.Write("  ");
            writer.Write(Short(entry.Category).PadRight(6));
            writer.Write("  ");
            writer.WriteLine(text);

            if (entry.Exception != null)
            {
                writer.WriteLine(entry.Exception.ToString());
            }
        }

        /// <summary>幅 を揃える。 揃っていないと、目 が縦に流せない</summary>
        static string Level(LogLevel level) => level switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => "    ",
        };

        /// <summary>名前空間を落として、最後の 1 つ だけ残す</summary>
        static string Short(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return "-";
            }

            int at = category.LastIndexOf('.');
            return at < 0 ? category : category.Substring(at + 1);
        }
    }
}