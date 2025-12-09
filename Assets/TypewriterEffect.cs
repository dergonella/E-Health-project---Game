using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Settings")]
    public float typingSpeed = 0.04f; // Smaller number = Faster typing

    private TextMeshProUGUI textComponent;
    private string fullText;

    void Start()
    {
        // 1. Find the TextMeshPro component attached to this object
        textComponent = GetComponent<TextMeshProUGUI>();

        // 2. Save the text you typed in the Inspector
        fullText = textComponent.text;

        // 3. Clear the text box so it starts empty
        textComponent.text = "";

        // 4. Start the typing animation
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        // Loop through every letter in the saved text
        foreach (char letter in fullText.ToCharArray())
        {
            textComponent.text += letter; // Add one letter
            yield return new WaitForSeconds(typingSpeed); // Wait a tiny bit
        }
    }
}
