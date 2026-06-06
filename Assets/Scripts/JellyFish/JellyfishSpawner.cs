using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PerformanceWindow
{
    private int windowSize;
    private Queue<bool> results = new Queue<bool>();

    public PerformanceWindow(int size = 20)
    {
        windowSize = size;
    }

    /// <summary>
    /// Implements a sliding window using FIFO queue logic to track recent player performance,
    /// avoiding historical data bias during real-time difficulty adaptation.
    /// </summary>

    public void AddResult(bool hit)
    {
        results.Enqueue(hit);
        if (results.Count > windowSize) //FIFO management
            results.Dequeue();
    }

    public float GetAccuracy()
    {
        if (results.Count == 0) return 1f;
        int hits = 0;
        foreach (var r in results) 
            if (r) hits++;
        return (float)hits / results.Count;
    }

    public int MissStreak()
    {
        int streak = 0;
        foreach (var r in results)
        {
            if (!r) streak++;
            else streak = 0;
        }
        return streak;
    }
}

public class JellyfishSpawner : MonoBehaviour
{
    public GameObject redJellyfishPrefab;
    [Header("References")]
    public Transform player;

    [Header("Lateral Bias Settings")]
    public bool useLateralBias = true;
    public GameObject blueJellyfishPrefab;

    [Range(0f, 1f)] public float redSpawnChance = 0.2f;
    [Range(0f, 1f)] public float flipChance = 0.6f;

    private FollowRoadByRaycast road;
    public int performanceWindowSize = 20;

    private PerformanceWindow perfWindow;

    [Header("Spawn Settings")]
    public float spawnStartDelay = 2.5f;
    public int maxActive = 40;
    public float verticalOffset = 1f;
    [Header("Adaptive Settings")]
    public bool adaptiveEnabled = true;
    public float adaptLerp = 0.2f;
    private float distanceSinceLastSpawn = 0f;

    private Vector3 lastPlayerPos;
    private float curSpawnDistance;
    private float startDelayTimer = 0f;
    private string currentBand = "stable";
    private int biasDirection = 0;
    private bool canSpawn = false;
    private int lastBiasDirection = 0;

    private float targetInterval;

    private float curInterval;
    private float curLateralRange;
    private float targetLateralRange;
    private float targetSpawnDistance;

    private const float tunnelHalfWidth = 5f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            road = player.GetComponent<FollowRoadByRaycast>();

        lastPlayerPos = player.position;
        startDelayTimer = spawnStartDelay;
        canSpawn = false;

        perfWindow = new PerformanceWindow(performanceWindowSize);

        curInterval = 10f;
        curLateralRange = 2f;
        curSpawnDistance = 28f;
    }

    void Update()
    {
        if (SessionManager.Instance != null && !SessionManager.Instance.IsActive)
            return;

        if (player == null || blueJellyfishPrefab == null || redJellyfishPrefab == null) return;

        if (!canSpawn)
        {
            startDelayTimer -= Time.deltaTime;
            if (startDelayTimer <= 0f)
                canSpawn = true;
            else
                return;
        }

        // Distance-based spawning routine utilizing frame-to-frame delta distance tracking
        float frameDist = Vector3.Distance(player.position, lastPlayerPos);
        distanceSinceLastSpawn += frameDist;
        lastPlayerPos = player.position;

        if (distanceSinceLastSpawn >= curInterval && CountActiveJellyfish() < maxActive)
        {
            SpawnSingle();
            distanceSinceLastSpawn = 0f;
        }
    }

    private void SpawnSingle()
    {
        float acc = GetAccuracy();
        int streak = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentStreak : 0;

        int prevBiasDirection = lastBiasDirection;
        UpdateAdaptiveBand(acc, streak);
        UpdateBiasDirection(acc, streak);
        bool flippedSides = (biasDirection != 0 && prevBiasDirection != 0 && biasDirection != prevBiasDirection);

        bool spawnRed = Random.value < redSpawnChance;
        GameObject prefabToSpawn = spawnRed ? redJellyfishPrefab : blueJellyfishPrefab;

        // Adaptive red/blue bias logic
        if (spawnRed)
        {
            // Cognitive inhibition task: force focal transitions by presenting distractor stimuli opposite to the last bias side
            if (lastBiasDirection != 0)
            {
                if (Random.value < 0.8f)
                    biasDirection = -lastBiasDirection; // 80% opposite side
                else
                    biasDirection = Random.value > 0.5f ? 1 : -1; // 20% random
            }

            if (Random.value < 0.5f)
                biasDirection = Mathf.RoundToInt(biasDirection * 0.5f);
        }
        else
        {
            if (Random.value < 0.15f)
                biasDirection = 0;
        }

        // Base position and direction setup
        Vector3 basePos;
        Vector3 rightDir;

        // Extract navigation boundaries directly from the raycast-based procedural road tracking data
        if (road != null && road.HasWallData)
        {
            Vector3 left = road.LeftWallPoint;
            Vector3 right = road.RightWallPoint;
            Vector3 center = (left + right) * 0.5f;
            rightDir = (right - left).normalized;
            basePos = center + player.forward * curSpawnDistance;
        }
        else
        {
            rightDir = player.right;
            basePos = player.position + player.forward * curSpawnDistance;
        }

        basePos.y += verticalOffset;

        // Apply lateral bias offset
        float lateralOffset = 0f;
        if (useLateralBias)
        {
            // Dynamically scale distribution metrics according to the user's running accuracy index
            float biasStrength = Mathf.Lerp(0.6f, 1.2f, acc);
            float flipDampen = flippedSides ? 0.6f : 1.0f;
            float biasAmount = Random.Range(curLateralRange * 0.5f, curLateralRange * biasStrength * flipDampen);
            lateralOffset = biasAmount * biasDirection;
            lateralOffset = Mathf.Clamp(lateralOffset, -tunnelHalfWidth * 0.9f, tunnelHalfWidth * 0.9f);
        }

        basePos += rightDir * lateralOffset;

        if (spawnRed)
            basePos -= rightDir * (lateralOffset * 0.4f);

        Quaternion uprightRotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject obj = Instantiate(prefabToSpawn, basePos, uprightRotation);

        Debug.Log($"Spawned: {obj.name} | RedChance={redSpawnChance:F2} | SpawnRed={spawnRed}");
        Debug.Log($"[Spawn] {BandColor(currentBand)}{currentBand.ToUpper()}</color> | " +
                  $"Type={(spawnRed ? "RED" : "BLUE")} | Side={(biasDirection == -1 ? "LEFT" : biasDirection == 1 ? "RIGHT" : "CENTER")} | " +
                  $"Acc={acc:F2} | RedChance={redSpawnChance:P0} | Interval={curInterval:F1} | Dist={curSpawnDistance:F1}");
    }


    private float GetAccuracy()
    {
        if (ScoreManager.Instance == null) return 1f;
        int c = ScoreManager.Instance.GetCollectedCount();
        int m = ScoreManager.Instance.GetMissedCount();
        return (float)c / Mathf.Max(1, c + m);
    }

    private void UpdateBiasDirection(float acc, int streak)
    {
        if (acc < 0.3f && streak < 2)
        {
            biasDirection = 0;
            lastBiasDirection = 0;
            return;
        }

        if (biasDirection == 0)
            biasDirection = Random.value > 0.5f ? 1 : -1;

        if (Random.value < flipChance)
            biasDirection = -biasDirection;

        if (Random.value < 0.1f)
            biasDirection = 0;

        lastBiasDirection = biasDirection;
    }

    /// <summary>
    /// Dynamically calibrates spawning difficulty parameters using linear interpolation (Lerp).
    /// Prevents abrupt gameplay shifts while stabilizing the player's cognitive Flow State.
    /// </summary>
    private void UpdateAdaptiveBand(float acc, int streak)
    {
        string newBand;

        if (acc >= 0.90f && streak >= 10) newBand = "excellent";
        else if (acc >= 0.75f) newBand = "stable";
        else if (acc >= 0.60f) newBand = "struggling";
        else newBand = "overloaded";

        switch (newBand)
        {
            case "excellent":
                targetInterval = 17f;
                targetSpawnDistance = 28f;
                targetLateralRange = 8f;
                redSpawnChance = 0.35f; 
                break;

            case "stable":
                targetInterval = 20f;
                targetSpawnDistance = 35f;
                targetLateralRange = 6.5f;
                redSpawnChance = 0.25f;
                break;

            case "struggling":
                targetInterval = 22f;
                targetSpawnDistance = 38f;
                targetLateralRange = 4.5f;
                redSpawnChance = 0.12f;
                break;

            default:
                targetInterval = 24f;
                targetSpawnDistance = 42f;
                targetLateralRange = 2.5f;
                redSpawnChance = 0.05f; 
                break;
        }

        curInterval = Mathf.Lerp(curInterval, targetInterval, adaptLerp);
        curSpawnDistance = Mathf.Lerp(curSpawnDistance, targetSpawnDistance, adaptLerp);
        curLateralRange = Mathf.Lerp(curLateralRange, targetLateralRange, adaptLerp);

        if (newBand != currentBand)
        {
            currentBand = newBand;
            Debug.Log($"[Adaptive] Band → {newBand.ToUpper()} | Interval={curInterval:F1} | Dist={curSpawnDistance:F1} | Range={curLateralRange:F1} | RedChance={redSpawnChance:P0}");

            TelemetryManager.Instance?.LogEvent("adapt", 0f, curLateralRange, streak);
        }
    }

    private int CountActiveJellyfish()
    {
        // Allocation-safe object lookup context for runtime profiling metrics
        return Object.FindObjectsByType<JellyfishCollectible>(FindObjectsSortMode.None).Length;
    }

    private string BandColor(string band)
    {
        switch (band)
        {
            case "excellent": return "<color=green>";
            case "stable": return "<color=cyan>";
            case "struggling": return "<color=yellow>";
            case "overloaded": return "<color=red>";
            default: return "<color=white>";
        }
    }
}