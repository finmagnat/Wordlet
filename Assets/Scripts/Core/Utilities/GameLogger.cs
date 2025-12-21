using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public class GameLogger
    {
        public async UniTask InitializeAsync()
        {
            await UniTask.Yield();
            Debug.Log("Logger initialized");
        }

        public void Log(string message) => Debug.Log($"[Game] {message}");
    }
}