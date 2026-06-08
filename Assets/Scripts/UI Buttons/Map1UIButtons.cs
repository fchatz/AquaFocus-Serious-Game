using UnityEngine;

public class Map1UIButtons : MonoBehaviour
{
    public void OnContinuePressed()
    {
        Time.timeScale = 1f;

        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.Resume();
        }

        Debug.Log("Continue pressed");
    }

    public void OnMenuPressed()
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        Debug.Log("Menu pressed");
    }
}
