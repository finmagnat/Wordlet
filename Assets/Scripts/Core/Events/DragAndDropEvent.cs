using UnityEngine;

namespace Core.Events
{
    public class DragAndDropEvent : IGameEvent
    {
        public GameObject gameObject;
        public Vector3 position;
    }
}