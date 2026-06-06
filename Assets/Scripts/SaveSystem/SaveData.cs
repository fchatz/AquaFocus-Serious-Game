using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // ---------------------------
    // COINS
    // ---------------------------
    public int totalCoins;

    // ---------------------------
    // OLD GAME STATS
    // ---------------------------
    public int totalBlueCollected;
    public int totalRedCollected;
    public float bestAccuracy;
    public int totalSessionsPlayed;

    // ---------------------------
    // SKINS
    // ---------------------------
    public int selectedSkinIndex;
    public bool[] unlockedSkins;

    // ---------------------------
    // DAILY MISSIONS
    // ---------------------------
    public DailyMissionSet dailyMissionSet;               // All missions of today
    public Dictionary<string, int> dailyMissionProgress;  // (optional)
    public string dailyMissionLastReset;                  // yyyy-MM-dd
    public int playtimeMinutesToday;                      // minutes accumulated today

    //map unlock
    public bool map2Unlocked;
    public bool map3Unlocked;

    public int map1SessionsPlayed;
    public int map2SessionsPlayed;


    // ---------------------------
    // CONSTRUCTOR (IMPORTANT!)
    // ---------------------------
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

        //test unlock maps
        map2Unlocked = false;
        map3Unlocked = false;
        map1SessionsPlayed = 0;
        map2SessionsPlayed = 0;
    }
}
