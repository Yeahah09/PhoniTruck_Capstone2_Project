using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;

public class DifficultySelector : MonoBehaviour
{
    private DatabaseReference dbRef;

    public GameObject difficultyPanel;
    public GameObject gamePanel;
    public PhonicsGameManager gameManager;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SelectDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("SelectedDifficulty", difficulty);
        PlayerPrefs.Save();

        string studentKey = FirebaseAuthStudent.LoggedInStudentKey;
        if (!string.IsNullOrEmpty(studentKey))
        {
            // ✅ Only save the chosen difficulty, no bonus points
            dbRef.Child("users").Child("students").Child(studentKey)
                 .Child("difficulty").SetValueAsync(difficulty);
        }

        // Hide difficulty menu, show game panel, and start the game
        difficultyPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameManager.StartGame();
    }
}
