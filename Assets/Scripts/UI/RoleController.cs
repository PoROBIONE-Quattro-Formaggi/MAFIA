using TMPro;
using UnityEngine;

namespace UI
{
    public class RoleController : MonoBehaviour
    {
        [Header("PROMPTS")]
        public TextMeshProUGUI prompt;
        public TextMeshProUGUI paragraph1;
        public TextMeshProUGUI paragraph2;
        public GameObject mafiaRole;
        public GameObject citizenRole;
        public GameObject doctorRole;

        [Header("BUTTONS")] 
        public GameObject okButton;

        [Header("SCREENS TO CHANGE TO")] 
        public GameObject night;

        private GameObject _roleScreen;

        private void OnStart()
        {
            _roleScreen = GetComponent<GameObject>();
        }

        private void OnDisable()
        {
            prompt.text = "You are ...";
            mafiaRole.SetActive(false);
            citizenRole.SetActive(false);
            doctorRole.SetActive(false);
            okButton.SetActive(false);
            paragraph1.gameObject.SetActive(false);
            paragraph2.gameObject.SetActive(false);
        }

        public void DisplayRole(string role)
        {
            // Set text values for prompts
            switch (role)
            {
                case "Mafia":
                    DisplayMafia();
                    break;
                case "Doctor":
                    DisplayDoctor();
                    break;
                case "Resident":
                    DisplayCitizen();
                    break;
            }
        
            // Show prompts and ok button
            paragraph1.gameObject.SetActive(true);
            paragraph2.gameObject.SetActive(true);
            okButton.SetActive(true);
        }

        private void DisplayMafia()
        {
            prompt.text = "You are in the";
            paragraph1.text = "Choose a target for elimination during the night voting.";
            paragraph2.text = "Select victims wisely to conquer the city without revealing your identity.";
            mafiaRole.SetActive(true);
        }
    
        private void DisplayDoctor()
        {
            prompt.text = "You are the";
            paragraph1.text = "Eliminate suspected Mafia members during the day voting.";
            paragraph2.text = "During the night voting select one person to protect from being killed by the Mafia.";
            doctorRole.SetActive(true);
        }
    
        private void DisplayCitizen()
        {
            prompt.text = "You are a";
            paragraph1.text = "Eliminate suspected Mafia members during the day voting.";
            paragraph2.text = "Be vigilant and make smart decisions to avoid being killed by the mafia.";
            citizenRole.SetActive(true);
        }

        public void D_OnOkButtonClicked()
        {
            Debug.Log("Ok button clicked");
        }

    }
}
