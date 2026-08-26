namespace Core.Services
{
    public interface IVibrationService : IService
    {
        bool IsEnabled { get; }
        void Play(VibrationType type = VibrationType.Light);
        void EnableVibration(bool value);
    }
}
