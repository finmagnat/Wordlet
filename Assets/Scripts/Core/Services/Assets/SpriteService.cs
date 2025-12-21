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

        public async UniTask<Sprite> GetSpriteAsync(string alias)
            => await _loader.LoadAssetAsync<Sprite>(alias);

        public bool IsLoaded(string alias)
            => _loader.IsLoaded(alias);

        public void Unload(string alias)
            => _loader.Unload(alias);
    }

    public interface ISpriteService
    {
        UniTask<Sprite> GetSpriteAsync(string alias);
        bool IsLoaded(string alias);
        void Unload(string alias);
    }
}