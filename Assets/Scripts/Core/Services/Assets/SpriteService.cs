using Core.Generated;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    public class SpriteService : ISpriteService
    {
        private readonly AddressablesLoader _loader;

        public SpriteService(AddressablesLoader loader)
        {
            _loader = loader;
        }

        public async UniTask<Sprite> GetSpriteAsync(AssetKey alias)
            => await _loader.LoadAssetAsync<Sprite>(alias.ToString());

        public bool IsLoaded(AssetKey alias)
            => _loader.IsLoaded(alias.ToString());

        public void Unload(AssetKey alias)
            => _loader.Unload(alias.ToString());
    }

    public interface ISpriteService
    {
        UniTask<Sprite> GetSpriteAsync(AssetKey alias);
        bool IsLoaded(AssetKey alias);
        void Unload(AssetKey alias);
    }
}