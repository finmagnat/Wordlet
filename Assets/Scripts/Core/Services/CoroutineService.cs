using System.Collections;
using UnityEngine;

namespace Core.Services
{
    public class CoroutineService : MonoBehaviour, IService
    {
        private static CoroutineService _instance;

        public static Coroutine StartRoutine(IEnumerator routine)
        {
            if (_instance == null)
            {
                var go = new GameObject("CoroutineService");
                _instance = go.AddComponent<CoroutineService>();
                Object.DontDestroyOnLoad(go);
            }

            return _instance.StartCoroutine(routine);
        }
    }
}