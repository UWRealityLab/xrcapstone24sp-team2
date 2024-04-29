using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(AvatarQuestionManager))]
public class AvatarQuestionManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draws the default inspector

        AvatarQuestionManager script = (AvatarQuestionManager)target;

        // Check if there are any sections loaded to avoid out-of-index errors
        if (script.SectionQuestions != null && script.SectionQuestions.Count > 0)
        {
            if (GUILayout.Button("Ask Random Question"))
            {
                // Randomly pick a section
                List<string> keys = script.SectionQuestions.Keys.ToList();
                int sectionIndex = Random.Range(0, keys.Count);
                string selectedSection = keys[sectionIndex];

                // Now pick a random question from the selected section
                List<string> questionsInSection = script.SectionQuestions[selectedSection];
                if (questionsInSection.Count > 0)
                {
                    int questionIndex = Random.Range(0, questionsInSection.Count);
                    string questionText = questionsInSection[questionIndex];  // Get the question text
                    script.AskQuestion(questionText);  // Call the AskQuestion method with the question text
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No questions are loaded or available.", MessageType.Warning);
        }
    }
}
