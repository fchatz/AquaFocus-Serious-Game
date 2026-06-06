using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor; // needed because AutoAssignSubmarineMaterials uses editor stuff
#endif

public class LoadCharacter : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;
    public TMP_Text label;

    [Header("Spawned Character Scale")]
    public bool overrideScale = false;        // if true, apply custom scale
    public Vector3 customScale = Vector3.one; // set in Inspector

    void Start()
    {
/*#if UNITY_EDITOR
        AutoAssignSubmarineMaterials.Assign();
#endif*/

        int selectedCharacter = 0;

        // Get selected skin from the current profile's SaveData
        if (GameDataManager.Instance != null && GameDataManager.Instance.Data != null)
        {
            selectedCharacter = GameDataManager.Instance.Data.selectedSkinIndex;
        }
        else
        {
            Debug.LogWarning("LoadCharacter: GameDataManager or Data is null. Defaulting selectedCharacter to 0.");
        }

        // Safety clamp
        if (selectedCharacter < 0 || selectedCharacter >= characterPrefabs.Length)
        {
            Debug.LogWarning("Selected character index out of range, defaulting to 0");
            selectedCharacter = 0;
        }

        GameObject prefab = characterPrefabs[selectedCharacter];

        // Instantiate the skin as a child of the player (this GameObject)
        GameObject clone = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation,
            transform // parent = Player
        );

        // Align with spawnPoint locally
        clone.transform.localPosition = spawnPoint.localPosition;
        clone.transform.localRotation = spawnPoint.localRotation;

        // Apply custom scale if enabled
        if (overrideScale)
        {
            clone.transform.localScale = customScale;
        }

        // Optional label update
        if (label != null)
        {
            label.text = prefab.name;
        }
    }
}
