using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models.Enums;

namespace WashingMachine.Models
{
    internal class MachineData
    {
        internal readonly object SyncRoot = new();
        public WashingPattern WashingPattern { get; set; }
        public ClothType ClothType { get; set; }
        public WashingMode CurrentMode { get; set; }
        public int Progress { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public MachineStatus Status { get; set; }
        public int Load { get; set; }
        public List<WashingMode> WashMethods { get; set; } = new();
        public TimeSpan ElapsedTime { get; set; }
    }
}