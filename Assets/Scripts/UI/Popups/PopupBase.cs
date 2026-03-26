using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public abstract class PopupBase<TPayload> : UIPopup<TPayload>
    {
        public override UniTask PrepareAsync(TPayload data)
        {
            return UniTask.CompletedTask;
        }
    }
}