using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface ISaveService : IService
    {
        UniTask Save();
    }
}