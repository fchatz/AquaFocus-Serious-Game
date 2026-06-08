using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int totalCoins;
    public int totalBlueCollected;
    public int totalRedCollected;
    public float bestAccuracy;
    public int totalSessionsPlayed;
    public int selectedSkinIndex;
    public bool[] unlockedSkins;
    public DailyMissionSet dailyMissionSet;               
    public Dictionary<string, int> dailyMissionProgress; 
    public string dailyMissionLastReset;                  
    public int playtimeMinutesToday;                    
    public bool map2Unlocked;
    public bool map3Unlocked;
    public int map1SessionsPlayed;
    public int map2SessionsPlayed;

    // Constructor
    public SaveData()
    {
        totalCoins = 0;
        totalBlueCollected = 0;
        totalRedCollected = 0;
        bestAccuracy = 0f;
        totalSessionsPlayed = 0;
        unlockedSkins = new bool[10];
        selectedSkinIndex = 0;
        dailyMissionSet = new DailyMissionSet();
        dailyMissionSet.missions = new List<DailyMission>();
        dailyMissionSet.lastGeneratedDate = "";
        dailyMissionProgress = new Dictionary<string, int>();
        dailyMissionLastReset = "";
        playtimeMinutesToday = 0;
        map2Unlocked = false;
        map3Unlocked = false;
        map1SessionsPlayed = 0;
        map2SessionsPlayed = 0;
    }
}
