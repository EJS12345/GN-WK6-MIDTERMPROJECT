using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidewaysRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float speed = 3f;

    // Update is called once per frame
    void Update()
    {
        // Rotates around the Y-axis (flat/horizontal sweeping motion)
        transform.Rotate(0f, speed * Time.deltaTime / 0.01f, 0f, Space.Self);
    }
}