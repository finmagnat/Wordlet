using Cysharp.Threading.Tasks;

namespace UI.Screens
{
    public abstract class GameScreen<TPayload> : GameScreenBase
    {
        public async UniTask PrepareAsync(TPayload payload)
        {
            await PrepareCommonAsync();
            await PrepareScreenAsync(payload);
        }

        protected virtual UniTask PrepareScreenAsync(TPayload payload)
        {
            return UniTask.CompletedTask;
        }
    }
}