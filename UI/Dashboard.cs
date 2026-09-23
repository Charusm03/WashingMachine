using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using WashingMachine.Models;
using WashingMachine.Models.Enums;
using WashingMachine.Services;

namespace WashingMachine.UI
{
    internal class Dashboard
    {
        private readonly WashingService _washingMachineService;
        ConsoleRenderer _renderer = new ConsoleRenderer();
        internal Dashboard(WashingService washingMachineService)
        {
            _washingMachineService = washingMachineService;
            _washingMachineService.StageStarted += (s, e) => _renderer.PushLog($"Stage started: {e.Mode}");
            _washingMachineService.StageCompleted += (s, e) => _renderer.PushLog($"Stage completed: {e.Mode}");
            _washingMachineService.WashingCompleted += (s, e) => _renderer.PushNotifications("Washing completed!");
            _washingMachineService.ClothesAdded += (s, e) => _renderer.PushNotifications("Clothes Added!");
            _washingMachineService.WashingPaused += (s, e) => _renderer.PushLog("Washing paused.");
            _washingMachineService.WashingResumed += (s, e) => _renderer.PushLog("Washing resumed.");
        }
        internal async Task Execute()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(40000);
            CancellationToken token = cts.Token;
            try
            {
                if (!UserInput())
                {
                    return;
                }
                WashingPattern pattern = WashingMachinePattern();
                ClothType cloth = WashingClothType();
                List<WashingMode> modes = WashingModeType();
                int load;
                while (true)
                {
                    Console.WriteLine("Enter the load [in KG]:");
                    string loadInKg = Console.ReadLine();
                    if (int.TryParse(loadInKg, out load) && load > 0)
                    {
                        break;
                    }
                }
                Console.Clear();
                var machineData = _washingMachineService.StartWashing(pattern, cloth, modes, load);
                Console.WriteLine(@$"==========================
SUMMARY
==========================
Washing Pattern :{machineData.WashingPattern}
Progress        :{machineData.Progress}
Current mode    :{machineData.CurrentMode}
Load            :{machineData.Load}
Cloth Type      :{machineData.ClothType}
Elapsed Time    :{machineData.ElapsedTime}
Remaining Time  :{machineData.RemainingTime}");
                Console.WriteLine("Press any key to start the washing cycle...");
                Console.ReadKey(intercept: true);
                Console.WriteLine("\n[RUNNING] Washing machine started!");
                _renderer.Init();
                var renderLoop = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        MachineData snapshot;
                        lock (machineData.SyncRoot)
                        {
                            snapshot = new MachineData
                            {
                                WashingPattern = machineData.WashingPattern,
                                ClothType = machineData.ClothType,
                                CurrentMode = machineData.CurrentMode,
                                Progress = machineData.Progress,
                                Status = machineData.Status,
                                Load = machineData.Load,
                                ElapsedTime = machineData.ElapsedTime,
                                RemainingTime = machineData.RemainingTime,
                            };
                        }
                        _renderer.RenderMachine(snapshot);
                        await Task.Delay(100, CancellationToken.None);
                    }
                });
                var inputLoop = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter)
                        {
                            cts.Cancel();
                        }
                        else if (key.Key == ConsoleKey.A)
                        {
                            _washingMachineService.Pause();
                            _renderer.PushLog("Paused — add clothes.");
                            int extra = PromptForExtraLoad();

                            _washingMachineService.AddClothes(machineData, extra);
                            _washingMachineService.Resume();
                            _renderer.PushLog($"Resumed. New load: {machineData.Load} kg");
                        }
                    }
                });

                await _washingMachineService.RunWashingCycleAsync(machineData, token);
                cts.Cancel();
                await Task.WhenAll(renderLoop, inputLoop).ContinueWith(_ => { });
                _renderer.PushNotifications("Washing completed! Press any key to finish.");
                Console.SetCursorPosition(0, 20);
                Console.ReadKey(true);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n[NOTICE] The wash cycle was cancelled or timed out!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] An error occurred: {ex.Message}");
                Console.ReadKey();
            }
        }

        private int PromptForExtraLoad()
        {
            _renderer.Suppress = true;
            try
            {
                lock (typeof(ConsoleRenderer)) // reuse-safe: just needs *some* exclusive window; simplest is a dedicated prompt area
                {
                    Console.SetCursorPosition(0, 18);
                    Console.Write(new string(' ', Console.WindowWidth - 1)); // clear the line first
                    Console.SetCursorPosition(0, 18);
                    Console.Write("Enter additional load (kg): ");
                    while (true)
                    {
                        string input = Console.ReadLine();
                        if (int.TryParse(input, out int extra) && extra > 0)
                        {
                            Console.SetCursorPosition(0, 18);
                            Console.Write(new string(' ', Console.WindowWidth - 1)); // clear prompt line when done
                            return extra;
                        }
                        Console.SetCursorPosition(0, 19);
                        Console.Write("Invalid input, try again: ".PadRight(Console.WindowWidth - 1));
                    }
                }
            }
            finally
            {
                _renderer.Suppress = false;
            }
        }

        private List<WashingMode> WashingModeType()
        {
            Console.WriteLine(@"[S] Soak
[W] Wash
[R] Rinse
[P] Spin
[D] Dry
[E] Exit");
            List<WashingMode> modes = new();
            while (true)
            {
                Console.WriteLine("Enter the Washing Modes you prefer [Press E to exit]:");
                ConsoleKeyInfo key = Console.ReadKey(false);
                Console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.W:
                        if (!modes.Contains(WashingMode.Wash))
                        {
                            modes.Add(WashingMode.Wash);
                            Console.WriteLine("-> Added: Wash");
                        }
                        continue;
                    case ConsoleKey.R:
                        if (!modes.Contains(WashingMode.Rinse))
                        {
                            modes.Add(WashingMode.Rinse);
                            Console.WriteLine("-> Added: Rinse");
                        }
                        continue;
                    case ConsoleKey.P:
                        if (!modes.Contains(WashingMode.Spin))
                        {
                            modes.Add(WashingMode.Spin);
                            Console.WriteLine("-> Added: Spin");
                        }
                        continue;
                    case ConsoleKey.S:
                        if (!modes.Contains(WashingMode.Soak))
                        {
                            modes.Add(WashingMode.Soak);
                            Console.WriteLine("-> Added: Soak");
                        }
                        continue;
                    case ConsoleKey.D:
                        if (!modes.Contains(WashingMode.Dry))
                        {
                            modes.Add(WashingMode.Dry);
                            Console.WriteLine("-> Added: Dry");
                        }
                        continue;
                    case ConsoleKey.E:
                        break;
                    default:
                        Console.WriteLine("Invalid input. Please select S, W, R, P, D, or E.");
                        continue;
                }
                return modes;
            }
        }

        private ClothType WashingClothType()
        {
            ClothType cloth;
            Console.WriteLine($@"[C] Cotton
[S] Synthetic
[W] Wool
[D] Denim");
            while (true)
            {
                Console.Write("Enter the cloth key:");
                ConsoleKeyInfo key = Console.ReadKey(false);
                switch (key.Key)
                {
                    case ConsoleKey.S:
                        cloth = ClothType.Synthetic;
                        break;
                    case ConsoleKey.W:
                        cloth = ClothType.Wool;
                        break;
                    case ConsoleKey.D:
                        cloth = ClothType.Denim;
                        break;
                    case ConsoleKey.C:
                        cloth = ClothType.Cotton;
                        break;
                    default:
                        Console.WriteLine("Invalid Choice. Press S or W or D or C....");
                        continue;
                }
                Console.WriteLine($"\nSelected cloth: {cloth}");
                return cloth;
            }
        }

        private bool UserInput()
        {
            while (true)
            {
                Console.WriteLine(@"
[S] Start 
[E] Exit");
                Console.Write("Selection: ");
                ConsoleKeyInfo key = Console.ReadKey(false);
                Console.WriteLine();

                if (key.Key == ConsoleKey.S)
                {
                    Console.WriteLine("\nWELCOME TO THE WASHING MACHINE HOME PAGE");
                    return true;
                }
                if (key.Key == ConsoleKey.E)
                {
                    Console.WriteLine("\nExiting the application...");
                    Thread.Sleep(1000);
                    return false;
                }

                Console.WriteLine("Invalid choice. Please press S to Start or E to Exit.");
            }
        }

        private WashingPattern WashingMachinePattern()
        {
            Console.WriteLine(@"
[Q] Quick
[D] Delicate
[H] Heavy
[S] Standard");
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(false);
                WashingPattern pattern;
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        pattern = WashingPattern.Quick;
                        break;

                    case ConsoleKey.D:
                        pattern = WashingPattern.Delicate;
                        break;

                    case ConsoleKey.H:
                        pattern = WashingPattern.Heavy;
                        break;

                    case ConsoleKey.S:
                        pattern = WashingPattern.Standard;
                        break;

                    default:
                        Console.WriteLine("\nInvalid choice. Please select Q, D, H or S:");
                        continue;
                }

                Console.WriteLine($"\nSelected pattern: {pattern}");
                return pattern;
            }
        }
    }
}