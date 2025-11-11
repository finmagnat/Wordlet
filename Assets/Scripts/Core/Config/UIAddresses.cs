using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Config/UI Addresses", fileName = "UIAddresses")]
    public class UIAddresses : ScriptableObject
    {
        [Header("Screens")]
        public AssetReferenceGameObject LoadingScreen; // Экран стартовой загрузки
        public AssetReferenceGameObject InGameLoadingScreen; // Экран загрузки между переходами "Главное меню - Игровой экран"
        public AssetReferenceGameObject MainMenu;
        //public AssetReferenceGameObject Settings;
        public AssetReferenceGameObject AIGameScreen;
        
        [Header("PopUps")]
        public AssetReferenceGameObject GameSetupPopup;
    }
}