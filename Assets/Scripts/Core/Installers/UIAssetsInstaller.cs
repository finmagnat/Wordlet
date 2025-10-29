using Core.Config;
using UnityEngine;
using Zenject;

public class UIAssetsInstaller : MonoInstaller
{
    [SerializeField] private UIAddresses _addresses;

    public override void InstallBindings()
    {
        Container.Bind<UIAddresses>().FromInstance(_addresses).AsSingle();
    }
}
