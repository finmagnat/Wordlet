using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IPlayFabService : IService
    {
        Task<LoginResult> LoginAsync(string customId);
        Task<Dictionary<string, int>> GetPlayerInventoryAsync();
    }

}