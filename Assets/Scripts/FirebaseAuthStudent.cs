using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseAuthStudent : MonoBehaviour
{
    [Header("Student UI Elements")]
    public TMP_InputField studentNameInput;
    public TMP_Dropdown teacherDropdown;
    public TMP_Text statusText;

    [Header("Game Panels")]
    public GameObject studentPanel;
    public GameObject difficultyPanel;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private Dictionary<int, string> teacherUidMap = new Dictionary<int, string>();

    // Store the logged-in student key globally
    public static string LoggedInStudentKey;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                LoadTeachersIntoDropdown();
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }

    private void LoadTeachersIntoDropdown()
    {
        dbRef.Child("users").Child("teachers").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                statusText.text = "Error loading teachers: " + args.DatabaseError.Message;
                return;
            }

            DataSnapshot snapshot = args.Snapshot;
            teacherDropdown.ClearOptions();
            teacherUidMap.Clear();

            var teacherList = new List<string>();
            int index = 0;

            foreach (var child in snapshot.Children)
            {
                if (child.Child("admin").Value != null && !string.IsNullOrEmpty(child.Child("admin").Value.ToString()))
                {
                    string teacherName = child.Child("name").Value?.ToString() ?? "Unnamed Teacher";
                    string teacherUid = child.Key;

                    teacherList.Add(teacherName);
                    teacherUidMap[index] = teacherUid;
                    index++;
                }
            }

            if (teacherList.Count > 0)
            {
                teacherDropdown.AddOptions(teacherList);
                statusText.text = "Teachers loaded successfully.";
            }
            else
            {
                statusText.text = "No teachers available.";
            }
        };
    }

    // Student joins under a teacher
    public void JoinStudent()
    {
        string studentName = studentNameInput.text;

        if (string.IsNullOrEmpty(studentName))
        {
            statusText.text = "Please enter your name.";
            return;
        }

        if (teacherDropdown.options.Count == 0)
        {
            statusText.text = "No teachers available.";
            return;
        }

        int selectedIndex = teacherDropdown.value;
        string teacherUid = teacherUidMap[selectedIndex];

        dbRef.Child("users").Child("students").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                statusText.text = "Error checking students: " + task.Exception;
                return;
            }

            DataSnapshot snapshot = task.Result;
            string existingKey = null;

            foreach (var child in snapshot.Children)
            {
                string existingName = child.Child("name").Value?.ToString();
                string existingTeacher = child.Child("teacher").Value?.ToString();

                if (existingName == studentName && existingTeacher == teacherUid)
                {
                    existingKey = child.Key;
                    break;
                }
            }

            if (existingKey != null)
            {
                // ✅ Student already exists → reuse
                LoggedInStudentKey = existingKey;
                statusText.text = "Welcome back, " + studentName + "!";
            }
            else
            {
                // ➕ Create new student profile with difficulty fields
                var studentData = new Dictionary<string, object>
                {
                    { "name", studentName },
                    { "role", "Student" },
                    { "teacher", teacherUid },
                    { "score", 0 },
                    { "easyPoints", 0 },
                    { "mediumPoints", 0 },
                    { "hardPoints", 0 },
                    { "difficulty", "" }
                };

                string studentKey = dbRef.Child("users").Child("students").Push().Key;
                dbRef.Child("users").Child("students").Child(studentKey).SetValueAsync(studentData);

                LoggedInStudentKey = studentKey;
                statusText.text = "Student joined under teacher successfully!";
                studentNameInput.text = "";
            }

            // Go to Screen
            PlayerPrefs.SetString("NextScene", "Main Game");
            SceneManager.LoadScene("LoadingScreen");
        });
    }

    // Update student score
    public void UpdateScore(string studentKey, int newScore)
    {
        dbRef.Child("users").Child("students").Child(studentKey).Child("score").SetValueAsync(newScore);
        statusText.text = "Score updated to " + newScore;
    }
}
