using UnityEngine;

public class ObstacleHit : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Prevent double hit

        if (other.CompareTag("Player"))
        {
            var hitHandler = other.GetComponent<PlayerObstacleHitHandler>();
            if (hitHandler != null && hitHandler.CanBeHit())
            {
                triggered = true;
                hitHandler.OnObstacleHit(this);
            }
        }
    }
}
