using CRMP_Auto_Calc.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        static Settings settings;
        static List<Pattern> patterns;
        static Chat chat;

        static bool patternsExists = false;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "CRMP Auto Calc";
            Console.CursorVisible = false;

            try
            {
                Write("Загрузка настроек ");
                LoadSettings();
                Write("■\n", Green);

                Write("Загрузка шаблонов ");
                LoadPatterns();
                Write("■\n", Green);

                chat = new Chat(settings.chatlogPath);
                chat.floodProtection = settings.floodProtection;
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

                        case ConsoleKey.D6:
                            settings.floodProtection = !settings.floodProtection;
                            break;

                        case ConsoleKey.D5:
                            settings.usePatterns = !settings.usePatterns;
                            if (settings.usePatterns)
                            {
                                LoadPatterns();
                            }
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
                Write("■\n", Red);
                Write($"-- {err.Message}\n{err.StackTrace}\n", DarkGray);
                Console.ReadKey(true);
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
                new Text(" 6  | ", DarkGray),
                new Text("Защита от флуда "),
                new Text($"■\n\n", settings.floodProtection ? Green : Red)
            });

            Write(new List<Text>
            {
                new Text(" 5  | ", DarkGray),
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

            Write(new List<Text>()
            {
                new Text("\n\n S  | ", DarkGray),
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
            if (File.Exists("settings.json"))
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            }
            else settings = new Settings();
        }

        static void LoadPatterns()
        {
            if (File.Exists("patterns.json"))
            {
                patternsExists = true;
                patterns = JsonConvert.DeserializeObject<List<Pattern>>(File.ReadAllText("patterns.json"));
            }
            else patternsExists = false;
        }

        static void StartCalc()
        {
            Console.Clear();
            Write("Создание копии чата ");
            if (settings.copyChatlog) File.Copy(settings.chatlogPath, settings.chatlogCopyPath, true);
            Write("■\n", Green);

            Task cancelTask = new Task(() =>
            {
                Console.ReadKey(true);
                chat.Stop();
            });

            cancelTask.Start();
            Write("Чтобы остановить, нажмите ESC\n\n", Gray);
            chat.Start();
        }

        static void Chat_OnChatStateChanged(bool isOpen)
        {
            if (isOpen) Write(MakeDividedText(" чат открыт ") + "\n", Green);
            else Write(MakeDividedText(" чат закрыт ") + "\n", Red);
        }

        static void Chat_OnNewMessage(ChatLine line)
        {
            Pattern pattern = new Pattern()
            {
                pattern = @"\d+\s[\+\-\*\/]\s\d+",
                sendMode = settings.senderType,
                answerDelay = settings.answerDelay
            };

            if (settings.usePatterns)
            {
                patterns.ForEach(p =>
                {
                    if (Regex.Match(line.WithoutColors().message, p.pattern).Success)
                    {
                        pattern = p;
                        return;
                    }
                });
            }

            Write($"{line.WithoutColors().message}\n", DarkGray);

            Match m = Regex.Match(line.WithoutColors().message, @"(?<n1>\d+)\s*(?<l>[\+\-\*\/])\s*(?<n2>\d+)");
            if (!m.Success) return;
            if (m.Groups.Count < 3) return;
            if (!int.TryParse(m.Groups["n1"].Value, out int n1) ||
                !int.TryParse(m.Groups["n2"].Value, out int n2) ||
                !m.Groups["l"].Success) return;
            int examplePos = line.WithoutColors().message.IndexOf(m.Value);
            if (examplePos != -1) WriteAt(examplePos, Console.CursorTop - 1, new Text(m.Value, Green));
            int answer = Solve(n1, n2, m.Groups["l"].Value);
            Write(MakeDividedText($" {answer.ToString()} "), Magenta);

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
