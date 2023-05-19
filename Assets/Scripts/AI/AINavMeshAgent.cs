using UnityEngine;
using UnityEngine.AI;

public class AINavMeshAgent : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;

    private void Awake()
    {
        // Get the reference to the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetDestinationToPoint(Vector3 point)
    {
        // Set the target position for the NavMeshAgent
        agent.SetDestination(point);
    }
}