using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI References")]
    public Canvas loadingCanvas;          // <-- Canvas, not GameObject
    public CanvasGroup canvasGroup;       // from FadePanel (optional, can be null)

    [Header("Spinner (optional)")]
    public Image spinner;

    [Header("Progress Bar")]
    public Image progressBarFill;         // assign ProgressBarFill (Image type = Filled)

    [Header("Text")]
    public TMP_Text loadingText;          // assign LoadingText (TMP)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Persist between scenes
        DontDestroyOnLoad(gameObject);
        if (loadingCanvas != null)
            DontDestroyOnLoad(loadingCanvas.gameObject);

        // Start hidden
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("LOADSCENE CALLED for " + sceneName);
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Show loading UI
        if (loadingCanvas != null)
            loadingCanvas.enabled = true;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;  // no fade yet, just visible

        if (loadingText != null)
            loadingText.text = "Please wait...";

        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;

        yield return new WaitForSeconds(0.1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBarFill != null)
                progressBarFill.fillAmount = progress;

            if (loadingText != null)
                loadingText.text = "Loaded " + Mathf.RoundToInt(progress * 100f) + "%";

            if (op.progress >= 0.9f)
            {
                if (loadingText != null)
                    loadingText.text = "Loading...";

                yield return new WaitForSeconds(0.25f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading UI
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
