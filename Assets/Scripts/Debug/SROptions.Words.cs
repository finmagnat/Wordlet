using System.ComponentModel;
using Core.DebugTools;
using Core.Services.ReportWords;

public partial class SROptions
{
    [Category("Dev/New & Report Words")]
    [DisplayName("Language"), Sort(1)]
    public DebugLanguage Language
    {
        get => DebugLanguageCode.SelectedLanguage;
        set => DebugLanguageCode.SelectedLanguage = value;
    }

    //------------ [Category("Dev/New Words")]
    [Category("Dev/New Words")]
    [DisplayName("Word To Add"), Sort(2)]
    public string NewWordToAdd
    {
        get => SRDebugNewWordsBridge.Instance != null
            ? SRDebugNewWordsBridge.Instance.WordToAdd
            : string.Empty;
        set
        {
            if (SRDebugNewWordsBridge.Instance == null) return;
            SRDebugNewWordsBridge.Instance.WordToAdd = value;
        }
    }

    [Category("Dev/New Words")]
    [DisplayName("Add Pending Word"), Sort(3)]
    public void AddPendingWord()
    {
        SRDebugNewWordsBridge.Instance?.AddPendingWord();
    }

    [Category("Dev/New Words")]
    [DisplayName("Get Pending Words To Clipboard"), Sort(4)]
    public void GetPendingWordsToClipboard()
    {
        SRDebugNewWordsBridge.Instance?.DumpPendingWordsToClipboard();
    }

    [Category("Dev/New Words")]
    [DisplayName("Clear Pending Words"), Sort(5)]
    public void ClearPendingWords()
    {
        SRDebugNewWordsBridge.Instance?.ClearAllPendingWords();
    }
    
    //------------ [Category("Dev/Report Words")]
    [Category("Dev/Report Words")]
    [DisplayName("Word To Add"), Sort(6)]
    public string ReportWordToAdd
    {
        get => SRDebugReportWordBridge.Instance != null
            ? SRDebugReportWordBridge.Instance.WordToAdd
            : string.Empty;
        set
        {
            if (SRDebugReportWordBridge.Instance == null) return;
            SRDebugReportWordBridge.Instance.WordToAdd = value;
        }
    }
    
    [Category("Dev/Report Words")]
    [DisplayName("Report Reason"), Sort(7)]
    public ReportReason ReportReason
    {
        get => SRDebugReportWordBridge.Instance != null
            ? SRDebugReportWordBridge.Instance.Reason
            : ReportReason.None;
        set
        {
            if (SRDebugReportWordBridge.Instance == null) return;
            SRDebugReportWordBridge.Instance.Reason = value;
        }
    }
    
    [Category("Dev/Report Words")]
    [DisplayName("Add Report Word"), Sort(8)]
    public void AddReportWord()
    {
        SRDebugReportWordBridge.Instance?.AddReportWord();
    }

    [Category("Dev/Report Words")]
    [DisplayName("Get Report Words To Clipboard"), Sort(9)]
    public void GetReportWordsToClipboard()
    {
        SRDebugReportWordBridge.Instance?.DumpReportWordsToClipboard();
    }

    [Category("Dev/Report Words")]
    [DisplayName("Clear Report Words"), Sort(10)]
    public void ClearReportWords()
    {
        SRDebugReportWordBridge.Instance?.ClearAllReportWords();
    }
}