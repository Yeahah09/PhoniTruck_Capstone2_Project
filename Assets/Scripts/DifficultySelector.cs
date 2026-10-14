using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    public void SelectDifficulty(string difficulty)
    {
        // Save chosen difficulty in PlayerPrefs (or pass via Firebase)
        PlayerPrefs.SetString("SelectedDifficulty", difficulty);
        PlayerPrefs.Save();

        // Load the main game scene
        SceneManager.LoadScene(1);
    }
}
