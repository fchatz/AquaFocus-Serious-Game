using UnityEngine;


[System.Serializable]
public class ProfileData
{
    public string playerName;
    public int age;
    public Sprite avatar;
    public string gender;
    public string parentPasswordHash;

    public bool map2Unlocked = false;
    public bool map3Unlocked = false;

    // Progress tracking (optional but recommended)
    public int map1SessionsPlayed = 0;
    public int map2SessionsPlayed = 0;

}
