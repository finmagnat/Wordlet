using System.ComponentModel;
using Core.DebugTools;

public partial class SROptions
{
    [Category("Dev/New Words")]
    [DisplayName("Language"), Sort(1)]
    public DebugLanguage Language
    {
        get => SRDebugNewWordsBridge.Instance != null
            ? DebugLanguageCode.SelectedLanguage
            : DebugLanguage.Ru;
        set
        {
            if (SRDebugNewWordsBridge.Instance == null) return;
            DebugLanguageCode.SelectedLanguage = value;
        }
    }

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
}