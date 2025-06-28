using System;
using UnityEngine;

public class FollowTwoTransforms : MonoBehaviour
{
    //Stretch, move and rotate a transform to fit two target transforms

    [SerializeField] private Transform _target1;
    public Transform target1
    {
        get => _target1; 
        set
        {
            _target1 = value;
            UpdatePosition();
        }
    }

    [SerializeField] private Transform _target2;
    public Transform target2
    {
        get => _target2;
        set
        {
            _target2 = value;
            UpdatePosition();
        }
    }
    [SerializeField] public bool follow;
    public event Action OnMove;

    private void Update()
    {
        UpdatePosition();
        OnMove?.Invoke();
    }

    private void UpdatePosition()
    {
        if (target1 == null || target2 == null) return;
        if (!follow) return;

        transform.position = (target1.position + target2.position) / 2;
        transform.rotation = Quaternion.LookRotation(target1.position - target2.position, Vector3.back);
        Vector3 scale = transform.localScale;
        scale.z = Vector3.Distance(target1.position, target2.position);
        transform.localScale = scale;
    }
}