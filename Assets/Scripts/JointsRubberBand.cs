using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class JointsRubberBand : MonoBehaviour
{
    [SerializeField] private GameObject chainLinkPrefab;
    [SerializeField] private int numLinks;
    [SerializeField] [SerializeAs("Max Distance Between Links")] private float maxDist;
    private GameObject[] links;
    private SpringJoint[] springs;
    private SpringJoint[] constraints;
    
    void Start()
    {
        links = new GameObject[numLinks];
        springs = new SpringJoint[numLinks];
        constraints = new SpringJoint[numLinks];

        //Instantiate and set positions
        for (int i = 0; i < links.Length; i++)
        {
            GameObject link = Instantiate(chainLinkPrefab, transform);
            links[i] = link;
            
            float angle = (float)Math.PI * 2 / links.Length * i;
            Vector3 position = Vector3.right * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
            link.transform.position = position;

            SpringJoint[] joints = link.GetComponents<SpringJoint>();
            springs[i] = joints[0];
            constraints[i] = joints[1];
        }
        
        //Set connections
        // joints[0][0].connectedBody = links[1].GetComponent<Rigidbody>();
        // joints[0][1].connectedBody = links[numLinks - 1].GetComponent<Rigidbody>();
        for (int i = 0; i < numLinks; i++)
        {
            int next = IncrementWrap(i, 1, numLinks);
            // int prev = IncrementWrap(i, -1, numLinks);
            springs[i].connectedBody = links[next].GetComponent<Rigidbody>();
            constraints[i].connectedBody = links[next].GetComponent<Rigidbody>();

            constraints[i].maxDistance = maxDist;
        }
        // joints[numLinks - 1][0].connectedBody = links[0].GetComponent<Rigidbody>();
        // joints[numLinks - 1][1].connectedBody = links[numLinks - 2].GetComponent<Rigidbody>();
    }

    // private void Update()
    // {
    //     //keep the links within maxDist of eachother
    //     //if they are too far away, translate one of them back
    //     
    //     for (int i = 0; i < numLinks; i++)
    //     {
    //         int nextInd = IncrementWrap(i, 1, numLinks);
    //         
    //         Transform current = links[i].transform;
    //         Transform next = links[nextInd].transform;
    //         Vector3 currentPos = current.position;
    //         Vector3 nextPos = next.position;
    //         
    //         float dist = Vector3.Distance(currentPos, nextPos);
    //
    //         if (dist > maxDist)
    //         {
    //             //the order here is crucial, the other way around won't work
    //             Vector3 targetPosition = Vector3.MoveTowards(nextPos,currentPos,float.PositiveInfinity);
    //             // next.position = targetPosition;
    //         }
    //     }
    // }

    private int IncrementWrap(int number, int increment, int maxExclusive)
    {
        //given a number and an increment (positive or negative) 
        //return number+increment wrapped within [0,max]
        number += increment;
        if (number < 0)
        {
            number += maxExclusive * (-number / maxExclusive + 1);
        }
        return number % maxExclusive;
    }
}
