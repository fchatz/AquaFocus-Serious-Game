using UnityEngine;
using System.Collections;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private Transform pivot;   // Point to rotate around (e.g. center of your level)
    [SerializeField] private float duration = 1f;

    private bool isRotating = false;

    public void RotateLeft()
    {
        if (!isRotating)
            StartCoroutine(RotateAroundY(-90f));
    }

    public void RotateRight()
    {
        if (!isRotating)
            StartCoroutine(RotateAroundY(90f));
    }

    private IEnumerator RotateAroundY(float totalAngle)
    {
        isRotating = true;

        // Fallback pivot: world origin if none assigned
        Vector3 pivotPos = pivot != null ? pivot.position : Vector3.zero;

        float elapsed = 0f;
        float currentAngle = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Target angle for this frame (0 → totalAngle over duration)
            float targetAngle = Mathf.Lerp(0f, totalAngle, t);

            // Delta from last frame
            float deltaAngle = targetAngle - currentAngle;
            currentAngle = targetAngle;

            // Rotate around WORLD Y axis
            transform.RotateAround(pivotPos, Vector3.up, deltaAngle);

            yield return null;
        }

        isRotating = false;
    }
}
