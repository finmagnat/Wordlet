using Core.Events;
using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class WordListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _wordText;
        
        private string _word;

        public void Initialize(string word)
        {
            _word = word;
            _wordText.text = $"{_word.Length} {_word}";
        }
        
        public void OnPressed() => EventBus.Raise(new ShowWordInfoEvent{word = _word});
    }
}