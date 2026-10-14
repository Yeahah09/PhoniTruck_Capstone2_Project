using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseAuthAdmin : MonoBehaviour
{
    [Header("Admin Signup UI Elements")]
    public TMP_InputField signupNameInput;
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TMP_Text signupStatusText;
    public GameObject signupPanel;

    [Header("Admin Login UI Elements")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginStatusText;
    public GameObject loginPanel;

    [Header("Admin Dashboard Panel")]
    public GameObject adminLoggedPanel;

    [Header("Admin Dashboard UI Elements")]
    public TMP_Text adminNameText;
    public TMP_InputField teacherEmailInput;   // input field for teacher Gmail
    public TMP_Text allTeachersText;           // shows all teachers under this admin
    public TMP_Dropdown teacherDropdown;       // dropdown for teachers

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
            Debug.Log("User is signed in: " + auth.CurrentUser.Email);
        else
            Debug.Log("No user signed in.");
    }

    public void SignupAdmin()
    {
        string name = signupNameInput.text;
        string email = signupEmailInput.text;
        string password = signupPasswordInput.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    signupStatusText.text = "Admin Signup Failed: " + task.Exception;
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                signupStatusText.text = "Admin Signup Success: " + newUser.Email;

                UserProfileData userData = new UserProfileData(newUser.Email, "Admin", name, "", "");
                string json = JsonUtility.ToJson(userData);
                dbRef.Child("users").Child("admins").Child(newUser.UserId).SetRawJsonValueAsync(json);

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

    public void LoginAdmin()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    loginStatusText.text = "Admin Login Failed: " + task.Exception;
                    return;
                }

                FirebaseUser user = task.Result.User;

                if (!user.IsEmailVerified)
                {
                    loginStatusText.text = "Please verify your email before logging in.";
                    auth.SignOut();
                    return;
                }

                // Check role before showing admin panel
                dbRef.Child("users").Child("admins").Child(user.UserId).GetValueAsync()
                    .ContinueWithOnMainThread(dbTask => {
                        if (dbTask.IsCompleted && dbTask.Result.Exists)
                        {
                            loginStatusText.text = "Admin Login Success: " + user.Email;
                            loginPanel.SetActive(false);
                            adminLoggedPanel.SetActive(true);

                            string adminName = dbTask.Result.Child("name").Value.ToString();
                            adminNameText.text = "Welcome, " + adminName + "!";

                            LoadTeachersIntoDropdown();
                        }
                        else
                        {
                            loginStatusText.text = "Not an admin account.";
                            auth.SignOut();
                        }
                    });

                loginEmailInput.text = "";
                loginPasswordInput.text = "";
            });
    }

    public void LogoutAdmin()
    {
        if (auth != null)
        {
            auth.SignOut();
            adminLoggedPanel.SetActive(false);
            loginPanel.SetActive(true);

            loginStatusText.text = "You have logged out.";
            loginEmailInput.text = "";
            loginPasswordInput.text = "";
        }
    }

    // Add teacher by Gmail (only if teacher exists in DB)
    public void AddTeacherByEmail()
    {
        string teacherEmail = teacherEmailInput.text;
        if (string.IsNullOrEmpty(teacherEmail))
        {
            allTeachersText.text = "Please enter a teacher Gmail.";
            return;
        }

        dbRef.Child("users").Child("teachers").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                allTeachersText.text = "Error checking teachers: " + task.Exception;
                return;
            }

            bool found = false;
            string teacherKey = "";

            foreach (var child in task.Result.Children)
            {
                if (child.Child("email").Value != null && child.Child("email").Value.ToString() == teacherEmail)
                {
                    found = true;
                    teacherKey = child.Key;
                    break;
                }
            }

            if (found)
            {
                // Link teacher to this admin
                dbRef.Child("users").Child("teachers").Child(teacherKey).Child("admin").SetValueAsync(auth.CurrentUser.UserId);
                allTeachersText.text = "Teacher linked to this admin: " + teacherEmail;
                teacherEmailInput.text = "";
                LoadTeachersIntoDropdown();
            }
            else
            {
                allTeachersText.text = "Teacher email not found in database. Cannot add.";
            }
        });
    }

    // Load teachers linked to this admin into dropdown
    private void LoadTeachersIntoDropdown()
    {
        if (auth.CurrentUser == null) return;

        string adminUid = auth.CurrentUser.UserId;

        dbRef.Child("users").Child("teachers").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) return;

            DataSnapshot snapshot = task.Result;
            teacherDropdown.ClearOptions();

            List<string> teacherList = new List<string>();
            foreach (var child in snapshot.Children)
            {
                if (child.Child("admin").Value != null && child.Child("admin").Value.ToString() == adminUid)
                {
                    string teacherEmail = child.Child("email").Value.ToString();
                    teacherList.Add(teacherEmail);
                }
            }

            teacherDropdown.AddOptions(teacherList);
            allTeachersText.text = "" + string.Join(", ", teacherList);
        });
    }

    // Remove teacher only from this admin (clear admin field)
    public void RemoveTeacherFromAdmin()
    {
        if (teacherDropdown.options.Count == 0) return;

        string selectedTeacherEmail = teacherDropdown.options[teacherDropdown.value].text;

        dbRef.Child("users").Child("teachers").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    if (child.Child("email").Value.ToString() == selectedTeacherEmail)
                    {
                        dbRef.Child("users").Child("teachers").Child(child.Key).Child("admin").SetValueAsync("");
                        allTeachersText.text = "Teacher removed from this admin: " + selectedTeacherEmail;
                        LoadTeachersIntoDropdown();
                        break;
                    }
                }
            }
        });
    }
}
