using UnityEngine;
using UnityEngine.SceneManagement;

public class SpikeInteraction : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        print("collided");
        if (collision.gameObject.CompareTag("Spike"))
        {
            print("Restarting scene");
            SceneManager.LoadScene("Game");
        }
    }
}
