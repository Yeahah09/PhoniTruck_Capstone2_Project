using UnityEngine;

[System.Serializable]
public class StudentProfileData
{
    public string name;
    public string role;
    public string teacher; // UID of the teacher
    public int score;      // student score

    public StudentProfileData(string name, string role, string teacher, int score = 0)
    {
        this.name = name;
        this.role = role;
        this.teacher = teacher;
        this.score = score;
    }
}
