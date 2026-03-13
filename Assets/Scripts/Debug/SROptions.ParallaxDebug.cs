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
        public int GyroStrengthX
        {
            get => _parallaxController != null ? _parallaxController.GyroStrengthX : 0;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroStrengthX = value;
            }
        }

        [Category("Parallax")]
        public int GyroStrengthY
        {
            get => _parallaxController != null ? _parallaxController.GyroStrengthY : 0;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroStrengthY = value;
            }
        }

        [Category("Parallax")]
        public int GyroSmoothing
        {
            get => _parallaxController != null ? _parallaxController.GyroSmoothing : 0;
            set
            {
                if (_parallaxController != null)
                    _parallaxController.GyroSmoothing = value;
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

        [Category("Parallax Actions")]
        public void SetTouchPreset()
        {
            if (_parallaxController == null) return;
            _parallaxController.Mode = UIParallaxMode.Touch;
        }

        [Category("Parallax Actions")]
        public void SetTouchAndGyroPreset()
        {
            if (_parallaxController == null) return;

            _parallaxController.Mode = UIParallaxMode.TouchAndGyro;
            _parallaxController.GyroStrengthX = 8;
            _parallaxController.GyroStrengthY = 8;
            _parallaxController.GyroSmoothing = 4;
        }
    }
}
