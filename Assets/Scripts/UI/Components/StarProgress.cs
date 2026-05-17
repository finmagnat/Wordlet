using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class StarProgress : MonoBehaviour
    {
        [SerializeField] protected Image _inactiveIcon;

        public void SetActive(bool active  = true)
        {
            _inactiveIcon.gameObject.SetActive(!active);
        }
    }
}