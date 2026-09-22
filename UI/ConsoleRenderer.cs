using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using WashingMachine.Models;

namespace WashingMachine.UI
{
    internal class ConsoleRenderer
    {
        private static readonly object _consoleLock = new();
        private const int HeaderRow = 0;
        private const int PatternRow = 2;
        private const int ClothRow = 3;
        private const int LoadRow = 4;
        private const int StageRow = 5;
        private const int StatusRow = 6;
        private const int ProgressRow = 7;
        private const int TimeRow = 8;
        private const int InstructionsRow = 11;
        private const int NotificationRow = 13;
        private const int LogStartRow = 15;
        private const int LogMaxRows = 5;
        private readonly Queue<string> logs = new();
        internal void Init()
        {
            Console.CursorVisible = false;
            Console.Clear();
        }
        internal void RenderMachine(MachineData data)
        {
            lock (_consoleLock)
            {
                WriteLine(HeaderRow, "==============================");
                WriteLine(PatternRow, $"Pattern  : {data.WashingPattern}");
                WriteLine(ClothRow, $"Cloth    : {data.ClothType}");
                WriteLine(LoadRow, $"Load     : {data.Load} kg");
                WriteLine(StageRow, $"Stage    : {data.CurrentMode}");
                WriteLine(StatusRow, $"Status   : {data.Status}");
                WriteLine(ProgressRow, $"Progress : {ProgressBar(data.Progress)} {data.Progress}%");
                WriteLine(TimeRow, $"Elapsed  : {data.ElapsedTime:mm\\:ss}   Remaining : {data.RemainingTime:mm\\:ss}");
                WriteLine(InstructionsRow, "[ENTER] cancel wash");
            }
        }

        private string ProgressBar(int percent)
        {
            int filled = percent / 5;
            return "[" + new String('0', filled) + new string(':', 20 - filled) + "]";
        }

        internal void PushNotifications(string message)
        {
            lock (_consoleLock)
            {
                WriteLine(NotificationRow,$"[NOTICE] {message}");
            }
        }
        internal void PushLog(string message)
        {
            lock (_consoleLock)
            {
                logs.Enqueue($"{DateTime.Now:HH:mm:ss} {message}");
                while (logs.Count > LogMaxRows)
                {
                    logs.Dequeue();
                }
                int row = LogStartRow;
                foreach(var log in logs)
                {
                    WriteLine(row, log);
                    row++;
                }
            }
        }
        private void WriteLine(int row, string text)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(text.PadRight(Console.WindowWidth-2));
        }
    }
}
