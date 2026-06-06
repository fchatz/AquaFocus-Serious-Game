using System;

[Serializable]
public class ProfileHeader
{
    public string id;        // GUID
    public string name;
    public int age;
    public string sex;
    public int avatarIndex;
    public string lastPlayed;  // string date for simplicity
}
