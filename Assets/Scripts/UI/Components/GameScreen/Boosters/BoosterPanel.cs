using Core.Events;
using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class BoosterPanel : MonoBehaviour
    {
        [SerializeField] private BoosterUI boosterLetter;
        [SerializeField] private BoosterUI boosterSlowdown;

        [Inject] private IInventoryService _inventory;

        private void Start()
        {
            EventBus.Subscribe<SlowdownStartEvent>(OnSlowdownStart);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SlowdownStartEvent>(OnSlowdownStart);
        }

        public void Refresh()
        {
            foreach (var booster in _inventory.Boosters.Values)
            {
                Debug.Log($"[BoosterPanel][Refresh] {booster.Type}: {booster.Count}");
                switch (booster.Type)
                {
                    case BoosterType.Letter:
                        boosterLetter.SetBoosterCount(booster.Count);
                        break;
                    case BoosterType.Slowdown:
                        boosterSlowdown.SetBoosterCount(booster.Count);
                        break;
                }
            }
        }

        public void OnUseLetter()
        {
            Debug.Log("Буковка КЛИК");
            if (!boosterLetter.IsActive)
            {
                Debug.Log("Буковка использована");
                EventBus.Raise(new UseBoosterEvent{ boosterType = BoosterType.Letter });
            }
        }

        public void OnUseSlowdown()
        {
            Debug.Log("Замедлялка КЛИК");
            if (!boosterSlowdown.IsActive)
            {
                Debug.Log("Замедлялка использована");
                EventBus.Raise(new UseBoosterEvent{ boosterType = BoosterType.Slowdown });
            }
        }
        
        private void OnSlowdownStart(SlowdownStartEvent startEvent)
        {
            (boosterSlowdown as SlowdownBooster).SetSeconds(startEvent.slowdownDelay);
            boosterSlowdown.ActivateBooster();
        }
    }
}