using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IAnalyticsProvider
    {
        string ProviderName { get; }
        bool IsEnabled { get; }

        UniTask InitializeAsync();
        void Track(AnalyticsEvent analyticsEvent);
    }
}
