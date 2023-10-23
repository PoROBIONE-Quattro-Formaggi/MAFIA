using UnityEngine;

namespace Managers
{
    public class NetworkManagerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject networkManager;

        private void Awake()
        {
            Instantiate(networkManager);
        }
    }
}