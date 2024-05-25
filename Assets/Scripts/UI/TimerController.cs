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
        }

        // return the current time of the timer (format = 'mm:ss')
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
