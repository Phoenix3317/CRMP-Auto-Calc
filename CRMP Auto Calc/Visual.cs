using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRMP_Auto_Calc.Models;
using static System.ConsoleColor;
using static Phoenix3317.ExtendedConsole.ExConsole;

namespace CRMP_Auto_Calc
{
    class Visual
    {
        static Settings settings = Program.settings;
        static List<Pattern> patterns = Program.patterns;
        static bool patternsExists = Program.patternsExists;

        public static void DrawException(Exception err)
        {
            Console.Clear();

            Write(new List<Text>()
            {
                new Text("\n   "),
                new Text(" Ошибка ", White, DarkRed),
                new Text("   "),
                new Text($" {err.Source} \n", White, DarkBlue),
                new Text($"-- {err.Message}\n\n", White, DarkRed)
            });
        }

        public static void DrawException(Exception err, out ConsoleKeyInfo key)
        {
            DrawException(err);
            key = Console.ReadKey(true);
        }

        public static void DrawPattern(Pattern pattern, int index, bool isSelected)
        {
            Write(new List<Text>()
            {
                new Text($"{(isSelected ? "0 | " : "")}#{index}| {pattern.pattern}\n", Black, isSelected ? DarkGreen : DarkYellow),
                new Text($"{(isSelected ? "1 | " : "")}Ответ: "),
                new Text(pattern.answer == "" ? "%answer%\n" : $"{pattern.answer}\n", Yellow),
                new Text($"{(isSelected ? "2 | " : "")}Задержка ответа: "),
                new Text($"{pattern.answerDelay}ms\n", Yellow),
                new Text($"{(isSelected ? "3 | " : "")}Отправлять ответ в "),
                new Text(pattern.sendMode == 0 ? "буфер обмена\n\n" : "чат\n\n", Yellow)
            });
        }

        public static void DrawPatternsEditorMenu()
        {
            Write(new List<Text>()
            {
                new Text(" A  | ", DarkGray),
                new Text("Новый шаблон\n"),

                new Text(" DEL| ", DarkGray),
                new Text("Удалить шаблон\n"),

                new Text(" С  | ", DarkGray),
                new Text("Копировать шаблон\n"),

                new Text(" ↑  | ", DarkGray),
                new Text("Выбрать шаблон\n"),

                new Text(" E  | ", DarkGray),
                new Text("Выбрать шаблон\n"),

                new Text(" ↓  | ", DarkGray),
                new Text("Выбрать шаблон\n"),

                new Text(" S  | ", DarkGray),
                new Text("Сохранить шаблоны в файл\n"),

                new Text(" ESC| ", DarkGray),
                new Text("Выход\n", Red),
            });
        }

        public static void DrawLogo()
        {
            Write(new List<Text>()
            {
                new Text("┌───────────────────┐\n", Gray),
                new Text("│", Gray),
                new Text("CRMP ", White),
                new Text("AUTO ", Blue),
                new Text("CALC     ", Red),
                new Text("│\n│", Gray),
                new Text(" Turn off the brain", DarkGray),
                new Text("│\n└───────────────────┘\n\n", Gray)
            });
        }

        public static void DrawMenu()
        {
            string param = "";
            Write(new List<Text>
            {
                new Text(" 1  | ", DarkGray),
                new Text("Отправлять ответ в "),
                new Text($"{(settings.senderType == 0 ? "буфер обмена" : "чат")}\n", Yellow)
            });

            param = "ничего не делать";
            if (settings.chatOpened != 0) param = settings.chatOpened == 1 ? "отправить ответ и закрыть чат" : "отправить ответ и вновь открыть чат";

            Write(new List<Text>
            {
                new Text(" 2  | ", DarkGray),
                new Text("Если чат открыт, то ", settings.senderType == 1 ? White : DarkGray),
                new Text(param + "\n", settings.senderType == 1 ? Yellow : DarkGray)
            });

            Write(new List<Text>
            {
                new Text(" 3  | ", DarkGray),
                new Text("Задержка отправки ответа = "),
                new Text($"{settings.answerDelay} ms\n", Yellow)
            });

            Write(new List<Text>
            {
                new Text(" 4  | ", DarkGray),
                new Text("Сохранять копию чата "),
                new Text($"■\n", settings.copyChatlog ? Green : Red)
            });

            Write(new List<Text>
            {
                new Text(" 5  | ", DarkGray),
                new Text("Защита от флуда "),
                new Text($"■\n\n", settings.floodProtection ? Green : Red)
            });

            Write(new List<Text>
            {
                new Text(" 6  | ", DarkGray),
                new Text("Использовать шаблоны "),
                new Text("(patterns.txt) ", DarkGray),
                new Text("■", settings.usePatterns ? Green : Red),
            });

            if (settings.usePatterns)
            {
                Write(new List<Text>
                {
                    new Text(patternsExists ? " -- загружено шаблонов: " : " -- шаблоны не найдены", DarkGray),
                    new Text(patternsExists ? $"{patterns.Count}" : "", Gray),
                });

                Write(new List<Text>()
                {
                    new Text("\n 7  | ", DarkGray),
                    new Text("Использовать только шаблоны "),
                    new Text("■", settings.onlyPatterns ? Green : Red)
                });
            }

            if (settings.usePatterns && patterns != null && patterns.Count > 0)
            {
                Write(new List<Text>()
                {
                    new Text("\n\n 8  | ", DarkGray),
                    new Text("Редактор шаблонов\n", Cyan),
                });
            }
            else Write("\n\n");

            Write(new List<Text>()
            {
                new Text(" S  | ", DarkGray),
                new Text("Сохранить настройки\n", Cyan),

                new Text(" R  | ", DarkGray),
                new Text("Сбросить настройки\n", Cyan),

                new Text(" ENT| ", DarkGray),
                new Text("Запустить Auto Calc\n", Cyan),

                new Text(" ESC| ", DarkGray),
                new Text("Выход\n\n", Red)
            });
        }
    }
}
