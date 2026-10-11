using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 5, -8);
    public float followSpeed = 10f;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate the camera pos
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // THE FIX: Ensure camera always faces directly at the player (the target), not itself
        transform.LookAt(target.position);
    }
}