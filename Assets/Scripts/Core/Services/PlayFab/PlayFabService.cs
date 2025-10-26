using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    // инициализация, авторизация, работа с профилем, виртуальной валютой, инвентарём.
    public class PlayFabService : IPlayFabService
    {
        public Task<LoginResult> LoginAsync(string customId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, int>> GetPlayerInventoryAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}