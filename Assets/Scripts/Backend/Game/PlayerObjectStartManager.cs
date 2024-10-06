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
        [SerializeField] private GameObject eventSystem;

        private void Start()
        {
            if (!IsOwner) return;
            GameObject.Find("Loading").SetActive(false);
            playerCanvas.SetActive(true);
            playerCamera.SetActive(true);
            eventSystem.SetActive(true);
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