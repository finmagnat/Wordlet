namespace Core.Services
{
    public interface IPlayFabAuthFacade
    {
        bool IsLoggedIn { get; }
        string PlayFabId { get; }
        string DisplayName { get; }

        void SetDisplayNameLocal(string name);
    }
}