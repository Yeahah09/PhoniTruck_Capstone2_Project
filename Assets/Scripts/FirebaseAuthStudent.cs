using UnityEngine;
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
    public TMP_Dropdown teacherDropdown;   // Dropdown to select teacher
    public TMP_Text statusText;

    [Header("Game Panels")]
    public GameObject studentPanel;        // panel with name + teacher selection
    public GameObject difficultyPanel;     // panel for difficulty selection

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    // Mapping dropdown index → teacher UID
    private Dictionary<int, string> teacherUidMap = new Dictionary<int, string>();

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
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

    // Load teachers from DB into dropdown
    private void LoadTeachersIntoDropdown()
    {
        dbRef.Child("users").Child("teachers").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                statusText.text = "Error loading teachers: " + task.Exception;
                return;
            }

            DataSnapshot snapshot = task.Result;
            teacherDropdown.ClearOptions();
            teacherUidMap.Clear();

            var teacherList = new List<string>();
            int index = 0;

            foreach (var child in snapshot.Children)
            {
                // Only show teachers that are linked to an admin
                if (child.Child("admin").Value != null && !string.IsNullOrEmpty(child.Child("admin").Value.ToString()))
                {
                    string teacherName = child.Child("name").Value != null ? child.Child("name").Value.ToString() : "Unnamed Teacher";
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
        });
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

        // Create student profile with score = 0
        StudentProfileData studentData = new StudentProfileData(studentName, "Student", teacherUid, 0);
        string json = JsonUtility.ToJson(studentData);

        // Generate a unique key for student (push ID)
        string studentKey = dbRef.Child("users").Child("students").Push().Key;
        dbRef.Child("users").Child("students").Child(studentKey).SetRawJsonValueAsync(json);

        statusText.text = "Student joined under teacher successfully!";
        studentNameInput.text = "";

        // Switch panels: hide student panel, show difficulty panel
        if (studentPanel != null) studentPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
    }

    // Update student score
    public void UpdateScore(string studentKey, int newScore)
    {
        dbRef.Child("users").Child("students").Child(studentKey).Child("score").SetValueAsync(newScore);
        statusText.text = "Score updated to " + newScore;
    }
}
