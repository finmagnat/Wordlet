using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IInternetConnectionService
    {
        bool IsOnline { get; }
        event Action<bool> OnlineChanged;
        UniTask InitializeAsync();
        void StartMonitoring();
        void StopMonitoring();
        UniTask<bool> CheckNowAsync(); // для кнопки "Проверить"
        UniTask WaitUntilOnlineAsync(CancellationToken ct = default);
    }
}