using DataStorage;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerGameController : MonoBehaviour
    {
        public TextMeshProUGUI playerQuoteText;
        public GameObject goVoteButton;


        private void OnEnable()
        {
            if (goVoteButton.activeSelf) SetPlayerQuoteString();
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void SetPlayerQuoteString()
        {
            var playerQuoteString = $"[{PlayerData.Name}] ";
        
            playerQuoteString += PlayerData.Role switch
            {
                // Assign question to information prompt
                "Mafia" => "I vote for to kill _",
                "Doctor" => "I vote to save _",
                "Resident" => "I think that _ is sus", // TODO we should display here the 'funny questions' polls I think (?)
                _ => playerQuoteString
            };
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }
    }
}
