using System;
using System.Collections.Generic;

using static Phoenix3317.ExtendedConsole.ExConsole;
using static System.ConsoleColor;

namespace Phoenix3317.ExtendedConsole
{
    class ExConsole
    {
        public class Text
        {
            public ConsoleColor foreground, background;
            public ColorSet Colors => new ColorSet(foreground, background);
            public string text;

            public Text(string text, ConsoleColor foreground = White, ConsoleColor background = Black)
            {
                this.text = text;
                this.foreground = foreground;
                this.background = background;
            }

            public Text(string text, ConsoleColor foreground)
            {
                this.text = text;
                this.foreground = foreground;
                this.background = Console.BackgroundColor;
            }

            public Text(string text, ColorSet colors)
            {
                this.text = text;
                this.foreground = colors.foreground;
                this.background = colors.background;
            }

            public static implicit operator Text(string text) => new Text(text, ColorSet.Current);
        }
        public class ColorSet
        {
            public ConsoleColor foreground, background;
            
            public static ColorSet Current
            {
                get => new ColorSet(Console.ForegroundColor, Console.BackgroundColor);
                set
                {
                    Console.ForegroundColor = value.foreground;
                    Console.BackgroundColor = value.background;
                }
            }

            public ColorSet(ConsoleColor foreground)
            {
                this.foreground = foreground;
                this.background = Console.BackgroundColor;
            }

            public ColorSet(ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
            {
                this.foreground = foreground;
                this.background = background;
            }

            public static implicit operator ColorSet(ConsoleColor color) => new ColorSet(color);
        }

        public static void WriteLines(List<Text> lines) => lines.ForEach(line => Write($"{line.text}\n", line.foreground, line.background));
        public static void Write(List<Text> text) => text.ForEach(t => Write(t));
        public static void Write(string text, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            ConsoleColor lForeground = Console.ForegroundColor;
            ConsoleColor lBackground = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(text);
            Console.ForegroundColor = lForeground;
            Console.BackgroundColor = lBackground;
        }
        public static void Write(Text text) => Write(text.text, text.foreground, text.background);
        public static void Write(string text) => Write(text, Console.ForegroundColor, Console.BackgroundColor);
        public static void Write(string text, ConsoleColor foreground) => Write(text, foreground, Console.BackgroundColor);
        public static void Write(string text, ColorSet colors) => Write(text, colors.foreground, colors.background);

        public static void WriteFrom(int x, int y, Text text)
        {
            Console.CursorLeft = x;
            Console.CursorTop = y;
            Write(text);
        }

        public static void WriteAt(int x, int y, Text text)
        {
            int _x = Console.CursorLeft;
            int _y = Console.CursorTop;
            WriteFrom(x, y, text);
            Console.CursorLeft = _x;
            Console.CursorTop = _y;
        }

        public static string MakeLine(char c, int count)
        {
            string result = "";
            for (int i = 0; i < count; i++) result += c;
            return result;
        }

        public static string MakeDividedText(string text, int maxWidth, char dividerChar = '─')
        {
            string divider = maxWidth > text.Length ? MakeLine(dividerChar, (maxWidth - text.Length) / 2) : "";
            return $"{divider}{text}{divider}";
        }

        public static string MakeDividedText(string text) => MakeDividedText(text, Console.WindowWidth);
    }
}