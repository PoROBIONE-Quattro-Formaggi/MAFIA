using UnityEngine;

namespace UI
{
    public class ClientGameSessionUIManager : MonoBehaviour
    {
        public GameObject infoBar;
        public GameObject alibiPrompt;
        public GameObject alibiInput;
        public GameObject voteButton;
        public GameObject okButton;
        public GameObject rolePrompt;
        
        private void OnEnable()
        {
            rolePrompt.SetActive(true);
            okButton.SetActive(true);
        }
    }
}
