using System.Collections.Generic;
using UI.Hub.View;
using UnityEngine;

namespace UI.Hub.Managers
{
    public class HubFrontManager : MonoBehaviour
    {
        [field: SerializeField]
        private ScreenChanger screenChanger;

        [field: SerializeField] 
        private List<BaseScreen> screenCollection = new (); 
            
            
        
        // TODO: implement BaseScreen class and inherit from it
        // - subscribe to all screen events here and react accordingly
        
        
        
        
        
        public void Initialize()
        {
            // TODO: return active screen?
            screenChanger.ChangeToMainScreen();
        }

        private void AttachToEvents()
        {
            
        }

        private void DetachFromEvents()
        {
            
        }
    }
}