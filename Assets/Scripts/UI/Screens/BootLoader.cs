using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootLoader : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField]
    private string mainMenuSceneName = "Main";

    [Header("Transition")]
    [SerializeField, Min(0f)]
    private float minimumSplashDuration = 0.5f;

    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.35f;

    [SerializeField]
    private CanvasGroup splashCanvasGroup;

    private bool _isLoading;

    private void Start()
    {
        LoadApplicationAsync(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid LoadApplicationAsync(
        System.Threading.CancellationToken cancellationToken)
    {
        if (_isLoading)
            return;

        _isLoading = true;

        splashCanvasGroup.alpha = 1f;
        splashCanvasGroup.blocksRaycasts = true;

        float startTime = Time.realtimeSinceStartup;

        try
        {
            // Здесь можно запустить только действительно обязательные операции.
            await InitializeCriticalSystemsAsync(cancellationToken);

            // Загружаем MainMenu, не уничтожая Boot-сцену.
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                mainMenuSceneName,
                LoadSceneMode.Additive);

            if (loadOperation == null)
                throw new InvalidOperationException(
                    $"Cannot load scene '{mainMenuSceneName}'.");

            await loadOperation.ToUniTask(
                cancellationToken: cancellationToken);

            Scene mainMenuScene =
                SceneManager.GetSceneByName(mainMenuSceneName);

            if (!mainMenuScene.IsValid() || !mainMenuScene.isLoaded)
                throw new InvalidOperationException(
                    $"Scene '{mainMenuSceneName}' was not loaded.");

            // Делаем MainMenu активной сценой.
            SceneManager.SetActiveScene(mainMenuScene);

            // Гарантируем минимальное время показа заставки.
            float elapsed = Time.realtimeSinceStartup - startTime;
            float remainingDuration =
                minimumSplashDuration - elapsed;

            if (remainingDuration > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(remainingDuration),
                    DelayType.UnscaledDeltaTime,
                    cancellationToken: cancellationToken);
            }

            /*
             * Даём MainMenu возможность пройти:
             * Awake → OnEnable → Start → LateUpdate
             * и отправить первый кадр на рендер.
             */
            await UniTask.Yield(
                PlayerLoopTiming.LastPostLateUpdate,
                cancellationToken);

            await UniTask.NextFrame(cancellationToken);

            // Теперь MainMenu уже находится под SplashCanvas.
            await FadeOutAsync(cancellationToken);

            splashCanvasGroup.blocksRaycasts = false;

            // После исчезновения заставки выгружаем Boot.
            Scene bootScene = gameObject.scene;

            AsyncOperation unloadOperation =
                SceneManager.UnloadSceneAsync(bootScene);

            if (unloadOperation != null)
            {
                await unloadOperation.ToUniTask(
                    cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Объект уничтожается — ничего делать не нужно.
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            /*
             * При ошибке оставляем заставку видимой.
             * Позже сюда можно добавить ErrorPopup или Retry.
             */
            splashCanvasGroup.alpha = 1f;
            splashCanvasGroup.blocksRaycasts = true;
            _isLoading = false;
        }
    }

    private async UniTask InitializeCriticalSystemsAsync(
        System.Threading.CancellationToken cancellationToken)
    {
        /*
         * Только минимально необходимая инициализация.
         *
         * Например:
         * - чтение локального сохранения;
         * - выбор языка;
         * - создание базового ServiceContainer.
         *
         * Не стоит здесь ждать:
         * - Remote Config;
         * - Analytics;
         * - Ads;
         * - авторизацию PlayFab;
         * - загрузку необязательных Addressables.
         */

        await UniTask.Yield(cancellationToken);
    }

    private async UniTask FadeOutAsync(
        System.Threading.CancellationToken cancellationToken)
    {
        float elapsed = 0f;
        float startAlpha = splashCanvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            cancellationToken.ThrowIfCancellationRequested();

            elapsed += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(elapsed / fadeDuration);

            // SmoothStep выглядит мягче обычного линейного fade.
            float easedTime =
                normalizedTime * normalizedTime *
                (3f - 2f * normalizedTime);

            splashCanvasGroup.alpha =
                Mathf.Lerp(startAlpha, 0f, easedTime);

            await UniTask.Yield(
                PlayerLoopTiming.Update,
                cancellationToken);
        }

        splashCanvasGroup.alpha = 0f;
    }
}