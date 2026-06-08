using System;
using System.Collections.Generic;
using UnityEngine;


public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public SaveData Data;
    public string currentProfileId;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // No profile selected yet, create empty data
        if (string.IsNullOrEmpty(currentProfileId) == false)
        {
            LoadGame(); 
        }
        else
        {
            Data = new SaveData();
        }

    }

    public void SetCurrentProfile(string profileId)
    {
        currentProfileId = profileId;
        LoadGame();
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

        SaveData loaded = SaveSystem.Load(currentProfileId);

        // Fallback if save load data is missing
        if (loaded == null)
        {
            Debug.LogWarning("No save found. Creating new save while preserving old data if any.");
            if (Data == null)
                Data = new SaveData();

            SaveSystem.Save(Data, currentProfileId);
            return;
        }

        Data = loaded;
    }


    public void SaveGame()
    {
        if (string.IsNullOrEmpty(currentProfileId))
        {
            Debug.LogWarning("SaveGame called, but no profile selected.");
            return;
        }

        SaveSystem.Save(Data, currentProfileId);
    }
}
