using System.Collections.Generic;

namespace WheelOfFortune.Infrastructure.Utilities
{
    public static class NumberStringCache
    {
        private const int PREWARMED_CACHE_SIZE = 1000;

        private static readonly string[] PrewarmedCache;
        private static readonly Dictionary<int, string> ExtendedCache;
        private static readonly object CacheLock = new();

        static NumberStringCache()
        {
            PrewarmedCache = new string[PREWARMED_CACHE_SIZE];
            ExtendedCache = new Dictionary<int, string>(64);

            for (int i = 0; i < PREWARMED_CACHE_SIZE; i++)
            {
                PrewarmedCache[i] = i.ToString();
            }
        }

        public static string Get(int value)
        {
            if (value >= 0 && value < PREWARMED_CACHE_SIZE)
            {
                return PrewarmedCache[value];
            }

            lock (CacheLock)
            {
                if (ExtendedCache.TryGetValue(value, out string cached))
                {
                    return cached;
                }

                string newValue = value.ToString();
                ExtendedCache[value] = newValue;
                return newValue;
            }
        }

        public static void PrewarmValues(params int[] values)
        {
            if (values == null)
            {
                return;
            }

            lock (CacheLock)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    int value = values[i];
                    if (value >= PREWARMED_CACHE_SIZE && !ExtendedCache.ContainsKey(value))
                    {
                        ExtendedCache[value] = value.ToString();
                    }
                }
            }
        }
    }
}
