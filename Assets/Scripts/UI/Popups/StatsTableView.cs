using TMPro;
using UnityEngine;

namespace UI.Popups
{
    public class StatsTableView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName;
        [SerializeField] private TextMeshProUGUI _opponentName;
        [SerializeField] private TextMeshProUGUI _playerScore;
        [SerializeField] private TextMeshProUGUI _opponentScore;
        [SerializeField] private TextMeshProUGUI _playerPasses;
        [SerializeField] private TextMeshProUGUI _opponentPasses;
        
        public void SetData(
            string playerName, 
            string opponentName, 
            uint playerScore, 
            uint opponentScore, 
            uint playerPasses, 
            uint opponentPasses,
            uint maxPasses
            )
        {
            _playerName.text = playerName;
            _opponentName.text = opponentName;
            _playerScore.text = playerScore.ToString();
            _opponentScore.text = opponentScore.ToString();
            _playerPasses.text = GetFormatedPasses(playerPasses, maxPasses);
            _opponentPasses.text = GetFormatedPasses(opponentPasses, maxPasses);
        }

        private string GetFormatedPasses(uint passes, uint maxPasses)
        {
            return passes == maxPasses ? 
                $"<color=red><b>{passes} / {maxPasses}</b></color>" : 
                $"{passes} / {maxPasses}";
        }
    }
}