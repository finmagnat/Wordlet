using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public abstract class BoosterUI : MonoBehaviour
    {
        private const string InfinityCounterText = "\u221E";

        public BoosterType Type => _boosterType;
        public bool IsActive { get; protected set; }
        public bool IsInfinite { get; private set; }
        public bool IsEmpty => !IsInfinite && _count <= 0;
        
        [SerializeField] protected TextMeshProUGUI _counterText;
        [SerializeField] protected bool _useEmpty;
        [SerializeField] protected GameObject _emptyGO;
        [SerializeField] protected bool _isAutoDisable;
        [SerializeField] protected Button _button;
        
        protected BoosterType _boosterType;
        protected int _count;
        
        public void SetBoosterData(BoosterType type, int count, bool isInfinite = false)
        {
            _boosterType = type;
            _count = count;
            IsInfinite = isInfinite;
            
            _counterText.text = IsInfinite ? InfinityCounterText : IsEmpty ? "0" : _count.ToString();
            if(_useEmpty)
                _emptyGO.SetActive(IsEmpty);

            if (_isAutoDisable)
                _button.interactable = IsInfinite || count > 0;
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
