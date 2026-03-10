using UnityEngine;

namespace Core.UI.Components
{
    public class LetterTileWobble : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _angleAmplitude = 4f;
        [SerializeField] private float _frequency = 0.9f;
        [SerializeField] private float _seed;

        private float _baseZ;

        private void Awake()
        {
            if (_target == null)
                _target = transform as RectTransform;

            if (_target != null)
                _baseZ = _target.localEulerAngles.z;

            if (Mathf.Approximately(_seed, 0f))
                _seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (_target == null)
                return;

            float z = _baseZ + Mathf.Sin(Time.unscaledTime * _frequency + _seed) * _angleAmplitude;
            _target.localEulerAngles = new Vector3(0f, 0f, z);
        }
    }
}