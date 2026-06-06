using UnityEngine;

public class Map1UIButtons : MonoBehaviour
{
    public void OnContinuePressed()
    {
        // Resume game
        Time.timeScale = 1f;

        // Resume SessionManager logic (if you want)
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.Resume();
        }

        Debug.Log("? Continue pressed");
    }

    public void OnMenuPressed()
    {
        // Go back to MainMenu
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        Debug.Log("?? Menu pressed");
    }
}
