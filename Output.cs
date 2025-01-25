using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_bot
{
    public static class Output
    {
        static LinkedList<string> consoleLines = new(new[] { "" });
        private static readonly System.Threading.Lock _consoleLock = new();

        public static ConsoleColor Background { get { return Console.BackgroundColor; } set { Console.BackgroundColor = value; } }
        public static ConsoleColor Foreground { get { return Console.ForegroundColor; } set { Console.ForegroundColor = value; } }
        

        public static void Draw(TwitterBot bot)
        {
             System.Threading.ThreadState state = bot._runThread != null ? bot._runThread.ThreadState : System.Threading.ThreadState.Unstarted;

            if(state == System.Threading.ThreadState.Running)
                Console.Clear();


            string fancy = $"--------~ {bot.DisplayName} ~--------";
            var what = Console.GetCursorPosition();
            Console.SetCursorPosition(0, 0);
            ClearLine();
            CoutL(new string(' ', (Console.WindowWidth - fancy.Length) / 2) + fancy);
            CoutL();
            


            if (bot._loginState < 3)
            {
                CoutL($"LOGGING INTO {bot.DisplayName}{(bot.ticks %3 == 0 ? "..." : "" )}");
                var elTime = DateTime.Now - bot._loginStart;

                CoutL($"Elapsed time {elTime.Minutes:D2}:{elTime.Seconds:D2}");
            }
            else if(state == System.Threading.ThreadState.Running){
                CoutL("\n\n~~~~RUNNING BOT~~~~");
                var elTime = DateTime.Now - bot._runStart;
                CoutL($"Elapsed time {elTime.Minutes:D2}:{elTime.Seconds:D2}");
            }
            else
            {
                DateTime endTime = DateTime.Now.AddSeconds(bot.timer);
                TimeSpan essentiallyACrashout = TimeSpan.FromSeconds(bot.timer);
                //CoutL($"{bot.timer} {bot.interval} {bot.Progress}");
                ClearLine();
                CoutL($"Next Tweet - {endTime.TimeOfDay.Hours:D2}:{endTime.TimeOfDay.Minutes:D2}:{endTime.TimeOfDay.Seconds:D2}");
                ClearLine();
                CoutL($"[{new string('#', (int)Math.Min(Math.Max(0,bot.Progress*20), 20))}{new string(' ', (int)Math.Min(Math.Max(0,(1 - bot.Progress) * 20),20))}] {bot.Progress*100}% - {essentiallyACrashout.Minutes:D2}:{essentiallyACrashout.Seconds:D2}");
            }

            CoutL("\n\n-------------------------------------");

            lock (_consoleLock)
            {
                foreach (string line in consoleLines)
                {
                    ClearLine();
                    if (Console.GetCursorPosition().Top + 3 > Console.WindowHeight)
                        break;
                    Cout(line.TrimEnd());
                }
            }
            Console.Out.Flush();
        }

        private static void ClearLine() => Cout(new string(' ', Console.WindowWidth-1) + "\r");
        private static void Cout(string str) => Console.Out.Write(str);
        private static void CoutL(string str = "") => Console.Out.WriteLine(str);

        public static void WriteLine(string contents) => Write(contents + "\n");
        public static void Write(string contents)
        {
            string[] lines = contents.Split('\n');
            lock (_consoleLock)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (i != lines.Length - 1)
                        consoleLines.AddFirst(line.Substring(0, Math.Min(line.Length, Console.WindowWidth)));
                    else if (consoleLines.First != null)
                        consoleLines.First.Value += line;
                }

                if (consoleLines.Count > Console.WindowHeight - 2)
                    while (consoleLines.Count > Console.WindowHeight - 15) consoleLines.RemoveLast();
            }
        }

        
    }
}
