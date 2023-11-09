using UnityEngine;

namespace UI
{
    public class HostGameSessionUIManager : MonoBehaviour
    {
        public GameObject mafiaStatus;
        public GameObject doctorStatus;
        public GameObject lastDeath;
        
        private void OnEnable()
        {
            // First night from lobby game start
            mafiaStatus.gameObject.SetActive(true);
            doctorStatus.gameObject.SetActive(true);
        }
    }
}
