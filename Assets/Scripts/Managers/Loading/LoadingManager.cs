using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI References")]
    public Canvas loadingCanvas;
    public CanvasGroup canvasGroup;

    [Header("Spinner (optional)")]
    public Image spinner;

    [Header("Progress Bar")]
    public Image progressBarFill;

    [Header("Text")]
    public TMP_Text loadingText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Configure persistent system flags across runtime scene transitions
        DontDestroyOnLoad(gameObject);
        if (loadingCanvas != null)
            DontDestroyOnLoad(loadingCanvas.gameObject);

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
        // Enable viewport visual components for scene tracking
        if (loadingCanvas != null)
            loadingCanvas.enabled = true;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

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

        // Smoothly clear display interface elements once setup tasks finish
        float fadeTime = 0.4f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        if (loadingCanvas != null)
            loadingCanvas.enabled = false;
    }
}