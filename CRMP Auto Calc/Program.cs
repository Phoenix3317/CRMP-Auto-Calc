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
        public static Settings settings = new Settings();
        public static List<Pattern> patterns = new List<Pattern>();
        public static bool patternsExists = false;

        static int totalMessages = 0;
        static int solvedExamples = 0;
        static string lastExample = "";
        static string answer = "";

        static Process game;
        static Chat chat;

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
                chat.OnKeyPressed += Chat_OnKeyPressed;

                while (true)
                {
                    Console.Clear();
                    Visual.DrawLogo();
                    Visual.DrawMenu();

                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.D1:
                            settings.manualMode = !settings.manualMode;
                            break;

                        case ConsoleKey.D2:
                            settings.senderType = settings.senderType == 0 ? 1 : 0;
                            break;

                        case ConsoleKey.D3:
                            settings.chatOpened = settings.chatOpened == 2 ? 0 : settings.chatOpened + 1;
                            break;

                        case ConsoleKey.D4:
                            Write("ms > ", Cyan);

                            string s = Console.ReadLine();
                            if (s == "") break;

                            if (DelayRange.TryParse(s, out DelayRange range))
                            {
                                settings.useRandomDelay = true;
                                settings.answerRandomDelay = range;
                            }
                            else if (int.TryParse(s, out int delay))
                            {
                                settings.useRandomDelay = false;
                                settings.answerDelay = delay;
                            }
                            else
                            {
                                WriteAt(0, Console.CursorTop - 1, new Text("ms > ", Red));
                                Write("\n-- задержка должна быть положительным числом и указываться в мс (1000мс = 1сек)\n-- Чтобы использовать случайную задержку, введите два положительных числа, разделенных пробелом или '-' (700 1200).\n-- Первое число должно быть меньше второго!", DarkGray);
                                Console.ReadKey(true);
                            }
                            break;

                        case ConsoleKey.D5:
                            settings.copyChatlog = !settings.copyChatlog;
                            break;

                        case ConsoleKey.D6:
                            settings.floodProtection = !settings.floodProtection;
                            break;

                        case ConsoleKey.D7:
                            settings.usePatterns = !settings.usePatterns;
                            if (settings.usePatterns)
                            {
                                Console.Clear();
                                LoadPatterns();
                            }
                            break;

                        case ConsoleKey.D8:
                            settings.onlyPatterns = !settings.onlyPatterns;
                            break;

                        case ConsoleKey.D9:
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

                        case ConsoleKey.F1:
                            Write("Клавиша > ", Cyan);
                            Write("\n-- Нажмите любую клавишу, кроме ESC, Backspace, Delete, F6 и Enter");
                            Console.SetCursorPosition(10, Console.CursorTop - 1);

                            ConsoleKey key = Console.ReadKey().Key;
                            if (key == ConsoleKey.Escape ||
                                key == ConsoleKey.Backspace ||
                                key == ConsoleKey.Delete ||
                                key == ConsoleKey.F6 ||
                                key == ConsoleKey.Enter) break;
                            settings.sendKey = (Keys)key;
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                Console.Clear();
                Visual.DrawException(err, out ConsoleKeyInfo key);

                if (key.Key == ConsoleKey.F10)
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

        private static void Chat_OnKeyPressed(KeyEventArgs e)
        {
            if (settings.manualMode)
            {
                if (e.KeyCode == settings.sendKey)
                {
                    chat.SendMsg(answer, settings.chatOpened);
                }
            }
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
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Green));
            }
            catch (Exception err)
            {
                WriteFrom(Console.CursorLeft - 1, Console.CursorTop, new Text("■\n", Red));
                Write($" -- {err.Message}\n\nНажмите любую клавишу...\nБудут применены настройки по умолчанию.\n\n", DarkGray);
                Console.ReadKey(true);
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

            Task gameWatcherTask = new Task(() =>
            {
                while (isWork)
                {
                    if (game.HasExited) isWork = false;
                    Thread.Sleep(500);
                }
            });

            if (settings.copyChatlog)
            {
                Write("Создание копии чата ");
                File.Copy(settings.chatlogPath, settings.chatlogCopyPath, true);
                Write("■\n", Green);
            }

            if (settings.waitGame)
            {
                Write("Ожидание запуска игры ");
                if (!WaitGame(settings.gameName, ref isWork)) return;
                Write("■\n", Green);

                gameWatcherTask.Start();
            }

            Write("Чтобы остановить, нажмите ESC\n\n", Gray);
            chat.StartAsync();
            
            while (isWork)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape) isWork = false;
            }

            chat.Stop();

            Write(new List<Text>()
            {
                new Text("Всего сообщений: "),
                new Text($"{totalMessages}\n", Yellow),

                new Text("Решено примеров: "),
                new Text($"{solvedExamples}\n", Yellow),

                new Text("Последний пример: "),
                new Text($"{lastExample}\n\n", Yellow),

                new Text("Нажмите любую клавишу...", DarkGray)
            });

            Console.ReadKey(true);
        }

        static bool WaitGame(string process, ref bool isWork)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(settings.chatlogPath), Path.GetFileName(settings.chatlogPath));

            while (isWork)
            {
                Process[] prcList = Process.GetProcessesByName(process);
                if (prcList.Length > 0 && !prcList[0].HasExited)
                {
                    WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed, 5000);

                    if (!result.TimedOut)
                    {
                        game = prcList[0];
                        return true;
                    }
                }

                Thread.Sleep(500);
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
            Random rand = new Random();
            Write($"{line.WithoutColors().message}\n", DarkGray);
            totalMessages++;

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

                if (settings.onlyPatterns && pattern == null) return;
            }

            if (!Calc.Solve(line.WithoutColors().message, out long result)) return;

            Match m = Calc.LastFullExample;
            int examplePos = line.WithoutColors().message.IndexOf(m.Value);
            if (examplePos != -1) WriteAt(examplePos, Console.CursorTop - 1, new Text(m.Value, settings.usePatterns && pattern != null ? Black : Green, settings.usePatterns && pattern != null ? DarkYellow : Black));

            answer = result.ToString();

            lastExample = $"{m.Value} = {answer}";

            if (pattern == null)
            {
                pattern = new Pattern()
                {
                    pattern = ".*",
                    answer = "",
                    answerDelay = settings.useRandomDelay ? rand.Next(settings.answerRandomDelay.Min, settings.answerRandomDelay.Max) : settings.answerDelay,
                    sendMode = settings.senderType
                };
            }

            if (pattern.answer != "") answer = pattern.answer.Replace("%answer%", answer);

            if (pattern.sendMode == 0) Clipboard.SetText(answer.ToString());
            else
            {
                if (!settings.manualMode)
                {
                    Thread.Sleep(pattern.answerDelay);
                    chat.SendMsg(answer, settings.chatOpened);
                }
            }
            Write(MakeDividedText($" {answer} {(!settings.manualMode ? "→ " : "")}"), Magenta);
            solvedExamples++;
        }
    }
}