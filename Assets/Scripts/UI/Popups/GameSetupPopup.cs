using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popups
{
    public class GameSetupPopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown _difficultyDropdown;
        [SerializeField] private TMP_Dropdown _turnTimeDropdown;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;

        private void Start()
        {
            _closeButton.onClick.AddListener(async () => await HideAsync());
            _startButton.onClick.AddListener(OnStartPressed);
        }

        private void OnStartPressed()
        {
            Debug.Log($"▶ Начинаем игру: " +
                      //$"ИИ={_opponentDropdown.captionText.text}, " +
                      $"Сложность={_difficultyDropdown.captionText.text}, " +
                      $"Время={_turnTimeDropdown.captionText.text}");
        }
    }
}