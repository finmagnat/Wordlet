using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AdaptiveKeyboardGrid : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private RectTransform _rectTransform;

        [SerializeField] private GridLayoutGroup _grid;

        [Header("Layout")] [SerializeField] private int _fixedRows = 4;
        [SerializeField] private bool _keepSquareCells = true;

        [Header("Limits")] [SerializeField] private Vector2 _minCellSize = new Vector2(48f, 48f);
        [SerializeField] private Vector2 _maxCellSize = new Vector2(120f, 120f);

        [Header("Optional")] [SerializeField] private bool _recalculateSpacing = false;
        [SerializeField] private Vector2 _minSpacing = new Vector2(2f, 2f);
        [SerializeField] private Vector2 _maxSpacing = new Vector2(8f, 8f);

        private int _childCount;
        
        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _grid = GetComponent<GridLayoutGroup>();
        }

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_grid == null)
                _grid = GetComponent<GridLayoutGroup>();
        }

        private void OnEnable()
        {
            RefreshLayout(_childCount);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_grid == null)
                _grid = GetComponent<GridLayoutGroup>();

            RefreshLayout(_childCount);
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            RefreshLayout(_childCount);
        }

        public void RefreshLayout(int childCount)
        {
            if (_rectTransform == null || _grid == null)
                return;
            
            if (childCount <= 0 || _fixedRows <= 0)
                return;
            
            _childCount = childCount;
            int rows = _fixedRows;
            int columns = Mathf.CeilToInt(childCount / (float)rows);

            Rect rect = _rectTransform.rect;

            float availableWidth =
                rect.width
                - _grid.padding.left
                - _grid.padding.right
                - _grid.spacing.x * (columns - 1);

            float availableHeight =
                rect.height
                - _grid.padding.top
                - _grid.padding.bottom
                - _grid.spacing.y * (rows - 1);

            float cellWidth = availableWidth / columns;
            float cellHeight = availableHeight / rows;

            if (_keepSquareCells)
            {
                float size = Mathf.Min(cellWidth, cellHeight);
                cellWidth = size;
                cellHeight = size;
            }

            cellWidth = Mathf.Clamp(cellWidth, _minCellSize.x, _maxCellSize.x);
            cellHeight = Mathf.Clamp(cellHeight, _minCellSize.y, _maxCellSize.y);

            _grid.cellSize = new Vector2(cellWidth, cellHeight);
            _grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            _grid.constraintCount = rows;

            if (_recalculateSpacing)
            {
                float usedWidth = cellWidth * columns;
                float usedHeight = cellHeight * rows;

                float extraWidth = rect.width - _grid.padding.left - _grid.padding.right - usedWidth;
                float extraHeight = rect.height - _grid.padding.top - _grid.padding.bottom - usedHeight;

                float spacingX = columns > 1 ? extraWidth / (columns - 1) : 0f;
                float spacingY = rows > 1 ? extraHeight / (rows - 1) : 0f;

                spacingX = Mathf.Clamp(spacingX, _minSpacing.x, _maxSpacing.x);
                spacingY = Mathf.Clamp(spacingY, _minSpacing.y, _maxSpacing.y);

                _grid.spacing = new Vector2(spacingX, spacingY);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }
    }
}