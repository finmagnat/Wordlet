using Core.Audio;
using Core.Events;
using Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UnityEngine.EventSystems;

namespace UI.Components
{
    public class SelectableLetter : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private GameObject _selectImage; // Выделение на кнопке
        [SerializeField] private GameObject _selectImageRed; // Выделение на кнопке Красным
        [SerializeField] private Image _mainBackground;
        [SerializeField] private uint _blinkCount = 3; // Количество миганий
        [SerializeField] private float _blinkDtDelay = 0.5f; // Интервал 0.5 секунды
        
        [Inject] private AudioService _audioService;
        
        private WordsField _wordsField;
        private float _suppressClickUntil;
        
        private float _blinkDtTimer = 0;
        private float _blinkCounter = 0;
        private bool _bModeBlink = false; // Режим мигания

        private string _letter;
        private bool _isHighlight = false; // Режим подсветки (буква выделена)
        private BoxCollider2D _collider;

        public int Index { get; set; } // Индекс элемента на поле [0 - n]

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Empty()) return;

            _suppressClickUntil = Time.unscaledTime + 0.05f;
            _wordsField?.BeginDragSelection(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Empty()) return;

            _wordsField?.ContinueDragSelection(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _wordsField?.EndDragSelection();
        }
        
        public void OnPressed()
        {
            // если PointerDown уже выбрал букву — не дублируем выбор по клику
            if (Time.unscaledTime < _suppressClickUntil)
                return;

            if (!Empty() && !IsHighlight())
            {
                ModeBlinkClear();
                EventBus.Raise(new LetterSelectEvent{ letter = this });
            }
        }

        public void Reset()
        {
            SetLetter("");
            UnHighlight();
        }
        
        /// <summary>
        /// Включить режим "Помигать буквой"
        /// </summary>
        internal void SetModeBlink() => _bModeBlink = true;
        

        internal bool HitTest(Vector3 position)
        {
            if (_collider)
            {
                //Debug.Log($"_collider.bounds: {_collider.bounds} : {position}");
                return _collider.OverlapPoint(position); 
            }
            return false;
        }

        internal void SetLetter(string letter)
        {
            _letter = letter;
            _letterText.text = _letter;
            _selectImageRed.SetActive(string.IsNullOrEmpty(letter) ? false : true);
        }

        internal string GetLetter() => _letter;
        
        internal char GetChar() => _letter != "" ? _letter[0] : ' ';
        
        internal bool Empty() => _letter == "";
        
        /// <summary>
        /// Выделить букву
        /// </summary>
        internal void Highlight()
        {
            ModeBlinkClear();
            _isHighlight = true;
            _selectImage.SetActive(_isHighlight);
            _selectImageRed.SetActive(false);
        }
        /// <summary>
        /// Убрать выделение с буквы
        /// </summary>
        internal void UnHighlight()
        {
            ModeBlinkClear();
            _isHighlight = false;
            _selectImage.SetActive(_isHighlight);
            _selectImageRed.SetActive(false);
        }

        internal bool IsHighlight() => _isHighlight;
        
        /// <summary>
        /// Отобразить или скрыть букву на поле
        /// </summary>
        /// <param name="value">bool</param>
        internal void ShowLetter(bool value)
        {
            ModeBlinkClear();
            if (!Empty())
            {
                if (value)
                {
                    _letterText.text = _letter;
                    if (_isHighlight && _selectImage)
                    {
                        _selectImage.SetActive(true);
                    }
                }
                else
                {
                    _letterText.text = "";
                    if (_isHighlight && _selectImage)
                    {
                        _selectImage.SetActive(false);
                    }
                }
            }            
        }
        
        internal void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;
        
        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
            _wordsField = GetComponentInParent<WordsField>();
        }

        private void Update()
        {
            if (_bModeBlink)
            {
                _blinkDtTimer += Time.deltaTime;
                if (_blinkDtTimer >= _blinkDtDelay)
                {
                    _blinkDtTimer = 0;
                    Blink();
                    if (_blinkCounter > _blinkCount)
                    {
                        ModeBlinkClear();
                        if (_isHighlight)
                        {
                            Highlight();
                        }
                        else
                        {
                            UnHighlight();
                        }
                    }
                }
            }
        }

        private void Blink()
        {
            if (_selectImage)
            {
                if (_selectImage.activeInHierarchy)
                {
                    _selectImage.SetActive(false);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterUnblinking);
                }
                else
                {
                    ++_blinkCounter;
                    _selectImage.SetActive(true);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterBlinking);
                }
            }
        }

        private void ModeBlinkClear()
        {
            if (_bModeBlink)
            {
                _bModeBlink = false;
                _blinkDtTimer = 0;
                _blinkCounter = 0;
            }
        }
    }
}