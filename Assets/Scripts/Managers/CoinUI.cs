using TMPro;
using UnityEngine;
using System.Collections;

public class CoinUI : MonoBehaviour
{
    public static CoinUI Instance;

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI popupText;
    public CanvasGroup popupCanvas;

    public RectTransform popupRect;   // ⬅ assign the popup text RectTransform
    public RectTransform coinTarget;  // ⬅ assign the coin icon RectTransform

    [Header("Audio")]
    public AudioClip rewardSound;
    private AudioSource audioSource;

    [Header("Animation Settings")]
    public float riseDuration = 0.6f;
    public float travelDuration = 0.4f;

    [Header("Session End")]
    public GameObject sessionEndPanel;

    private void Awake()
    {
        CoinUI.Instance = this;
        popupCanvas.alpha = 0;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        sessionEndPanel.SetActive(false);
    }

    private void Start()
    {
        if (CoinManager.Instance != null)
            CoinManager.Instance.UpdateUI();
    }

    // Called by CoinManager
    public void UpdateCoinText(int value)
    {
        coinText.text = value.ToString();
    }

    // Called by Session end
    public void PlayCoinPopup(int amount)
    {
        popupText.text = "+" + amount;

        if (rewardSound != null)
            audioSource.PlayOneShot(rewardSound, 4f);

        StopAllCoroutines();
        StartCoroutine(PopupRoutine());

        //ΕΔΩ
        sessionEndPanel.SetActive(true);
    }


    private IEnumerator PopupRoutine()
    {
        popupCanvas.alpha = 1f;

        // -----------------------------
        // STEP 1: POP AND RISE
        // -----------------------------
        Vector3 startPos = popupRect.localPosition;
        Vector3 risePos = startPos + new Vector3(0, 40f, 0);

        popupRect.localScale = Vector3.one * 0.6f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;

            float ease = t * t * (3f - 2f * t); // smoothstep

            popupRect.localPosition = Vector3.Lerp(startPos, risePos, ease);

            // pop scale
            float s = Mathf.Lerp(0.6f, 1.2f, ease);
            popupRect.localScale = new Vector3(s, s, s);

            yield return null;
        }

        // -----------------------------
        // STEP 2: TRAVEL TO COIN ICON
        // -----------------------------
        Vector3 worldTarget = coinTarget.position;
        Vector3 localTarget = popupRect.parent.InverseTransformPoint(worldTarget);

        Vector3 travelStart = popupRect.localPosition;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / travelDuration;

            float ease = t * t * (3f - 2f * t);

            popupRect.localPosition = Vector3.Lerp(travelStart, localTarget, ease);

            // fade out
            popupCanvas.alpha = 1f - ease;

            // shrink slightly at the end
            popupRect.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one * 0.8f, ease);

            yield return null;
        }

        // reset
        popupCanvas.alpha = 0;
        popupRect.localScale = Vector3.one;
        popupRect.localPosition = startPos;

        // -----------------------------
        // STEP 3: UPDATE COIN TOTAL NOW
        // -----------------------------
        CoinManager.Instance.UpdateUI();
    }
}
