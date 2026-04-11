using TMPro;
using UnityEngine;

namespace UI.Loading
{
    public class BannerItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textHeader;

        private void Awake()
        {
            _textHeader.text = "";
        }
    }
}