using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Boosters", fileName = "BoostersIconsConfig")]
    public class BoostersIconsConfig : ScriptableObject
    {
        public List<BoosterIconData> Items => _items;
        
        [SerializeField] private List<BoosterIconData> _items = new ();
        
        private readonly Dictionary<string, string> _cache = new();

        public BoostersIconsConfig()
        {
            foreach (BoosterIconData item in _items)
                _cache[item.Type.ToString()] = item.IconAssetKey;
        }

        public string GetAlias(BoosterType type) => _cache[type.ToString()];
    }
    
    [Serializable]
    public struct BoosterIconData
    {
        public BoosterType Type;
        public string IconAssetKey;
    }
}
