using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public class AudioService : IAudioService
    {
        private readonly AddressablesLoader _loader;

        private readonly Dictionary<string, AudioClip> _sfxCache = new();
        private AudioSource _sfxSource;

        private float _sfxVolume = 1f;

        public AudioService(AddressablesLoader loader)
        {
            _loader = loader;
        }

        public UniTask InitializeAsync()
        {
            // Создаём скрытый GameObject под AudioSource
            var go = new GameObject("[AudioService]");
            Object.DontDestroyOnLoad(go);

            _sfxSource = go.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.volume = _sfxVolume;

            return UniTask.CompletedTask;
        }

        public async UniTask PlaySfxAsync(string addressKey)
        {
            if (string.IsNullOrEmpty(addressKey))
                return;

            // 1️⃣ берём из кэша или загружаем
            if (!_sfxCache.TryGetValue(addressKey, out var clip))
            {
                clip = await _loader.LoadAssetAsync<AudioClip>(addressKey);
                if (clip == null)
                {
                    Debug.LogWarning($"🔇 Audio clip not found: {addressKey}");
                    return;
                }

                _sfxCache[addressKey] = clip;
            }

            // 2️⃣ проигрываем (OneShot позволяет накладывать звуки)
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            if (_sfxSource != null)
                _sfxSource.volume = _sfxVolume;
        }
    }
}