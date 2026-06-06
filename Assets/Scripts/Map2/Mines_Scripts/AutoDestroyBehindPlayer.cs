using UnityEngine;

public class AutoDestroyBehindPlayer : MonoBehaviour
{
    public Transform player;
    public float destroyDelay = 10f;

    private bool counted = false;

    void Update()
    {
        if (player == null) return;

        // If mine passed behind player
        if (!counted && transform.position.z < player.position.z - 2f)
        {
            ObstacleSpawner.Instance.avoidStreak++;
            counted = true;

            //track sto scoremanagermap2.cs ta swsta miss gia na vgaloume reward
            ScoreManagerMap2.Instance.AddPassedMine();

            // DAILY MISSION — Avoid Mines
            DailyMissionManager.Instance.AddProgress("avoid_mines", 1);


            // TELEMETRY — Mine avoided
            TelemetryManager.Instance?.LogEvent(
                "mine_avoided",
                ObstacleSpawner.Instance.road.forwardSpeed,
                0f,
                ObstacleSpawner.Instance.avoidStreak
            );

            Debug.Log(
            $"<color=#00ffff>[Mine Avoided]</color> " +
            $"Avoid Streak: <b>{ObstacleSpawner.Instance.avoidStreak}</b>"
        );

            // Check streak recovery
            ObstacleSpawner.Instance.TrySpeedRecovery();
        }

        if (transform.position.z < player.position.z - destroyDelay)
        {
            Destroy(gameObject);
        }
    }
}
