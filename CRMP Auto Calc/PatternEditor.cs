using System;
using System.Collections.Generic;
using System.IO;

using CRMP_Auto_Calc.Models;
using Newtonsoft.Json;
using static System.ConsoleColor;
using static Phoenix3317.ExtendedConsole.ExConsole;

namespace CRMP_Auto_Calc
{
    static class PatternEditor
    {
        public static List<Pattern> patterns = new List<Pattern>();

        private static int selectedPattern = -1;
        private static Pattern sPattern
        {
            get
            {
                if (selectedPattern > 0 && selectedPattern < patterns.Count) return patterns[selectedPattern];
                else return null;
            }
        }

        private static bool isWork = false;

        public static void Start(List<Pattern> patterns = null)
        {
            if (patterns != null) PatternEditor.patterns = patterns;

            isWork = true;
            ConsoleKey key;

            while (isWork)
            {
                Console.Clear();
                patterns.ForEach(pattern =>
                {
                    int index = patterns.IndexOf(pattern);
                    bool selected = index == selectedPattern;

                    Write(new List<Text>()
                    {
                        new Text($"{(selected ? "0 | " : "")}#{index}| {pattern.pattern}\n", Black, selected ? DarkGreen : DarkYellow),
                        new Text($"{(selected ? "1 | " : "")}Ответ: "),
                        new Text(pattern.answer == "" ? "%answer%\n" : $"{pattern.answer}\n", Yellow),
                        new Text($"{(selected ? "2 | " : "")}Задержка ответа: "),
                        new Text($"{pattern.answerDelay}ms\n", Yellow),
                        new Text($"{(selected ? "3 | " : "")}Отправлять ответ в "),
                        new Text(pattern.sendMode == 0 ? "буфер обмена\n\n" : "чат\n\n", Yellow)
                    });
                });

                Write(new List<Text>()
                {
                    new Text(" N  | ", DarkGray),
                    new Text("Новый шаблон\n"),

                    new Text(" D  | ", DarkGray),
                    new Text("Копировать шаблон\n"),

                    new Text(" E  | ", DarkGray),
                    new Text("Выбрать шаблон\n"),

                    new Text(" ENT| ", DarkGray),
                    new Text("Снять выделение\n"),

                    new Text(" S  | ", DarkGray),
                    new Text("Сохранить шаблоны\n"),

                    new Text(" ESC| ", DarkGray),
                    new Text("Выход\n", Red),
                });

                key = Console.ReadKey(true).Key;

                switch(key)
                {
                    case ConsoleKey.N:
                        patterns.Add(new Pattern());
                        break;

                    case ConsoleKey.E:
                        Write("id > ", Cyan);
                        if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index > patterns.Count)
                        {
                            Write("\n-- id должен быть положительным числом от 0 до " + patterns.Count, DarkGray);
                            Console.ReadKey(true);
                        }
                        else selectedPattern = index;
                        break;

                    case ConsoleKey.Enter:
                        selectedPattern = -1;
                        break;

                    case ConsoleKey.S:
                        Save();
                        break;

                    case ConsoleKey.Escape: return;

                    case ConsoleKey.UpArrow:
                        selectedPattern = selectedPattern <= 0 ? patterns.Count - 1 : selectedPattern - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        selectedPattern = selectedPattern >= patterns.Count - 1 ? 0 : selectedPattern + 1;
                        break;
                }

                if (selectedPattern < 0) continue;

                switch(key)
                {
                    case ConsoleKey.D0:
                        Write("pattern > ", Cyan);
                        sPattern.pattern = Console.ReadLine();
                        break;

                    case ConsoleKey.D1:
                        Write("answer > ", Cyan);
                        sPattern.answer = Console.ReadLine();
                        break;

                    case ConsoleKey.D2:
                        Write("answerDelay > ", Cyan);
                        if (!int.TryParse(Console.ReadLine(), out int delay) || delay < 0)
                        {
                            Write("answerDelay должен быть положительным числом.", Red);
                            Console.ReadKey(true);
                        } else sPattern.answerDelay = delay;
                        break;

                    case ConsoleKey.D3:
                        sPattern.sendMode = sPattern.sendMode == 0 ? 1 : 0;
                        break;

                    case ConsoleKey.D:
                        patterns.Add(patterns[selectedPattern]);
                        break;
                }
            }
        }

        public static void Stop() => isWork = false;

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(patterns, Formatting.Indented);
            File.WriteAllText("patterns.json", json);
        }
    }
}
