using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    public Transform target;     
    public LayerMask wallMask;

    [Header("Lane detect")]
    public float lookAhead = 1.2f;
    public float rayHeight = 1.5f;
    public float castLength = 15f;
    public float sphereRadius = 0.3f;

    [Header("Camera placement")]
    public float distanceBehind = 6f;
    public float height = 3f;
    public float posSmooth = 0.12f;
    public float rotLerp = 12f;
    public float extraPitch = -10f;

    private Vector3 _vel;
    private Vector3 _lastLaneCenter;
    private bool _hasLane = false;
    private float _lastHalfWidth = 2f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 fwd = target.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        Vector3 origin = target.position + fwd * lookAhead + Vector3.up * rayHeight;

        bool hitL = Physics.SphereCast(origin, sphereRadius, -right, out RaycastHit hL, castLength, wallMask, QueryTriggerInteraction.Ignore);
        bool hitR = Physics.SphereCast(origin, sphereRadius, right, out RaycastHit hR, castLength, wallMask, QueryTriggerInteraction.Ignore);

        Vector3 laneCenter = target.position;

        if (hitL && hitR)
        {
            Vector3 wallVec = hR.point - hL.point; wallVec.y = 0f;
            if (Vector3.Dot(wallVec, right) < 0f)
            {
                var tmp = hL; hL = hR; hR = tmp;
            }

            laneCenter = (hL.point + hR.point) * 0.5f;
            _lastHalfWidth = Vector3.Distance(hL.point, hR.point) * 0.5f;
            _hasLane = true;
        }
        else if (hitL && _hasLane)
        {
            laneCenter = hL.point + right * _lastHalfWidth;
        }
        else if (hitR && _hasLane)
        {
            laneCenter = hR.point - right * _lastHalfWidth;
        }
        else if (_hasLane)
        {
            laneCenter = _lastLaneCenter;
        }

        // smooth lane center
        if (_hasLane)
            _lastLaneCenter = Vector3.Lerp(_lastLaneCenter, laneCenter, 1f - Mathf.Exp(-posSmooth / Mathf.Max(Time.deltaTime, 0.0001f)));
        else
            _lastLaneCenter = laneCenter;

        Vector3 back = -fwd;
        Vector3 camTargetPos = _lastLaneCenter + back * distanceBehind + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, camTargetPos, ref _vel, posSmooth);
        Quaternion camTargetRot = Quaternion.LookRotation(fwd, Vector3.up);
        camTargetRot *= Quaternion.Euler(extraPitch, 0f, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, camTargetRot, Time.deltaTime * rotLerp);

        Debug.DrawRay(origin, -right * castLength, hitL ? Color.red : Color.gray);
        Debug.DrawRay(origin, right * castLength, hitR ? Color.green : Color.gray);
    }
}
