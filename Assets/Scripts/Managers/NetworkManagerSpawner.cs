using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class NetworkManagerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject networkManager;

        private void Awake()
        {
            if (FindObjectOfType<NetworkManager>() != null) return;
            Instantiate(networkManager);
        }
    }
}