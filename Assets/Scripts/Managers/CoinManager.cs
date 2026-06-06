using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int TotalCoins = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load coins from save data
        if (GameDataManager.Instance != null)
            TotalCoins = GameDataManager.Instance.Data.totalCoins;

        // Update UI if exists
        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);
    }

    // Add coins
    public void AddCoins(int amount)
    {
        TotalCoins += amount;
        Debug.Log($"[Coins] Added {amount}, Total = {TotalCoins}");
        GameDataManager.Instance.Data.totalCoins = TotalCoins;
        GameDataManager.Instance.SaveGame();

        // Update UI if exists
        if (CoinUI.Instance != null)
        {
            CoinUI.Instance.UpdateCoinText(TotalCoins);
            CoinUI.Instance.PlayCoinPopup(amount);
        }
    }

    // Remove coins (for shop later)
    public bool SpendCoins(int amount)
    {
        if (TotalCoins < amount) return false;

        TotalCoins -= amount;

        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);

        return true;
    }

    public void UpdateUI()
    {
        if (CoinUI.Instance != null)
            CoinUI.Instance.UpdateCoinText(TotalCoins);
    }

}
