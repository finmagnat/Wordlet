using Core.Data;
using Cysharp.Threading.Tasks;

namespace Core.Services
{
    public interface ISaveService : IService
    {
        UniTask SaveAsync();
        bool HasSave();
        UniTask<SaveGameData> LoadAsync();
        UniTask ClearAsync();
    }
}