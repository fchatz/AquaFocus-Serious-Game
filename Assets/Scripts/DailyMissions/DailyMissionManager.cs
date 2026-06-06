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

    // ----------------------------------------------------
    // INITIALIZE MISSIONS
    // ----------------------------------------------------
    public void InitializeDailyMissions()
    {
        if (GameDataManager.Instance.Data.dailyMissionSet == null)
            GameDataManager.Instance.Data.dailyMissionSet = new DailyMissionSet();

        var data = GameDataManager.Instance.Data;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        // NEW DAY → regenerate missions
        if (string.IsNullOrEmpty(data.dailyMissionSet.lastGeneratedDate) ||
            data.dailyMissionSet.lastGeneratedDate != today)
        {
            GenerateDailyMissions();
            data.dailyMissionSet.lastGeneratedDate = today;
            GameDataManager.Instance.SaveGame();
        }

        // Restore playtime progress
        var playMission = GetMission("play_time");
        if (playMission != null)
        {
            playMission.progress = data.playtimeMinutesToday;

            if (playMission.progress >= playMission.target)
                playMission.completed = true;
        }
    }

    // ----------------------------------------------------
    // CREATE MISSIONS
    // ----------------------------------------------------
    private void GenerateDailyMissions()
    {
        var data = GameDataManager.Instance.Data;

        data.dailyMissionSet.missions = new List<DailyMission>
        {
            new DailyMission("collect_blue", 50),
            new DailyMission("avoid_red_limit", 20),
            new DailyMission("hit_correct", 15),
            new DailyMission("avoid_mines", 10),
            new DailyMission("play_time", 20)
        };
    }

    // ----------------------------------------------------
    // GET SPECIFIC MISSION
    // ----------------------------------------------------
    public DailyMission GetMission(string id)
    {
        foreach (var m in GameDataManager.Instance.Data.dailyMissionSet.missions)
            if (m.missionId == id)
                return m;

        return null;
    }

    // ----------------------------------------------------
    // UPDATE PROGRESS
    // ----------------------------------------------------
    public void AddProgress(string missionId, int amount)
    {
        var missions = GameDataManager.Instance.Data.dailyMissionSet.missions;

        foreach (var m in missions)
        {
            if (m.missionId == missionId && !m.completed)
            {
                m.progress += amount;

                // SPECIAL CASE — red jelly avoid
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

                // SPECIAL CASE — PLAYTIME persistence
                if (missionId == "play_time")
                {
                    GameDataManager.Instance.Data.playtimeMinutesToday = m.progress;
                }

                GameDataManager.Instance.SaveGame();
                return;
            }
        }
    }

    // ----------------------------------------------------
    // CHECK IF ALL COMPLETED
    // ----------------------------------------------------
    public bool CheckAllCompleted()
    {
        foreach (var m in GameDataManager.Instance.Data.dailyMissionSet.missions)
            if (!m.completed) return false;

        return true;
    }

    // ----------------------------------------------------
    // CLAIM REWARDS
    // ----------------------------------------------------
    public bool ClaimMission(DailyMission mission)
    {
        if (!mission.completed || mission.claimed)
            return false;

        mission.claimed = true;

        // Reward coins
        CoinManager.Instance.AddCoins(mission.reward);

        GameDataManager.Instance.SaveGame();
        return true;
    }
}
