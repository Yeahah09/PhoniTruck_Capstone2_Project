using UnityEngine;

public class Cloud_Text : MonoBehaviour
{
    [SerializeField] private GameObject prompt;
    [SerializeField] private AudioSource letterSound;

    private PixelPromptManager promptManager;

    private void Start()
    {
        promptManager = FindFirstObjectByType<PixelPromptManager>();

        // This prompt starts hidden
        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (promptManager != null && prompt != null)
        {
            promptManager.ShowPrompt(prompt);
            letterSound.Play();
        }
    }
}