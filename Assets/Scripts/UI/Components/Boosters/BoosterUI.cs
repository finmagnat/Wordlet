using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public abstract class BoosterUI : MonoBehaviour
    {
        public BoosterType Type => _boosterType;
        public bool IsActive { get; protected set; }
        public bool IsEmpty => _count <= 0;
        
        [SerializeField] protected TextMeshProUGUI _counterText;
        [SerializeField] protected bool _useEmpty;
        [SerializeField] protected GameObject _emptyGO;
        [SerializeField] protected bool _isAutoDisable;
        [SerializeField] protected Button _button;
        
        protected BoosterType _boosterType;
        protected int _count;
        
        public void SetBoosterData(BoosterType type, int count)
        {
            _boosterType = type;
            _count = count;
            
            _counterText.text = IsEmpty ? "0" : _count.ToString();
            if(_useEmpty)
                _emptyGO.SetActive(IsEmpty);

            if (_isAutoDisable)
                _button.interactable = count > 0;
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