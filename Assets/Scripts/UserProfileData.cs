using System;

[System.Serializable]
public class UserProfileData
{
    public string email;
    public string role;
    public string name;
    public string admin;   // only used for teachers (points to admin UID)
    public string teacher; // only used for students (points to teacher UID)

    public UserProfileData(string email, string role, string name, string admin = "", string teacher = "")
    {
        this.email = email;
        this.role = role;
        this.name = name;
        this.admin = admin;
        this.teacher = teacher;
    }
}

