using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;

    private CinemachineCamera cam;

    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
    }

    void Update()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            float newZoom = cam.Lens.OrthographicSize;

            newZoom -= scroll * zoomSpeed * Time.deltaTime;

            cam.Lens.OrthographicSize = Mathf.Clamp(
                newZoom,
                minZoom,
                maxZoom
            );
        }
    }
}
