using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using WashingMachine.Services;
using WashingMachine.Models.Enums;

namespace WashingMachine.UI
{
    internal class Dashboard
    {
        private readonly WashingService _washingMachineService;
        internal Dashboard(WashingService washingMachineService)
        {
            _washingMachineService = washingMachineService;
        }
        internal void Execute()
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
                if(int.TryParse(loadInKg,out load))
                {
                    break;
                }
            }
            _washingMachineService.StartWashing(pattern, cloth, modes,load);
        }

        private List<WashingMode> WashingModeType()
        {
            Console.WriteLine(@"[S] Soak
[W] Wash
[R] Rinse
[P] Spin
[D] Dry");
            List<WashingMode> modes = new();
            while (true)
            {
                Console.WriteLine("Enter the Washing Modes you prefer [Press E to exit]:");
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.W:
                        modes.Add( WashingMode.Wash);
                        continue;
                    case ConsoleKey.R:
                        modes.Add(WashingMode.Rinse);
                        continue;
                    case ConsoleKey.P:
                        modes.Add(WashingMode.Spin);
                        continue;
                    case ConsoleKey.S:
                        modes.Add(WashingMode.Soak);
                        continue;
                    case ConsoleKey.D:
                        modes.Add(WashingMode.Dry);
                        continue;
                    case ConsoleKey.E:
                        break;
                    default:
                        Console.WriteLine("Invalid input. Please select W or R or P or S or D...");
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
                Console.WriteLine("Enter the cloth key:");
                ConsoleKeyInfo key = Console.ReadKey(true);
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
[E] Exit
Press any Key:");
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.S)
                {
                    Console.WriteLine("\nWELCOME TO THE WASHING MACHINE'S HOME PAGE");
                    return true;
                }
                else if (key.Key == ConsoleKey.E)
                {
                    Console.WriteLine("\nExiting the application");
                    Thread.Sleep(1000);
                    return false;
                }
                else
                {
                    Console.WriteLine("\nInvalid choice. Please press S to Start or E to Exit.");
                }
                return false;
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
                ConsoleKeyInfo key = Console.ReadKey(true);
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
