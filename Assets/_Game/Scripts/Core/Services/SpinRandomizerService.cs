using UnityEngine;
using WheelOfFortune.Data.Rewards;
using WheelOfFortune.Infrastructure.Interfaces;
using Random = System.Random;

namespace WheelOfFortune.Core.Services
{
    public sealed class SpinRandomizerService : ISpinRandomizer
    {
        private const int INVALID_SLICE_INDEX = -1;
        private const float MINIMUM_CHANCE = 0.0f;
        private const float MAXIMUM_CHANCE = 1.0f;
        private const int MAX_SLICE_COUNT = 16;

        private readonly float _baseBombChance;
        private readonly float _bombChanceIncrement;
        private readonly float _maximumBombChance;
        private readonly int _incrementIntervalZones;
        private readonly int[] _cachedNonBombIndices;

        private Random _random;
        private float _runtimeBombChance;
        private float _currentBombChance;
        private bool _hasForcedSlice;
        private bool _hasRuntimeBombChance;
        private int _forcedSliceIndex;
        private int _cachedNonBombCount;
        private int _cachedBombIndex;

        public SpinRandomizerService(float baseBombChance, float bombChanceIncrement, float maximumBombChance,
            int incrementIntervalZones, bool useCustomSeed, int customSeed)
        {
            _baseBombChance = Mathf.Clamp(baseBombChance, MINIMUM_CHANCE, MAXIMUM_CHANCE);
            _bombChanceIncrement = Mathf.Clamp(bombChanceIncrement, MINIMUM_CHANCE, MAXIMUM_CHANCE);
            _maximumBombChance = Mathf.Clamp(maximumBombChance, MINIMUM_CHANCE, MAXIMUM_CHANCE);
            _incrementIntervalZones = incrementIntervalZones;
            _cachedNonBombIndices = new int[MAX_SLICE_COUNT];
            _cachedNonBombCount = 0;
            _cachedBombIndex = INVALID_SLICE_INDEX;

            if (useCustomSeed)
            {
                SetSeed(customSeed);
            }
            else
            {
                UseRandomSeed();
            }
        }

        public void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        public void UseRandomSeed()
        {
            _random = new Random();
        }

        public int ResolveSliceIndex(WheelSliceConfig[] slices, int currentZoneIndex)
        {
            if (slices == null || slices.Length == 0)
            {
                return 0;
            }

            if (_hasForcedSlice)
            {
                int forcedIndex = Mathf.Clamp(_forcedSliceIndex, 0, slices.Length - 1);
                _hasForcedSlice = false;
                return forcedIndex;
            }

            CacheSliceIndices(slices);

            if (_cachedBombIndex < 0)
            {
                return _random.Next(0, slices.Length);
            }

            float bombChance = GetBombChance(currentZoneIndex);
            double roll = _random.NextDouble();

            if (roll < bombChance)
            {
                return _cachedBombIndex;
            }

            if (_cachedNonBombCount <= 0)
            {
                return _random.Next(0, slices.Length);
            }

            int randomNonBombIndex = _random.Next(0, _cachedNonBombCount);
            return _cachedNonBombIndices[randomNonBombIndex];
        }

        private void CacheSliceIndices(WheelSliceConfig[] slices)
        {
            _cachedNonBombCount = 0;
            _cachedBombIndex = INVALID_SLICE_INDEX;

            if (slices == null)
            {
                return;
            }

            int sliceCount = Mathf.Min(slices.Length, MAX_SLICE_COUNT);

            for (int index = 0; index < sliceCount; index++)
            {
                if (slices[index].IsBomb)
                {
                    _cachedBombIndex = index;
                }
                else
                {
                    _cachedNonBombIndices[_cachedNonBombCount] = index;
                    _cachedNonBombCount++;
                }
            }
        }

        public void ForceNextSlice(int sliceIndex)
        {
            if (sliceIndex < 0)
            {
                ClearForcedSlice();
                return;
            }

            _forcedSliceIndex = sliceIndex;
            _hasForcedSlice = true;
        }

        public void ClearForcedSlice()
        {
            _forcedSliceIndex = INVALID_SLICE_INDEX;
            _hasForcedSlice = false;
        }

        public void SetBombChance(float chance)
        {
            float clampedChance = Mathf.Clamp(chance, MINIMUM_CHANCE, MAXIMUM_CHANCE);
            _runtimeBombChance = clampedChance;
            _currentBombChance = clampedChance;
            _hasRuntimeBombChance = true;
        }

        private float GetBombChance(int currentZoneIndex)
        {
            if (_hasRuntimeBombChance)
            {
                _currentBombChance = _runtimeBombChance;
                return _currentBombChance;
            }

            if (currentZoneIndex <= 1)
            {
                _currentBombChance = MINIMUM_CHANCE;
                return _currentBombChance;
            }

            int interval = Mathf.Max(1, _incrementIntervalZones);
            int zoneIndex = Mathf.Max(1, currentZoneIndex);
            int increments = (zoneIndex - 1) / interval;
            float progressiveChance = _baseBombChance + (increments * _bombChanceIncrement);
            _currentBombChance = Mathf.Clamp(progressiveChance, _baseBombChance, _maximumBombChance);

            return _currentBombChance;
        }

    }
}
