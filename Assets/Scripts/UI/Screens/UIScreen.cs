using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Screens
{
    public abstract class UIScreen : MonoBehaviour
    {
        public virtual UniTask ShowAsync()
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public virtual UniTask HideAsync()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }

    public abstract class UIScreen<TPayload> : UIScreen, IUIElement<TPayload>
    {
        public abstract UniTask PrepareAsync(TPayload payload);
    }
}