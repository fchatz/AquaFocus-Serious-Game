using UnityEngine;

public class PlayerObstacleHitHandler : MonoBehaviour
{
    [Header("Invulnerability")]
    public float invulnDuration = 2.5f;
    private float invulnTimer = 0f;

    private FollowRoadByRaycast followRoad;
    private ScreenFeedback feedback;

    private FollowRoadByRaycast road;


    public bool debug = false;
    private void D(string msg) { if (debug) Debug.Log("<color=#ff8800>[ObstacleHit]</color> " + msg); }

    private SubmarineGlowFeedback glow;


    void Awake()
    {
        followRoad = GetComponent<FollowRoadByRaycast>();
        feedback = FindFirstObjectByType<ScreenFeedback>();
        glow = GetComponent<SubmarineGlowFeedback>();
        road = FindFirstObjectByType<FollowRoadByRaycast>();


    }

    void Update()
    {
        if (invulnTimer > 0f)
            invulnTimer -= Time.deltaTime;
    }

    public bool CanBeHit()
    {
        return invulnTimer <= 0f;
    }

    public void OnObstacleHit(ObstacleHit obstacle)
    {
        D("Obstacle HIT");
        invulnTimer = invulnDuration;

        TelemetryManager.Instance?.LogEvent(
            "speed_penalty",
            road.forwardSpeed,
            0f,
            ObstacleSpawner.Instance.avoidStreak
        );


        // Slowdown
        if (followRoad != null)
            followRoad.OnMissedJellyfish();

        // UI pulse
        if (feedback != null)
            feedback.TriggerRedPulse();

        // Notify spawner to update accuracy
        if (ObstacleSpawner.Instance != null)
        {
            ObstacleSpawner.Instance.RegisterHit();
            TelemetryManager.Instance?.LogEvent(
                "mine_hit",
                road.forwardSpeed,
                0f,
                ObstacleSpawner.Instance.avoidStreak
            );
        }
        


        // Remove obstacle
        Destroy(obstacle.gameObject);

        D($"Invuln until: {Time.time + invulnDuration:F2}");
    }
}
