using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Popups
{
    public abstract class UIPopup : MonoBehaviour
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
}