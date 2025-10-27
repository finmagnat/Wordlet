using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    // регистрирует все сервисы.
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EventBus>().AsSingle();
            Container.Bind<ConfigService>().AsSingle();
            Container.Bind<GameLogger>().AsSingle();
        }

        public override void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Debug.Log("Initializing services...");
            await UniTask.Delay(500); // пример ожидания
            Debug.Log("✅ Core initialized!");
        }
    }
}