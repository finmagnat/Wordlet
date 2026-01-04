using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IAudioService : IService
    {
        UniTask PlaySfxAsync(string addressKey);
        void SetSfxVolume(float value); // 0..1
    }
}