using UnityEngine;

public class CloudDrag : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        offset = transform.position - cam.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseDrag()
    {
        Vector3 newPos = cam.ScreenToWorldPoint(Input.mousePosition) + offset;
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }
}
