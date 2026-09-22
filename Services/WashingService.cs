using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models;
using WashingMachine.Models.Enums;
using WashingMachine.Services.Interfaces;

namespace WashingMachine.Services
{
    internal class WashingService : IWashingService
    {
        private readonly WashingConfiguration _configurations;
        internal WashingService(WashingConfiguration configurations)
        {
            _configurations = configurations;
        }
        public MachineData StartWashing(
            WashingPattern pattern,
            ClothType cloth,List<WashingMode> methods,int load)
        {
            if(methods.Count==0 || methods == null)
            {
                throw new InvalidOperationException("At least one wash method must be selected.");
            }
            if (load <= 0)
            {
                throw new InvalidOperationException("Load must be greater than zero.");
            }
            TimeSpan totalTimeRemaining = TimeSpan.Zero;
            foreach (var mode in methods)
            {
                totalTimeRemaining += _configurations.GetDuration(pattern, mode, cloth);
            }
            Console.WriteLine("\nWashing machine started!");
            MachineData machineData = new MachineData
            {
                WashingPattern = pattern,
                ClothType = cloth,
                WashMethods=methods,
                CurrentMode = methods[0],
                Load=load,
                ElapsedTime=TimeSpan.Zero,
                Progress = 0,
                Status = MachineStatus.Idle,
                RemainingTime = TimeSpan.Zero,
            };
            return machineData;
        }

        private TimeSpan GetDuration(WashingPattern pattern, ClothType cloth, WashingMode mode)
        {
            return (pattern, cloth, mode) switch
            {
                (WashingPattern.Quick, ClothType.Wool, WashingMode.Spin) => TimeSpan.FromMinutes(15),
            };
        }
    }
}
