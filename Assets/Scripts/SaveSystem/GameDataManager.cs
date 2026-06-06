using System;
using System.Collections.Generic;
using UnityEngine;


public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public SaveData Data;
    public string currentProfileId;   // which profile's save we're working with

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // No profile selected yet, so just create empty data
        if (string.IsNullOrEmpty(currentProfileId) == false)
        {
            LoadGame();  // load the selected profile's save
        }
        else
        {
            Data = new SaveData();
        }

    }

    // Call this when a profile is selected
    public void SetCurrentProfile(string profileId)
    {
        currentProfileId = profileId;
        LoadGame();
        // Ensure older saves (without the new fields) still work
        if (Data.unlockedSkins == null)
            Data.unlockedSkins = new bool[10];

        if (Data.dailyMissionSet == null)
            Data.dailyMissionSet = new DailyMissionSet();

        if (Data.dailyMissionSet.missions == null)
            Data.dailyMissionSet.missions = new List<DailyMission>();

        if (Data.dailyMissionProgress == null)
            Data.dailyMissionProgress = new Dictionary<string, int>();

    }


    public void LoadGame()
    {
        if (string.IsNullOrEmpty(currentProfileId))
        {
            Debug.LogWarning("LoadGame called, but no profile selected. Using empty data.");
            Data = new SaveData();
            return;
        }

        // Try load save file
        SaveData loaded = SaveSystem.Load(currentProfileId);

        // If load failed → DO NOT replace Data with new SaveData
        if (loaded == null)
        {
            Debug.LogWarning("No save found. Creating new save while preserving old data if any.");

            // Keep existing Data, do NOT wipe coins
            if (Data == null)
                Data = new SaveData();

            SaveSystem.Save(Data, currentProfileId);
            return;
        }

        // Successful load → assign it
        Data = loaded;
    }


    public void SaveGame()
    {
        if (string.IsNullOrEmpty(currentProfileId))
        {
            Debug.LogWarning("SaveGame called, but no profile selected. Ignoring.");
            return;
        }

        SaveSystem.Save(Data, currentProfileId);
    }
}
