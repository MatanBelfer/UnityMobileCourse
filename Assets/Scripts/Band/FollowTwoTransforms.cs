using System;
using UnityEngine;

public class FollowTwoTransforms : MonoBehaviour
{
    //Stretch, move and rotate a transform to fit two target transforms

    [SerializeField] public Transform target1;
    [SerializeField] public Transform target2;
    [SerializeField] public bool follow;

    private void Update()
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