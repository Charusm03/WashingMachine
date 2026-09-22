using System;
using System.Collections.Generic;
using System.Text;

namespace WashingMachine.Models
{
    internal class WashingHistory
    {
        private Dictionary<DateTime, MachineData> history=new ();
    }
}
