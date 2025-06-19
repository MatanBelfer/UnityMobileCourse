using System;
using UnityEngine;

[ExecuteAlways]
public class MyVerticalLayout : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform[] children;
    private int numChidren;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnTransformChildrenChanged()
    {
        children = GetComponentsInChildren<RectTransform>();
        numChidren = children.Length;
    }

    private void Update()
    {
        for (int i = 0; i < numChidren; i++)
        {
            var child = children[i];
            Vector2 prevMax = child.anchorMax;
            Vector2 prevMin = child.anchorMin;
            float min = (float)i / numChidren;
            float max = (float)(i + 1) / numChidren;
            child.anchorMin = new Vector2(prevMin.x, min);
            child.anchorMax = new Vector2(prevMax.x, max);
        }
    }
}
