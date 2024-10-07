using Unity.Netcode;
using UnityEngine;

namespace Backend.Hub
{
    public class NetworkManagerSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;

        public void Initialize()
        {
            if (FindObjectOfType<NetworkManager>() != null) return;
            Instantiate(networkManager.gameObject);
        }
    }
}