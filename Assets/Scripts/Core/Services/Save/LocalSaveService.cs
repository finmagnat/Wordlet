using Cysharp.Threading.Tasks;

namespace Core.Services
{
    /*
     * локальное сохранение
     * + синк с сервером
     */
    
    public class LocalSaveService : ISaveService
    {
        public async UniTask Save()
        {
            // TODO: реализовать сохранение игры
            await UniTask.CompletedTask;
        }
    }
}