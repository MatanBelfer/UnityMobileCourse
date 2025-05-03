using System;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public bool isFollowing { get; private set; }
    [SerializeField] public GridManager grid;

    private void Update()
    {
        if (isFollowing)
        {
            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0f;
            transform.position = mousepos;

            if (Input.GetMouseButtonUp(0))
            {
                StopFollowing();
            }
        }
    }

    private void OnMouseDown()
    {
        if (!isFollowing)
        {
            StartFollowing();
        }
    }

    public void StartFollowing()
    {
        isFollowing = true;
        transform.parent = null;
    }

    public void StopFollowing()
    {
        //find the closest point on the frid and go to it
        isFollowing = false;
        Transform landingPoint = grid.GetClosestPoint(transform.position);
        transform.parent = landingPoint;
        transform.localPosition = Vector3.zero;
    }
}
