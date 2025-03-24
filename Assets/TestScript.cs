using UnityEngine;

public class TestScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Bucket"))
        {
            Debug.Log("Player can pick up the bucket!");
        }
    }
}