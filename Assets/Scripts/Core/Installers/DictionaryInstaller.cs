using Core.DataDictionary;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    public class DictionaryInstaller : MonoInstaller
    {
        [SerializeField] private DictionaryManagerPresenter _presenter;

        public override void InstallBindings()
        {
            Container.Bind<DictionaryManagerPresenter>()
                .FromInstance(_presenter)
                .AsSingle()
                .NonLazy();
        }
    }
}