using UnityEngine;
using UnityEngine.UI;

public class ButtonStreakController : MonoBehaviour
{
    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Sprites")]
    [Tooltip("Sprite used when button is idle/disabled.")]
    public Sprite idleSprite;   
    [Tooltip("Sprite used when the active squid is the correct one (blue).")]
    public Sprite correctSprite;  
    [Tooltip("Sprite used when the active squid is the wrong one (red).")]
    public Sprite wrongSprite;
    [Header("Streak UI")]
    public Text streakText;

    [Header("Timing (base values)")]
    [Tooltip("How long the player has to react at low difficulty.")]
    public float easyRoundDuration = 2.0f;
    [Tooltip("How long the player has to react at high difficulty.")]
    public float hardRoundDuration = 1.0f;

    [Tooltip("Break between rounds when performance is low (seconds).")]
    public float easyBreakDuration = 1.0f;
    [Tooltip("Break between rounds when performance is high (seconds).")]
    public float hardBreakDuration = 0.3f;

    [Tooltip("How quickly timing adapts to accuracy.")]
    [Range(0.01f, 1f)]
    public float adaptLerp = 0.15f;

    [Header("Blue / Red probability")]
    [Tooltip("Base chance for a blue squid when accuracy is low.")]
    [Range(0f, 1f)]
    public float maxBlueProbability = 0.75f;
    [Tooltip("Minimum chance for a blue squid when accuracy is very high.")]
    [Range(0f, 1f)]
    public float minBlueProbability = 0.35f;

    [Header("Integration")]
    [Tooltip("Pause squid rounds while the player is invulnerable after a mine hit.")]
    public PlayerObstacleHitHandler obstacleHitHandler;
    [Tooltip("Reference to movement to log current speed in telemetry.")]
    public FollowRoadByRaycast road;

    [Header("Debug")]
    public bool enableDebug = false;

    private float roundStartTime = 0f;


    // runtime state
    private float currentRoundDuration;
    private float currentBreakDuration;
    private float currentBlueProbability;

    private int streak = 0;
    private int totalRounds = 0;
    private int totalCorrect = 0;

    [Header("Start Delay")]
    public float startDelay = 2.0f;


    // Counters
    [HideInInspector] public int totalBlueShown = 0;
    [HideInInspector] public int totalBlueClicked = 0;
    [HideInInspector] public int totalBlueMissed = 0;

    [HideInInspector] public int totalRedShown = 0;
    [HideInInspector] public int totalRedClicked = 0;   // Errors
    [HideInInspector] public int totalRedIgnored = 0;   // Correct inhibitions

    [HideInInspector] public float totalBlueReactionTime = 0f;
    [HideInInspector] public int blueReactionCount = 0;

    [HideInInspector] public float totalRedReactionTime = 0f;   // Only when player clicks red
    [HideInInspector] public int redReactionCount = 0;


    private Button activeButton = null;
    private bool activeIsBlue = false;
    private bool clickedThisRound = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;


    private void Awake()
    {
        if (road == null)
            road = FindFirstObjectByType<FollowRoadByRaycast>();
        if (obstacleHitHandler == null)
            obstacleHitHandler = FindFirstObjectByType<PlayerObstacleHitHandler>();
    }

    private void Start()
    {
        // Initialize timing from easy side
        currentRoundDuration = easyRoundDuration;
        currentBreakDuration = easyBreakDuration;
        currentBlueProbability = maxBlueProbability;

        // Initial setup: both buttons idle
        SetButtonState(leftButton, idleSprite, false);
        SetButtonState(rightButton, idleSprite, false);

        UpdateStreakText();

        // Start main logic with delay
        StartCoroutine(StartAfterDelay());
    }

    private System.Collections.IEnumerator StartAfterDelay()
    {

        yield return new WaitForSeconds(startDelay);


        StartCoroutine(RoundLoop());
    }



    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }


    private System.Collections.IEnumerator RoundLoop()
    {
        while (true)
        {
            if (SessionManager.Instance != null &&
                SessionManager.Instance.hasSessionStarted &&
                !SessionManager.Instance.IsActive)
            {
                yield break;
            }


            // Pause rounds while the player is invulnerable after a mine hit
            if (obstacleHitHandler != null && !obstacleHitHandler.CanBeHit())
            {
                if (activeButton != null)
                {
                    SetButtonState(leftButton, idleSprite, false);
                    SetButtonState(rightButton, idleSprite, false);
                    activeButton = null;
                }

                yield return null;
                continue;
            }

            // Start new round
            SetupNewRound();

            clickedThisRound = false;
            float timer = 0f;

            while (timer < currentRoundDuration)
            {
                if (clickedThisRound)
                    break;

                timer += Time.deltaTime;
                yield return null;
            }

            // Handle outcome if no click
            if (!clickedThisRound && activeButton != null)
            {
                HandleNoClickOutcome();
            }

            // End of round: reset buttons to idle & disabled
            SetButtonState(leftButton, idleSprite, false);
            SetButtonState(rightButton, idleSprite, false);

            activeButton = null;
            activeIsBlue = false;

            // Adapt difficulty after each round
            AdaptDifficulty();

            // Break between rounds
            if (currentBreakDuration > 0f)
            {
                float breakTimer = 0f;
                while (breakTimer < currentBreakDuration)
                {
                    if (obstacleHitHandler != null && !obstacleHitHandler.CanBeHit())
                        break;

                    breakTimer += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SetupNewRound()
    {
        if (leftButton == null || rightButton == null)
            return;

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        // Choose which button is active (left or right)
        bool chooseLeft = Random.value < 0.5f;
        activeButton = chooseLeft ? leftButton : rightButton;
        Button otherButton = chooseLeft ? rightButton : leftButton;

        roundStartTime = Time.time;


        SetButtonState(otherButton, idleSprite, false);

        // Decide if active button is blue or red
        activeIsBlue = Random.value < currentBlueProbability;
        Sprite activeSprite = activeIsBlue ? correctSprite : wrongSprite;

        // Track counts + telemetry
        if (activeIsBlue)
        {
            totalBlueShown++;
            LogTelemetryEvent("squid_blue_shown");

            if (ScoreManagerMap2.Instance != null)
                ScoreManagerMap2.Instance.blueShown++;
        }
        else
        {
            totalRedShown++;
            LogTelemetryEvent("squid_red_shown");

            if (ScoreManagerMap2.Instance != null)
                ScoreManagerMap2.Instance.redShown++;
        }

        // Active button uses its squid sprite & is clickable
        SetButtonState(activeButton, activeSprite, true);

        leftButton.onClick.AddListener(() => OnButtonClicked(leftButton));
        rightButton.onClick.AddListener(() => OnButtonClicked(rightButton));

        if (enableDebug)
        {
            Debug.Log($"[Squid] New round | Active={(chooseLeft ? "Left" : "Right")} | Color={(activeIsBlue ? "Blue" : "Red")} | RoundDur={currentRoundDuration:F2} | Break={currentBreakDuration:F2} | BlueProb={currentBlueProbability:F2}");
        }
    }

    private void OnButtonClicked(Button clickedButton)
    {
        if (clickedButton != activeButton || !clickedButton.interactable)
            return;

        clickedThisRound = true;
        totalRounds++;

        float reactionTime = Time.time - roundStartTime;


        if (activeIsBlue)
        {
            // Correct action: click the blue squid
            totalBlueClicked++;
            totalCorrect++;
            streak++;

            // Accumulate blue reaction stats
            totalBlueReactionTime += reactionTime;
            blueReactionCount++;

            PlaySound(correctSound); 
            TelemetryManager.Instance.LogEvent("squid_blue_reaction", reactionTime, 0, streak);

            LogTelemetryEvent("squid_blue_clicked");

            if (ScoreManagerMap2.Instance != null)
            {
                ScoreManagerMap2.Instance.blueReactionCount++;
                ScoreManagerMap2.Instance.blueClicked++;
                ScoreManagerMap2.Instance.totalBlueReaction += reactionTime;
            }

            // Daily missions
            DailyMissionManager.Instance.AddProgress("hit_correct", 1);

        }
        else
        {
            totalRedClicked++;
            streak = 0;

            // Accumulate red reaction stats
            totalRedReactionTime += reactionTime;
            redReactionCount++;

            PlaySound(incorrectSound);
            TelemetryManager.Instance.LogEvent("squid_red_reaction", reactionTime, 0, streak);

            LogTelemetryEvent("squid_red_clicked");

            if (ScoreManagerMap2.Instance != null)
            {
                ScoreManagerMap2.Instance.redClicked++;
                ScoreManagerMap2.Instance.totalRedReaction += reactionTime;
                ScoreManagerMap2.Instance.redReactionCount++;
            }
        }


        UpdateStreakText();
    }

    private void HandleNoClickOutcome()
    {
        totalRounds++;

        if (activeIsBlue)
        {
            // Missed a blue squid
            totalBlueMissed++;
            streak = 0;

            float reactionTime = currentRoundDuration;
            TelemetryManager.Instance.LogEvent("squid_blue_timeout", reactionTime, 0, streak);


            PlaySound(incorrectSound);     

            LogTelemetryEvent("squid_blue_missed");

            if (ScoreManagerMap2.Instance != null)
            {
                ScoreManagerMap2.Instance.blueMissed++;
            }
        }
        else
        {
            // Correctly ignored a red squid
            totalRedIgnored++;
            totalCorrect++;

            TelemetryManager.Instance.LogEvent("squid_red_ignore_rt", 0f, 0, streak);


            PlaySound(correctSound);      

            LogTelemetryEvent("squid_red_ignored");

            if (ScoreManagerMap2.Instance != null)
            {
                ScoreManagerMap2.Instance.redIgnored++;
            }
        }

        UpdateStreakText();
    }

    private void AdaptDifficulty()
    {
        // Overall accuracy = correct actions / total rounds
        float accuracy = (totalRounds > 0) ? (float)totalCorrect / totalRounds : 1f;

        float t = Mathf.InverseLerp(0.5f, 0.9f, accuracy);
        t = Mathf.Clamp01(t);

        // Lerp timing toward target difficulty
        float targetRound = Mathf.Lerp(easyRoundDuration, hardRoundDuration, t);
        float targetBreak = Mathf.Lerp(easyBreakDuration, hardBreakDuration, t);
        float targetBlueProb = Mathf.Lerp(maxBlueProbability, minBlueProbability, t);

        currentRoundDuration = Mathf.Lerp(currentRoundDuration, targetRound, adaptLerp);
        currentBreakDuration = Mathf.Lerp(currentBreakDuration, targetBreak, adaptLerp);
        currentBlueProbability = Mathf.Lerp(currentBlueProbability, targetBlueProb, adaptLerp);

        if (enableDebug)
        {
            Debug.Log($"[Squid Adapt] acc={accuracy:F2} t={t:F2} | round={currentRoundDuration:F2} | break={currentBreakDuration:F2} | blueProb={currentBlueProbability:F2}");
        }
    }

    private void SetButtonState(Button button, Sprite sprite, bool interactable)
    {
        if (button == null) return;

        if (sprite != null)
        {
            button.image.sprite = sprite;
            button.image.preserveAspect = true;
        }

        button.interactable = interactable;
    }

    private void UpdateStreakText()
    {
        if (streakText != null)
        {
            streakText.text = "Streak: " + streak;
        }
    }

    private void LogTelemetryEvent(string type)
    {
        if (TelemetryManager.Instance == null)
            return;

        float speed = (road != null) ? road.forwardSpeed : 0f;

        TelemetryManager.Instance.LogEvent(type, speed, 0f, streak);
    }
}
