using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager Instance;

    [Header("Music")]
    public AudioSource musicSource;

    // List of scenes where music should play
    public string[] menuScenes = {
        "MainMenu",
        "ProfileSelection",
        "ProfileCreate",
        "Options",
        "Store",
        "StageSelect",
        "ParentDashboard"
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMenu = false;

        foreach (string s in menuScenes)
        {
            if (scene.name == s)
            {
                isMenu = true;
                break;
            }
        }

        if (isMenu)
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            if (musicSource.isPlaying)
                musicSource.Stop();
        }
    }
}
