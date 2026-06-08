using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        FollowRoadByRaycast player = other.GetComponent<FollowRoadByRaycast>();
        if (player != null)
        {
            Debug.Log("End Trigger Reached, ending session!");
            EndSequence.Instance.PlaySequence(player.transform);
        }
    }
}
