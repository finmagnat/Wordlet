using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public sealed class StarterBonusService : IStarterBonusService
    {
        public bool IsAvailable { get; }
        
        private readonly PlayFabAuthService _auth;
        private readonly ConfigService _configService;

        public StarterBonusService(PlayFabAuthService auth, ConfigService configService)
        {
            _auth = auth;
            _configService = configService;
        }

        public async UniTask InitializeAsync()
        {
            
        }
        
    }

}
