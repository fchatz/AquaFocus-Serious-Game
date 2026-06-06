using UnityEngine;

public enum JellyType { Blue, Red }

public class JellyfishCollectible : MonoBehaviour
{
    public JellyType jellyType = JellyType.Blue;
    public float despawnDelay = 0f;

    [Header("Audio")]
    public AudioClip collectBlueSound;
    public AudioClip collectRedSound;
    private AudioSource audioSource;
    private Transform player;
    private bool collected = false;
    private bool behindPlayer = false;
    private float behindTimer = 0f;
    private FollowRoadByRaycast road;

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            road = playerObj.GetComponent<FollowRoadByRaycast>();
        }

        // 🎧 Ensure audio source exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = 0.8f;
        }
    }

    private void Update()
    {
        if (player == null || collected) return;

        Vector3 toJelly = transform.position - player.position;

        //elegxos an exei perasei thn medousa
        if (Vector3.Dot(player.forward, toJelly) < 0)
        {
            if (!behindPlayer)
            {
                behindPlayer = true;
                behindTimer = 0f;

                // 🧠 Log telemetry for misses or avoids
                if (TelemetryManager.Instance != null && road != null)
                {
                    string eventType = jellyType == JellyType.Blue ? "miss" : "avoid";
                    TelemetryManager.Instance.LogEvent(
                        eventType,
                        road.CurrentSpeed,
                        road.CurrentOffset,
                        ScoreManager.Instance.CurrentStreak
                    );
                }

                // ❌ Only count blue misses
                if (jellyType == JellyType.Blue && ScoreManager.Instance != null)
                    ScoreManager.Instance.AddMissed(1);

                // ⭐ DAILY MISSION — Avoid red jellyfish
                if (jellyType == JellyType.Red)
                {
                    DailyMissionManager.Instance.AddProgress("avoid_red_limit", 1);
                }
            }
            else
            {
                behindTimer += Time.deltaTime;
                if (behindTimer >= despawnDelay)
                    Destroy(gameObject); //optimize gia na katastrafoume thn saloufa
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        FollowRoadByRaycast road = other.GetComponent<FollowRoadByRaycast>();

        // 🔹 Hide the jelly instantly for instant feedback
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        // 🔹 Optionally disable the collider too, so it doesn't trigger again
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // 🔹 Play correct sound
        if (audioSource != null)
        {
            switch (jellyType)
            {
                case JellyType.Blue:
                    if (collectBlueSound != null)
                        audioSource.PlayOneShot(collectBlueSound, 0.9f);
                    ScoreManager.Instance.AddScore(1);
                    break;

                case JellyType.Red:
                    if (collectRedSound != null)
                        audioSource.PlayOneShot(collectRedSound, 0.9f);
                    //red canvas pulse
                    ScreenFeedback feedback = FindFirstObjectByType<ScreenFeedback>();
                    if (feedback != null)
                        feedback.TriggerRedPulse();

                    ScoreManager.Instance.AddWrong(1);
                    break;
            }
        }

        // 🔹 Handle telemetry and streak logic
        if (road != null)
        {
            road.OnCollectJellyfish();

            if (TelemetryManager.Instance != null)
            {
                TelemetryManager.Instance.LogEvent(
                    jellyType == JellyType.Blue ? "collect" : "wrong",
                    road.CurrentSpeed,
                    road.CurrentOffset,
                    ScoreManager.Instance.CurrentStreak
                );
            }

            // DAILY MISSIONS
            if (jellyType == JellyType.Blue)
            {
                DailyMissionManager.Instance.AddProgress("collect_blue", 1);
            }

        }

        // 🔹 Delay destruction slightly to allow sound to finish
        Destroy(gameObject, 0.35f);
    }
}