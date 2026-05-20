namespace Core.Services
{
    public interface IStarterBonusService : IService
    {
        bool IsAvailable { get; }
    }
}