using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 7.0f;
    public float mouseSensitivity = 3.0f;
    public float heightOffset = 1.0f;

    private float pitch = 15f;
    private float yaw = 0f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

    }

    void LateUpdate()
    {
        if (target == null) return;

        // rotate camera if the game is actively being played (time isn't frozen)
        if (Time.timeScale > 0f)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -15f, 60f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 lookPosition = target.position + (Vector3.up * heightOffset);
        Vector3 cameraPosition = lookPosition + rotation * new Vector3(0, 0, -distance);

        transform.position = cameraPosition;
        transform.LookAt(lookPosition);
    }
}