using Core.Services;
using UnityEngine;

namespace Tests
{

    public class ZenjectPing : IInitializable
    {
        public void Initialize()
        {
            Debug.Log("🎯 Zenject container initialized successfully!");
        }
    }
}