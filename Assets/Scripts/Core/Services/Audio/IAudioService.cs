using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface IAudioService : IService
    {
        UniTask PlaySfxAsync(string path);
    }
}