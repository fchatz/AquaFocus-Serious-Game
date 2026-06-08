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
    [SerializeField] private int worldCount = 3;  

    [Header("Scenes")]
    [SerializeField] private string[] sceneNames;  

    [Header("World Names")]
    [SerializeField] private string[] worldNames;   
    [SerializeField] private TextMeshProUGUI worldNameText;

    [Header("UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI selectButtonText;


    private bool isRotating = false;
    private int currentIndex = 0;  

    


    private float StepAngle => 360f / Mathf.Max(1, worldCount); 

    private void Start()
    {
        var data = GameDataManager.Instance.Data;
        if (worldCount < 1) worldCount = 1;
        currentIndex = Mathf.Clamp(currentIndex, 0, worldCount - 1);
        UpdateWorldName();
        UpdateSelectButtonState();
    }

    public void RotateLeft()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateSteps(-1));
        }
    }

    public void RotateRight()
    {
        if (!isRotating)
        {
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
            Debug.LogWarning("StageSelect: assign at least 'worldCount' scene names in the inspector.");
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

        currentIndex = (currentIndex + stepDelta) % worldCount;
        if (currentIndex < 0) currentIndex += worldCount;

        UpdateWorldName();
        isRotating = false;
        UpdateSelectButtonState();
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
        if (selectButton == null) return;

        var data = GameDataManager.Instance.Data;
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

        // Map 1 always unlocked
        if (currentIndex == 0)
        {
            RequirementsPanel.Instance.Hide();
            selectButtonText.text = "PLAY";
            return;
        }

        // Map 2
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

        // Map 3
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
}
