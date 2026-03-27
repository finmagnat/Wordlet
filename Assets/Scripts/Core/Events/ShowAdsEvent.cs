namespace Core.Events
{
    public class ShowAdsEvent : IGameEvent
    {
        public bool IsShow { get; private set; }
        
        public ShowAdsEvent(bool isShow) => IsShow = isShow;   
    }
}