using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Config/UI Addresses", fileName = "UIAddresses")]
    public class UIAddresses : ScriptableObject
    {
        [Header("Screens")]
        public AssetReferenceGameObject LoadingScreen;
        public AssetReferenceGameObject MainMenu;
        public AssetReferenceGameObject Settings;
        public AssetReferenceGameObject GameScreen;
        
        [Header("PopUps")]
        public AssetReferenceGameObject GameSetupPopup;
    }
}