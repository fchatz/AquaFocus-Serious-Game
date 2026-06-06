using UnityEngine;
using UnityEngine.UI;
using TMPro;
// If you use TextMeshPro, replace Text with TextMeshProUGUI and add: using TMPro;

public class SkinShop : MonoBehaviour
{
    [Header("References")]
    public CharacterSelection characterSelection;   // drag CharacterSelection here
    public TextMeshProUGUI coinsText;                          // UI text showing coins
    public Button mainButton;                       // Buy/Select button
    public TextMeshProUGUI mainButtonLabel;                    // Text on that button
    public GameObject selectedButton;               // "Selected" (non-interactable) button

    [Header("Prices")]
    public int[] skinPrices;                        // one price per skin (same order as characters[])

    private SaveData Data => GameDataManager.Instance.Data;

    void Start()
    {
        InitUnlockedArray();
        InitSelectedSkin();
        RefreshUI();
    }

    /// <summary>
    /// Ensure Data.unlockedSkins is the right size and has a default unlocked skin 0.
    /// </summary>
    private void InitUnlockedArray()
    {
        if (GameDataManager.Instance == null || Data == null)
        {
            Debug.LogWarning("SkinShop: No GameDataManager/Data found.");
            return;
        }

        int count = characterSelection.characters.Length;

        // Safety: ensure skinPrices length matches
        if (skinPrices == null || skinPrices.Length != count)
        {
            skinPrices = new int[count];
            for (int i = 0; i < count; i++)
                skinPrices[i] = 100; // default; override in Inspector
        }

        bool[] existing = Data.unlockedSkins;

        if (existing == null || existing.Length != count)
        {
            bool[] newArray = new bool[count];

            // Copy any existing values if sizes differ
            if (existing != null)
            {
                int copyLen = Mathf.Min(existing.Length, count);
                for (int i = 0; i < copyLen; i++)
                    newArray[i] = existing[i];
            }

            // By default, first skin unlocked
            if (count > 0)
                newArray[0] = true;

            Data.unlockedSkins = newArray;
            GameDataManager.Instance.SaveGame();
        }
    }

    /// <summary>
    /// Ensure selectedSkinIndex is within range.
    /// </summary>
    private void InitSelectedSkin()
    {
        if (GameDataManager.Instance == null || Data == null)
            return;

        int count = characterSelection.characters.Length;

        if (Data.selectedSkinIndex < 0 || Data.selectedSkinIndex >= count)
        {
            Data.selectedSkinIndex = 0;
            GameDataManager.Instance.SaveGame();
        }

        // Keep CharacterSelection in sync with save
        characterSelection.selectedCharacter = Data.selectedSkinIndex;
    }

    /// <summary>
    /// Called by CharacterSelection when current preview skin changes.
    /// </summary>
    public void OnSkinChanged()
    {
        RefreshUI();
    }

    /// <summary>
    /// Hook this to the main buy/select button onClick.
    /// </summary>
    public void OnMainButtonPressed()
    {
        int index = characterSelection.selectedCharacter;

        if (Data == null)
            return;

        bool[] unlocked = Data.unlockedSkins;
        if (unlocked == null || index < 0 || index >= unlocked.Length)
            return;

        if (!unlocked[index])
        {
            TryBuySkin(index);
        }
        else
        {
            SelectSkin(index);
        }
    }

    private void TryBuySkin(int index)
    {
        int price = skinPrices[index];
        int coins = Data.totalCoins;

        if (coins < price)
        {
            Debug.Log("You don't have enough corals to buy the submarine " + index);
            // You can also show a popup or sound.
            return;
        }

        // Spend coins
        Data.totalCoins -= price;

        // Unlock this skin for THIS profile
        Data.unlockedSkins[index] = true;

        GameDataManager.Instance.SaveGame();

        RefreshUI();
    }

    private void SelectSkin(int index)
    {
        Data.selectedSkinIndex = index;
        GameDataManager.Instance.SaveGame();

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (GameDataManager.Instance == null || Data == null)
            return;

        int index = characterSelection.selectedCharacter;
        int coins = Data.totalCoins;

        if (coinsText != null)
            coinsText.text = coins.ToString();

        bool[] unlocked = Data.unlockedSkins;
        bool isUnlocked = (unlocked != null &&
                           index >= 0 &&
                           index < unlocked.Length &&
                           unlocked[index]);

        int selectedIndex = Data.selectedSkinIndex;
        bool isSelected = (selectedIndex == index);

        if (!isUnlocked)
        {
            // LOCKED → show price
            if (mainButton != null)
            {
                mainButton.gameObject.SetActive(true);
                mainButton.interactable = coins >= skinPrices[index];
            }

            if (mainButtonLabel != null)
                mainButtonLabel.text = skinPrices[index] + " Corals";

            if (selectedButton != null)
                selectedButton.SetActive(false);
        }
        else if (!isSelected)
        {
            // UNLOCKED but not selected → show "Select"
            if (mainButton != null)
            {
                mainButton.gameObject.SetActive(true);
                mainButton.interactable = true;
            }

            if (mainButtonLabel != null)
                mainButtonLabel.text = "Select";

            if (selectedButton != null)
                selectedButton.SetActive(false);
        }
        else
        {
            // CURRENTLY SELECTED → show "Selected" UI
            if (mainButton != null)
                mainButton.gameObject.SetActive(false);

            if (selectedButton != null)
                selectedButton.SetActive(true);
        }
    }
}
