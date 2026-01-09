using WheelOfFortune.Data.Rewards;
using WheelOfFortune.Infrastructure.Interfaces;

namespace WheelOfFortune.Core.Services
{
    public sealed class CheatService : ICheatService
    {
        private const int INVALID_SLICE_INDEX = -1;
        private const int MAX_SLICE_COUNT = 16;

        private readonly IWheelGameService _gameService;
        private readonly ISpinRandomizer _spinRandomizer;
        private readonly System.Random _random;
        private readonly int[] _cachedNonBombIndices;

        public CheatService(IWheelGameService gameService, ISpinRandomizer spinRandomizer)
        {
            _gameService = gameService;
            _spinRandomizer = spinRandomizer;
            _random = new System.Random();
            _cachedNonBombIndices = new int[MAX_SLICE_COUNT];
        }

        public void ForceNextSlice(int sliceIndex)
        {
            _gameService?.ForceNextSlice(sliceIndex);
        }

        public void ClearForcedSlice()
        {
            _gameService?.ClearForcedSlice();
        }

        public bool TryForceBombSlice()
        {
            WheelSliceConfig[] slices = _gameService?.GetWorkingSlices();

            if (slices == null || slices.Length == 0)
            {
                return false;
            }

            int bombSliceIndex = FindBombSliceIndex(slices);

            if (bombSliceIndex < 0)
            {
                return false;
            }

            if (bombSliceIndex >= slices.Length)
            {
                return false;
            }

            _gameService.ForceNextSlice(bombSliceIndex);
            return true;
        }

        public bool TryForceRandomNonBombSlice()
        {
            WheelSliceConfig[] slices = _gameService?.GetWorkingSlices();

            if (slices == null || slices.Length == 0)
            {
                return false;
            }

            int nonBombSliceIndex = GetRandomNonBombSliceIndex(slices);

            if (nonBombSliceIndex < 0)
            {
                return false;
            }

            if (nonBombSliceIndex >= slices.Length)
            {
                return false;
            }

            if (slices[nonBombSliceIndex].IsBomb)
            {
                return false;
            }

            _gameService.ForceNextSlice(nonBombSliceIndex);
            return true;
        }

        public void SetBombChance(float chance)
        {
            _spinRandomizer?.SetBombChance(chance);
        }

        public void SetSeed(int seed)
        {
            _spinRandomizer?.SetSeed(seed);
        }

        public void UseRandomSeed()
        {
            _spinRandomizer?.UseRandomSeed();
        }

        private static int FindBombSliceIndex(WheelSliceConfig[] slices)
        {
            for (int index = 0; index < slices.Length; index++)
            {
                if (slices[index].IsBomb)
                {
                    return index;
                }
            }

            return INVALID_SLICE_INDEX;
        }

        private int GetRandomNonBombSliceIndex(WheelSliceConfig[] slices)
        {
            if (slices == null || slices.Length == 0)
            {
                return INVALID_SLICE_INDEX;
            }

            int nonBombCount = 0;
            int sliceCount = slices.Length < MAX_SLICE_COUNT ? slices.Length : MAX_SLICE_COUNT;

            for (int index = 0; index < sliceCount; index++)
            {
                if (!slices[index].IsBomb)
                {
                    _cachedNonBombIndices[nonBombCount] = index;
                    nonBombCount++;
                }
            }

            if (nonBombCount <= 0)
            {
                return INVALID_SLICE_INDEX;
            }

            int randomIndex = _random.Next(0, nonBombCount);
            int selectedSliceIndex = _cachedNonBombIndices[randomIndex];

            if (selectedSliceIndex < 0 || selectedSliceIndex >= slices.Length || slices[selectedSliceIndex].IsBomb)
            {
                return INVALID_SLICE_INDEX;
            }

            return selectedSliceIndex;
        }
    }
}
