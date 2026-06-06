using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProfile
{
    public string id;                // GUID
    public string name;
    public int age;
    public string sex;
    public int avatarIndex;

    public string parentPasswordHash;

    // Minimal stats
    public int totalBlueCollected;
    public int totalRedCollected;
    public int totalSessionsPlayed;
    public float bestAccuracy;

  //  public List<SessionResult> sessionHistory = new List<SessionResult>();
}
