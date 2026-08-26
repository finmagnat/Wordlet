namespace Core.Services
{
    public interface IVibrationService : IService
    {
        bool IsEnabled { get; }
        void Play();
        void EnableVibration(bool value);
    }
}