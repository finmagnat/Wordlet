using Inventory;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    public abstract class BoosterUI : MonoBehaviour
    {
        public bool IsActive { get; protected set; }
        public bool IsEmpty => _count <= 0;
        
        [SerializeField] protected GameObject _emptyGO;
        [SerializeField] protected TextMeshProUGUI _counterText;
        
        protected BoosterType _boosterType;
        protected int _count;
        
        public void SetBoosterData(BoosterType type, int count)
        {
            _boosterType = type;
            _count = count;
            
            if (_count > 0)
            {
                _emptyGO?.SetActive(false);
                _counterText.text = _count.ToString();
            }
            else
            {
                _emptyGO?.SetActive(true);
                _counterText.text = "";
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