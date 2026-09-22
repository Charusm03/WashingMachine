using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models;
using WashingMachine.Models.Enums;

namespace WashingMachine.Services.Interfaces
{
    internal interface IWashingService
    {
        MachineData StartWashing(WashingPattern pattern, ClothType cloth, List<WashingMode> methods, int load);

    }
}
