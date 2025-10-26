using Core.Installers;

namespace Core.Bootstrap
{
    // живёт в первой сцене (BootstrapScene) и инициализирует ядро.
    public class GameBootstrap
    {
        private async void Start()
        {
            await ZenjectInstaller.InitializeAsync();
            // SceneLoader.Load("MainScene"); // TODO: SceneLoader == ?
        }
    }
}