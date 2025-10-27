using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tests
{

    public class UniTaskTest : MonoBehaviour
    {
        private async void Start()
        {
            Debug.Log("Waiting 1 second...");
            await UniTask.Delay(1000);
            Debug.Log("Done!");
        }
    }
}