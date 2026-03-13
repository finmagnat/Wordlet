using UI.Parallax;
using UnityEngine;

namespace Core.DebugTools
{
    public sealed class ParallaxDebugBootstrap : MonoBehaviour
    {
        [SerializeField] private UIParallaxController _parallaxController;
        [SerializeField] private bool _showPanelOnStart;

        private ParallaxDebugOptions _options;

        private void Start()
        {
            if (_parallaxController == null)
                _parallaxController = FindObjectOfType<UIParallaxController>();

            if (_parallaxController == null)
            {
                Debug.LogWarning("[ParallaxDebugBootstrap] UIParallaxController not found.");
                return;
            }

            _options = new ParallaxDebugOptions(_parallaxController);
            SRDebug.Instance.AddOptionContainer(_options);

            if (_showPanelOnStart)
                SRDebug.Instance.ShowDebugPanel();
        }

        private void OnDestroy()
        {
            if (_options != null && SRDebug.Instance != null)
                SRDebug.Instance.RemoveOptionContainer(_options);
        }
    }
}