using UnityEngine;

namespace Core.Config
{
    [System.Serializable]
    public struct ComplexityAISettings
    {
        [Tooltip("Уровень сложности игры с ИИ")]
        public ComplexityAI СomplexityAiLevel;
        [Tooltip("Приоритетное количество символов в слове для данного уровня сложности")]
        public uint MaxWordLength;
        [Tooltip("Случайное количество символов в слове для данного уровня сложности (между минимальным и максимальным)")]
        public bool IsRandomWordLength;
        [Tooltip("Максимально допустимое количество пропусков")]
        public uint MaxPasses;
    }
}