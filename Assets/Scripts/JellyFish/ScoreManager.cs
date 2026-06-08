using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Reference")]
    public TextMeshProUGUI jellyfishText;

    [Header("Flash Settings")]
    public Color collectColor = Color.green;
    public Color missColor = Color.red;
    public Color wrongColor = new Color(1f, 0.3f, 0.3f);
    public float flashDuration = 0.3f;

    private int score = 0;
    private int missed = 0;
    private int wrong = 0;

    private int currentStreak = 0;
    public int CurrentStreak => currentStreak;

    private Color originalColor;
    private float flashTimer = 0f;
    private Color targetColor;

    public int GetCollectedCount() => score;
    public int GetMissedCount() => missed;
    public int GetWrongCount() => wrong;

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
        if (jellyfishText != null)
            originalColor = jellyfishText.color;

        UpdateUI();
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (jellyfishText != null)
            {
                float t = 1f - (flashTimer / flashDuration);
                jellyfishText.color = Color.Lerp(targetColor, originalColor, t);
            }
        }
    }

    // Process blue jellyfish collection scoring mechanics
    public void AddScore(int amount)
    {
        score += amount;
        currentStreak += 1;
        UpdateUI();
        FlashText(collectColor);
    }

    // Process missed target events and handling reset conditions
    public void AddMissed(int amount)
    {
        missed += amount;
        currentStreak = 0;
        UpdateUI();
        FlashText(missColor);
    }

    // Process obstacle collision or wrong selection penalties
    public void AddWrong(int amount)
    {
        wrong += amount;
        currentStreak = 0;
        UpdateUI();
        FlashText(wrongColor);
    }

    public void ResetStreak()
    {
        currentStreak = 0;
        UpdateUI();
    }

    private void FlashText(Color color)
    {
        if (jellyfishText == null) return;
        targetColor = color;
        flashTimer = flashDuration;
        jellyfishText.color = color;
    }

    private void UpdateUI()
    {
        if (jellyfishText != null)
        {
            jellyfishText.text = $"{score}";
        }
    }
}