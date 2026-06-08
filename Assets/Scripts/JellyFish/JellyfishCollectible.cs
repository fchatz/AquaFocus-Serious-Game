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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // Enabled 3D spatial blend
            audioSource.volume = 0.8f;
        }
    }

    private void Update()
    {
        if (player == null || collected) return;

        Vector3 toJelly = transform.position - player.position;

        if (Vector3.Dot(player.forward, toJelly) < 0)
        {
            if (!behindPlayer)
            {
                behindPlayer = true;
                behindTimer = 0f;
            }

            behindTimer += Time.deltaTime;
            if (behindTimer >= despawnDelay)
            {
                if (jellyType == JellyType.Blue)
                {
                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.AddMissed(1);
                }

                if (road != null)
                {
                    road.OnMissJellyfish();
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player") || other.CompareTag("Submarine"))
        {
            collected = true;

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

                    // Trigger full screen visual feedback
                    ScreenFeedback feedback = FindFirstObjectByType<ScreenFeedback>();
                    if (feedback != null)
                        feedback.TriggerRedPulse();

                    ScoreManager.Instance.AddWrong(1);
                    break;
            }
        }

        // Process systems telemetry and streak logic
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

            // Update daily mission validation status
            if (jellyType == JellyType.Blue)
            {
                DailyMissionManager.Instance.AddProgress("collect_blue", 1);
            }
        }

        // Object destruction delayed to let audio clips finish playback
        Destroy(gameObject, 0.35f);
    }
}