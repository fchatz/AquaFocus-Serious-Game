using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    [Header("Characters")]
    public GameObject[] characters;
    public int selectedCharacter = 0;

    [Header("Store / UI")]
    public SkinShop skinShop;

    private void Start()
    {
/*#if UNITY_EDITOR
        AutoAssignSubmarineMaterials.Assign();
#endif*/
        int indexFromSave = 0;

        // Read selected skin from current profile's save
        if (GameDataManager.Instance != null && GameDataManager.Instance.Data != null)
        {
            indexFromSave = GameDataManager.Instance.Data.selectedSkinIndex;
        }

        if (indexFromSave < 0 || indexFromSave >= characters.Length)
            indexFromSave = 0;

        selectedCharacter = indexFromSave;

        // Activate only the current character
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == selectedCharacter);
        }

        if (skinShop != null)
            skinShop.OnSkinChanged();
    }

    public void NextCharacter()
    {
        characters[selectedCharacter].SetActive(false);
        selectedCharacter = (selectedCharacter + 1) % characters.Length;
        characters[selectedCharacter].SetActive(true);

        if (skinShop != null)
            skinShop.OnSkinChanged();
    }

    public void PreviousCharacter()
    {
        characters[selectedCharacter].SetActive(false);
        selectedCharacter--;
        if (selectedCharacter < 0)
        {
            selectedCharacter += characters.Length;
        }
        characters[selectedCharacter].SetActive(true);

        if (skinShop != null)
            skinShop.OnSkinChanged();
    }

    public void ReturnButton()
    {
        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
