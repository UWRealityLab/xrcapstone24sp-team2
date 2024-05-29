using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using TMPro;
using UnityEngine;

public class GradeDisplay : MonoBehaviour
{
    [SerializeField] CommunicationManager communicationManager;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text textSuggestions;

    public void DisplayGrade()
    {
        string display = "Grade:\n";
        (string rubric, int grade, string feedback)[]  grade = communicationManager.GetGrade();
        int totalGrade = 0;
        for (int i = 0; i < grade.Length; i++)
        {
            totalGrade += grade[i].grade;
            display += grade[i].rubric + ": " + grade[i].grade + "\n";
            display += grade[i].feedback + "\n\n";
        }
        display += "Total Grade: " + totalGrade + "/50";
        text.text = display;
        string suggestionsText = "Suggestions:\n";
        List<string> suggestionList = communicationManager.GetSuggestions();
        for (int i = 0; i < suggestionList.Count; i++)
        {
            suggestionsText += " * " + suggestionList[i]  + "\n\n";
        }
        textSuggestions.text = suggestionsText;

    }

}
