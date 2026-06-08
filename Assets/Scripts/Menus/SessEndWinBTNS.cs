using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void OnPlayAgainPressed()
    {
        Time.timeScale = 1f;
        LoadingManager.Instance.LoadScene("LevelSelect");
    }

    public void OnHomePressed()
    {
        Time.timeScale = 1f;
        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
