using Core.Config;
using Core.Events;
using Core.Services.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class BoosterUI : MonoBehaviour
    {
        public BoosterType Type;
        
        public bool IsActive { get; protected set; }
        public bool IsInfinite { get; protected set; }
        public bool IsEmpty => !IsInfinite && _count <= 0;
        
        [SerializeField] protected TextMeshProUGUI _counterText;
        [SerializeField] protected TextMeshProUGUI _infiniteText;
        [SerializeField] protected bool _useEmpty;
        [SerializeField] protected GameObject _emptyGO;
        [SerializeField] protected bool _isAutoDisable;
        [SerializeField] protected Button _button;
        
        protected int _count;
        
        public void SetBoosterData(BoosterItem  data)
        {
            if (data != null)
            {
                _count = data.Count;
                IsInfinite = data.IsInfinite;
            }
            else
            {
                _count = 0;
            }

            if (IsInfinite)
            {
                _infiniteText.gameObject.SetActive(true); // "\u221E"
                _counterText.text = "";
            }
            else
            {
                _infiniteText.gameObject.SetActive(false);
                _counterText.text = IsEmpty ? "0" : _count.ToString();
            }

            if(_useEmpty)
                _emptyGO.SetActive(IsEmpty);

            if (_isAutoDisable)
                _button.interactable = IsInfinite || _count > 0;
        }
        
        public void UseBoosterHandler()
        {
            if (!IsActive)
            {
                Debug.Log($"Использовать {Type}, IsEmpty = {IsEmpty}");
                EventBus.Raise(new UseBoosterEvent{ boosterType = Type, isEmpty = IsEmpty});
            }
        }

        public virtual void ActivateBooster()
        {
            IsActive = true;
        }

        public virtual void Cancel()
        {
            IsActive = false;
        }
    }
}
