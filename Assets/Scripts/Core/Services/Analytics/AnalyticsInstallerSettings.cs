using System;
using UnityEngine;

namespace Core.Services
{
    [Serializable]
    public sealed class AnalyticsInstallerSettings
    {
        [SerializeField] private bool _enableGameAnalytics = true;

        public bool EnableGameAnalytics => _enableGameAnalytics;
    }
}
