using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneChangerController : MonoBehaviour
    {
        #region Components

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        #endregion

        #region Properties

        [SerializeField] private int _sceneNumber;

        private bool _isConfirming = false;

        #endregion

        #region UI Functions

        public void OnPressed()
        {
            // If confirming, change scene.
            if (_isConfirming)
            {
                // Disable interaction and indicate loading.
                _button.interactable = false;
                _text.text = "Loading...";
                
                // Load scene.
                SceneManager.LoadSceneAsync(_sceneNumber);
            }
            else
            {
                // Change state to confirming.
                _isConfirming = true;
                
                // Change button text.
                _text.text = "Change Scene?";
                
                // Reset after 3 seconds.
                Invoke(nameof(Reset), 3f);
            }
        }
        
        private void Reset()
        {
            _isConfirming = false;
            _text.text = "Change Scene";
        }

        #endregion
    }
}
