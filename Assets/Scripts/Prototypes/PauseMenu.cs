using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] GameObject assistantMenu;
    [SerializeField] GameObject assistantPrefab;
    [SerializeField] AudioSource popupAudio;   // <-- add this

    private void Start()
    {
        StartCoroutine(AssistantPause());
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        assistantMenu.SetActive(false);
        assistantPrefab.SetActive(false);

        if (popupAudio != null)
            popupAudio.Stop();   // <-- stop audio when window closes
    }

    IEnumerator AssistantPause()
    {
        yield return new WaitForSeconds(2);
        Time.timeScale = 0f;
        assistantMenu.SetActive(true);
        assistantPrefab.SetActive(true);

        if (popupAudio != null)
            popupAudio.Play();   // <-- play audio when window shows

    }
}
