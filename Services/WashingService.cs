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
        internal WashingService(WashingConfiguration configurations)
        {
            _configurations = configurations;
        }
        internal event EventHandler<StagesEventArgs> StageStarted;
        internal event EventHandler<StagesEventArgs> StageCompleted;
        internal event EventHandler<ClothesAddedEventArgs> ClothesAdded;
        internal event EventHandler<WashingCompletedEventArgs> WashingCompleted;
        private readonly WashingConfiguration _configurations;
        TimerService _timerService = new TimerService();
        protected virtual void OnStageStarted(StagesEventArgs e)
        {
            StageStarted?.Invoke(this, e);
        }
        protected virtual void OnStageCompleted(StagesEventArgs e)
        {
            StageCompleted?.Invoke(this, e);
        }

        protected virtual void OnWashingCompleted(WashingCompletedEventArgs e)
        {
            WashingCompleted?.Invoke(this, e);
        }
        public MachineData StartWashing(WashingPattern pattern, ClothType cloth, List<WashingMode> methods, int load)
        {
            if (methods == null || methods.Count == 0)
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
            MachineData machineData = new MachineData
            {
                WashingPattern = pattern,
                ClothType = cloth,
                WashMethods = methods,
                CurrentMode = methods[0],
                Load = load,
                ElapsedTime = TimeSpan.Zero,
                Progress = 0,
                Status = MachineStatus.Idle,
                RemainingTime = totalTimeRemaining,
            };
            return machineData;
        }
        internal async Task RunWashingCycleAsync(MachineData machineData, CancellationToken cancellationToken)
        {
            machineData.Status = MachineStatus.Running;
            TimeSpan baselineRemainingTime = machineData.RemainingTime;
            TimeSpan totalCompletedStagesDuration = TimeSpan.Zero;
            foreach (var mode in machineData.WashMethods)
            {
                machineData.CurrentMode = mode;

                TimeSpan stageDuration = _configurations.GetDuration(machineData.WashingPattern, mode, machineData.ClothType);
                OnStageStarted(new StagesEventArgs(mode, stageDuration));

                var progress = new Progress<int>(percent =>
                {
                    lock (machineData.SyncRoot)
                    {
                        machineData.Progress = percent;
                        double currentStageElapsedMs = (percent / 100.0) * stageDuration.TotalMilliseconds;
                        TimeSpan totalTimeElapsedSoFar = totalCompletedStagesDuration + TimeSpan.FromMilliseconds(currentStageElapsedMs);

                        machineData.ElapsedTime = totalTimeElapsedSoFar;
                        machineData.RemainingTime = baselineRemainingTime - totalTimeElapsedSoFar;
                    }
                });

                await _timerService.RunAsync(stageDuration, progress, cancellationToken);
                totalCompletedStagesDuration += stageDuration;
                OnStageCompleted(new StagesEventArgs(mode, stageDuration));
            }

            machineData.Status = MachineStatus.Completed;
            machineData.RemainingTime = TimeSpan.Zero;
            machineData.Progress = 100;
            OnWashingCompleted(new WashingCompletedEventArgs(machineData));
        }
    }
}