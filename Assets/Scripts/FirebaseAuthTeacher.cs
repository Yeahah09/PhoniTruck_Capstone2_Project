using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseAuthTeacher : MonoBehaviour
{
    [Header("Teacher Signup UI Elements")]
    public TMP_InputField signupNameInput;
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TMP_Text signupStatusText;
    public GameObject signupPanel;

    [Header("Teacher Login UI Elements")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginStatusText;
    public GameObject loginPanel;

    [Header("Teacher Dashboard Panel")]
    public GameObject teacherLoggedPanel;
    public TMP_Text teacherNameText;
    public TMP_Text studentsAndScoresText;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;

                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null);
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != null)
            Debug.Log("Teacher signed in: " + auth.CurrentUser.Email);
        else
            Debug.Log("No teacher signed in.");
    }

    public void SignupTeacher()
    {
        string name = signupNameInput.text;
        string email = signupEmailInput.text;
        string password = signupPasswordInput.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(authTask => {
                if (authTask.IsFaulted || authTask.IsCanceled)
                {
                    signupStatusText.text = "Teacher Signup Failed: " + authTask.Exception;
                    return;
                }

                FirebaseUser newUser = authTask.Result.User;
                signupStatusText.text = "Teacher Signup Success: " + newUser.Email;

                // Do not auto-assign admin here
                string adminUid = "";

                UserProfileData userData = new UserProfileData(newUser.Email, "Teacher", name, adminUid, "");
                string json = JsonUtility.ToJson(userData);
                dbRef.Child("users").Child("teachers").Child(newUser.UserId).SetRawJsonValueAsync(json);

                newUser.SendEmailVerificationAsync().ContinueWithOnMainThread(verifyTask => {
                    if (verifyTask.IsFaulted || verifyTask.IsCanceled)
                        signupStatusText.text += "\nFailed to send verification email.";
                    else
                        signupStatusText.text += "\nVerification email sent!";
                });

                signupPanel.SetActive(false);
                loginPanel.SetActive(true);

                signupNameInput.text = "";
                signupEmailInput.text = "";
                signupPasswordInput.text = "";
            });
    }

    public void LoginTeacher()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    loginStatusText.text = "Teacher Login Failed: " + task.Exception;
                    return;
                }

                FirebaseUser user = task.Result.User;

                if (!user.IsEmailVerified)
                {
                    loginStatusText.text = "Please verify your email before logging in.";
                    auth.SignOut();
                    return;
                }

                // Check role before showing teacher panel
                dbRef.Child("users").Child("teachers").Child(user.UserId).GetValueAsync()
                    .ContinueWithOnMainThread(dbTask => {
                        if (dbTask.IsCompleted && dbTask.Result.Exists)
                        {
                            loginStatusText.text = "Teacher Login Success: " + user.Email;
                            loginPanel.SetActive(false);
                            teacherLoggedPanel.SetActive(true);

                            string teacherName = dbTask.Result.Child("name").Value.ToString();
                            teacherNameText.text = "Teacher’s name: " + teacherName;

                            LoadStudentsForTeacher();
                        }
                        else
                        {
                            loginStatusText.text = "Not a teacher account.";
                            auth.SignOut();
                        }
                    });

                loginEmailInput.text = "";
                loginPasswordInput.text = "";
            });
    }

    public void LogoutTeacher()
    {
        if (auth != null)
        {
            auth.SignOut();
            teacherLoggedPanel.SetActive(false);
            loginPanel.SetActive(true);

            loginStatusText.text = "You have logged out successfully.";
            loginEmailInput.text = "";
            loginPasswordInput.text = "";
        }
    }

    // Load students for the logged-in teacher
    public void LoadStudentsForTeacher()
    {
        if (auth.CurrentUser == null) return;

        string teacherUid = auth.CurrentUser.UserId;

        dbRef.Child("users").Child("students").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && task.Result.Exists)
            {
                List<string> studentLines = new List<string>();

                foreach (var child in task.Result.Children)
                {
                    if (child.Child("teacher").Value.ToString() == teacherUid)
                    {
                        string studentName = child.Child("name").Value.ToString();
                        int score = int.Parse(child.Child("score").Value.ToString());
                        studentLines.Add(studentName + " - Score: " + score);
                    }
                }

                studentsAndScoresText.text = "Students and Scores:\n" + string.Join("\n", studentLines);
            }
        });
    }
}
