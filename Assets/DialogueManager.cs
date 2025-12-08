using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

public class DialogueManager : MonoBehaviour
{
    [Header("Navigation Settings")]
    public string nextLevelName; 
    public float typingSpeed = 0.04f; // Speed of typing (lower is faster)

    [Header("UI Components")]
    public TextMeshProUGUI dialogueText;
    public Image backgroundDisplay;
    public Button nextButton;

    [Header("Data Sources")]
    public DialogueData personaA_Data; 
    public DialogueData personaB_Data; 
    public DialogueData personaC_Data; 

    private DialogueData currentData;
    private int sentenceIndex = 0;
    private bool isTyping = false; // To check if we are currently typing

    void Start()
    {
        // 1. Select Data
        if (GameContext.SelectedPersona == PlayerPersona.PersonaA) currentData = personaA_Data;
        else if (GameContext.SelectedPersona == PlayerPersona.PersonaB) currentData = personaB_Data;
        else if (GameContext.SelectedPersona == PlayerPersona.PersonaC) currentData = personaC_Data;

        // 2. Setup
        if(currentData != null)
        {
            if (currentData.defaultBackground != null && backgroundDisplay != null) 
                backgroundDisplay.sprite = currentData.defaultBackground;
            
            // Start the first sentence
            StartCoroutine(TypeSentence(currentData.sentences[0]));
        }
        
        nextButton.onClick.AddListener(OnNextClicked);
    }

    // The Magic Function for Typing Effect
    IEnumerator TypeSentence(StoryFrame frame)
    {
        isTyping = true;
        dialogueText.text = ""; // Clear text
        
        // Update Image immediately
        if (frame.frameImage != null && backgroundDisplay != null)
            backgroundDisplay.sprite = frame.frameImage;

        // Type letter by letter
        foreach (char letter in frame.text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void OnNextClicked()
    {
        // If we are still typing, complete the sentence instantly!
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentData.sentences[sentenceIndex].text;
            isTyping = false;
            return;
        }

        // If typing is done, go to next sentence
        sentenceIndex++;
        if (sentenceIndex < currentData.sentences.Length)
        {
            StartCoroutine(TypeSentence(currentData.sentences[sentenceIndex]));
        }
        else
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }
}