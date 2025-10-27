using System.Threading.Tasks;

namespace Core.Services
{
    public interface IInitializable
    {
        Task InitializeAsync();
    }
}