using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerController : MonoBehaviour
    {
        #region Components

        [SerializeField]
        private TMP_Text timerText;

        [SerializeField]
        private TMP_Text timerStateText;

        #endregion

        #region Properties

        private bool _isTimerRunning;
        private float _elapsedTime;

        #endregion

        #region Unity

        private void FixedUpdate()
        {
            if (!_isTimerRunning)
            {
                return;
            }

            // Convert elapsed time to a TimeSpan object.
            var time = TimeSpan.FromSeconds(_elapsedTime);
            timerText.text = time.ToString("mm':'ss");

            // Increment elapsed time.
            _elapsedTime += Time.fixedDeltaTime;
        }

        #endregion

        #region UI Functions

        public void ToggleTimer()
        {
            if (_isTimerRunning)
            {
                PauseTimer();
            }
            else
            {
                ResumeTimer();
            }
        }

        public void ResumeTimer()
        {
            _isTimerRunning = true;
            timerStateText.text = "Pause";
        }

        public void PauseTimer()
        {
            _isTimerRunning = false;
            timerStateText.text = "Resume";
        }

        public void ResetTimer()
        {
            _elapsedTime = 0;
            timerText.text = "00:00";
            if (!_isTimerRunning)
            {
                timerStateText.text = "Start";
            }
        }

        #endregion
    }
}
