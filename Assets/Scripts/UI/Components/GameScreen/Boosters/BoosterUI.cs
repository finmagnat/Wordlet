using TMPro;
using UnityEngine;

namespace UI.Components
{
    public abstract class BoosterUI : MonoBehaviour
    {
        [SerializeField] protected GameObject _emptyGO;
        [SerializeField] protected TextMeshProUGUI _counterText;

        public bool IsActive { get; protected set; }
        
        public void SetBoosterCount(int boosterCount)
        {
            if (boosterCount > 0)
            {
                _emptyGO.SetActive(false);
                _counterText.text = boosterCount.ToString();
            }
            else
            {
                _emptyGO.SetActive(true);
                _counterText.text = "";
            }
        }
        
        public abstract void ActivateBooster();

        public virtual void Cancel()
        {
            IsActive = false;
        }
    }
}