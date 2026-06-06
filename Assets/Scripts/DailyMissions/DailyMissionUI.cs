using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyMissionUI : MonoBehaviour
{
    public GameObject missionPrefab;
    public Transform container;
    public TextMeshProUGUI resetTimerText;

    public Transform rewardSpawnPoint;
    public GameObject floatingRewardPrefab;

    // --- COLORS ---
    private Color greyColor = new Color(0.4f, 0.4f, 0.4f);
    private Color greenColor = new Color(0.0f, 0.75f, 0.0f);
    private Color redColor = new Color(0.75f, 0.0f, 0.0f);

    private Color bright = new Color(1f, 1f, 1f, 1f);
    private Color faded = new Color(1f, 1f, 1f, 0.35f);

    void Start()
    {
        // Make sure missions are initialized for the current profile
        if (DailyMissionManager.Instance != null)
            DailyMissionManager.Instance.InitializeDailyMissions();

        RefreshUI();
    }

    private void Update()
    {
        UpdateResetTimer();
    }

    public void RefreshUI()
    {
        Debug.Log("[DailyMissionUI] RefreshUI called.");

        // Safety: make sure GameDataManager and mission list exist
        if (GameDataManager.Instance == null || GameDataManager.Instance.Data == null)
        {
            Debug.LogError("[DailyMissionUI] GameDataManager or Data is NULL!");
            return;
        }

        if (GameDataManager.Instance.Data.dailyMissionSet == null ||
            GameDataManager.Instance.Data.dailyMissionSet.missions == null)
        {
            Debug.LogWarning("[DailyMissionUI] dailyMissionSet or missions list is NULL. No missions to show.");
            return;
        }

        var missions = GameDataManager.Instance.Data.dailyMissionSet.missions;
        Debug.Log("[DailyMissionUI] Mission count = " + missions.Count);

        // Clear previous entries
        foreach (Transform child in container)
            Destroy(child.gameObject);

        foreach (var m in missions)
        {
            var entry = Instantiate(missionPrefab, container);

            // Assign texts
            entry.transform.Find("MissionText").GetComponent<TextMeshProUGUI>().text =
                GetMissionDescription(m.missionId);

            entry.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>().text =
                $"{m.progress} / {m.target}";

            // Get UI elements
            Button claimButton = entry.transform.Find("ClaimButton").GetComponent<Button>();
            Image coinIcon = claimButton.transform.Find("CoinIcon").GetComponent<Image>();

            // Animation components
            CoinGlow glow = coinIcon.GetComponent<CoinGlow>();
            ButtonPulse pulse = claimButton.GetComponent<ButtonPulse>();

            // --- UI STATE LOGIC ---

            // CASE 1 — Already claimed
            if (m.claimed)
            {
                claimButton.interactable = false;
                claimButton.image.color = greyColor;
                coinIcon.color = faded;

                if (glow) glow.EnableGlow(false);
                if (pulse) pulse.EnablePulse(false);
            }
            // CASE 2 — Completed but not claimed
            else if (m.completed)
            {
                claimButton.interactable = true;
                claimButton.image.color = greenColor;
                coinIcon.color = bright;

                if (glow) glow.EnableGlow(true);
                if (pulse) pulse.EnablePulse(true);
            }
            // CASE 3 — Not completed yet
            else
            {
                claimButton.interactable = false;
                claimButton.image.color = redColor;
                coinIcon.color = faded;

                if (glow) glow.EnableGlow(false);
                if (pulse) pulse.EnablePulse(false);
            }

            // Add button listener
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() =>
            {
                if (DailyMissionManager.Instance != null &&
                    DailyMissionManager.Instance.ClaimMission(m))
                {
                    ShowReward(m.reward);
                    RefreshUI();
                }
            });
        }
    }

    string GetMissionDescription(string id)
    {
        switch (id)
        {
            case "collect_blue": return "Collect 50 blue jellyfish";
            case "avoid_red_limit": return "Avoid 20 red jellyfish";
            case "hit_correct": return "Collect 15 blue squids";
            case "avoid_mines": return "Avoid 10 mines";
            case "play_time": return "Play for 20 minutes";
        }
        return id;
    }

    private void UpdateResetTimer()
    {
        DateTime now = DateTime.Now;

        DateTime nextReset = now.Date.AddDays(1);
        TimeSpan remaining = nextReset - now;

        resetTimerText.text =
            $"Missions reset in: <color=#FF0000>{remaining:hh\\:mm\\:ss}</color>";
    }

    public void ShowReward(int amount)
    {
        GameObject popup = Instantiate(floatingRewardPrefab, rewardSpawnPoint);
        popup.GetComponent<FloatingReward>().Play(amount);
    }
}
