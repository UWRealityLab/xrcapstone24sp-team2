using UnityEngine;

public class AvatarGenerationTrigger : MonoBehaviour
{
    public ChatGPTManager chatGPTManager;

    void Start()
    {
        if (chatGPTManager != null)
            chatGPTManager.OnResponsesReady.AddListener(HandleResponsesReady);
    }

    private void HandleResponsesReady()
    {
        Debug.Log("ResponsesReady event received. Starting avatar generation...");
        AvatarCreator.Main();
    }

    void OnDestroy()
    {
        if (chatGPTManager != null)
            chatGPTManager.OnResponsesReady.RemoveListener(HandleResponsesReady);
    }
}
