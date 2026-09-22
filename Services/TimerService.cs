using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models;
using WashingMachine.Services.Interfaces;

namespace WashingMachine.Services
{
    internal class TimerService : ITimerService
    {
        private const int TotalIntervalMillisecond = 100;

        public async Task RunAsync(TimeSpan duration, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (duration <= TimeSpan.Zero)
            {
                progress?.Report(100);
                return;
            }
            DateTime startTime = DateTime.UtcNow;
            double totalStageMilliSeconds = duration.TotalMilliseconds;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                double elapsedMilliSeconds = (DateTime.UtcNow - startTime).TotalMilliseconds;
                int percentage = (int)Math.Min(100, (elapsedMilliSeconds / totalStageMilliSeconds) * 100);
                progress?.Report(percentage);
                if (elapsedMilliSeconds >= totalStageMilliSeconds)
                {
                    break;
                }
                await Task.Delay(TotalIntervalMillisecond, cancellationToken);
            }
        }
    }
}