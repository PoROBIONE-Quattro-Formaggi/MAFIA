using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class NetworkManagerSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;

        private void Awake()
        {
            if (FindObjectOfType<NetworkManager>() != null) return;
            Instantiate(networkManager.gameObject);
        }
    }
}