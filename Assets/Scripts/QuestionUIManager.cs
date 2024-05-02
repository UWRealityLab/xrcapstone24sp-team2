using UnityEngine;
using UnityEngine.UI;

public class QuestionUIManager : MonoBehaviour
{
    public Button askButton;
    public AvatarQuestionManager avatarQuestionManager;

    void Start()
    {
        askButton.onClick.AddListener(avatarQuestionManager.AskQuestion);
    }
}
