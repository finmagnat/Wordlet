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
        [SerializeField] private Image _mainBackground;
        [SerializeField] private uint _blinkCount = 3; // Количество миганий
        [SerializeField] private float _blinkDtDelay = 0.5f; // Интервал 0.5 секунды
        
        [Header("Спрайты фона и выделения")]
        [SerializeField] private Sprite _mainBackgroundEmpty; // Фон пустой ячейки по умолчанию (skined)
        [SerializeField] private Sprite _mainBackgroundFilled; // Фон с установленной буквой (skined)
        [SerializeField] private Sprite _selectedCell; // Выделение выбранной ячейки (пустой или с только что установленной буквой) (оранжевый)
        [SerializeField] private Sprite _selectedLetter; // Выделение буквы (при выделении слова - желтый)
        
        [Inject] private AudioService _audioService;
        
        public int Index { get; set; } // Индекс элемента на поле [0 - n]
        
        private bool IsHighlight => _highlightState == HighlightState.Highlighted;
        
        private WordsField _wordsField;
        private float _suppressClickUntil;
        
        private float _blinkDtTimer = 0;
        private float _blinkCounter = 0;
        private bool _bModeBlink; // Режим мигания
        private bool _isIlluminated; // Состояние подсветки в Режиме мигания
        private string _letter;
        
        private BoxCollider2D _collider;

        private HighlightState _highlightState = HighlightState.None; // Режим подсветки (выделена буква или ячейка)
        enum HighlightState { None, SelectedCell, Highlighted };

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
                        HighlightCell();
                    }
                }
            }
        }
        
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
            
            if (Empty() && !IsHighlight)
            {
                EventBus.Raise(new CellSelectEvent{ letter = this }); // Игрок кликнул по пустой ячейке
                return;
            }

            if (!Empty() && !IsHighlight)
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
        
        internal void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;
        
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
        }

        internal string GetLetter() => _letter;
        
        internal char GetChar() => _letter != "" ? _letter[0] : ' ';
        
        internal bool Empty() => _letter == "";
        
        /// <summary>
        /// Выделить пустую ячейку
        /// </summary>
        internal void HighlightCell()
        {
            ModeBlinkClear();
            SetHighlightState(HighlightState.SelectedCell);
        }
        
        /// <summary>
        /// Выделить букву
        /// </summary>
        internal void Highlight()
        {
            ModeBlinkClear();
            SetHighlightState(HighlightState.Highlighted);
        }
        
        /// <summary>
        /// Убрать выделение с буквы
        /// </summary>
        internal void UnHighlight()
        {
            ModeBlinkClear();
            SetHighlightState(HighlightState.None);
        }

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
                    SetHighlightState(_highlightState);
                }
                else
                {
                    _letterText.text = "";
                    _mainBackground.sprite = _mainBackgroundEmpty;
                }
            }            
        }
        
        private void SetHighlightState(HighlightState state)
        {
            _highlightState = state;
            _mainBackground.sprite = state switch
            {
                HighlightState.SelectedCell => _selectedCell,
                HighlightState.Highlighted => _selectedLetter,
                _ => Empty() ? _mainBackgroundEmpty : _mainBackgroundFilled
            };
        }
        
        

        private void Blink()
        {
            _isIlluminated = !_isIlluminated;

            if (_isIlluminated)
            {
                _mainBackground.sprite = _mainBackgroundFilled;
                _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterUnblinking);
            }
            else
            {
                ++_blinkCounter;
                _mainBackground.sprite = _selectedCell;
                _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterBlinking);
            }
        }

        private void ModeBlinkClear()
        {
            if (_bModeBlink)
            {
                _bModeBlink = false;
                _isIlluminated = false;
                _blinkDtTimer = 0;
                _blinkCounter = 0;
            }
        }
    }
}