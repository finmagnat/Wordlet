using Core.Data;
using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public class NoAdsPopup : MessagePopup<MessageBoxData>
    {
        public override UniTask PrepareAsync(MessageBoxData data)
        {
            SetWindowData(data);
            return UniTask.CompletedTask;
        }
    }
}