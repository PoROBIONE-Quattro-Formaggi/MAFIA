using Backend.Hub.Managers;
using UnityEngine;

namespace Backend.Hub
{
    public class HubInitializer : MonoBehaviour
    {
        [SerializeField] private HubBackendManager hubBackendManager;

        private void Start()
        {
            hubBackendManager.Initialize();
            // TODO initialize frontend manager
        }
    }
}