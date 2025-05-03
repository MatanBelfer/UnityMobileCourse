using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RubberBand : MonoBehaviour
{
    public Transform[] anchors; // Four fixed points
    public Transform draggable; // Player-dragged endpoint
    public float springStrength = 50f; // Force pulling back
    public float maxStretchDistance = 5f;

    private LineRenderer line;
    private Vector3 originalPosition;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        originalPosition = draggable.position;
        line.positionCount = anchors.Length + 1; // 4 anchors + draggable end
    }

    void Update()
    {
        HandleDragging();
        ApplySpringPhysics();
        UpdateLine();
    }

    void HandleDragging()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Limit max stretch
            Vector3 dir = worldPos - originalPosition;
            if (dir.magnitude > maxStretchDistance)
                dir = dir.normalized * maxStretchDistance;

            draggable.position = originalPosition + dir;
        }
    }

    void ApplySpringPhysics()
    {
        if (!Input.GetMouseButton(0))
        {
            Vector3 toOrigin = originalPosition - draggable.position;
            draggable.position += toOrigin * springStrength * Time.deltaTime;
        }
    }

    void UpdateLine()
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            line.SetPosition(i, anchors[i].position);
        }

        line.SetPosition(anchors.Length, draggable.position);
    }
}

