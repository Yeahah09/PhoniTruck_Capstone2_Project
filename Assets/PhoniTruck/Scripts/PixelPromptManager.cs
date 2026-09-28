using UnityEngine;

public class PixelPromptManager : MonoBehaviour
{
    private GameObject currentPrompt;

    public void ShowPrompt(GameObject newPrompt)
    {
        // Hide the previous prompt
        if (currentPrompt != null)
        {
            currentPrompt.SetActive(false);
        }

        // Show the new prompt
        if (newPrompt != null)
        {
            newPrompt.SetActive(true);
            currentPrompt = newPrompt;
        }
    }
}