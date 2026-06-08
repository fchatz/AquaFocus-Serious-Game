using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Session Settings")]
    public float sessionDuration = 15f;
    public bool autoStart = true;

    private float sessionTimer = 0f;
    private bool sessionActive = false;

    // Daily missions
    private float playtimeSeconds = 0f;

    // Flag
    public bool hasSessionStarted = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        
        if (autoStart)
            StartSession();

        if (GameDataManager.Instance != null && GameDataManager.Instance.Data != null)
        {
            int previousBlue = GameDataManager.Instance.Data.totalBlueCollected;
        }
        else
        {
            Debug.LogWarning("GameDataManager not initialized before SessionManager.Start()");
        }
    }

    private void Update()
    {
        if (!sessionActive) return;

        playtimeSeconds += Time.deltaTime;

        if (playtimeSeconds >= 60f)
        {
            playtimeSeconds -= 60f;

            // Add 1 minute to daily mission
            DailyMissionManager.Instance.AddProgress("play_time", 1);

            // Save playtime
            GameDataManager.Instance.SaveGame();
        }
    }

    public void StartSession()
    {
        Debug.Log("Session started");
        sessionActive = true;
        sessionTimer = 0f;
        hasSessionStarted = true;

        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddMissed(-ScoreManager.Instance.GetMissedCount());
        }
    }


    public void EndSession()
    {
        if (!IsActive)
            return;

        Debug.Log("Session ended");

        IsActive = false;
        sessionActive = false;

        // Map-based reward system

        string map = TelemetryManager.Instance.CurrentMapName;
        int coinsEarned = 0;
        Debug.Log("END SESSION MAP NAME = " + TelemetryManager.Instance.CurrentMapName);


        if (map == "Map1Main")
        {
            // Map 1 Reward
            int collected = ScoreManager.Instance.GetCollectedCount();
            int missed = ScoreManager.Instance.GetMissedCount();
            float accuracy = (float)collected / Mathf.Max(1, collected + missed);

            coinsEarned = Mathf.RoundToInt(Mathf.Lerp(40f, 120f, accuracy));
            Debug.Log($"[Map1 Reward] Accuracy={accuracy:F2} | Coins Earned={coinsEarned}");
        }
        else if (map.Contains("Map2"))
        {
            // Map 2 reward
            coinsEarned = ScoreManagerMap2.Instance.CalculateReward();
            Debug.Log($"[Map2 Reward] Coins Earned={coinsEarned}");
        }
        else if (map.Contains("Map3"))
        {
            // Map 3 reward
            int collected = ScoreManager.Instance != null ? ScoreManager.Instance.GetCollectedCount() : 0;
            int missed = ScoreManager.Instance != null ? ScoreManager.Instance.GetMissedCount() : 0;
            float accuracy = (float)collected / Mathf.Max(1, collected + missed);

            int jellyCoins = Mathf.RoundToInt(Mathf.Lerp(30f, 80f, accuracy));

            int map2Part = ScoreManagerMap2.Instance != null
                ? ScoreManagerMap2.Instance.CalculateReward()
                : 0;

            coinsEarned = jellyCoins + map2Part;

            Debug.Log($"[Map3 Reward] JellyAcc={accuracy:F2} | JellyCoins={jellyCoins} | Map2Part={map2Part} | Total={coinsEarned}");
        }


        CoinManager.Instance.AddCoins(coinsEarned);

        CoinUI.Instance.PlayCoinPopup(coinsEarned);

        // 3) Freeze gameplay
        JellyfishSpawner spawner = FindAnyObjectByType<JellyfishSpawner>();
        if (spawner != null)
            spawner.enabled = false;

        FollowRoadByRaycast player = FindAnyObjectByType<FollowRoadByRaycast>();
        if (player != null)
            player.enabled = false;

        Debug.Log("Gameplay frozen - session complete.");

        // 4) Export telemetry
        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.ExportTelemetry();

        // 5) SAVE SYSTEM UPDATE
        Debug.Log("GameDataManager.Instance = " + (GameDataManager.Instance == null ? "NULL" : "OK"));
        Debug.Log("GameDataManager.Instance.Data = " + (GameDataManager.Instance?.Data == null ? "NULL" : "OK"));
        Debug.Log("ScoreManager.Instance = " + (ScoreManager.Instance == null ? "NULL" : "OK"));

        // Update jellyfish stats for Map1 and Map3
        if ((map.Contains("Map1") || map.Contains("Map3")) && ScoreManager.Instance != null)
        {
            GameDataManager.Instance.Data.totalBlueCollected += ScoreManager.Instance.GetCollectedCount();
            GameDataManager.Instance.Data.totalRedCollected += ScoreManager.Instance.GetWrongCount();
        }

        // Per-map session counters
        if (map.Contains("Map1"))
        {
            GameDataManager.Instance.Data.map1SessionsPlayed++;
        }
        else if (map.Contains("Map2"))
        {
            GameDataManager.Instance.Data.map2SessionsPlayed++;
        }

        GameDataManager.Instance.Data.totalSessionsPlayed++;

        // Evaluate unlocks after updating stats
        if (MapUnlockManager.Instance != null)
        {
            MapUnlockManager.Instance.EvaluateUnlocks();
        }

        GameDataManager.Instance.SaveGame();

        Debug.Log(" Save completed. Session fully ended.");

    }






    public float GetTimeRemaining()
    {
        return Mathf.Max(0f, sessionDuration - sessionTimer);
    }

    public bool IsActive
    {
        get => sessionActive;
        private set => sessionActive = value;
    }
    
    public void Resume()
    {
        
        Time.timeScale = 1f;
        
    }

}

