using UnityEngine;

namespace Core.Config
{
    [System.Serializable]
    public struct ComplexityAISettings
    {
        [Tooltip("Уровень сложности игры с ИИ")]
        public ComplexityAI СomplexityAiLevel;
        [Tooltip("Приоритетное количество символов в слове для данного уровня сложности")]
        public uint WordLength;
        [Tooltip("Максимально допустимое количество пропусков")]
        public uint MaxPasses;
    }
}