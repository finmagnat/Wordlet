using Core.Config;
using Core.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Screens
{
    // TODO: экран игры с ИИ
    public class AIGameScreen : UIScreen
    {
        [SerializeField] private Button _playAIButton;
        
        [Inject] private IUIManager _ui;

        private void Start()
        {
            
        }
        
        public void OnPlayClicked()
        {
            Debug.Log("Play button clicked!");
        }

    }
}