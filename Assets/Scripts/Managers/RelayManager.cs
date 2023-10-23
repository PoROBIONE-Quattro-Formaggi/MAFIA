using System.Threading.Tasks;
using DataStorage;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RelayManager>();
                }

                return _instance;
            }
        }

        private static RelayManager _instance;
        private Allocation _createdAllocation;

        private string _joinCode;

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

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == Scenes.MainScene) return;
            var joinCode = PlayerPrefs.GetString(PpKeys.KeyStartGame);
            var isHost = PlayerPrefs.GetInt(PpKeys.KeyIsHost);
            if (joinCode == "0") // Error - no join code was registered
            {
                SceneChanger.ChangeToMainScene();
            }
            else if (isHost == 1)
            {
                CreateRelay();
            }
            else
            {
                JoinRelay(joinCode);
            }
        }

        public async Task<string> GetRelayCode(int maxClientsNum)
        {
            _createdAllocation = await RelayService.Instance.CreateAllocationAsync(maxClientsNum);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(_createdAllocation.AllocationId);
            return _joinCode;
        }

        private void CreateRelay()
        {
            try
            {
                var relayServerData = new RelayServerData(_createdAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }

        private static async void JoinRelay(string joinCode)
        {
            try
            {
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}