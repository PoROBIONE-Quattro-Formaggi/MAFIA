using Unity.Netcode;

namespace Managers
{
    public class NetworkCommunicationManager : NetworkBehaviour
    {
        public static NetworkCommunicationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetworkCommunicationManager>();
                }

                return _instance;
            }
        }

        public bool IsNetworkSpawned { get; private set; }

        private static NetworkCommunicationManager _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            IsNetworkSpawned = true;
        }

        public static void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public static void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}