using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float rotationDuration = 1f;

    [Header("World Count")]
    [SerializeField] private int worldCount = 3;   // 👈 We’ll use 3, but can be changed

    [Header("Scenes (match worldCount)")]
    [SerializeField] private string[] sceneNames;   // e.g. size 3 in Inspector

    [Header("World Names (for UI, match worldCount)")]
    [SerializeField] private string[] worldNames;   // e.g. size 3 in Inspector
    [SerializeField] private TextMeshProUGUI worldNameText;

    [Header("UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI selectButtonText;


    private bool isRotating = false;
    private int currentIndex = 0;  // 0..worldCount-1

    


    private float StepAngle => 360f / Mathf.Max(1, worldCount);  // 120° if worldCount == 3

    private void Start()
    {
        //test gia map unlock
        var data = GameDataManager.Instance.Data;

        // Clamp currentIndex in case user sets weird values in inspector
        if (worldCount < 1) worldCount = 1;
        currentIndex = Mathf.Clamp(currentIndex, 0, worldCount - 1);

        UpdateWorldName();
        UpdateSelectButtonState();
    }

    public void RotateLeft()
    {
        if (!isRotating)
        {
            // -1 step => -120° when worldCount == 3
            StartCoroutine(RotateSteps(-1));
        }
    }

    public void RotateRight()
    {
        if (!isRotating)
        {
            // +1 step => +120° when worldCount == 3
            StartCoroutine(RotateSteps(+1));
        }
    }

    public void ReturnMenu()
    {
        LoadingManager.Instance.LoadScene("MainMenu");
    }

    public void PressPlay()
    {
        if (isRotating) return;

        if (sceneNames == null || sceneNames.Length < worldCount)
        {
            Debug.LogWarning("StageSelect: Please assign at least 'worldCount' scene names in the inspector.");
            return;
        }

        string sceneToLoad = sceneNames[currentIndex];

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("StageSelect: Scene name is empty for index " + currentIndex);
            return;
        }



        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Rotates around the pivot by a number of "steps".
    /// For 3 worlds: 1 step = 120 degrees.
    /// stepDelta can be +1 (right) or -1 (left), or any integer.
    /// </summary>
    private IEnumerator RotateSteps(int stepDelta)
    {
        if (worldCount <= 0)
            yield break;

        isRotating = true;
        UpdateSelectButtonState();   // Disable button while rotating

        float totalAngle = StepAngle * stepDelta;
        Vector3 pivotPos = pivot != null ? pivot.position : Vector3.zero;

        float elapsed = 0f;
        float currentAngle = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);

            float targetAngle = Mathf.Lerp(0f, totalAngle, t);
            float deltaAngle = targetAngle - currentAngle;
            currentAngle = targetAngle;

            transform.RotateAround(pivotPos, Vector3.up, deltaAngle);

            yield return null;
        }

        // Update index: wrap around 0..worldCount-1
        currentIndex = (currentIndex + stepDelta) % worldCount;
        if (currentIndex < 0) currentIndex += worldCount;

        UpdateWorldName();

        isRotating = false;
        UpdateSelectButtonState();   // Re-enable button
        //test
        UpdateRequirementsPanel();

    }

    private void UpdateWorldName()
    {
        if (worldNameText == null)
            return;

        if (worldNames != null && worldNames.Length > currentIndex)
        {
            worldNameText.text = worldNames[currentIndex];
        }
        else
        {
            worldNameText.text = "";
        }
    }

    private void UpdateSelectButtonState()
    {
        //if (selectButton != null)
        //{
        //    selectButton.interactable = !isRotating;
        //}

        if (selectButton == null) return;

        var data = GameDataManager.Instance.Data;

        // Default: button enabled
        bool unlocked = true;

        if (currentIndex == 1)   // Map 2
            unlocked = data.map2Unlocked;
        else if (currentIndex == 2)  // Map 3
            unlocked = data.map3Unlocked;

        selectButton.interactable = unlocked && !isRotating;
        selectButtonText.text = unlocked ? "PLAY" : "<color=red>LOCKED</color>";
    }

    private void UpdateRequirementsPanel()
    {
        var data = GameDataManager.Instance.Data;

        // MAP 1 → unlocked always
        if (currentIndex == 0)
        {
            RequirementsPanel.Instance.Hide();
            selectButtonText.text = "PLAY";
            return;
        }

        // MAP 2
        if (currentIndex == 1)
        {
            if (!data.map2Unlocked)
            {
                RequirementsPanel.Instance.ShowMap2Requirements();
                selectButtonText.text = "<color=red>LOCKED</color>";
            }
            else
            {
                RequirementsPanel.Instance.Hide();
                selectButtonText.text = "PLAY";
            }
            return;
        }

        // MAP 3
        if (currentIndex == 2)
        {
            if (!data.map3Unlocked)
            {
                RequirementsPanel.Instance.ShowMap3Requirements();
                selectButtonText.text = "<color=red>LOCKED</color>";
            }
            else
            {
                RequirementsPanel.Instance.Hide();
                selectButtonText.text = "PLAY";
            }
            return;
        }
    }


    //public void OnMap2LockedPressed()
    //{
    //    Popup.Show("Για να ξεκλειδώσεις τον Χάρτη 2:\n" +
    //               "• Ακρίβεια ≥ 70%\n" +
    //               "• Λιγότερα από 15% λάθος κόκκινα\n" +
    //               "• Σειρά ≥ 10\n" +
    //               "• Τουλάχιστον 3 συνεδρίες");
    //}

    //public void OnMap3LockedPressed()
    //{
    //    Popup.Show("Για να ξεκλειδώσεις τον Χάρτη 3:\n" +
    //               "• Αποφυγή ναρκών ≥ 65%\n" +
    //               "• Σωστοί Μπλε ≥ 70%\n" +
    //               "• Σωστή αγνόηση Κόκκινων ≥ 80%\n" +
    //               "• Αντίδραση ≤ 1.2s\n" +
    //               "• Τουλάχιστον 4 συνεδρίες");
    //}

}
