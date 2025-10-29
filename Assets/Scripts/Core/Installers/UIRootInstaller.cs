using Core.Services;
using UnityEngine;
using Zenject;

public class UIRootInstaller : MonoInstaller
{
    [SerializeField] private Transform _screensRoot;
    [SerializeField] private Transform _popupsRoot;

    public override void InstallBindings()
    {
        Container.Bind<UIService>().AsSingle().WithArguments(_screensRoot, _popupsRoot);
    }
}
