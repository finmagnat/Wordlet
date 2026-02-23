using System;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface INoInternetPopup
    {
        void ShowBlocking(Action onCheckClicked); // показать, кнопка "Проверить" дергает onCheckClicked
        UniTask Hide();
    }
}