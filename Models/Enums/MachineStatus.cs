using System;
using System.Collections.Generic;
using System.Text;

namespace WashingMachine.Models.Enums
{
    internal enum MachineStatus
    {
        Idle,
        Running, 
        Paused, 
        Completed,
        Cancelled,
    }
}
