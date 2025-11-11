using Core.Config;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    [CreateAssetMenu(menuName = "Installers/GameConfigsInstaller")]
    public class GameConfigsInstaller : ScriptableObjectInstaller<GameConfigsInstaller>
    {
        [SerializeField] private GameConfigsContainer _container;

        public override void InstallBindings()
        {
            if (_container == null)
                _container = Resources.Load<GameConfigsContainer>("Config/GameConfigsContainer");

            if (_container == null)
            {
                Debug.LogError("❌ GameConfigsContainer not found in Resources/Config/");
                return;
            }

            foreach (var so in _container.configs)
            {
                if (!so) continue;
                Container.Bind(so.GetType()).FromInstance(so).AsSingle();
                Debug.Log($"✅ Config bound: {so.GetType().Name}");
            }
        }
    }
}