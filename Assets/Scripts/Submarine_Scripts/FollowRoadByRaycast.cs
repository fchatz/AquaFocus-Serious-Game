using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FollowRoadByRaycast : MonoBehaviour
{
    [Header("Forward movement")]
    public float forwardSpeed = 10f;
    public float minSpeed = 5f;          // minimum submarine speed after penalties
    public float maxSpeed = 10f;         // maximum speed (original top speed)
    public float speedRecoverRate = 1f;  // speed recovery per second when performing well
    public float speedPenaltyAmount = 1f; // how much speed is reduced per missed jellyfish
    public float penaltyDuration = 3f;   // how long a penalty affects the player before recovery starts

    [Header("Side / centering")]
    public float centerFollowSpeed = 20f;
    public float manualSideSpeed = 4f;
    [Tooltip("Keep this at 1: -1 = full left, 1 = full right")]
    public float maxManualOffset = 1f;

    [Header("Turning")]
    public float directionLerp = 0.25f;
    public float maxTurnDegreesPerSec = 150f;
    public float rotateSpeed = 14f;

    [Header("Side rays")]
    public float rayHeight = 1.5f;
    public float rayLength = 12f;
    public float lookAheadNear = 1.0f;
    public float lookAheadFar = 3.0f;
    public LayerMask wallMask;

    [Header("Ground")]
    public float groundRayHeight = 5f;
    public float groundRayLength = 20f;
    public LayerMask groundMask;

    [Header("Per-side padding")]
    public float leftPadding = 0.1f;
    public float rightPadding = 0.1f;

    [Header("Ray smoothing")]
    [Range(0f, 1f)]
    public float raySmoothFactor = 0.15f;     // lower = smoother, higher = more responsive

    private CharacterController _cc;
    private float _manualOffset;
    private Vector3 _lastCenter;
    private Vector3 _lastForward;
    private bool _hasLast;

    private Vector3 _smoothedNearLeft;
    private Vector3 _smoothedNearRight;
    private bool _hasSmoothedNear;

    public bool HasWallData { get; private set; }
    public Vector3 LeftWallPoint { get; private set; }
    public Vector3 RightWallPoint { get; private set; }

    public Vector3 RoadCenter => _lastCenter;

    private int lastMissedCount = 0;
    private float penaltyTimer = 0f;

    public float CurrentSpeed => forwardSpeed;
    public float CurrentOffset => _manualOffset;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        maxSpeed = forwardSpeed; // store original top speed
    }

    private void Update()
    {
        HandleDynamicSpeed(); // manages speed adaptively

        // -----------------------------
        // 1) INPUT 
        // -----------------------------
        float h = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1f;
        }
        _manualOffset += h * manualSideSpeed * Time.deltaTime;
        _manualOffset = Mathf.Clamp(_manualOffset, -maxManualOffset, maxManualOffset);

        // -----------------------------
        // 2) WALL RAYCASTING & MOVEMENT
        // -----------------------------
        Vector3 baseForward = (_hasLast && _lastForward.sqrMagnitude > 0.001f)
            ? _lastForward.normalized
            : transform.forward;
        Vector3 baseRight = Vector3.Cross(Vector3.up, baseForward).normalized;

        Vector3 oNear = transform.position + baseForward * lookAheadNear + Vector3.up * rayHeight;
        Vector3 oFar = transform.position + baseForward * lookAheadFar + Vector3.up * rayHeight;

        bool nLeft = Physics.Raycast(oNear, -baseRight, out RaycastHit nL, rayLength, wallMask, QueryTriggerInteraction.Ignore);
        bool nRight = Physics.Raycast(oNear, baseRight, out RaycastHit nR, rayLength, wallMask, QueryTriggerInteraction.Ignore);
        Debug.DrawLine(oNear, oNear - baseRight * rayLength, nLeft ? Color.red : Color.gray);
        Debug.DrawLine(oNear, oNear + baseRight * rayLength, nRight ? Color.green : Color.gray);

        bool fLeft = Physics.Raycast(oFar, -baseRight, out RaycastHit fL, rayLength, wallMask, QueryTriggerInteraction.Ignore);
        bool fRight = Physics.Raycast(oFar, baseRight, out RaycastHit fR, rayLength, wallMask, QueryTriggerInteraction.Ignore);
        Debug.DrawLine(oFar, oFar - baseRight * rayLength, fLeft ? Color.magenta : Color.gray);
        Debug.DrawLine(oFar, oFar + baseRight * rayLength, fRight ? Color.cyan : Color.gray);

        bool gotNear = nLeft && nRight;
        bool gotFar = fLeft && fRight;

        Vector3 corrCenter;
        Vector3 corrForward;
        Vector3 corrRight;

        if (gotNear)
        {
            // Spatial validation: enforce strict left-to-right orientation layout across the tracking corridor
            Vector3 nWall = nR.point - nL.point;
            nWall.y = 0f;
            if (Vector3.Dot(nWall, baseRight) < 0f)
            {
                var tmp = nL; nL = nR; nR = tmp;
                nWall = nR.point - nL.point;
                nWall.y = 0f;
            }

            // Low-pass filter routine: mitigates spatial aliasing and jitter caused by high-frequency surface noise on procedural meshes
            if (!_hasSmoothedNear)
            {
                _smoothedNearLeft = nL.point;
                _smoothedNearRight = nR.point;
                _hasSmoothedNear = true;
            }
            else
            {
                _smoothedNearLeft = Vector3.Lerp(_smoothedNearLeft, nL.point, raySmoothFactor);
                _smoothedNearRight = Vector3.Lerp(_smoothedNearRight, nR.point, raySmoothFactor);
            }

            Vector3 nWallSmoothed = _smoothedNearRight - _smoothedNearLeft;
            nWallSmoothed.y = 0f;

            Vector3 nCenter = (_smoothedNearLeft + _smoothedNearRight) * 0.5f;

            Vector3 nearFwd;
            if (_hasLast)
            {
                Vector3 delta = nCenter - _lastCenter;
                delta.y = 0f;
                nearFwd = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.Cross(Vector3.up, nWallSmoothed.normalized);
            }
            else
            {
                nearFwd = Vector3.Cross(Vector3.up, nWallSmoothed.normalized);
                if (Vector3.Dot(nearFwd, baseForward) < 0f) nearFwd = -nearFwd;
            }

            Vector3 finalFwd = nearFwd;
            if (gotFar)
            {
                Vector3 fWall = fR.point - fL.point;
                fWall.y = 0f;
                if (Vector3.Dot(fWall, baseRight) < 0f)
                {
                    var tmp2 = fL; fL = fR; fR = tmp2;
                    fWall = fR.point - fL.point;
                    fWall.y = 0f;
                }

                Vector3 fCenter = (fL.point + fR.point) * 0.5f;
                Vector3 fwd2 = fCenter - nCenter;
                fwd2.y = 0f;
                if (fwd2.sqrMagnitude > 0.0001f)
                {
                    fwd2 = fwd2.normalized;
                    finalFwd = Vector3.Slerp(nearFwd, fwd2, 0.5f); // Look-ahead predictive trajectory smoothing
                }
            }

            finalFwd.y = 0f;
            if (finalFwd.sqrMagnitude < 0.0001f) finalFwd = baseForward;
            if (_hasLast && Vector3.Dot(finalFwd, _lastForward) < 0f)
                finalFwd = -finalFwd;

            Vector3 blended = _hasLast
                ? Vector3.Slerp(_lastForward, finalFwd, directionLerp)
                : finalFwd;
            blended = ClampTurn(_lastForward, blended, maxTurnDegreesPerSec * Time.deltaTime);

            corrForward = blended.normalized;
            corrRight = Vector3.Cross(Vector3.up, corrForward).normalized;
            corrCenter = nCenter;

            _lastCenter = corrCenter;
            _lastForward = corrForward;
            _hasLast = true;

            LeftWallPoint = _smoothedNearLeft;
            RightWallPoint = _smoothedNearRight;
            HasWallData = true;
        }
        else
        {
            if (_hasLast)
            {
                corrCenter = _lastCenter;
                corrForward = _lastForward;
                corrRight = Vector3.Cross(Vector3.up, corrForward).normalized;
            }
            else
            {
                corrCenter = transform.position;
                corrForward = baseForward;
                corrRight = baseRight;
            }

            HasWallData = false;
            _hasSmoothedNear = false;     // Reset filtering context upon tracking state loss
        }

        Vector3 targetPos = transform.position;
        if (gotNear)
        {
            Vector3 toMe = transform.position - corrCenter;
            float currentSide = Vector3.Dot(toMe, corrRight);

            float distLeft = Vector3.Distance(corrCenter, LeftWallPoint);
            float distRight = Vector3.Distance(corrCenter, RightWallPoint);

            // Bounds computation: enforces safety padding zones relative to filtered cross-sectional constraints
            float leftLimit = -(Mathf.Max(0f, distLeft - leftPadding));
            float rightLimit = (Mathf.Max(0f, distRight - rightPadding));

            float t = (_manualOffset + 1f) * 0.5f;
            float desiredSide = Mathf.Lerp(leftLimit, rightLimit, t);

            float sideError = desiredSide - currentSide;
            float sideMove = sideError * centerFollowSpeed * Time.deltaTime;

            targetPos = transform.position + corrRight * sideMove;
        }

        Vector3 forwardMove = (_hasLast ? _lastForward : corrForward) * (forwardSpeed * Time.deltaTime);
        Vector3 finalPos = targetPos + forwardMove;

        // Ground clamping routine: keeps the submarine locked to the seafloor plane topology via raycasting vertical re-projections
        if (Physics.Raycast(finalPos + Vector3.up * groundRayHeight, Vector3.down,
                            out RaycastHit gHit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (gHit.normal.y > 0.5f)
                finalPos.y = gHit.point.y;
        }

        _cc.Move(finalPos - transform.position);

        Vector3 lookDir = (_hasLast ? _lastForward : corrForward);
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }
    }

    /// <summary>
    /// Monitors performance metrics across frames to scale user traversal velocities adaptively.
    /// </summary>
    private void HandleDynamicSpeed()
    {
        if (ScoreManager.Instance == null) return;

        int missed = ScoreManager.Instance.GetMissedCount();
        if (missed > lastMissedCount)
        {
            forwardSpeed = Mathf.Max(minSpeed, forwardSpeed - speedPenaltyAmount);
            lastMissedCount = missed;

            if (TelemetryManager.Instance != null)
                TelemetryManager.Instance.LogEvent("penalty", forwardSpeed);
        }
    }

    private Vector3 ClampTurn(Vector3 from, Vector3 to, float maxDeg)
    {
        if (from.sqrMagnitude < 0.0001f) return to.normalized;
        if (to.sqrMagnitude < 0.0001f) return from.normalized;

        float ang = Vector3.SignedAngle(from, to, Vector3.up);
        float clamped = Mathf.Clamp(ang, -maxDeg, maxDeg);
        return Quaternion.AngleAxis(clamped, Vector3.up) * from.normalized;
    }

    public void OnMissedJellyfish()
    {
        forwardSpeed = Mathf.Max(minSpeed, forwardSpeed - speedPenaltyAmount);
        penaltyTimer = penaltyDuration;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.LogEvent("miss_penalty", forwardSpeed);
    }

    public void OnCollectJellyfish()
    {
        float recoveryBoost = speedRecoverRate * 0.5f;
        forwardSpeed = Mathf.Min(maxSpeed, forwardSpeed + recoveryBoost);

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.LogEvent("collect_bonus", forwardSpeed);
    }
}