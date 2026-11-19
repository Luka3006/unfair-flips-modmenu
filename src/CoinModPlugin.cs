using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace UnfairFlipsCoinMod
{
    /// <summary>
    /// Lets the player set the heads chance and the flip duration of Unfair Flips.
    ///
    /// The game never saves these two values; it only saves shop levels and re-applies
    /// them on load. The mod therefore only overrides the runtime fields on CoinFlip and
    /// tracks the game's own value alongside (including shop purchases), so that turning
    /// an override off restores exactly what the game would have had.
    /// </summary>
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public sealed class CoinModPlugin : BaseUnityPlugin
    {
        private const float MinDuration = 0.1f;
        private const float MaxDuration = 10f;
        private const float CoinSearchInterval = 0.5f;
        private const int WindowId = 0x0C01F11B;

        private static readonly FieldInfo IsFlippingField =
            typeof(CoinFlip).GetField("isFlipping", BindingFlags.Instance | BindingFlags.NonPublic);

        // Config
        private ConfigEntry<bool> _overrideHeads;
        private ConfigEntry<float> _headsPercent;
        private ConfigEntry<bool> _overrideDuration;
        private ConfigEntry<float> _flipDuration;
        private ConfigEntry<bool> _preventEnding;
        private ConfigEntry<KeyboardShortcut> _menuKey;
        private ConfigEntry<bool> _showMenuOnStart;

        // Runtime state
        private readonly OverriddenValue _heads = new OverriddenValue();
        private readonly OverriddenValue _duration = new OverriddenValue();
        private CoinFlip _coin;
        private float _nextCoinSearch;
        private bool _showMenu;
        private bool _configDirty;
        private bool _errorLogged;
        private Rect _windowRect = new Rect(20f, 20f, 380f, 0f);
        private GUIStyle _warningStyle;
        private bool _showEndings;
        private int _pendingEnding = -1;

        private void Awake()
        {
            // Sliders change values every frame, so save manually instead of on every set.
            Config.SaveOnConfigSet = false;

            _overrideHeads = Config.Bind("Heads Chance", "Enabled", true,
                "Let the mod control the heads chance.");
            _headsPercent = Config.Bind("Heads Chance", "Percent", 50f,
                new ConfigDescription("Heads chance in percent. Game default: 20 (+5 per shop upgrade).",
                    new AcceptableValueRange<float>(0f, 100f)));

            _overrideDuration = Config.Bind("Flip Speed", "Enabled", true,
                "Let the mod control the flip duration.");
            _flipDuration = Config.Bind("Flip Speed", "Duration (seconds)", 1f,
                new ConfigDescription("How long one flip takes. Lower is faster. Game default: 5 (-0.2 per shop upgrade).",
                    new AcceptableValueRange<float>(MinDuration, MaxDuration)));

            _preventEnding = Config.Bind("Extras", "Prevent ending", false,
                "Getting 10 heads in a row ends the game and WIPES YOUR SAVE. " +
                "When enabled, the heads streak is capped at 9 so the ending never triggers.");

            _menuKey = Config.Bind("Menu", "Toggle key", new KeyboardShortcut(KeyCode.F1),
                "Key that shows or hides the mod menu.");
            _showMenuOnStart = Config.Bind("Menu", "Show on start", true,
                "Show the mod menu when the game starts.");

            Config.Save();
            _showMenu = _showMenuOnStart.Value;

            if (IsFlippingField == null)
                Logger.LogWarning("Field CoinFlip.isFlipping not found (game update?). Flip duration changes will apply immediately instead of after the current flip.");

            Logger.LogInfo($"{PluginInfo.Name} {PluginInfo.Version} loaded. Menu key: {_menuKey.Value}");
        }

        private void Update()
        {
            if (_menuKey.Value.IsDown())
            {
                _showMenu = !_showMenu;
                if (!_showMenu) SaveConfigIfDirty();
            }

            try
            {
                ApplyOverrides();
            }
            catch (Exception e)
            {
                // Never spam the log every frame if a game update breaks something.
                if (!_errorLogged)
                {
                    Logger.LogError($"Failed to apply overrides: {e}");
                    _errorLogged = true;
                }
            }
        }

        private void ApplyOverrides()
        {
            if (_coin == null)
            {
                if (Time.unscaledTime < _nextCoinSearch) return;
                _nextCoinSearch = Time.unscaledTime + CoinSearchInterval;

                _coin = FindObjectOfType<CoinFlip>();
                _heads.Reset();
                _duration.Reset();
                if (_coin == null) return;
            }

            bool flipping = IsFlipping();

            // The heads roll happens once at the start of a flip, so this is safe to set any time.
            _heads.Sync(_overrideHeads.Value, PercentToChance(_headsPercent.Value),
                () => _coin.flipHeadsChance, v => _coin.flipHeadsChance = v);

            // The duration is read every frame during a flip, so only change it between flips.
            if (!flipping)
            {
                _duration.Sync(_overrideDuration.Value, Mathf.Clamp(_flipDuration.Value, MinDuration, MaxDuration),
                    () => _coin.flipDuration, v => _coin.flipDuration = v);
            }

            // The next heads brings the streak back to 9, but never to 10.
            if (_preventEnding.Value && !flipping && _coin.GetHeadsStreak() >= 9)
                _coin.SetHeadsStreak(8);
        }

        /// <summary>
        /// The game shows the chance as (int)(chance * 100), which truncates values like
        /// 0.29f to 28%. A tiny nudge makes the in-game counter match the chosen percentage.
        /// </summary>
        private static float PercentToChance(float percent)
        {
            if (percent <= 0f) return 0f;
            if (percent >= 100f) return 1f;
            return percent / 100f + 0.00001f;
        }

        private bool IsFlipping()
        {
            return IsFlippingField != null && _coin != null && (bool)IsFlippingField.GetValue(_coin);
        }

        private void SaveConfigIfDirty()
        {
            if (!_configDirty) return;
            Config.Save();
            _configDirty = false;
        }

        private void OnApplicationQuit() => SaveConfigIfDirty();

        private void OnDestroy() => SaveConfigIfDirty();

        // ------------------------------------------------------------------ GUI

        private void OnGUI()
        {
            if (!_showMenu) return;

            float scale = Mathf.Max(1f, Screen.height / 1080f);
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            _windowRect = GUILayout.Window(WindowId, _windowRect, DrawWindow,
                $"{PluginInfo.Name} v{PluginInfo.Version}  ({_menuKey.Value} to hide)");

            // Keep the window on screen.
            float maxX = Screen.width / scale - _windowRect.width;
            float maxY = Screen.height / scale - _windowRect.height;
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, Mathf.Max(0f, maxX));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, Mathf.Max(0f, maxY));

            GUI.matrix = oldMatrix;
        }

        private void DrawWindow(int id)
        {
            GUILayout.Space(4f);

            // Heads chance
            SetIfChanged(_overrideHeads, GUILayout.Toggle(_overrideHeads.Value, " Custom heads chance"));
            GUI.enabled = _overrideHeads.Value;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{_headsPercent.Value:0}%", GUILayout.Width(50f));
            float heads = Mathf.Round(GUILayout.HorizontalSlider(_headsPercent.Value, 0f, 100f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1")) heads -= 1f;
            if (GUILayout.Button("+1")) heads += 1f;
            if (GUILayout.Button("20%")) heads = 20f;
            if (GUILayout.Button("50%")) heads = 50f;
            if (GUILayout.Button("100%")) heads = 100f;
            GUILayout.EndHorizontal();

            SetIfChanged(_headsPercent, Mathf.Clamp(heads, 0f, 100f));
            GUI.enabled = true;

            GUILayout.Space(10f);

            // Flip speed
            SetIfChanged(_overrideDuration, GUILayout.Toggle(_overrideDuration.Value, " Custom flip duration"));
            GUI.enabled = _overrideDuration.Value;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{_flipDuration.Value:0.0} s", GUILayout.Width(50f));
            float duration = RoundToTenth(GUILayout.HorizontalSlider(_flipDuration.Value, MinDuration, MaxDuration));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-0.1")) duration -= 0.1f;
            if (GUILayout.Button("+0.1")) duration += 0.1f;
            if (GUILayout.Button("0.2 s")) duration = 0.2f;
            if (GUILayout.Button("1 s")) duration = 1f;
            if (GUILayout.Button("5 s")) duration = 5f;
            GUILayout.EndHorizontal();

            SetIfChanged(_flipDuration, Mathf.Clamp(RoundToTenth(duration), MinDuration, MaxDuration));
            GUI.enabled = true;

            GUILayout.Space(10f);

            // Extras
            SetIfChanged(_preventEnding, GUILayout.Toggle(_preventEnding.Value, " Prevent ending (cap streak at 9)"));
            if (!_preventEnding.Value && _overrideHeads.Value && _headsPercent.Value >= 70f)
                GUILayout.Label("<color=#ffcc66>Warning: 10 heads in a row ends the game and wipes your save!</color>", WarningStyle());

            GUILayout.Space(10f);

            DrawEndings();

            GUILayout.Space(10f);

            // Status
            if (_coin != null)
            {
                string gameHeads = _heads.Active ? $"  (game: {_heads.Natural * 100f:0}%)" : string.Empty;
                string gameDuration = _duration.Active ? $"  (game: {_duration.Natural:0.0} s)" : string.Empty;
                GUILayout.Label($"Heads chance: {_coin.flipHeadsChance * 100f:0}%{gameHeads}");
                GUILayout.Label($"Flip duration: {_coin.flipDuration:0.0} s{gameDuration}");
                GUILayout.Label($"Heads streak: {_coin.GetHeadsStreak()}");
            }
            else
            {
                GUILayout.Label("Waiting for the coin...");
            }

            GUI.DragWindow();
        }

        private void DrawEndings()
        {
            _showEndings = GUILayout.Toggle(_showEndings, _showEndings ? " [-] Play ending" : " [+] Play ending");
            if (!_showEndings) return;

            bool canPlay = _coin != null && !IsFlipping();

            if (_pendingEnding < 0)
            {
                if (!canPlay)
                    GUILayout.Label("Available when the coin is not flipping.");

                GUI.enabled = canPlay;
                for (int i = 0; i < EndingTrigger.Endings.Length; i++)
                {
                    if (GUILayout.Button(EndingTrigger.Endings[i].Label))
                        _pendingEnding = i;
                }
                GUI.enabled = true;
                return;
            }

            // Confirmation step: every ending wipes the save, exactly like in the normal game.
            GUILayout.Label($"<color=#ffcc66>Play \"{EndingTrigger.Endings[_pendingEnding].Label}\"? " +
                            "This ends the run and WIPES YOUR SAVE (money, upgrades, flips).</color>", WarningStyle());
            GUILayout.BeginHorizontal();
            GUI.enabled = canPlay;
            if (GUILayout.Button("Yes, play it"))
            {
                int index = _pendingEnding;
                _pendingEnding = -1;
                PlayEnding(index);
            }
            GUI.enabled = true;
            if (GUILayout.Button("Cancel"))
                _pendingEnding = -1;
            GUILayout.EndHorizontal();
        }

        private void PlayEnding(int index)
        {
            EndingTrigger.Ending ending = EndingTrigger.Endings[index];
            string error;
            try
            {
                error = EndingTrigger.Play(_coin, index);
            }
            catch (Exception e)
            {
                error = e.ToString();
            }

            if (error != null)
            {
                Logger.LogError($"Could not play ending \"{ending.Label}\": {error}");
                return;
            }

            Logger.LogInfo($"Playing ending \"{ending.Label}\" ({ending.Achievement}).");
            _showMenu = false; // get out of the way of the cutscene
            SaveConfigIfDirty();
        }

        private GUIStyle WarningStyle()
        {
            if (_warningStyle == null)
                _warningStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            return _warningStyle;
        }

        private static float RoundToTenth(float value) => Mathf.Round(value * 10f) / 10f;

        private void SetIfChanged(ConfigEntry<bool> entry, bool value)
        {
            if (entry.Value == value) return;
            entry.Value = value;
            _configDirty = true;
        }

        private void SetIfChanged(ConfigEntry<float> entry, float value)
        {
            if (Mathf.Approximately(entry.Value, value)) return;
            entry.Value = value;
            _configDirty = true;
        }
    }
}
