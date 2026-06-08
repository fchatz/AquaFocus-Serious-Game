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

    private Color greyColor = new Color(0.4f, 0.4f, 0.4f);
    private Color greenColor = new Color(0.0f, 0.75f, 0.0f);
    private Color redColor = new Color(0.75f, 0.0f, 0.0f);

    private Color bright = new Color(1f, 1f, 1f, 1f);
    private Color faded = new Color(1f, 1f, 1f, 0.35f);

    void Start()
    {
        // Ensure missions are initialized for the current profile
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

        // Validate that GameDataManager and mission data exist
        if (GameDataManager.Instance == null || GameDataManager.Instance.Data == null)
        {
            Debug.LogError("[DailyMissionUI] GameDataManager or Data is null!");
            return;
        }

        // Clear existing UI elements
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        var missions = GameDataManager.Instance.Data.dailyMissionSet.missions;

        foreach (var m in missions)
        {
            GameObject go = Instantiate(missionPrefab, container);

            TextMeshProUGUI descText = go.transform.Find("Description").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI progText = go.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>();
            Slider slider = go.transform.Find("Slider").GetComponent<Slider>();
            Button claimButton = go.transform.Find("ClaimButton").GetComponent<Button>();
            Image coinIcon = go.transform.Find("ClaimButton/CoinIcon").GetComponent<Image>();
            TextMeshProUGUI rewardText = go.transform.Find("ClaimButton/RewardText").GetComponent<TextMeshProUGUI>();

            CoinGlow glow = coinIcon.GetComponent<CoinGlow>();
            ButtonPulse pulse = claimButton.GetComponent<ButtonPulse>();

            descText.text = GetMissionDescription(m.missionId);
            rewardText.text = m.reward.ToString();

            slider.maxValue = m.target;
            slider.value = m.progress;
            progText.text = $"{m.progress}/{m.target}";

            if (!m.completed)
            {
                claimButton.interactable = false;
                progText.color = redColor;
                coinIcon.color = bright;

                if (glow) glow.EnableGlow(false);
                if (pulse) pulse.EnablePulse(false);
            }
            else if (m.completed && !m.claimed)
            {
                claimButton.interactable = true;
                progText.color = greenColor;
                coinIcon.color = bright;

                if (glow) glow.EnableGlow(true);
                if (pulse) pulse.EnablePulse(true);
            }
            else if (m.claimed)
            {
                claimButton.interactable = false;
                progText.text = "Claimed";
                progText.color = greyColor;
                coinIcon.color = faded;

                if (glow) glow.EnableGlow(false);
                if (pulse) pulse.EnablePulse(false);
            }

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
        if (floatingRewardPrefab == null || rewardSpawnPoint == null) return;

        GameObject go = Instantiate(floatingRewardPrefab, rewardSpawnPoint);
        FloatingReward fr = go.GetComponent<FloatingReward>();
        if (fr != null)
        {
            fr.Play(amount);
        }
    }
}