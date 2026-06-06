using UnityEngine;

/// <summary>
/// Attach this to the BLUE jellyfish prefab.
/// On spawn, it checks for nearby mines and tries to move away.
/// If it can't find a safe spot after a few attempts, it destroys itself.
/// </summary>
public class AvoidMinesOnSpawn : MonoBehaviour
{
    [Header("Proximity Settings")]
    [Tooltip("Minimum distance this jellyfish must keep from any mine.")]
    public float minDistanceToMines = 4f;

    [Tooltip("How many times we try to nudge sideways to find a safe spot.")]
    public int maxRepositionAttempts = 5;

    [Tooltip("Base sideways distance per attempt (scaled by attempt number).")]
    public float lateralStepSize = 2f;

    [Header("Mine Detection")]
    [Tooltip("Layer mask used to detect mines. Set this to your 'Mine' layer in the Inspector.")]
    public LayerMask mineLayer;

    [Header("Behaviour")]
    [Tooltip("If true and no safe spot is found, the jellyfish destroys itself.")]
    public bool destroyIfNoSafeSpot = true;

    [Header("Debug")]
    public bool debugLogs = false;
    public bool debugDrawGizmo = false;

    private Transform player;

    private void Awake()
    {
        // Find the player (used only to know what "sideways" is).
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void Start()
    {
        // Pull config from the boot-scene manager if it exists
        if (SpawnProximityManager.Instance != null)
        {
            minDistanceToMines = SpawnProximityManager.Instance.minDistanceBlueToMines;
            mineLayer = SpawnProximityManager.Instance.mineLayer;
        }

        TryRelocateAwayFromMines();
    }


    private void TryRelocateAwayFromMines()
    {
        // If no mine layer is set, there is nothing we can safely check.
        if (mineLayer == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[AvoidMinesOnSpawn] Mine layer mask is 0. Please assign your 'Mine' layer in the Inspector.");
            return;
        }

        int attempt = 0;

        // Use player's right vector as "lateral" direction if available, else world right.
        Vector3 right = (player != null) ? player.right : Vector3.right;

        // First check: are we already too close?
        while (attempt < maxRepositionAttempts && IsTooCloseToMine())
        {
            attempt++;

            // Alternate moving right and left: +, -, +, -, ...
            float direction = (attempt % 2 == 1) ? 1f : -1f;

            // Step size increases with each attempt (1x, 2x, 3x, ...)
            float stepDistance = lateralStepSize * attempt * direction;

            Vector3 newPos = transform.position + right * stepDistance;
            transform.position = newPos;

            if (debugLogs)
            {
                Debug.Log($"[AvoidMinesOnSpawn] Attempt {attempt}: moved {stepDistance:F2} units sideways.");
            }
        }

        // After all attempts, if we're still too close, decide what to do.
        if (IsTooCloseToMine())
        {
            if (destroyIfNoSafeSpot)
            {
                if (debugLogs)
                {
                    Debug.Log("[AvoidMinesOnSpawn] Still too close to a mine after all attempts → destroying jellyfish.");
                }
                Destroy(gameObject);
            }
            else if (debugLogs)
            {
                Debug.Log("[AvoidMinesOnSpawn] Still too close to a mine but destroyIfNoSafeSpot = false, keeping jellyfish.");
            }
        }
        else if (debugLogs)
        {
            Debug.Log("[AvoidMinesOnSpawn] Found a safe position away from mines.");
        }
    }

    private bool IsTooCloseToMine()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, minDistanceToMines, mineLayer);
        return hits != null && hits.Length > 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDrawGizmo) return;

        Gizmos.DrawWireSphere(transform.position, minDistanceToMines);
    }
}
