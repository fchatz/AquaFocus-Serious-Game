using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";

    private void Start()
    {

        Time.timeScale = 1f;

        // Load the main menu after managers are ready
        LoadingManager.Instance.LoadScene(firstSceneName);
    }
}
