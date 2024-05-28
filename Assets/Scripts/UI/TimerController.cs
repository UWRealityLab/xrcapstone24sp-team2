using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerController : MonoBehaviour
    {
        [SerializeField]
        private TranscriptionLogger transcriptionLogger;

        #region Components

        [SerializeField]
        private TMP_Text timerText;

        [SerializeField]
        private TMP_Text timerStateText;

        [SerializeField]
        private GameObject restartButtonGameObject;

        [SerializeField] private GameObject qaButtonGameObject;

        [SerializeField]
        private TMP_Text qaButtonText;


        #endregion

        #region Properties

        private bool _isTimerRunning;
        private float _elapsedTime;
        public bool isDone;

        #endregion

        #region Unity

        private void FixedUpdate()
        {
            // No updates when the timer is not running
            if (!_isTimerRunning)
            {
                return;
            }

            // update timer text
            var time = TimeSpan.FromSeconds(_elapsedTime);
            timerText.text = time.ToString("mm':'ss");

            // increment elapsed time
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

            // Hide reset and QA buttons.
            restartButtonGameObject.SetActive(false);
            qaButtonGameObject.SetActive(false);
        }

        public void PauseTimer()
        {
            _isTimerRunning = false;
            timerStateText.text = "Resume";

            // Show reset and QA buttons.
            restartButtonGameObject.SetActive(true);
            qaButtonGameObject.SetActive(true);
        }

        public void ResetTimer()
        {
            _elapsedTime = 0;
            timerText.text = "00:00";
            if (!_isTimerRunning)
            {
                timerStateText.text = "Start";
            }
            transcriptionLogger.ResetTranscript();
        }

        public string GetCurrentTime()
        {
            TimeSpan time = TimeSpan.FromSeconds(_elapsedTime);
            return time.ToString("mm':'ss");
        }

        public float GetElapsedTimeInSeconds()
        {
            return _elapsedTime;
        }

        public bool IsRunning()
        {
            return _isTimerRunning;
        }

        #endregion
    }

}
