using UnityEngine;

namespace UI.Screens
{
    public class MainMenuScreen : UIScreen
    {
        public void OnPlayClicked()
        {
            Debug.Log("Play button clicked!");
        }

        public void OnSettingsClicked()
        {
            Debug.Log("Settings clicked!");
        }
    }
}