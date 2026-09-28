using UnityEngine;
using System.Collections.Generic;

public class PlayerVisibility : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Fade Settings")]
    [SerializeField, Range(0f, 1f)] private float fadeAlpha = 0.25f;
    [SerializeField] private float fadeSpeed = 5f;

    private Dictionary<Renderer, float> fadedObjects =
        new Dictionary<Renderer, float>();

    private void Update()
    {
        FindObstacles();
        UpdateFades();
    }

    private void FindObstacles()
    {
        if (cameraTransform == null)
            return;

        Vector3 direction = transform.position - cameraTransform.position;
        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(
            cameraTransform.position,
            direction.normalized,
            distance
        );

        HashSet<Renderer> currentObstacles = new HashSet<Renderer>();

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();

            if (renderer != null && renderer.gameObject != gameObject)
            {
                currentObstacles.Add(renderer);

                if (!fadedObjects.ContainsKey(renderer))
                {
                    fadedObjects.Add(renderer, 1f);
                }
            }
        }

        // Fade objects that are no longer blocking the player back in
        List<Renderer> objectsToRemove = new List<Renderer>();

        foreach (Renderer renderer in fadedObjects.Keys)
        {
            if (!currentObstacles.Contains(renderer))
            {
                objectsToRemove.Add(renderer);
            }
        }

        foreach (Renderer renderer in objectsToRemove)
        {
            fadedObjects.Remove(renderer);
        }
    }

    private void UpdateFades()
    {
        List<Renderer> invalidObjects = new List<Renderer>();

        foreach (var pair in fadedObjects)
        {
            Renderer renderer = pair.Key;

            if (renderer == null)
            {
                invalidObjects.Add(renderer);
                continue;
            }

            float currentAlpha = pair.Value;

            // Fade out
            currentAlpha = Mathf.Lerp(
                currentAlpha,
                fadeAlpha,
                Time.deltaTime * fadeSpeed
            );

            fadedObjects[renderer] = currentAlpha;

            foreach (Material material in renderer.materials)
            {
                Color color = material.color;
                color.a = currentAlpha;
                material.color = color;
            }
        }

        foreach (Renderer renderer in invalidObjects)
        {
            fadedObjects.Remove(renderer);
        }
    }

    private void OnDisable()
    {
        // Restore objects when player is disabled
        foreach (Renderer renderer in fadedObjects.Keys)
        {
            if (renderer == null)
                continue;

            foreach (Material material in renderer.materials)
            {
                Color color = material.color;
                color.a = 1f;
                material.color = color;
            }
        }

        fadedObjects.Clear();
    }
}