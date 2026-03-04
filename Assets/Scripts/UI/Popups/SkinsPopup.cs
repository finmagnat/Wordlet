using System.Collections.Generic;
using Core.Audio;
using Core.Config;
using Core.Services;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class SkinsPopup : UIPopup
    {
        [SerializeField] private SkinButton _buttonPrefab;
        [SerializeField] private Transform _scrollListContent;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _applyButton;

        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private AudioService _audioService;
        
        private readonly List<SkinButton> _buttons = new();

        private SkinType _newSkin;
        private SkinType _oldSkin;

        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
            });
            
            _applyButton.onClick.AddListener(async () =>
            {
                Apply();
                await HideAsync();
            });
        }

        private void Apply()
        {
            if (_skinsService.SkinCurrent.SkinType != _newSkin)
            {
                _skinsService.SaveSkinCurrent(_newSkin);
                _audioService?.PlaySfxAsync(Sounds.SoundSfx_SkinChanged);
                //Dictionary<string, string> paramDictionary = new() { { Constants.Type, _newSkin.ToString() } };
                //_analyticsManager.SendEvent(Constants.LanguagePressedEvent, paramDictionary);
            }
        }

        public override async UniTask ShowAsync()
        {
            _oldSkin = _skinsService.SkinCurrent.SkinType;
            if (_buttons == null || _buttons.Count == 0)
            {
                foreach (var skinItem in _skinsService.Config.Skins)
                {
                    SkinButton skinButton = Instantiate(_buttonPrefab, _scrollListContent, false);
                    skinButton.SetSkinData(skinItem.ColorPreviewTile, skinItem.SkinType);
                    skinButton.button.onClick.AddListener(() =>
                    {
                        SelectSkin(skinItem.SkinType);
                    });

                    _buttons.Add(skinButton);
                }
            }

            SelectSkin(_oldSkin);
            
            await base.ShowAsync();
        }
        
        private void SelectSkin(SkinType skinType)
        {
            _newSkin = skinType;

            foreach (var button in _buttons)
                button.SetActiveStatus(button.SkinType == skinType);
        }
    }
}