using UnityEngine;

namespace UI
{
    public class HelpButtonController : MonoBehaviour
    {
        #region Components

        [SerializeField] private GameObject helpPanel;

        #endregion

        #region UI Functions

        public void ToggleHelpPanel()
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
        }

        #endregion
    }
}