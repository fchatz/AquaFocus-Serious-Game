using UnityEngine;

public class ScoreManagerMap2 : MonoBehaviour
{
    public static ScoreManagerMap2 Instance;

    // Mines
    public int totalPassedMines = 0;
    public int totalHitMines = 0;

    public void AddPassedMine() => totalPassedMines++;
    public void AddHitMine() => totalHitMines++;

    // Squids
    public int blueShown = 0;
    public int blueClicked = 0;
    public int blueMissed = 0;

    public int redShown = 0;
    public int redClicked = 0;
    public int redIgnored = 0;

    public float totalBlueReaction = 0f;
    public int blueReactionCount = 0;

    public float totalRedReaction = 0f;
    public int redReactionCount = 0;

    public float AvgBlueReaction => blueReactionCount > 0 ? totalBlueReaction / blueReactionCount : 0f;
    public float AvgRedReaction => redReactionCount > 0 ? totalRedReaction / redReactionCount : 0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Reward
    public int CalculateReward()
    {
        float mineCoins = totalPassedMines * 0.5f;
        float squidCoins = blueClicked * 0.5f;

        return Mathf.RoundToInt(mineCoins + squidCoins);
    }


}
