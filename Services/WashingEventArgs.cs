using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models;
using WashingMachine.Models.Enums;

namespace WashingMachine.Services
{
    internal class WashingEventArgs : EventArgs
    {
        public DateTime TimeStamp { get; } = DateTime.Now;
    }
    internal class StagesEventArgs : WashingEventArgs
    {
        public WashingMode Mode { get; set; }
        public TimeSpan Duration { get; }
        public StagesEventArgs(WashingMode mode, TimeSpan stageDuration) : base()
        {
            Mode = mode;
            Duration = stageDuration;
        }
    }
    internal class ClothesAddedEventArgs : WashingEventArgs
    {
        public int PreviousLoad { get; }
        public int NewLoad { get; }

        public ClothesAddedEventArgs(int previousLoad, int newLoad)
        {
            PreviousLoad = previousLoad;
            NewLoad = newLoad;
        }
    }
    internal class WashingCompletedEventArgs : WashingEventArgs
    {
        public MachineData FinalState { get; }

        public WashingCompletedEventArgs(MachineData finalState)
        {
            FinalState = finalState;
        }
    }
}