using Core.Data;
using Cysharp.Threading.Tasks;

namespace UI.Popups
{
    public class FinishGamePopup : MessagePopup<FinishGamePopupData>
    {
        public StatsTableView statsTable;
        
        public override async UniTask PrepareAsync(FinishGamePopupData data)
        {
            // Если тут есть иконки/аватары/скины из Addressables —
            // грузишь их здесь и await-ишь тут же.

            statsTable.SetData(
                data.OwnerName,
                data.OpponentName,
                data.OwnerScore,
                data.OpponentScore,
                data.OwnerPass,
                data.OpponentPass,
                data.MaxPasses
            );

            await UniTask.CompletedTask;
        }
    }
}