using DataStorage;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class PlayerObjectStartManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerCanvas;
        [SerializeField] private GameObject playerCamera;

        private void Start()
        {
            if (!IsOwner) return;
            playerCanvas.SetActive(true);
            playerCamera.SetActive(true);
            if (PlayerPrefs.GetInt(PpKeys.KeyIsHost) == 1)
            {
                GetComponentInChildren<HostGameSessionUIManager>(true).gameObject.SetActive(true);
            }
            else
            {
                GetComponentInChildren<ClientGameSessionUIManager>(true).gameObject.SetActive(true);
            }
        }
    }
}