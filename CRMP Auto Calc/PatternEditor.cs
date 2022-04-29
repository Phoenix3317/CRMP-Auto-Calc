using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CRMP_Auto_Calc.Models;

using static System.ConsoleColor;
using static Phoenix3317.ExtendedConsole.ExConsole;

namespace CRMP_Auto_Calc
{
    static class PatternEditor
    {
        public static List<Pattern> patterns = new List<Pattern>();

        private static int selectedPattern = 0;
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

                    Visual.DrawPattern(pattern, index, selected);
                });

                Visual.DrawPatternsEditorMenu();

                key = Console.ReadKey(true).Key;

                switch(key)
                {
                    case ConsoleKey.A:
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

                    case ConsoleKey.C:
                        patterns.Add(patterns[selectedPattern]);
                        break;

                    case ConsoleKey.Delete:
                        Write("Чтобы удалить, нажмите Y\n", White, Red);
                        Write("Или иную клавишу, чтобы вернуться");
                        if (Console.ReadKey(true).Key == ConsoleKey.Y) patterns.RemoveAt(selectedPattern);
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
