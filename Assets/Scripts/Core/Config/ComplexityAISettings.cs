using UnityEngine;

namespace Core.Config
{
    [System.Serializable]
    public struct ComplexityAISettings
    {
        [Tooltip("Уровень сложности игры с ИИ")]
        public ComplexityAI СomplexityAiLevel;
        
        [Tooltip("Максимально допустимое количество пропусков")]
        public uint MaxPasses;
        
        [Tooltip("Случайное количество символов в слове для данного уровня сложности (между минимальным и максимальным)")]
        public bool IsRandomWordLength;
        
        [Tooltip("Приоритетное количество символов в слове для данного уровня сложности: Max")]
        public RangeInteger WordLength;
    }

    [System.Serializable]
    public struct RangeInteger
    {
        public int Min;
        public int Max;
    }
}