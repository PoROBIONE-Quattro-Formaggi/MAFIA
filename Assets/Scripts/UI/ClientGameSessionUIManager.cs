using UnityEngine;

namespace UI
{
    public class ClientGameSessionUIManager : MonoBehaviour
    {
        public GameObject infoBar;
        public GameObject alibiPrompt;
        public GameObject alibiInput;
        public GameObject goVoteButton;
        public GameObject okButton;
        public GameObject rolePrompt;

        private void OnEnable()
        {
            rolePrompt.SetActive(true);
            okButton.SetActive(true);
        }

        public void OnOkButtonClicked()
        {
            okButton.SetActive(false);
            rolePrompt.SetActive(false);
            
            // TODO: set information for information prompt + actually animate prompt
            infoBar.SetActive(true);
            goVoteButton.SetActive(true);
        }

        public void OnGoVoteButtonClicked()
        {
            infoBar.SetActive(false);
            goVoteButton.SetActive(false);
        }
    }
}