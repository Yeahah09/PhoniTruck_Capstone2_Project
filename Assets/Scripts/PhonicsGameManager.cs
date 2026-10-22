using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;

public class PhonicsGameManager : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    [Header("UI Elements")]
    public TMP_Text wordBubbleText;
    public TMP_Text latestScore; 
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    [Header("Achievement System")]
    public GameObject achievementPanel;
    public TMP_Text achievementTitleText;
    public TMP_Text achievementDescriptionText;

    private int correctStreak = 0;

    private bool unlocked5Streak;
    private bool unlocked10Streak;
    private bool unlocked15Streak;
    private bool unlocked20Streak;

    [Header("Word Description Panel")]
    public GameObject wordDescriptionPanel;   // The panel itself
    public TMP_Text wordBubbleTextCorrect;    // Drag the TMP_Text for the word
    public TMP_Text wordDescriptionText;      // Drag the TMP_Text for the description
    public UnityEngine.UI.Button nextWordButton;
    public UnityEngine.UI.Button soundButton;

    [Header("Word Audio Clips")]
    public List<AudioClip> wordAudioClips;   // Drag audio files here
    public AudioSource audioSource;          // Drag an AudioSource here

    // Counters
    private int easyPoints, mediumPoints, hardPoints;
    private int easyCorrect, easyWrong;
    private int mediumCorrect, mediumWrong;
    private int hardCorrect, hardWrong;
    private int latestScoreValue; // most recent score earned


    [Header("Prefabs")]
    public GameObject cloudPrefab;
    public Transform cloudSpawnArea;
    public List<DropZone> dropZones;

    [Header("Audio Manager")]
    public PhonicsAudioManager audioManager;

    private Difficulty currentDifficulty;
    private List<string> wordPool = new List<string>();
    private int currentWordIndex;
    private int currentLetterIndex;
    private int score;
    private int lives = 3;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    private readonly string[] easyWords = { "cat", "ship", "flag", "bad", "trap", "hen", "clap", "drum", "step", "plan" };
    private readonly string[] mediumWords = { "feather", "splendid", "crocodile", "strawberry", "blanket", "rooster", "cobweb", "contest", "stretching", "delight", "scramble", "triangle", "twinkle", "tremble", "shrivel", "costume", "thunder", "chapter", "dolphin", "crunchy" };
    private readonly string[] hardWords = { "trickster", "wonderful", "graduation", "congratulate", "delighted", "traveling", "benches", "creativity", "wondering", "wednesday", "thorough", "psychology", "sanitizing", "mischievous", "thankfully", "mindfully", "thoroughout", "beautiful", "photograph", "celebration" };

    // ✅ Simple descriptions for your words
    private Dictionary<string, string> wordDescriptions = new Dictionary<string, string>()
{
    { "cat", "A small animal that says meow." },
    { "ship", "A big boat that sails on water." },
    { "flag", "A piece of cloth that shows a country." },
    { "bad", "Something not good." },
    { "trap", "A thing used to catch animals." },
    { "hen", "A bird that lays eggs." },
    { "clap", "To put your hands together to make a sound." },
    { "drum", "A round instrument you hit to make music." },
    { "step", "To move your foot forward when walking." },
    { "plan", "An idea of what you want to do." },

    { "feather", "A light soft covering found on birds" },
    { "splendid", "Very impressive, beautiful, or excellent" },
    { "crocodile", "A large reptile that lives in rivers and wetlands" },
    { "strawberry", "A small sweet red fruit with seeds on the outside" },
    { "blanket", "A large piece of warm cloth used on a bed" },
    { "rooster", "An adult male chicken" },
    { "cobweb", "A spider's old or abandoned web" },
    { "contest", "A competition to determine a winner" },
    { "stretching", "Extending muscles or body parts to increase flexibility" },
    { "delight", "A feeling of great pleasure or happiness" },
    { "scramble", "To move or climb quickly using hands and feet" },
    { "triangle", "A shape with three sides and three angles" },
    { "twinkle", "To shine with a flickering or sparkling light" },
    { "tremble", "To shake slightly from fear, excitement, or cold" },
    { "shrivel", "To wrinkle and become smaller because of dryness" },
    { "costume", "Clothing worn to represent a character or theme" },
    { "thunder", "The loud sound that follows lightning" },
    { "chapter", "A main section of a book" },
    { "dolphin", "A highly intelligent marine mammal" },
    { "crunchy", "Having a firm texture that makes a crisp sound when bitten" },

    { "trickster", "Someone who likes to play tricks." },
    { "wonderful", "Something very good and nice." },
    { "graduation", "A ceremony when students finish school." },
    { "congratulate", "To say 'Well done!' to someone." },
    { "delighted", "Feeling very happy." },
    { "traveling", "Going from one place to another." },
    { "benches", "Long seats where people can sit." },
    { "creativity", "Using your imagination to make new things." },
    { "wondering", "Thinking about something with curiosity." },
    { "wednesday", "The third day of the school week." },
    { "thorough", "Doing something very carefully." },
    { "psychology", "The study of how people think and feel." },
    { "sanitizing", "Cleaning something to make it safe." },
    { "mischievous", "Playful but sometimes a little naughty." },
    { "thankfully", "Feeling glad or grateful." },
    { "mindfully", "Paying close attention to what you do." },
    { "throughout", "All the way through something." },
    { "beautiful", "Something very pretty to look at." },
    { "photograph", "A picture taken with a camera." },
    { "celebration", "A happy event like a party." }
};


    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        float volume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (volume <= 0.05f)
            volume = 0f;

        if (audioSource != null)
            audioSource.volume = volume;

        StartGame();
    }

    private Dictionary<string, AudioClip> wordAudioMap = new Dictionary<string, AudioClip>();

    void Awake()
    {
        // Build dictionary from inspector list
        foreach (var clip in wordAudioClips)
        {
            if (clip != null)
            {
                // Clip name must match the word string (e.g., "cat", "cloud")
                wordAudioMap[clip.name.ToLower()] = clip;
            }
        }
    }



    public void StartGame()
    {
        string diff = PlayerPrefs.GetString("SelectedDifficulty", "Easy");
        if (diff == "Medium") SetDifficulty(Difficulty.Medium);
        else if (diff == "Hard") SetDifficulty(Difficulty.Hard);
        else SetDifficulty(Difficulty.Easy);
    }

    public void PlayWordAudio(string word)
    {
        string key = word.ToLower();

        Debug.Log("Looking for word audio: " + key);

        if (wordAudioMap.ContainsKey(key))
        {
            Debug.Log("FOUND WORD AUDIO: " + key);

            audioSource.Stop();
            audioSource.clip = wordAudioMap[key];
            audioSource.Play();
        }
        else
        {
            Debug.LogError("MISSING AUDIO FOR: " + key);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource == null)
            return;

        if (volume <= 0.05f)
            volume = 0f;

        audioSource.volume = volume;
    }

    public void OnSoundButtonClicked()
    {
        if (wordBubbleTextCorrect != null && !string.IsNullOrEmpty(wordBubbleTextCorrect.text))
        {
            //// PER LETTER 
            //PhonicsAudioManager.instance.PlayWord(wordBubbleTextCorrect.text);
            PlayWordAudio(wordBubbleTextCorrect.text);
        }
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
        lives = 3; // ✅ reset lives only
        gameOverPanel.SetActive(false);

        // ✅ Load saved progress instead of resetting
        LoadStudentProgress();

        ShowNextWord();
    }

    public void OnNextWordButtonClicked()
    {
        // Hide the panel
        wordDescriptionPanel.SetActive(false);

        // Continue with the next word
        ShowNextWord();
    }


    private string GetWordDescription(string word)
    {
        if (wordDescriptions.ContainsKey(word))
            return wordDescriptions[word];
        return "No description available.";
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
            wordBubbleText.text = targetWord;
            currentLetterIndex = 0;

            dropZones[0].gameObject.SetActive(true);
            dropZones[0].ClearSlot();

            ShowNextLetter(targetWord, currentLetterIndex);
        }
        else
        {
            EndGame(true);
        }
    }

    private void ShowNextLetter(string targetWord, int letterIndex)
    {
        foreach (Transform child in cloudSpawnArea)
            Destroy(child.gameObject);

        char correctPhoneme = targetWord[letterIndex];

        // Build list of phonemes: 1 correct + 3 random distractors
        List<string> phonemes = new List<string>();
        phonemes.Add(correctPhoneme.ToString());

        string allPhonemes = "abcdefghijklmnopqrstuvwxyz";
        for (int i = 0; i < 3; i++)
        {
            char randomPhoneme;
            do
            {
                randomPhoneme = allPhonemes[Random.Range(0, allPhonemes.Length)];
            } while (randomPhoneme == correctPhoneme || phonemes.Contains(randomPhoneme.ToString()));

            phonemes.Add(randomPhoneme.ToString());
        }

        // ✅ Shuffle the list so correct answer isn’t always first
        for (int i = 0; i < phonemes.Count; i++)
        {
            int rand = Random.Range(i, phonemes.Count);
            string temp = phonemes[i];
            phonemes[i] = phonemes[rand];
            phonemes[rand] = temp;
        }

        // Spawn clouds in randomized order
        for (int i = 0; i < phonemes.Count; i++)
        {
            SpawnCloud(phonemes[i], i);
        }
    }


    private void SpawnCloud(string letter, int index)
    {
        GameObject cloud = Instantiate(cloudPrefab, cloudSpawnArea);
        cloud.GetComponent<DraggablePhonic>().SetLetter(letter);

        RectTransform rt = cloud.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(index * 150, 0);
    }

    public void CheckLetter(string chosenPhoneme)
    {
        string targetWord = wordPool[currentWordIndex];
        char correctPhoneme = targetWord[currentLetterIndex];

        if (chosenPhoneme.ToLower() == correctPhoneme.ToString().ToLower())
        {
            dropZones[0].slotText.text = chosenPhoneme;
            dropZones[0].HighlightCorrect();

            currentLetterIndex++;

            if (currentLetterIndex >= targetWord.Length)
            {
                wordBubbleText.text = targetWord;
                audioManager.PlayCorrect();
                OnCorrectAnswer();
                Invoke("ShowNextWord", 1.5f);
            }
            else
            {
                ShowNextLetter(targetWord, currentLetterIndex);
            }
        }
        else
        {
            audioManager.PlayWrong();
            OnWrongAnswer();
        }
    }

    public void OnNextCorrectWordButtonClicked()
    {
        wordDescriptionPanel.SetActive(false);
        ShowNextWord();
    }


    private void OnCorrectAnswer()
    {

        int points = 0;
        switch (currentDifficulty)
        {
            case Difficulty.Easy: points = 200; easyCorrect++; easyPoints += 200; break;
            case Difficulty.Medium: points = 300; mediumCorrect++; mediumPoints += 300; break;
            case Difficulty.Hard: points = 500; hardCorrect++; hardPoints += 500; break;
        }

        correctStreak++;
        if (correctStreak == 5 && !unlocked5Streak)
        {
            unlocked5Streak = true;

            UnlockAchievement(
                "Hot Streak",
                "5 Correct Answers In A Row");
            //PlayWordAudio(completedWord);
        }

        if (correctStreak == 10 && !unlocked10Streak)
        {
            unlocked10Streak = true;

            UnlockAchievement(
                "Super Streak",
                "10 Correct Answers In A Row");
            //PlayWordAudio(completedWord);
        }

        if (correctStreak == 15 && !unlocked15Streak)
        {
            unlocked15Streak = true;

            UnlockAchievement(
                "Amazing Streak",
                "15 Correct Answers In A Row");
        }

        if (correctStreak == 20 && !unlocked20Streak)
        {
            unlocked20Streak = true;

            UnlockAchievement(
                "Phonics Champion",
                "20 Correct Answers In A Row");
        }

        latestScoreValue += points;
        currentWordIndex++;

        UpdateUI();
        SaveStudentProgress();

        // ✅ Show description panel
        string completedWord = wordPool[currentWordIndex - 1];
        wordBubbleTextCorrect.text = completedWord;
        wordDescriptionText.text = GetWordDescription(completedWord);
        wordDescriptionPanel.SetActive(true);

        // ✅ Auto-play audio for the word
        PlayWordAudio(completedWord);
    }

    private void OnWrongAnswer()
    {
        lives--;
        correctStreak = 0;
        UpdateUI();

        if (lives <= 0)
        {
            latestScoreValue = 0; 

            switch (currentDifficulty)
            {
                case Difficulty.Easy: easyWrong++; break;
                case Difficulty.Medium: mediumWrong++; break;
                case Difficulty.Hard: hardWrong++; break;
            }

            EndGame(false);
        }
    }

    private void EndGame(bool completed)
    {
        gameOverPanel.SetActive(true);
        wordBubbleText.text = completed ? "Well done!" : "Game Over!";
        latestScoreValue = 0; 
        SaveStudentProgress();
    }




    private void UpdateUI()
    {
        scoreText.text = "Score: " + latestScoreValue.ToString();
        latestScore.text = latestScoreValue.ToString();
        livesText.text = "Lives: " + lives;
    }



    private void SaveStudentProgress()
    {
        if (string.IsNullOrEmpty(FirebaseAuthStudent.LoggedInStudentKey)) return;
        string studentUid = FirebaseAuthStudent.LoggedInStudentKey;

        // ✅ Compute total score from difficulty points
        int totalScore = easyPoints + mediumPoints + hardPoints;

        // Save total score and difficulty
        dbRef.Child("users").Child("students").Child(studentUid).Child("score").SetValueAsync(totalScore);
        dbRef.Child("users").Child("students").Child(studentUid).Child("difficulty").SetValueAsync(currentDifficulty.ToString());

        // Save Easy stats
        dbRef.Child("users").Child("students").Child(studentUid).Child("easyPoints").SetValueAsync(easyPoints);
        dbRef.Child("users").Child("students").Child(studentUid).Child("easyCorrect").SetValueAsync(easyCorrect);
        dbRef.Child("users").Child("students").Child(studentUid).Child("easyWrong").SetValueAsync(easyWrong);

        // Save Medium stats
        dbRef.Child("users").Child("students").Child(studentUid).Child("mediumPoints").SetValueAsync(mediumPoints);
        dbRef.Child("users").Child("students").Child(studentUid).Child("mediumCorrect").SetValueAsync(mediumCorrect);
        dbRef.Child("users").Child("students").Child(studentUid).Child("mediumWrong").SetValueAsync(mediumWrong);

        // Save Hard stats
        dbRef.Child("users").Child("students").Child(studentUid).Child("hardPoints").SetValueAsync(hardPoints);
        dbRef.Child("users").Child("students").Child(studentUid).Child("hardCorrect").SetValueAsync(hardCorrect);
        dbRef.Child("users").Child("students").Child(studentUid).Child("hardWrong").SetValueAsync(hardWrong);
    }


    private void LoadStudentProgress()
    {
        if (string.IsNullOrEmpty(FirebaseAuthStudent.LoggedInStudentKey)) return;
        string studentUid = FirebaseAuthStudent.LoggedInStudentKey;

        dbRef.Child("users").Child("students").Child(studentUid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snap = task.Result;

                score = int.Parse(snap.Child("score").Value?.ToString() ?? "0");
                easyPoints = int.Parse(snap.Child("easyPoints").Value?.ToString() ?? "0");
                mediumPoints = int.Parse(snap.Child("mediumPoints").Value?.ToString() ?? "0");
                hardPoints = int.Parse(snap.Child("hardPoints").Value?.ToString() ?? "0");

                easyCorrect = int.Parse(snap.Child("easyCorrect").Value?.ToString() ?? "0");
                easyWrong = int.Parse(snap.Child("easyWrong").Value?.ToString() ?? "0");
                mediumCorrect = int.Parse(snap.Child("mediumCorrect").Value?.ToString() ?? "0");
                mediumWrong = int.Parse(snap.Child("mediumWrong").Value?.ToString() ?? "0");
                hardCorrect = int.Parse(snap.Child("hardCorrect").Value?.ToString() ?? "0");
                hardWrong = int.Parse(snap.Child("hardWrong").Value?.ToString() ?? "0");

                UpdateUI();
            }
        });
    }

    private void ShowAchievement(string title, string description)
    {
        if (achievementPanel == null)
            return;

        achievementPanel.SetActive(true);

        achievementTitleText.text = title;
        achievementDescriptionText.text = description;

        CancelInvoke(nameof(HideAchievement));
        Invoke(nameof(HideAchievement), 4f);
    }

    private void HideAchievement()
    {
        achievementPanel.SetActive(false);
    }

    private void UnlockAchievement(string achievementName, string description)
    {
        ShowAchievement(achievementName, description);

        if (string.IsNullOrEmpty(FirebaseAuthStudent.LoggedInStudentKey))
            return;

        string studentUid =
            FirebaseAuthStudent.LoggedInStudentKey;

        dbRef.Child("users")
            .Child("students")
            .Child(studentUid)
            .Child("achievements")
            .Child(achievementName.Replace(" ", "_"))
            .SetValueAsync(true);
    }   























}
