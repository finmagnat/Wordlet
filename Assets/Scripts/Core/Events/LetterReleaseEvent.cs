using UnityEngine;

namespace Core.Events
{
    public class LetterReleaseEvent : IGameEvent
    {
        public string letter;
        public Vector3 position;
    }
}