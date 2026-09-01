using System.ComponentModel;
using Core.Config;
using Core.DebugTools;
using UnityEngine;

public partial class SROptions
{
    [Category("Dev/Game")]
    [DisplayName("Настройка времени хода"), NumberRange(GameConfig.MinDurationGameSeconds, int.MaxValue), Increment(1), Sort(1)]
    public int DurationGameSeconds
    {
        get
        {
            if (SRDebugGameSettingsBridge.Instance != null)
                return SRDebugGameSettingsBridge.Instance.DurationGameSeconds;

            int duration = PlayerPrefs.GetInt(
                PlayerPrefsKey.DurationGame,
                GameDurationSettings.MinDurationGameSeconds);

            return GameDurationSettings.ClampDurationGameSeconds(duration);
        }
        set
        {
            if (SRDebugGameSettingsBridge.Instance != null)
                SRDebugGameSettingsBridge.Instance.DurationGameSeconds = value;
            else
                GameDurationSettings.SetDurationGameSeconds(value);
        }
    }
    
    [Category("Dev/Game")]
    [DisplayName("Включить автоматическую победу"), Sort(2)]
    public bool IsAutoWin
    {
        get
        {
            return GameDebug.IsAutoWin;
        }
        set
        {
            GameDebug.IsAutoWin = value;
        }
    }
}
