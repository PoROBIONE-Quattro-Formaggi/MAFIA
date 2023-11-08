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
            if (!IsOwner)
            {
                Destroy(gameObject);
            }

            playerCanvas.SetActive(true);
            playerCamera.SetActive(true);
        }
    }
}