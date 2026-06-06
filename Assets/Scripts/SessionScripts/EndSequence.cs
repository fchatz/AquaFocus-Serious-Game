using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndSequence : MonoBehaviour
{
    public static EndSequence Instance;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    [Header("Camera Settings")]
    public Transform cameraRig;  // the camera holder
    public float cameraMoveBackDistance = 8f;
    public float cameraMoveDuration = 1.5f;

    [Header("Submarine Settings")]
    public float forwardSpeedDuringEnd = 5f;

    private bool isPlaying = false;
    private Transform player;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySequence(Transform playerSub)
    {
        if (isPlaying) return;

        isPlaying = true;
        player = playerSub;

        Time.timeScale = 1f;

        // Disable CharacterController if present
        CharacterController cc = playerSub.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Stop regular movement
        FollowRoadByRaycast movement = player.GetComponent<FollowRoadByRaycast>();
        if (movement != null)
            movement.enabled = false;

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        float t = 0f;

        Vector3 camStart = cameraRig.position;
        Vector3 camTarget = camStart - cameraRig.forward * cameraMoveBackDistance;

        Color c = fadeImage.color;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / cameraMoveDuration;

            // Submarine moves forward
            player.Translate(Vector3.forward * forwardSpeedDuringEnd * Time.unscaledDeltaTime);

            // Camera slowly slides backward
            cameraRig.position = Vector3.Lerp(camStart, camTarget, t);

            // Fade out
            float fadeT = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, fadeT);

            yield return null;
        }

        // After animation
        SessionManager.Instance.EndSession();
    }
}
