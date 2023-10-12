using Unity.Netcode;
using UnityEngine;

public class PlayerUIController : NetworkBehaviour
{
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private GameObject playerCamera;

    private void Start()
    {
        if (!IsOwner) return;
        playerCanvas.SetActive(true);
        playerCamera.SetActive(true);
    }

    private void Update()
    {
        if (!IsOwner) return;
    }
}