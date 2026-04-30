using Core.Services;
using TMPro;
using UnityEngine;
using Zenject;

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
        
        [Inject] private LocalizationService _localization;
        
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
            var textScore = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTableTextScore);
            var textPass = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTableTextPasses);
                
            _playerName.text = playerName;
            _opponentName.text = opponentName;

            string playerScoreFormated = playerScore.ToString();
            if (playerPasses < maxPasses)
            {
                if(playerScore > opponentScore)
                    playerScoreFormated = $"<color=green><b>{playerScore}</b></color>";
                if(playerScore < opponentScore)
                    playerScoreFormated = $"<color=red><b>{playerScore}</b></color>";
            }
            
            _playerScore.text = textScore + playerScoreFormated;
            _opponentScore.text = textScore + opponentScore;
            _playerPasses.text = textPass + GetFormatedPasses(playerPasses, maxPasses);
            _opponentPasses.text = textPass + GetFormatedPasses(opponentPasses, maxPasses);
        }

        private string GetFormatedPasses(uint passes, uint maxPasses)
        {
            if(maxPasses <= 0)
                return passes.ToString();
            
            return passes == maxPasses ? 
                $"<color=red><b>{passes} / {maxPasses}</b></color>" : 
                $"{passes} / {maxPasses}";
        }
    }
}