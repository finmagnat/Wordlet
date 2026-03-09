using UnityEngine;

namespace Core.UI.Components
{
    public class HeroLettersOrbit: MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private RectTransform _orbitRoot;
        [SerializeField] private RectTransform _bigLetter;

        [Header("Orbit")]
        [SerializeField] private float _orbitSpeed = 3f;

        [Header("Big Letter")]
        [SerializeField] private float _bigLetterPulseAmplitude = 0.03f;
        [SerializeField] private float _bigLetterPulseFrequency = 1.2f;

        [Header("Tiles")]
        [SerializeField] private OrbitTile[] _tiles;

        private Vector3 _bigLetterBaseScale;

        private void Awake()
        {
            if (_bigLetter != null)
                _bigLetterBaseScale = _bigLetter.localScale;

            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].Target == null)
                    continue;

                _tiles[i].BasePosition = _tiles[i].Target.anchoredPosition;
                _tiles[i].Seed = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float time = Time.unscaledTime;

            UpdateOrbit(dt);
            UpdateBigLetter(time);
            UpdateTiles(time);
        }

        private void UpdateOrbit(float dt)
        {
            if (_orbitRoot == null)
                return;

            _orbitRoot.Rotate(0f, 0f, _orbitSpeed * dt);
        }

        private void UpdateBigLetter(float time)
        {
            if (_bigLetter == null)
                return;

            float pulse = 1f + Mathf.Sin(time * _bigLetterPulseFrequency) * _bigLetterPulseAmplitude;
            _bigLetter.localScale = _bigLetterBaseScale * pulse;
        }

        private void UpdateTiles(float time)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].Target == null)
                    continue;

                float x = Mathf.Sin(time * _tiles[i].FloatFrequencyX + _tiles[i].Seed) * _tiles[i].FloatAmplitudeX;
                float y = Mathf.Cos(time * _tiles[i].FloatFrequencyY + _tiles[i].Seed) * _tiles[i].FloatAmplitudeY;

                _tiles[i].Target.anchoredPosition = _tiles[i].BasePosition + new Vector2(x, y);
            }
        }

        [System.Serializable]
        public class OrbitTile
        {
            public RectTransform Target;

            [Header("Float")]
            public float FloatAmplitudeX = 4f;
            public float FloatAmplitudeY = 6f;
            public float FloatFrequencyX = 0.9f;
            public float FloatFrequencyY = 1.2f;

            [HideInInspector] public Vector2 BasePosition;
            [HideInInspector] public float Seed;
        }
    }
}