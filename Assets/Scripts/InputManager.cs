using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset actionAsset;
    private InputAction nextQuestionAction;

    private void Awake()
    {
        // Obtain a reference to the action map and actions
        var actionMap = actionAsset.FindActionMap("UI Controls");
        nextQuestionAction = actionMap.FindAction("NextQuestion");

        // Register an event listener for the action
        nextQuestionAction.performed += _ => AskNextQuestion();
    }

    private void AskNextQuestion()
    {
        // You can directly call your QuestionManager method here
        FindObjectOfType<QuestionManager>().AskNextQuestion();
    }

    private void OnEnable()
    {
        // Enable the action when the object becomes active
        nextQuestionAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the action when the object becomes inactive
        nextQuestionAction.Disable();
    }
}
