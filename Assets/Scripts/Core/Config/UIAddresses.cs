using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Config/UI Addresses", fileName = "UIAddresses")]
    public class UIAddresses : ScriptableObject
    {
        public AssetReferenceGameObject LoadingScreen;
        public AssetReferenceGameObject MainMenu;
        public AssetReferenceGameObject Settings;
        public AssetReferenceGameObject GameScreen;
        // etc.
    }
}