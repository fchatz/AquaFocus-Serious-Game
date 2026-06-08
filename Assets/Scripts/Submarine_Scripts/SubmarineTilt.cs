using UnityEngine;
using UnityEngine.InputSystem;

public class SubmarineTilt : MonoBehaviour
{
    public float tiltAngle = 10f;
    public float tiltSpeed = 8f;

    private float targetTilt = 0f;

    void Update()
    {
        float tilt = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            tilt = tiltAngle;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            tilt = -tiltAngle;

        targetTilt = Mathf.Lerp(targetTilt, tilt, Time.deltaTime * tiltSpeed);
        Vector3 e = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(e.x, e.y, targetTilt);

    }
}
