using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PodiumController : MonoBehaviour
    {
        #region Components

        [SerializeField]
        private GameObject restartButtonGameObject;
        
        [SerializeField]
        private GameObject startButtonGameObject;

        [SerializeField] private Button qaButton;
        [SerializeField] private TMP_Text qaButtonText;

        #endregion

        #region UI Functions

        public void QAPressed()
        {
            // Hide other podium buttons.
            restartButtonGameObject.SetActive(false);
            startButtonGameObject.SetActive(false);
            
            // Disable Q/A button and display loading
            qaButton.interactable = false;
            qaButtonText.text = "Loading...";
        }

        public void QAReceived()
        {
            // Show other podium buttons.
            restartButtonGameObject.SetActive(true);
            startButtonGameObject.SetActive(true);
            
            // Re-enable Q/A button and display Q/A
            qaButton.interactable = true;
            qaButtonText.text = "Q/A";
        }

        #endregion
    }
}
