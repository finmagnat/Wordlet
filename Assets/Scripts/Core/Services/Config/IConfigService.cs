using Core.Config;

namespace Core.Services
{
    public interface IConfigService : IService
    {
        GameConfig Game { get; }
        SkinsConfig Skins { get; }
        ShopCatalog Shop { get; }
        AdsConfig Ads { get; }
    }
}
