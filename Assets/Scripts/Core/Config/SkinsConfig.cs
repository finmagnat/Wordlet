using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(menuName = "Wordlet/Config/Skins", fileName = "SkinsConfig")]
    public class SkinsConfig : ScriptableObject
    {
        public List<SkinData> Skins => _skins;
        
        [Space(10)]
        [Tooltip("Скин по умолчанию")]
        public SkinType SkinByDefault = SkinType.PINK;
        
        [SerializeField] private List<SkinData> _skins = new ();
        
        public SkinData GetSkinByType(SkinType skinType) =>
            _skins.Find(item => item.SkinType == skinType);
    }
}