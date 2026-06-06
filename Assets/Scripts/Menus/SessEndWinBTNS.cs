using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{


    // Called by the "Play Again" button
    public void OnPlayAgainPressed()
    {
        // Reset time scale (VERY IMPORTANT)
        Time.timeScale = 1f;

        LoadingManager.Instance.LoadScene("LevelSelect");
    }

    // Called by the "Home" button
    public void OnHomePressed()
    {

        // Reset time scale (VERY IMPORTANT)
        Time.timeScale = 1f;

        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
