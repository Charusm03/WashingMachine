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

        public async Task RunAsync(
            TimeSpan duration,
            IProgress<int> progress,
            SemaphoreSlim pauseGate,
            CancellationToken cancellationToken)
        {
            if (duration <= TimeSpan.Zero)
            {
                progress?.Report(100);
                return;
            }

            DateTime startTime = DateTime.UtcNow;
            TimeSpan accumulatedPauseTime = TimeSpan.Zero;
            double totalStageMilliSeconds = duration.TotalMilliseconds;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Acquire-then-immediately-release: this is not a critical section,
                // it's a checkpoint. If the gate is closed (paused), WaitAsync suspends
                // right here — mid-stage, mid-tick — until Resume() reopens it.
                DateTime pauseStart = DateTime.UtcNow;
                await pauseGate.WaitAsync(cancellationToken);
                pauseGate.Release();
                accumulatedPauseTime += DateTime.UtcNow - pauseStart;

                double elapsedMilliSeconds = (DateTime.UtcNow - startTime - accumulatedPauseTime).TotalMilliseconds;
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