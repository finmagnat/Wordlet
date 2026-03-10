using UnityEngine;

namespace Core.UI.Components
{
    public class HeroLettersOrbit : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private RectTransform _orbitRoot;
        [SerializeField] private RectTransform _bigLetter;

        [Header("Root Drift")]
        [SerializeField] private bool _useRootDrift = true;
        [SerializeField] private float _rootDriftAmplitudeX = 4f;
        [SerializeField] private float _rootDriftAmplitudeY = 6f;
        [SerializeField] private float _rootDriftFrequencyX = 0.18f;
        [SerializeField] private float _rootDriftFrequencyY = 0.24f;

        [Header("Big Letter")]
        [SerializeField] private float _bigLetterPulseAmplitude = 0.018f;
        [SerializeField] private float _bigLetterPulseFrequency = 0.75f;

        [Header("Tiles")]
        [SerializeField] private OrbitTile[] _tiles;

        private Vector3 _bigLetterBaseScale;
        private Vector2 _orbitRootBasePosition;

        private void Awake()
        {
            if (_bigLetter != null)
                _bigLetterBaseScale = _bigLetter.localScale;

            if (_orbitRoot != null)
                _orbitRootBasePosition = _orbitRoot.anchoredPosition;

            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].Target == null)
                    continue;

                _tiles[i].BasePosition = _tiles[i].Target.anchoredPosition;
                _tiles[i].BaseRotationZ = NormalizeAngle(_tiles[i].Target.localEulerAngles.z);

                if (Mathf.Approximately(_tiles[i].Seed, 0f))
                    _tiles[i].Seed = Random.Range(0f, 100f);

                if (Mathf.Approximately(_tiles[i].JumpSeed, 0f))
                    _tiles[i].JumpSeed = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;

            UpdateRootDrift(time);
            UpdateBigLetter(time);
            UpdateTiles(time);
        }

        private void UpdateRootDrift(float time)
        {
            if (_orbitRoot == null || !_useRootDrift)
                return;

            float x = Mathf.Sin(time * _rootDriftFrequencyX) * _rootDriftAmplitudeX;
            float y = Mathf.Cos(time * _rootDriftFrequencyY) * _rootDriftAmplitudeY;

            _orbitRoot.anchoredPosition = _orbitRootBasePosition + new Vector2(x, y);
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
                OrbitTile tile = _tiles[i];

                if (tile.Target == null)
                    continue;

                float x = Mathf.Sin(time * tile.FloatFrequencyX + tile.Seed) * tile.FloatAmplitudeX;
                float y = Mathf.Cos(time * tile.FloatFrequencyY + tile.Seed) * tile.FloatAmplitudeY;

                float jump = 0f;
                if (tile.UseJump)
                {
                    // Редкие мягкие “бульки”, без резких рывков.
                    float jumpWave = Mathf.Sin(time * tile.JumpFrequency + tile.JumpSeed);
                    jump = Mathf.Max(0f, jumpWave) * tile.JumpAmplitude;
                }

                tile.Target.anchoredPosition = tile.BasePosition + new Vector2(x, y + jump);

                float wobble = Mathf.Sin(time * tile.WobbleFrequency + tile.Seed) * tile.WobbleAngle;
                float z = tile.BaseRotationZ + wobble;
                tile.Target.localRotation = Quaternion.Euler(0f, 0f, z);
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
                angle -= 360f;

            while (angle < -180f)
                angle += 360f;

            return angle;
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

            [Header("Wobble")]
            public float WobbleAngle = 5f;
            public float WobbleFrequency = 0.9f;

            [Header("Optional Jump")]
            public bool UseJump = false;
            public float JumpAmplitude = 8f;
            public float JumpFrequency = 0.45f;

            [Header("Random")]
            public float Seed;

            [HideInInspector] public Vector2 BasePosition;
            [HideInInspector] public float BaseRotationZ;
            [HideInInspector] public float JumpSeed;
        }
    }
}