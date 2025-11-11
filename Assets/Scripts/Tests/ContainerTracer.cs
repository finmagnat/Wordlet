using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Tests
{

    public class ContainerTracer : MonoBehaviour
    {
        private readonly DiContainer _container;

        public ContainerTracer(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            Debug.Log($"🧩 IInitializable triggered in container: {_container}");
        }
    }
}