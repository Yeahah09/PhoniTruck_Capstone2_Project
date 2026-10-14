using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class PhonicsGameManager : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    [Header("UI Elements")]
    public TMP_Text wordBubbleText;
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    [Header("Prefabs")]
    public GameObject cloudPrefab;
    public Transform cloudSpawnArea;
    public List<DropZone> dropZones;

    [Header("Audio Manager")]
    public PhonicsAudioManager audioManager; // reference to audio manager

    private Difficulty currentDifficulty;
    private List<string> wordPool = new List<string>();
    private int currentWordIndex;
    private int score;
    private int lives = 3;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    // Word lists
    private readonly string[] easyWords = {
        "cat","ship","flag","bad","trap","hen","clap","drum","step","plan",
        "fish","rock","train","flown","swing","snake","chair","brush","spoon","clock"
    };

    private readonly string[] mediumWords = {
        "beach","travel","sheep","float","bread","cream","plate","snake","cloud","beach",
        "flick","plum","cube","light","flight","brush","thumb","whale","frog","green"
    };

    private readonly string[] hardWords = {
        "knight","write","lamb","comb","phone","graph","measure","giant","city","ocean",
        "caught","earth","storm","flower","thought","bright","through","beautiful","photograph","celebration"
    };

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        string diff = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        if (diff == "Medium") SetDifficulty(Difficulty.Medium);
        else if (diff == "Hard") SetDifficulty(Difficulty.Hard);
        else SetDifficulty(Difficulty.Easy);
    }

    public void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        wordPool.Clear();

        switch (diff)
        {
            case Difficulty.Easy: wordPool.AddRange(easyWords); break;
            case Difficulty.Medium: wordPool.AddRange(mediumWords); break;
            case Difficulty.Hard: wordPool.AddRange(hardWords); break;
        }

        ShuffleWords();
        currentWordIndex = 0;
        score = 0;
        lives = 3;
        gameOverPanel.SetActive(false);

        UpdateUI();
        ShowNextWord();
    }

    private void ShuffleWords()
    {
        for (int i = 0; i < wordPool.Count; i++)
        {
            int rand = Random.Range(i, wordPool.Count);
            string tempWord = wordPool[i];
            wordPool[i] = wordPool[rand];
            wordPool[rand] = tempWord;
        }
    }

    private void ShowNextWord()
    {
        if (currentWordIndex < wordPool.Count)
        {
            string targetWord = wordPool[currentWordIndex];
            wordBubbleText.text = ""; // hidden until built

            // clear old clouds
            foreach (Transform child in cloudSpawnArea)
                Destroy(child.gameObject);

            // ✅ spawn correct phoneme clouds
            foreach (char c in targetWord)
            {
                GameObject cloud = Instantiate(cloudPrefab, cloudSpawnArea);
                cloud.GetComponent<DraggablePhonic>().phonicLetter = c.ToString();
            }

            // ✅ spawn distractor clouds (wrong choices)
            string allPhonemes = "abcdefghijklmnopqrstuvwxyz";
            int distractorCount = 0;

            // scale distractors by difficulty
            switch (currentDifficulty)
            {
                case Difficulty.Easy: distractorCount = 1; break;
                case Difficulty.Medium: distractorCount = 2; break;
                case Difficulty.Hard: distractorCount = 3; break;
            }

            for (int i = 0; i < distractorCount; i++)
            {
                char randomPhoneme;
                // ensure distractor is not part of the target word
                do
                {
                    randomPhoneme = allPhonemes[Random.Range(0, allPhonemes.Length)];
                } while (targetWord.Contains(randomPhoneme.ToString()));

                GameObject cloud = Instantiate(cloudPrefab, cloudSpawnArea);
                cloud.GetComponent<DraggablePhonic>().phonicLetter = randomPhoneme.ToString();
            }

            // clear drop zones
            foreach (var zone in dropZones)
                zone.ClearSlot();

            // play audio for the target word
            audioManager.PlayWord(targetWord);
        }
        else
        {
            EndGame(true);
        }
    }


    public void CheckWord()
    {
        string assembledWord = "";
        foreach (var zone in dropZones)
            assembledWord += zone.slotText.text;

        string targetWord = wordPool[currentWordIndex];

        if (assembledWord.ToLower() == targetWord.ToLower())
        {
            LogWordResult(targetWord, true);
            OnCorrectAnswer();
            foreach (var zone in dropZones) zone.HighlightCorrect();
            wordBubbleText.text = targetWord;
        }
        else
        {
            LogWordResult(targetWord, false);
            OnWrongAnswer();
        }
    }

    private void OnCorrectAnswer()
    {
        int baseScore = 0;
        switch (currentDifficulty)
        {
            case Difficulty.Easy: baseScore = 200; break;
            case Difficulty.Medium: baseScore = 300; break;
            case Difficulty.Hard: baseScore = 500; break;
        }

        score += baseScore;
        currentWordIndex++;

        if (lives == 3 && currentWordIndex == wordPool.Count)
        {
            score += 50; // bonus
        }

        UpdateUI();
        SaveStudentProgress();
        Invoke("ShowNextWord", 1.5f);
    }

    private void OnWrongAnswer()
    {
        lives--;
        UpdateUI();
        if (lives <= 0) EndGame(false);
    }

    private void EndGame(bool completed)
    {
        gameOverPanel.SetActive(true);
        wordBubbleText.text = completed ? "Well done!" : "Game Over!";
        SaveStudentProgress();
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        livesText.text = "Lives: " + lives;
    }

    // ✅ Save student progress to Firebase
    private void SaveStudentProgress()
    {
        if (auth.CurrentUser == null) return;

        string studentUid = auth.CurrentUser.UserId;

        dbRef.Child("users").Child("students").Child(studentUid).Child("score").SetValueAsync(score);
        dbRef.Child("users").Child("students").Child(studentUid).Child("difficulty").SetValueAsync(currentDifficulty.ToString());
    }

    // ✅ Log each word attempt
    private void LogWordResult(string word, bool correct)
    {
        if (auth.CurrentUser == null) return;

        string studentUid = auth.CurrentUser.UserId;
        string logKey = dbRef.Child("users").Child("students").Child(studentUid).Child("history").Push().Key;

        var logData = new Dictionary<string, object>
        {
            { "word", word },
            { "result", correct ? "correct" : "wrong" }
        };

        dbRef.Child("users").Child("students").Child(studentUid).Child("history").Child(logKey).SetValueAsync(logData);
    }
}
