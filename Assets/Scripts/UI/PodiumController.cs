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
        [SerializeField] GameObject displayGrade;
        [SerializeField] GradeDisplay gradeDisplay;
        private Boolean isDone = false;

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
            } else
            {
                record.CreateGraph();
                displayGrade.SetActive(true);
                gradeDisplay.DisplayGrade();
                startButtonGameObject.SetActive(false);
                qaButton.gameObject.SetActive(false);
                
            }
            
        }

        public void QAReceived()
        {
            if (!isDone)
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
        }

        #endregion
    }
}
