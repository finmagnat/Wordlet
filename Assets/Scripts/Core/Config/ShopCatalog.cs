using System;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Shop/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [Header("Config Service")] 
        public bool EnablePurchasePushToServer = true;
        
        [Header("Products")]
        public List<ShopPackConfig> Packs = new();
    }

    [Serializable]
    public sealed class ShopPackConfig
    {
        public string ProductId;

        public string Title;
        [TextArea] public string Description;

        public List<ShopRewardConfig> Rewards = new();

        // Заглушка для Этапа 1 (пока стора нет)
        public string DebugPriceText = "—";
        public bool DebugAvailable = true;
    }

    [Serializable]
    public sealed class ShopRewardConfig
    {
        //public string ItemId; // booster ids / no_ads etc
        public BoosterType ItemId; // booster ids / no_ads etc
        public int Amount;
    }
}