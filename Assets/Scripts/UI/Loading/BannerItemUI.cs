using Core.Config;
using TMPro;
using UnityEngine;

namespace UI.Loading
{
    public class BannerItemUI : MonoBehaviour
    {
        public BannerType BannerType => _bannerType;
        
        [SerializeField] private BannerType _bannerType;
        [SerializeField] private TextMeshProUGUI _textHeader;

        private void Awake()
        {
            _textHeader.text = "";
        }
    }
}