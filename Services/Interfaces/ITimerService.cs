using System;
using System.Collections.Generic;
using System.Text;

namespace WashingMachine.Services.Interfaces
{
    internal interface ITimerService
    {
        Task RunAsync(TimeSpan duration, IProgress<int> progress, CancellationToken cancellationToken);
    }
}