using System;
using System.Collections.Generic;
using System.Text;
using WashingMachine.Models.Enums;

namespace WashingMachine.Models
{
    internal class WashingConfiguration
    {
        private static readonly Dictionary<WashingMode, TimeSpan> BaseDurations = new()
        {
            [WashingMode.Soak] = TimeSpan.FromSeconds(4),
            [WashingMode.Wash] = TimeSpan.FromSeconds(6),
            [WashingMode.Rinse] = TimeSpan.FromSeconds(4),
            [WashingMode.Spin] = TimeSpan.FromSeconds(3),
            [WashingMode.Dry] = TimeSpan.FromSeconds(5),
        };
        private static readonly Dictionary<WashingPattern, double> PatternMultipliers = new()
        {
            [WashingPattern.Quick] = 0.5,
            [WashingPattern.Standard] = 1.0,
            [WashingPattern.Heavy] = 1.5,
            [WashingPattern.Delicate] = 0.8,
        };

        private static readonly Dictionary<ClothType, double> ClothMultipliers = new()
        {
            [ClothType.Cotton] = 1.0,
            [ClothType.Synthetic] = 0.8,
            [ClothType.Wool] = 0.6,
            [ClothType.Denim] = 1.3,
        };
        public TimeSpan GetDuration(WashingPattern pattern, WashingMode mode, ClothType type)
        {
            if(!BaseDurations.TryGetValue(mode,out var baseDuration))
            {
                throw new KeyNotFoundException($"No base duration configured for {mode}.");
            }
            if(!PatternMultipliers.TryGetValue(pattern,out double patternFactor))
            {
                throw new KeyNotFoundException($"No multiplier configured for pattern {pattern}.");
            }
            if(!ClothMultipliers.TryGetValue(type,out double clothFactor))
            {
                throw new KeyNotFoundException($"No multiplier configured for cloth {type}.");
            }
            double totalSeconds = baseDuration.TotalSeconds * patternFactor * clothFactor;
            return TimeSpan.FromSeconds(totalSeconds);
        }
    }
    internal class StageSettings
    {
        public TimeSpan Duration { get; set; }
        public int WaterLevel { get; set; }
        public int Temperature { get; set; }
        public int SpinSpeed { get; set; }
    }
}
