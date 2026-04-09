using Core.Events;
using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class WordListItem : MonoBehaviour
    {
        public TextMeshProUGUI wordText;
        
        public void OnPressed() => EventBus.Raise(new ShowWordInfoEvent());
    }
}