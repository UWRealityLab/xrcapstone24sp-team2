using System;
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
        [SerializeField] Recording record;
        [SerializeField] PaceDisplayController paceDisplay;
        [SerializeField] GameObject displayGrade;
        [SerializeField] GradeDisplay gradeDisplay;
        private Boolean isDone = false;
        private Boolean buttonPressed = false;

        #endregion

        #region UI Functions

        public void QAPressed()
        {
            if (!isDone)
            {
                // Hide other podium buttons.
                restartButtonGameObject.SetActive(false);
                startButtonGameObject.SetActive(false);

                // Disable Q/A button and display loading
                qaButton.interactable = false;
                qaButtonText.text = "Loading...";
                buttonPressed = true;
            } else
            {
                record.CreateGraph();
                displayGrade.SetActive(true);
                gradeDisplay.DisplayGrade();
                startButtonGameObject.SetActive(false);
                qaButton.gameObject.SetActive(false);
                paceDisplay.CreateGraph();

            }
            
        }

        public void QAReceived()
        {
            if (!isDone && buttonPressed)
            {
                // Show other podium buttons.
                restartButtonGameObject.SetActive(true);
                startButtonGameObject.SetActive(true);

                // Re-enable Q/A button and display Q/A
                qaButton.interactable = true;
                qaButtonText.text = "Done";
                isDone = true;
            }
            
        }

        public void ClearResults()
        {
            displayGrade.SetActive(false);
            record.DisableGraph();
            isDone = false;
            qaButtonText.text = "Q/A";
            restartButtonGameObject.SetActive(false);
            startButtonGameObject.SetActive(true);
            paceDisplay.HideGraph();
            buttonPressed = false;
            qaButton.gameObject.SetActive(false);
        }

        public Boolean done()
        {
            return isDone;
        }

        #endregion
    }
}
