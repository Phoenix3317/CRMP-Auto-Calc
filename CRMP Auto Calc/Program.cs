using CRMP_Auto_Calc.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Phoenix3317.ExtendedConsole.ExConsole;
using static System.ConsoleColor;

namespace CRMP_Auto_Calc
{
    class Program
    {
        static List<Pattern> patterns = new List<Pattern>();
        static Settings settings;
        static Chat chat;

        static bool patternsExists = false;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "CRMP Auto Calc";
            Console.CursorVisible = false;

            try
            {
                LoadSettings();
                LoadPatterns();

                chat = new Chat(settings.chatlogPath)
                {
                    FloodProtection = settings.floodProtection
                };
                chat.OnNewMessage += Chat_OnNewMessage;
                chat.OnChatStateChanged += Chat_OnChatStateChanged;

                while (true)
                {
                    Console.Clear();
                    DrawLogo();
                    DrawMenu();

                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.D1:
                            settings.senderType = settings.senderType == 0 ? 1 : 0;
                            break;

                        case ConsoleKey.D2:
                            settings.chatOpened = settings.chatOpened == 2 ? 0 : settings.chatOpened + 1;
                            break;

                        case ConsoleKey.D3:
                            Write("ms > ", Cyan);
                            if (!int.TryParse(Console.ReadLine(), out int delay) || delay < 0)
                            {
                                WriteAt(0, Console.CursorTop - 1, new Text("ms > ", Red));
                                Write("\n-- задержка указывается положительным числом", DarkGray);
                                Console.ReadKey(true);
                            }
                            else settings.answerDelay = delay;
                            break;

                        case ConsoleKey.D4:
                            settings.copyChatlog = !settings.copyChatlog;
                            break;

                        case ConsoleKey.D5:
                            settings.floodProtection = !settings.floodProtection;
                            break;

                        case ConsoleKey.D6:
                            settings.usePatterns = !settings.usePatterns;
                            if (settings.usePatterns)
                            {
                                Console.Clear();
                                LoadPatterns();
                            }
                            break;

                        case ConsoleKey.D7:
                            PatternEditor.Start(patterns);
                            break;

                        case ConsoleKey.S:
                            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
                            break;

                        case ConsoleKey.R:
                            settings = new Settings();
                            break;

                        case ConsoleKey.Enter:
                            StartCalc();
                            break;

                        case ConsoleKey.Escape:
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                Console.Clear();

                Write(new List<Text>()
                {
                    new Text("\n   "),
                    new Text(" Ошибка ", White, DarkRed),
                    new Text("   "),
                    new Text($" {err.Source} \n", White, DarkBlue),
                    new Text($"-- {err.Message}\n\n", White, DarkRed),
                    new Text($"Нажмите любую клавишу...", DarkGray)
                });

                if (Console.ReadKey(true).Key == ConsoleKey.F10)
                {
                    Write(new List<Text>()
                    {
                        new Text("\n   "),
                        new Text(" Stack trace ", White, DarkYellow),
                        new Text($"\n{err.StackTrace}\n\n", Black, DarkYellow),
                        new Text($"Нажмите любую клавишу...", DarkGray)
                    });
                    Console.ReadKey(true);
                }
            }
        }

        static void DrawLogo()
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

        static void DrawMenu()
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
                new Text($"■", settings.usePatterns ? Green : Red),
            });

            if (settings.usePatterns)
            {
                Write(new List<Text>
                {
                    new Text(patternsExists ? " -- загружено шаблонов: " : " -- шаблоны не найдены", DarkGray),
                    new Text(patternsExists ? $"{patterns.Count}" : "", Gray),
                });
            }

            if (settings.usePatterns && patterns != null && patterns.Count > 0)
            {
                Write(new List<Text>()
                {
                    new Text("\n\n 7  | ", DarkGray),
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

        static void LoadSettings()
        {
            Write("Загрузка настроек ");
            Write("■", Yellow);
            try
            {
                if (File.Exists("settings.json"))
                {
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
                }
                else settings = new Settings();
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Green));
            }
            catch (Exception err)
            {
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Red));
                Write($" -- {err.Message}\n\nНажмите любую клавишу...\nБудут применены настройки по умолчанию.\n\n", DarkGray);
                Console.ReadKey(true);
                settings = new Settings();
            }
        }

        static void LoadPatterns()
        {
            Write("Загрузка шаблонов ");
            Write("■", Yellow);
            try
            {
                patterns = new List<Pattern>();
                if (File.Exists("patterns.json"))
                {
                    patternsExists = true;
                    patterns = JsonConvert.DeserializeObject<List<Pattern>>(File.ReadAllText("patterns.json"));
                }
                else patternsExists = false;
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Green));
            }
            catch (Exception err)
            {
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Red));
                Write($" -- {err.Message}\n\nНажмите любую клавишу...\nШаблоны будут отключены.\n\n", DarkGray);
                Console.ReadKey(true);
                settings.usePatterns = false;
            }
        }

        static void StartCalc()
        {
            Console.Clear();
            bool isWork = true;

            Task cancelTask = new Task(() =>
            {
                while (isWork)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape) isWork = false;
                }
                if (chat.IsWork) chat.Stop();
            });

            if (settings.copyChatlog)
            {
                Write("Создание копии чата ");
                File.Copy(settings.chatlogPath, settings.chatlogCopyPath, true);
                Write("■\n", Green);
            }

            cancelTask.Start();

            if (settings.waitGame)
            {
                Write("Ожидание запуска игры ");
                if (!WaitProcess(settings.gameName, ref isWork)) return;
                Write("■\n", Green);
            }

            Write("Чтобы остановить, нажмите ESC\n\n", Gray);
            chat.Start();
        }

        static bool WaitProcess(string process, ref bool isWork)
        {
            while (isWork)
            {
                if (Process.GetProcessesByName(process).Length > 0) return true;
            }
            return false;
        }

        static void Chat_OnChatStateChanged(bool isOpen)
        {
            if (isOpen) Write(MakeDividedText(" чат открыт ") + "\n", Green);
            else Write(MakeDividedText(" чат закрыт ") + "\n", Red);
        }

        static void Chat_OnNewMessage(ChatLine line)
        {
            Pattern pattern = null;
            Write($"{line.WithoutColors().message}\n", DarkGray);

            if (settings.usePatterns)
            {
                patterns.ForEach(p =>
                {
                    if (Regex.Match(line.WithoutColors().message, p.pattern, p.ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).Success)
                    {
                        pattern = p;
                        return;
                    }
                });

                if (pattern == null) return;
            }

            pattern = new Pattern()
            {
                pattern = @"\d+\s[\+\-\*\/]\s\d+",
                sendMode = settings.senderType,
                answerDelay = settings.answerDelay
            };

            Match m = Regex.Match(line.WithoutColors().message, @"(?<n1>\d+)\s*(?<l>[\+\-\*\/])\s*(?<n2>\d+)");
            if (!m.Success) return;
            if (m.Groups.Count < 3) return;
            if (!int.TryParse(m.Groups["n1"].Value, out int n1) ||
                !int.TryParse(m.Groups["n2"].Value, out int n2) ||
                !m.Groups["l"].Success) return;
            if (n1 == 0 && n2 == 0) return;
            int examplePos = line.WithoutColors().message.IndexOf(m.Value);
            if (examplePos != -1) WriteAt(examplePos, Console.CursorTop - 1, new Text(m.Value, settings.usePatterns ? Black : Green, settings.usePatterns ? DarkYellow : Black));
            string answer = Solve(n1, n2, m.Groups["l"].Value).ToString();
            Write(MakeDividedText($" {answer} "), Magenta);

            if (pattern.answer != "") answer = pattern.answer.Replace("%answer%", answer);

            if (pattern.sendMode == 0) Clipboard.SetText(answer.ToString());
            else
            {
                Thread.Sleep(pattern.answerDelay);
                chat.SendMsg(answer.ToString(), settings.chatOpened);
            }
        }

        static int Solve(int n1, int n2, string l)
        {
            switch (l)
            {
                case "+": return n1 + n2;
                case "-": return n1 - n2;
                case "*": return n1 * n2;
                case "/": return n1 / n2;
                case "^": return n1 ^ n2;
                case "%": return n1 % n2;
                default: return n1 + n2;
            }
        }
    }
}
