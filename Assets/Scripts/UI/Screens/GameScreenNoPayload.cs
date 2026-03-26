using Core.UI;
using Cysharp.Threading.Tasks;

namespace UI.Screens
{
    public abstract class GameScreenNoPayload : GameScreen<NoPayload>, IUIScreenPreparable
    {
        public UniTask PrepareAsync()
        {
            return PrepareAsync(NoPayload.Value);
        }

        protected sealed override UniTask PrepareScreenAsync(NoPayload payload)
        {
            return PrepareScreenAsync();
        }

        protected virtual UniTask PrepareScreenAsync()
        {
            return UniTask.CompletedTask;
        }
    }
}