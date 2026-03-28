using System.ComponentModel;
using UI.Parallax;
using UnityEngine;

namespace Core.DebugTools
{
    public sealed class ParallaxDebugOptions
    {
        private readonly UIParallaxController _parallaxController;

        public ParallaxDebugOptions(UIParallaxController parallaxController)
        {
            _parallaxController = parallaxController;
        }

        [Category("Parallax")]
        public UIParallaxMode Mode
        {
            get => _parallaxController != null ? _parallaxController.Mode : UIParallaxMode.Touch;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.Mode = value;
            }
        }

        [Category("Parallax")]
        public float TouchStrengthX
        {
            get => _parallaxController != null ? _parallaxController.TouchStrengthX : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.TouchStrengthX = value;
            }
        }

        [Category("Parallax")]
        public float TouchStrengthY
        {
            get => _parallaxController != null ? _parallaxController.TouchStrengthY : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.TouchStrengthY = value;
            }
        }

        [Category("Parallax")]
        public float GyroStrengthX
        {
            get => _parallaxController != null ? _parallaxController.GyroStrengthX : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroStrengthX = value;
            }
        }

        [Category("Parallax")]
        public float GyroStrengthY
        {
            get => _parallaxController != null ? _parallaxController.GyroStrengthY : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroStrengthY = value;
            }
        }

        [Category("Parallax")]
        public float GyroSmoothing
        {
            get => _parallaxController != null ? _parallaxController.GyroSmoothing : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroSmoothing = value;
            }
        }

        [Category("Parallax")]
        public float GyroMultiplier
        {
            get => _parallaxController != null ? _parallaxController.GyroMultiplier : 0f;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroMultiplier = value;
            }
        }

        [Category("Parallax")]
        public bool SimulateGyroInEditor
        {
            get => _parallaxController != null && _parallaxController.SimulateGyroInEditor;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.SimulateGyroInEditor = value;
            }
        }

        [Category("Parallax")]
        public bool InvertX
        {
            get => _parallaxController != null && _parallaxController.InvertX;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.InvertX = value;
            }
        }

        [Category("Parallax")]
        public bool InvertY
        {
            get => _parallaxController != null && _parallaxController.InvertY;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.InvertY = value;
            }
        }

        [Category("Parallax Debug")]
        public string RawAcceleration
        {
            get
            {
                if (_parallaxController == null) return "N/A";
                Vector2 a = _parallaxController.DebugRawAcceleration;
                return $"x={a.x:F3}, y={a.y:F3}";
            }
        }

        [Category("Parallax Debug")]
        public string SmoothedGyro
        {
            get
            {
                if (_parallaxController == null) return "N/A";
                Vector2 a = _parallaxController.DebugGyroSmoothed;
                return $"x={a.x:F3}, y={a.y:F3}";
            }
        }

        [Category("Parallax Debug")]
        public string FinalOffset
        {
            get
            {
                if (_parallaxController == null) return "N/A";
                Vector2 a = _parallaxController.DebugFinalOffset;
                return $"x={a.x:F3}, y={a.y:F3}";
            }
        }

        [Category("Parallax Actions")]
        public void SetTouchPreset()
        {
            if (_parallaxController == null) return;

            _parallaxController.Mode = UIParallaxMode.Touch;
            _parallaxController.TouchStrengthX = 1.5f;
            _parallaxController.TouchStrengthY = 2.25f;
        }

        [Category("Parallax Actions")]
        public void SetTouchAndGyroPreset()
        {
            if (_parallaxController == null) return;

            _parallaxController.Mode = UIParallaxMode.TouchAndGyro;
            _parallaxController.TouchStrengthX = 1.5f;
            _parallaxController.TouchStrengthY = 2.25f;
            _parallaxController.GyroStrengthX = 8f;
            _parallaxController.GyroStrengthY = 10f;
            _parallaxController.GyroSmoothing = 6f;
            _parallaxController.GyroMultiplier = 1.5f;
        }

        [Category("Parallax Actions")]
        public void ResetLayers()
        {
            _parallaxController?.ResetLayersToCurrentPosition();
        }
    }
}