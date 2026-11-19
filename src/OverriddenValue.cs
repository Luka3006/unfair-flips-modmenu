using System;

namespace UnfairFlipsCoinMod
{
    /// <summary>
    /// Overrides a game value while tracking every change the game itself makes to it
    /// (e.g. shop upgrades), so the correct original value can be restored when the
    /// override is turned off.
    /// </summary>
    internal sealed class OverriddenValue
    {
        private float _applied;

        /// <summary>True while the mod is overriding the value.</summary>
        public bool Active { get; private set; }

        /// <summary>The value the game would have without the mod.</summary>
        public float Natural { get; private set; }

        /// <summary>Forget all tracking, e.g. when the game object was recreated.</summary>
        public void Reset() => Active = false;

        public void Sync(bool wantOverride, float target, Func<float> get, Action<float> set)
        {
            if (Active)
            {
                float current = get();
                if (current != _applied)
                    Natural += current - _applied; // the game changed the value since our last write
            }

            if (wantOverride)
            {
                if (!Active)
                {
                    Natural = get();
                    Active = true;
                }

                _applied = target;
                set(target);
            }
            else if (Active)
            {
                set(Natural);
                Active = false;
            }
        }
    }
}
