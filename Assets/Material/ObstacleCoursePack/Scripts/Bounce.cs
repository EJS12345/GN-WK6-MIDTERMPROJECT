using UnityEngine;

public class Bounce : MonoBehaviour
{
    public float Force = 5f;
    public float StunTime = 0.3f;

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Find the Rigidbody of whatever touched the bounce pad
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        // 2. CRITICAL SAFETY CHECK: Only try to bounce objects that actually have a Rigidbody
        if (rb != null)
        {
            // Push the player straight up, multiplied by your Force variable
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset fall speed
            rb.AddForce(Vector3.up * Force, ForceMode.Impulse);
        }
    }
}