using System.ComponentModel;
using Core.DebugTools;

public partial class SROptions
{
    [Category("Dev/Daily Bonus")]
    [DisplayName("Day"), NumberRange(1, 7), Increment(1), Sort(1)]
    public int DailyBonusDay
    {
        get => SRDebugDailyBonusBridge.Instance != null
            ? SRDebugDailyBonusBridge.Instance.Day
            : 1;
        set
        {
            if (SRDebugDailyBonusBridge.Instance == null) return;
            SRDebugDailyBonusBridge.Instance.Day = value;
        }
    }

    [Category("Dev/Daily Bonus")]
    [DisplayName("Last Result"), Sort(2)]
    public string DailyBonusLastResult => SRDebugDailyBonusBridge.Instance != null
        ? SRDebugDailyBonusBridge.Instance.LastResult
        : "Bridge is not ready";

    [Category("Dev/Daily Bonus")]
    [DisplayName("Set Active Day"), Sort(10)]
    public void SetDailyBonusActiveDay()
    {
        SRDebugDailyBonusBridge.Instance?.SetActiveDay();
    }

    [Category("Dev/Daily Bonus")]
    [DisplayName("Mark Claimed Today"), Sort(11)]
    public void MarkDailyBonusClaimedToday()
    {
        SRDebugDailyBonusBridge.Instance?.MarkClaimedToday();
    }

    [Category("Dev/Daily Bonus")]
    [DisplayName("Simulate Next Day Ready"), Sort(12)]
    public void SimulateDailyBonusNextDayReady()
    {
        SRDebugDailyBonusBridge.Instance?.SimulateNextDayReady();
    }

    [Category("Dev/Daily Bonus")]
    [DisplayName("Reset"), Sort(13)]
    public void ResetDailyBonus()
    {
        SRDebugDailyBonusBridge.Instance?.Reset();
    }

    [Category("Dev/Daily Bonus")]
    [DisplayName("Refresh"), Sort(14)]
    public void RefreshDailyBonus()
    {
        SRDebugDailyBonusBridge.Instance?.Refresh();
    }
}
