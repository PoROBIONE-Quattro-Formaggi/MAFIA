using TMPro;
using UnityEngine;

namespace UI
{
    public class HostGameSessionUIManager : MonoBehaviour
    {
        public TextMeshProUGUI mafiaStatus;
        public TextMeshProUGUI doctorStatus;
        public TextMeshProUGUI lastDeath;
        
        private void OnEnable()
        {
            // First night from lobby game start
            mafiaStatus.gameObject.SetActive(true);
            doctorStatus.gameObject.SetActive(true);
        }
    }
}
