namespace Core.Services
{
    public interface IGamePauseService
    {
        bool IsPaused { get; }
        void PushPause(object token);  // стек/рефкаунт паузы
        void SetUserPause(bool paused);
        void PopPause(object token);
    }
}