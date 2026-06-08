using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteConfirmationUI : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    private System.Action onConfirm;

    public void Show(System.Action confirmCallback)
    {
        onConfirm = confirmCallback;
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        yesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
