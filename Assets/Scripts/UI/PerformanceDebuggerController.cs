using TMPro;
using UnityEngine;

namespace UI
{
    public class PerformanceDebuggerController : MonoBehaviour
    {
        #region Components

        [SerializeField]
        private TMP_Text fpsLabel;

        #endregion

        #region Unity

        private void Update()
        {
            fpsLabel.text = $"FPS: {(int)(1 / Time.deltaTime)}";
        }

        #endregion
    }
}
