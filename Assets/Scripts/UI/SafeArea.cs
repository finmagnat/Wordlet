using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private bool ignoreVerticalInsets = true;

    private RectTransform _rt;
    private Rect _lastSafe;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) return;
        if (_rt == null) _rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Apply()
    {
        if (_rt == null) return;

        var sa = Screen.safeArea;

        if (ignoreVerticalInsets)
        {
            sa.y = 0;
            sa.height = Screen.height;
        }

        if (sa == _lastSafe) return;
        _lastSafe = sa;

        var anchorMin = sa.position;
        var anchorMax = sa.position + sa.size;

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