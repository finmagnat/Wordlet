using Core.Data;
using TMPro;
using UnityEngine;

namespace Core.UI.Components
{
    public class ProgressVictories : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _counterText;
        [SerializeField] protected StarProgress[] _stars;

        public void SetData(FinishGamePopupData data)
        {
            _counterText.text = $"{data.Reward.WinsInSeriesCount} / {data.Reward.WinsInSeriesMax}";
            
            int maxValue = _stars.Length;
            int currentValue = data.Reward.WinsInSeriesCount;
            
            if(currentValue > maxValue)
                currentValue = maxValue;
            else if(currentValue < 0)
                currentValue = 0;
            
            for (int i = 0, n = 1; i < maxValue; i++, n++)
            {
                _stars[i].SetActive(n <= currentValue);
            }
        }
    }
}