using System;
using System.Threading;
using Core.Generated;
using Core.UI;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Services
{
    public sealed class InternetConnectionService : IInternetConnectionService, IDisposable
    {
        public bool IsOnline { get; private set; }
        
        public event Action<bool> OnlineChanged;

        private readonly IUIManager _uiManager;
        private readonly IGamePauseService _pauseService;

        private readonly object _pauseToken = new object();

        private CancellationTokenSource _cts;
        private bool _monitoring;

        private bool _popupShown;
        private NoInternetPopup _popupInstance;

        // защита от гонок
        private readonly SemaphoreSlim _stateGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _checkGate = new SemaphoreSlim(1, 1);

        private const string ProbeUrl = "https://clients3.google.com/generate_204";
        private readonly int _probeTimeoutSeconds = 3;
        private readonly float _pollIntervalSeconds = 1.0f;

        public InternetConnectionService(IUIManager uiManager, IGamePauseService pauseService)
        {
            _uiManager = uiManager;
            _pauseService = pauseService;
        }

        public UniTask InitializeAsync()
        {
            StartMonitoring();
            return UniTask.CompletedTask;
        }

        public void StartMonitoring()
        {
            if (_monitoring) return;
            _monitoring = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            MonitorLoopAsync(_cts.Token).Forget();
        }

        public void StopMonitoring()
        {
            _monitoring = false;
            _cts?.Cancel();
        }

        public void Dispose()
        {
            StopMonitoring();
            _cts?.Dispose();
            _cts = null;
        }

        public async UniTask<bool> CheckNowAsync()
        {
            return await EvaluateConnectionAndApplyAsync(_cts?.Token ?? default);
        }
        
        public async UniTask WaitUntilOnlineAsync(CancellationToken ct = default)
        {
            // Быстро: если уже онлайн — выходим
            if (IsOnline) return;

            // Иначе ждём. Периодически перепроверяем (или можно через CompletionSource)
            while (!ct.IsCancellationRequested)
            {
                await CheckNowAsync();
                if (IsOnline) return;
                await UniTask.Delay(300, cancellationToken: ct);
            }

            ct.ThrowIfCancellationRequested();
        }

        private async UniTaskVoid MonitorLoopAsync(CancellationToken ct)
        {
            await EvaluateConnectionAndApplyAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_pollIntervalSeconds), cancellationToken: ct);
                await EvaluateConnectionAndApplyAsync(ct);
            }
        }

        private async UniTask<bool> EvaluateConnectionAndApplyAsync(CancellationToken ct)
        {
            // не даём параллельные проверки (poll + кнопка)
            await _checkGate.WaitAsync(ct);
            bool online;
            try
            {
                online = await HasInternetAsync(ct);
            }
            finally
            {
                _checkGate.Release();
            }

            if (online == IsOnline)
            {
                // если оффлайн и попап ещё не успели показать из-за гонки — дожмём
                if (!online)
                    await ApplyOfflineAsync();
                return IsOnline;
            }

            if (IsOnline != online)
            {
                IsOnline = online;
                OnlineChanged?.Invoke(IsOnline);
            }

            if (!IsOnline)
                await ApplyOfflineAsync();
            else
                await ApplyOnlineAsync();

            return IsOnline;
        }

        private async UniTask ApplyOfflineAsync()
        {
            await _stateGate.WaitAsync();
            try
            {
                if (!_pauseService.IsPaused)
                    _pauseService.PushPause(_pauseToken);

                if (_popupShown)
                    return;

                _popupShown = true;

                _popupInstance = await _uiManager.ShowPopupAsync<NoInternetPopup>(AssetKey.NoInternetPopup);
                _popupInstance.ShowBlocking(OnCheckClicked);
            }
            finally
            {
                _stateGate.Release();
            }
        }

        private async UniTask ApplyOnlineAsync()
        {
            await _stateGate.WaitAsync();
            try
            {
                _pauseService.PopPause(_pauseToken);

                if (!_popupShown)
                    return;

                _popupShown = false;

                // если у UIManager есть HidePopupAsync<T>() возвращающий UniTask — ждём
                await _uiManager.HidePopupAsync<NoInternetPopup>();

                _popupInstance = null;
            }
            finally
            {
                _stateGate.Release();
            }
        }

        private void OnCheckClicked()
        {
            CheckNowAsync().Forget();
        }

        private async UniTask<bool> HasInternetAsync(CancellationToken ct)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            try
            {
                using var req = UnityWebRequest.Get(ProbeUrl);
                req.timeout = _probeTimeoutSeconds;

                await req.SendWebRequest().WithCancellation(ct);

                return req.result == UnityWebRequest.Result.Success
                       && (req.responseCode == 204 || req.responseCode == 200);
            }
            catch
            {
                return false;
            }
        }
    }
}