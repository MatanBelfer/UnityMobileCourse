using System.Linq;
using UnityEngine;

public class BandAnimation : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
    }
    
    public void Vibrate()
    {
        //set the start time of the animation shader to the current time
        block.SetFloat("_StartTime", Time.time);
        meshRenderer.SetPropertyBlock(block);
        //test - get the amplitude
        block.GetFloat("_Amplitude");
        meshRenderer.GetPropertyBlock(block);
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Vibrate();
    }    
}
