using UnityEngine;

namespace UnfairFlipsCoinMod
{
    /// <summary>
    /// Plays a specific ending on demand.
    ///
    /// In CoinFlip.Flip() the ending is chosen right after the 10th heads in a row with
    /// <c>num = Random.Range(0f, 1f)</c>. The synchronous part of Flip() calls Unity's
    /// random generator exactly three times before that point:
    ///   1. Random.Range(3, 6)      flip spin multiplier
    ///   2. Random.Range(0f, 1f)    heads roll (forced to heads here)
    ///   3. Random.Range(0f, 1f)    ending selection
    /// (The flip sound would be a 4th call, but at streak 9 the game uses a fixed sound.)
    ///
    /// We search for a seed whose 3rd value falls into the wanted ending's range, set the
    /// heads streak to 9, force heads and start a normal flip. The game then plays the
    /// ending exactly as it would naturally, including the achievement.
    /// </summary>
    internal static class EndingTrigger
    {
        internal readonly struct Ending
        {
            public readonly string Label;
            public readonly string Achievement;
            public readonly float Min;
            public readonly float Max;

            public Ending(string label, string achievement, float min, float max)
            {
                Label = label;
                Achievement = achievement;
                Min = min;
                Max = max;
            }
        }

        /// <summary>Ranges as checked in CoinFlip.Flip() (num &lt; 0.2, &lt; 0.4, &lt; 0.6, &lt; 0.9, else).</summary>
        public static readonly Ending[] Endings =
        {
            new Ending("Roll away", "ENDING_ROLL", 0.0f, 0.2f),
            new Ending("Eggbug", "ENDING_EGGBUG", 0.2f, 0.4f),
            new Ending("Fly up", "ENDING_UP", 0.4f, 0.6f),
            new Ending("Explosion", "ENDING_BOOM", 0.6f, 0.9f),
            new Ending("10 heads celebration", "ENDING_10FLIP", 0.9f, 1.0f),
        };

        private const int MaxSeedSearch = 100000;
        private const float Margin = 0.01f; // stay clear of the range borders

        /// <summary>Returns an error message, or null on success.</summary>
        public static string Play(CoinFlip coin, int endingIndex)
        {
            if (coin == null) return "Coin not found.";
            if (endingIndex < 0 || endingIndex >= Endings.Length) return "Unknown ending.";

            Ending ending = Endings[endingIndex];
            Random.State originalState = Random.state;

            if (!TryFindSeed(ending, out int seed))
            {
                Random.state = originalState;
                return "No suitable random seed found.";
            }

            float originalChance = coin.flipHeadsChance;
            try
            {
                coin.SetHeadsStreak(9);
                coin.flipHeadsChance = 2f; // Random.Range(0f, 1f) < 2 is always heads
                Random.InitState(seed);
                coin.DoFlip(); // runs synchronously up to the ending selection
            }
            finally
            {
                coin.flipHeadsChance = originalChance;
                Random.state = originalState;
            }

            return null;
        }

        private static bool TryFindSeed(Ending ending, out int seed)
        {
            for (seed = 1; seed <= MaxSeedSearch; seed++)
            {
                Random.InitState(seed);
                Random.Range(3, 6);
                Random.Range(0f, 1f);
                float value = Random.Range(0f, 1f);
                if (value >= ending.Min + Margin && value < ending.Max - Margin)
                    return true;
            }

            seed = 0;
            return false;
        }
    }
}
