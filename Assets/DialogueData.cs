using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Story/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Global Settings")]
    public Sprite defaultBackground; // Fallback background
    public string speakerName;

    [Header("The Story")]
    public StoryFrame[] sentences; // This used to be string[], now it is complex!
}

// This creates the "box" inside the Element slot that holds both Text AND Image
[System.Serializable] 
public struct StoryFrame
{
    [TextArea(3, 10)]
    public string text;
    public Sprite frameImage; // Drag your specific scene image here!
}