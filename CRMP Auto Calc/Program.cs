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

                while (true)
                {
                    Console.Clear();
                    Visual.DrawLogo();
                    Visual.DrawMenu();

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
                            settings.onlyPatterns = !settings.onlyPatterns;
                            break;

                        case ConsoleKey.D8:
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

            Task cancelTask = new Task(() =>
            {
                while (isWork)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape) isWork = false;
                }

                if (chat.IsWork) chat.Stop();
            });

            Task gameWatcherTask = new Task(() =>
            {
                while (isWork)
                {
                    if (game.HasExited) chat.Stop();
                    Thread.Sleep(500);
                }
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

                gameWatcherTask.Start();
            }

            Write("Чтобы остановить, нажмите ESC\n\n", Gray);
            chat.Start();

            isWork = false;
            Write("\nНажмите любую клавишу...", Yellow);
            Console.ReadKey(true);
        }

        static bool WaitProcess(string process, ref bool isWork)
        {
            while (isWork)
            {
                Process[] prcList = Process.GetProcessesByName(process);
                if (prcList.Length > 0 && !prcList[0].HasExited)
                {
                    game = prcList[0];
                    return true;
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

                if (settings.onlyPatterns && pattern == null) return;
            }

            if (!Calc.Solve(line.WithoutColors().message, out long result)) return;

            Match m = Calc.LastFullExample;
            int examplePos = line.WithoutColors().message.IndexOf(m.Value);
            if (examplePos != -1) WriteAt(examplePos, Console.CursorTop - 1, new Text(m.Value, settings.usePatterns && pattern != null ? Black : Green, settings.usePatterns && pattern != null ? DarkYellow : Black));

            string answer = result.ToString();

            if (pattern == null)
            {
                pattern = new Pattern()
                {
                    pattern = ".*",
                    answer = "",
                    answerDelay = settings.answerDelay,
                    sendMode = settings.senderType
                };
            }

            if (pattern.answer != "") answer = pattern.answer.Replace("%answer%", answer);

            if (pattern.sendMode == 0) Clipboard.SetText(answer.ToString());
            else
            {
                Thread.Sleep(pattern.answerDelay);
                chat.SendMsg(answer, settings.chatOpened);
            }
            Write(MakeDividedText($" {answer} "), Magenta);
        }
    }
}