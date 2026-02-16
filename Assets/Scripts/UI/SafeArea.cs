using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform _rt;
    Rect _lastSafeArea;
    ScreenOrientation _lastOrientation;
    Vector2Int _lastResolution;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        // На iOS safeArea может меняться при повороте/системных жестах/входящих звонках и т.п.
        if (_lastSafeArea != Screen.safeArea ||
            _lastOrientation != Screen.orientation ||
            _lastResolution.x != Screen.width ||
            _lastResolution.y != Screen.height)
        {
            Apply();
        }
    }

    void Apply()
    {
        Rect sa = Screen.safeArea;

        _lastSafeArea = sa;
        _lastOrientation = Screen.orientation;
        _lastResolution = new Vector2Int(Screen.width, Screen.height);

        // safeArea в якоря (0..1)
        Vector2 anchorMin = sa.position;
        Vector2 anchorMax = sa.position + sa.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
}