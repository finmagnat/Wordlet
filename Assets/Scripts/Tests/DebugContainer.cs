using Core.Services;
using UnityEngine;

namespace Tests
{

    public class DebugContainer : IInitializable
    {
        private readonly UIService _ui;
        public DebugContainer(UIService ui)
        {
            _ui = ui;
        }

        public void Initialize()
        {
            Debug.Log("✅ UIService successfully injected!");
        }
    }
}