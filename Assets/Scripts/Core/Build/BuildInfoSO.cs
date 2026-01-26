using UnityEngine;

namespace Core.Build
{
    [CreateAssetMenu(menuName = "Core/Build/Build Info", fileName = "BuildInfo")]
    public class BuildInfoSO : ScriptableObject
    {
        [Header("Generated at build time")]
        public string buildUtc;
        public string buildLocal;
        public string unityVersion;
        public string platform;
        public string configuration; // Dev/Release условно
        public string versionName;
        public int androidVersionCode; // если хочешь хранить вручную/авто
    }
}