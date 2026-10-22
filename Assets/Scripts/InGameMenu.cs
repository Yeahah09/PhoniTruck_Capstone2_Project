using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class InGameMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public GameObject PauseMenu;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text studentInfoText;
    [SerializeField] private TMP_Text teacherInfoText;
    [SerializeField] private TMP_Text pointsText;


    private DatabaseReference dbRef;

    private void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (!string.IsNullOrEmpty(FirebaseAuthStudent.LoggedInStudentKey))
        {
            // ✅ Attach live listener so points auto-update
            ListenToStudentInfo(FirebaseAuthStudent.LoggedInStudentKey);
        }
        else
        {
            Debug.LogError("No student key found. Did you log in first?");
        }
    }

    public void OnYesButtonClicked()
    {
        
        PlayerPrefs.SetString("NextScene", "GameMainMenu");
        SceneManager.LoadScene("LoadingScreen");
    }

    public void OnCancelButtonClicked()
    {
        PauseMenu.SetActive(false);
    }

    // ✅ Live listener for student info
    private void ListenToStudentInfo(string studentKey)
    {
        dbRef.Child("users").Child("students").Child(studentKey).ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null) return;
            DataSnapshot snapshot = args.Snapshot;

            string studentName = snapshot.Child("name").Value?.ToString() ?? "Unknown";
            string teacherUid = snapshot.Child("teacher").Value?.ToString() ?? "None";

            string easyPoints = snapshot.Child("easyPoints").Value?.ToString() ?? "0";
            string easyCorrect = snapshot.Child("easyCorrect").Value?.ToString() ?? "0";
            string easyWrong = snapshot.Child("easyWrong").Value?.ToString() ?? "0";

            string mediumPoints = snapshot.Child("mediumPoints").Value?.ToString() ?? "0";
            string mediumCorrect = snapshot.Child("mediumCorrect").Value?.ToString() ?? "0";
            string mediumWrong = snapshot.Child("mediumWrong").Value?.ToString() ?? "0";

            string hardPoints = snapshot.Child("hardPoints").Value?.ToString() ?? "0";
            string hardCorrect = snapshot.Child("hardCorrect").Value?.ToString() ?? "0";
            string hardWrong = snapshot.Child("hardWrong").Value?.ToString() ?? "0";

            // ✅ Compute totals
            int easyTotal = int.Parse(easyCorrect) + int.Parse(easyWrong);
            int mediumTotal = int.Parse(mediumCorrect) + int.Parse(mediumWrong);
            int hardTotal = int.Parse(hardCorrect) + int.Parse(hardWrong);

            string easyPercent = easyTotal > 0 ? ((int.Parse(easyCorrect) * 100 / easyTotal)).ToString() + "%" : "0%";
            string mediumPercent = mediumTotal > 0 ? ((int.Parse(mediumCorrect) * 100 / mediumTotal)).ToString() + "%" : "0%";
            string hardPercent = hardTotal > 0 ? ((int.Parse(hardCorrect) * 100 / hardTotal)).ToString() + "%" : "0%";

            // ✅ Calculate total score from difficulty points
            int totalScore = int.Parse(easyPoints) + int.Parse(mediumPoints) + int.Parse(hardPoints);

            // ✅ Update UI text with line breaks
            studentInfoText.text = "Student: " + studentName;
            pointsText.text =
                $"Easy: {easyPoints} (Correct {easyCorrect} Wrong {easyWrong}, {easyPercent})\n" +
                $"Medium: {mediumPoints} (Correct {mediumCorrect} Wrong {mediumWrong}, {mediumPercent})\n" +
                $"Hard: {hardPoints} (Correct {hardCorrect} Wrong {hardWrong}, {hardPercent})\n" +
                $"Total: {totalScore}";

            // Teacher info
            dbRef.Child("users").Child("teachers").Child(teacherUid).GetValueAsync().ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled) return;
                DataSnapshot teacherSnap = t.Result;
                string teacherName = teacherSnap.Child("name").Value?.ToString() ?? "Unknown Teacher";
                teacherInfoText.text = "Teacher: " + teacherName;
            });
        };
    }
}
