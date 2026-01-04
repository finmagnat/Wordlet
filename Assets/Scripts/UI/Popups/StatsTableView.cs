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
        
        public void SetText(
            string playerName, 
            string opponentName, 
            string playerScore, 
            string opponentScore, 
            string playerPasses, 
            string opponentPasses)
        {
            _playerName.text = playerName;
            _opponentName.text = opponentName;
            _playerScore.text = playerScore;
            _opponentScore.text = opponentScore;
            _playerPasses.text = playerPasses;
            _opponentPasses.text = opponentPasses;
        }
    }
}