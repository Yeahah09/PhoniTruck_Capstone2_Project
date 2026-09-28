using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private Transform movePos;
    [SerializeField] private SpriteRenderer front;
    [SerializeField] private SpriteRenderer back;
    [SerializeField] private SpriteRenderer order;

    private NavMeshAgent agent;
    private bool hasStartedMoving = false;
    private bool hasArrived = false;

    private void Awake()
    {
        agent = GetComponentInChildren<NavMeshAgent>();

        back.enabled = false;
        order.enabled = false;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !hasArrived)
        {
            agent.destination = movePos.position;
            hasStartedMoving = true;
        }

        if (hasStartedMoving &&
            !hasArrived &&
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            hasArrived = true;

            front.enabled = false;
            back.enabled = true;
            order.enabled = true;
        }
    }
}
