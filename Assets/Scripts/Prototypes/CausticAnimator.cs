using UnityEngine;

[RequireComponent(typeof(Light))]
public class CausticAnimator : MonoBehaviour
{
    public float scrollSpeedX = 0.05f;
    public float scrollSpeedY = 0.03f;
    public float rotateSpeed = 10f;

    private Light causticLight;
    private float offsetX, offsetY, rotation;

    void Start()
    {
        causticLight = GetComponent<Light>();
    }

    void Update()
    {
        offsetX += scrollSpeedX * Time.deltaTime;
        offsetY += scrollSpeedY * Time.deltaTime;
        rotation += rotateSpeed * Time.deltaTime;

        Matrix4x4 mat = Matrix4x4.TRS(
            new Vector3(offsetX, offsetY, 0),
            Quaternion.Euler(0, 0, rotation),
            Vector3.one
        );

        Shader.SetGlobalMatrix("_LightCookieMatrix", mat);
    }
}
