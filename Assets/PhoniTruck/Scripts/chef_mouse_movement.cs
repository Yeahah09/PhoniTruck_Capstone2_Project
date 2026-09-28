using UnityEngine;
using UnityEngine.InputSystem;

public class chef_mouse_movement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private LayerMask walkableLayer;

    private Vector3 targetPos;
    private bool hasTarget = false;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, walkableLayer))
            {
                targetPos = hit.point;
                hasTarget = true;
            }
        }

        if (hasTarget)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                hasTarget = false;
            }
        }
    }
}
