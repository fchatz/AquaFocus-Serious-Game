using UnityEngine;
using System;
using System.Collections.Generic;

public class DailyMissionManager : MonoBehaviour
{
    public static DailyMissionManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Initialize daily missions for the active profile
    public void InitializeDailyMissions()
    {
        if (GameDataManager.Instance.Data.dailyMissionSet == null)
            GameDataManager.Instance.Data.dailyMissionSet = new DailyMissionSet();

        var data = GameDataManager.Instance.Data;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // Generate missions if a new day is detected
        if (string.IsNullOrEmpty(data.dailyMissionSet.lastGeneratedDate) ||
            data.dailyMissionSet.lastGeneratedDate != today)
        {
            GenerateDailyMissions();
            data.dailyMissionSet.lastGeneratedDate = today;
            GameDataManager.Instance.SaveGame();
        }

        // Restore active playtime progress
        var playMission = GetMission("play_time");
        if (playMission != null)
        {
            playMission.progress = data.playtimeMinutesToday;

            if (playMission.progress >= playMission.target)
                playMission.completed = true;
        }
    }

    public void AddProgress(string missionId, int amount)
    {
        var missions = GameDataManager.Instance.Data.dailyMissionSet.missions;

        foreach (var m in missions)
        {
            if (m.missionId == missionId && !m.completed)
            {
                m.progress += amount;

                if (missionId == "avoid_red_limit")
                {
                    if (m.progress >= m.target)
                        m.completed = true;
                }
                else
                {
                    if (m.progress >= m.target)
                        m.completed = true;
                }

                if (missionId == "play_time")
                {
                    GameDataManager.Instance.Data.playtimeMinutesToday = m.progress;
                }

                GameDataManager.Instance.SaveGame();
                return;
            }
        }
    }

    // Check if all active missions are completed
    public bool CheckAllCompleted()
    {
        foreach (var m in GameDataManager.Instance.Data.dailyMissionSet.missions)
            if (!m.completed) return false;

        return true;
    }

    // Process and claim mission rewards
    public bool ClaimMission(DailyMission mission)
    {
        if (!mission.completed || mission.claimed)
            return false;

        mission.claimed = true;

        CoinManager.Instance.AddCoins(mission.reward);

        GameDataManager.Instance.SaveGame();
        return true;
    }

    private DailyMission GetMission(string id)
    {
        foreach (var m in GameDataManager.Instance.Data.dailyMissionSet.missions)
            if (m.missionId == id) return m;
        return null;
    }

    private void GenerateDailyMissions()
    {
        var list = GameDataManager.Instance.Data.dailyMissionSet.missions;
        list.Clear();

        list.Add(new DailyMission("collect_blue", 50, 40));
        list.Add(new DailyMission("avoid_red_limit", 20, 50));
        list.Add(new DailyMission("hit_correct", 15, 40));
        list.Add(new DailyMission("avoid_mines", 10, 60));
        list.Add(new DailyMission("play_time", 20, 50));
    }
}