using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance;

    [Header("Debug")]
    public bool enableDebug = false;

    [Header("References")]
    public Transform player;
    public FollowRoadByRaycast road;
    public GameObject[] obstaclePrefabs;

    [Header("Vertical Positioning")]
    public float verticalOffset = 1.5f;

    [Header("Base Settings")]
    public float baseInterval = 15f;
    public float minInterval = 8f;
    public float maxInterval = 22f;

    public float spawnDistance = 35f;
    public float minSpawnDistance = 25f;
    public float maxSpawnDistance = 45f;

    public float lateralMargin = 0.5f;

    [Header("Adaptation")]
    public float adaptLerp = 0.1f;

    private float curInterval;
    private float lastSpawnZ;

    private int totalSpawned = 0;
    private int totalHits = 0;

    private float accuracy => (totalSpawned == 0) ? 1 : (1f - (float)totalHits / totalSpawned); //Υπολογίζει σε πραγματικό χρόνο την ακρίβεια αποφυγής του παίκτη: 1−(totalHits/totalSpawned)

    private float maxHalfWidth = 0.6f;

    [Header("Start Delay")]
    public float spawnStartDelay = 2.5f;
    private float startDelayTimer;
    private bool canSpawn = false;


    // -----------------------------
    // NEW: Avoid Streak System
    // -----------------------------
    [Header("Avoid Reward System")]
    public int avoidStreak = 0;
    public int streakNeededForSpeedRecovery = 5;
    public float speedRecoveryAmount = 1f; // how much speed to restore
    // -----------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        curInterval = baseInterval;
        lastSpawnZ = player.position.z;

        startDelayTimer = spawnStartDelay; // <<< ADD THIS
        canSpawn = false;

        CacheMaxObstacleWidth();
    }

    void CacheMaxObstacleWidth()
    {
        float max = 0f;
        foreach (var prefab in obstaclePrefabs)
        {
            var box = prefab.GetComponentInChildren<BoxCollider>();
            if (box != null)
            {
                float halfWidth = box.bounds.extents.x;
                if (halfWidth > max) max = halfWidth;
            }
        }
        maxHalfWidth = max;

        D($"MaxHalfWidth cached: {maxHalfWidth:F2}");
    }

    void Update()
    {

        // --- Start Delay ---
        if (!canSpawn)
        {
            startDelayTimer -= Time.deltaTime;
            if (startDelayTimer <= 0f)
                canSpawn = true;
            else
                return; // stop Update until delay finishes
        }


        float dist = player.position.z - lastSpawnZ;

        if (dist >= curInterval)
        {
            SpawnObstacle();
            lastSpawnZ = player.position.z;
        }

        AdaptiveUpdate();
    }

    void AdaptiveUpdate()
    {
        float tInt = baseInterval;
        float tDist = spawnDistance;

        if (accuracy >= 0.9f)
        {
            tInt = Mathf.Lerp(baseInterval, minInterval, 0.7f);
            tDist = Mathf.Lerp(spawnDistance, maxSpawnDistance, 0.6f);
        }
        else if (accuracy >= 0.75f)
        {
            tInt = Mathf.Lerp(baseInterval, minInterval, 0.4f);
            tDist = spawnDistance;
        }
        else if (accuracy >= 0.6f)
        {
            tInt = Mathf.Lerp(baseInterval, maxInterval, 0.5f);
            tDist = Mathf.Lerp(spawnDistance, minSpawnDistance, 0.4f);
        }
        else
        {
            tInt = Mathf.Lerp(baseInterval, maxInterval, 0.8f);
            tDist = Mathf.Lerp(spawnDistance, minSpawnDistance, 0.6f);
        }

        curInterval = Mathf.Lerp(curInterval, tInt, adaptLerp);
        spawnDistance = Mathf.Lerp(spawnDistance, tDist, adaptLerp);
    }

    void SpawnObstacle()
    {
        if (road == null || !road.HasWallData) return;

        Vector3 left = road.LeftWallPoint;
        Vector3 right = road.RightWallPoint;

        Vector3 forward = player.forward;
        Vector3 center = (left + right) * 0.5f;
        Vector3 rightDir = (right - left).normalized;

        Vector3 pos = center + forward * spawnDistance;
        pos.y += verticalOffset;

        float tunnelHalfWidth = Vector3.Distance(left, center);
        float maxOffset = tunnelHalfWidth - maxHalfWidth - lateralMargin;

        float offset = Random.Range(-maxOffset, maxOffset);
        pos += rightDir * offset;

        var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject mine = Instantiate(prefab, pos, Quaternion.identity, transform);

        TelemetryManager.Instance?.LogEvent(
            "mine_spawned",
            road.forwardSpeed,
            0f,
            ObstacleSpawner.Instance.avoidStreak
        );


        // Add destroy behavior
        mine.AddComponent<AutoDestroyBehindPlayer>().player = player;

        D($"SPAWN #{totalSpawned + 1} | position:{pos}");
        totalSpawned++;
    }

    // -----------------------------
    // NEW: Streak → Speed Recovery
    // -----------------------------
    public void TrySpeedRecovery()
    {
        if (avoidStreak >= streakNeededForSpeedRecovery)
        {
            if (road != null)
            {
                float oldSpeed = road.forwardSpeed;
                float recoveryAmount = road.speedRecoverRate;

                road.forwardSpeed = Mathf.Min(road.maxSpeed, road.forwardSpeed + recoveryAmount);

                // TELEMETRY - speed recovered
                TelemetryManager.Instance?.LogEvent(
                    "speed_recovered",
                    road.forwardSpeed,
                    0f,
                    avoidStreak
                );

                if (enableDebug)
                {
                    Debug.Log(
                        $"<color=#55FF55>[Speed Recovery]</color> " +
                        $"Streak reached <b>{streakNeededForSpeedRecovery}</b> → " +
                        $"Recovered: <b>{recoveryAmount:F2}</b> | " +
                        $"Speed: <b>{oldSpeed:F2} → {road.forwardSpeed:F2}</b>"
                    );
                }
            }

            if (enableDebug)
            {
                Debug.Log("<color=#ffaa00>[Streak Reset]</color> Successful recovery. Streak reset to 0.");
            }

            avoidStreak = 0;
        }
    }



    // -----------------------------

    public void RegisterHit()
    {
        totalHits++;

        //add to hit sto ScoreManagerMap2.cs gia na kanoume track wste na vgaloume to reward
        ScoreManagerMap2.Instance.AddHitMine();


        // reset streak when player hits a mine
        avoidStreak = 0;

        if (enableDebug)
        {
            Debug.Log(
                $"<color=#ff4444>[Mine Hit]</color> " +
                $"Hits: <b>{totalHits}</b> | " +
                $"Streak reset to 0 | " +
                $"Accuracy: <b>{accuracy:F2}</b>"
            );
        }
    }

    private void D(string msg)
    {
        if (enableDebug)
            Debug.Log("<color=#00e1ff><b>[ObstacleSpawner]</b></color> " + msg);
    }
}
