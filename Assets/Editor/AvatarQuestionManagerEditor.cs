using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AvatarQuestionManager))]
public class AvatarQuestionManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        AvatarQuestionManager script = (AvatarQuestionManager)target;

        // Check if there are any questions loaded to avoid out-of-index errors
        if (script.allQuestions != null && script.allQuestions.Count > 0)
        {
            if (GUILayout.Button("Ask Random Question"))
            {
                // Randomly pick a question from the loaded questions
                int index = Random.Range(0, script.allQuestions.Count);
                string questionText = script.allQuestions[index];  // Get the question text
                script.AskQuestion();  // Call the AskQuestion method directly
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No questions are loaded or available.", MessageType.Warning);
        }
    }
}