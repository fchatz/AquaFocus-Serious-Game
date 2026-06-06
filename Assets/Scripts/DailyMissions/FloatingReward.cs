using UnityEngine;
using System.Collections;
using TMPro;

public class FloatingReward : MonoBehaviour
{
    public float moveUpSpeed = 50f;
    public float fadeSpeed = 1.5f;
    private TextMeshProUGUI text;
    private CanvasGroup group;

    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        group = gameObject.AddComponent<CanvasGroup>();
    }

    public void Play(int amount)
    {
        text.text = $"+{amount}";
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float t = 0f;
        while (t < 1f)
        {
            transform.localPosition += Vector3.up * moveUpSpeed * Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t);
            t += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }
}
