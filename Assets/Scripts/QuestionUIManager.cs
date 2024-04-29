using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestionUIManager : MonoBehaviour
{
    public Dropdown sectionDropdown;
    public Button askButton;
    public AvatarQuestionManager avatarQuestionManager;

    // Declare a dictionary to hold section questions locally
    private Dictionary<string, List<string>> sections;

    void Start()
    {
        askButton.onClick.AddListener(AskSelectedQuestion);

        // Subscribe to data ready event from avatar question manager
        avatarQuestionManager.OnDataReady += SetupSections;
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        avatarQuestionManager.OnDataReady -= SetupSections;
    }

    // Setup sections when data is ready
    private void SetupSections()
    {
        sections = avatarQuestionManager.SectionQuestions;  // Store the section questions locally
        sectionDropdown.options.Clear();

        foreach (var section in sections)
        {
            Dropdown.OptionData newOption = new Dropdown.OptionData(section.Key);
            sectionDropdown.options.Add(newOption);
        }

        sectionDropdown.RefreshShownValue();
        // Disable the ask button if no sections are available
        askButton.interactable = sectionDropdown.options.Count > 0;
    }

    private void AskSelectedQuestion()
    {
        if (sections != null && sections.Count > 0 && sectionDropdown.options.Count > sectionDropdown.value)
        {
            string selectedSection = sectionDropdown.options[sectionDropdown.value].text;
            List<string> questions = sections[selectedSection];
            if (questions != null && questions.Count > 0)
            {
                string questionText = questions[0]; // Example: pick the first question for simplicity
                avatarQuestionManager.AskQuestion(questionText);
            }
        }
    }
}
