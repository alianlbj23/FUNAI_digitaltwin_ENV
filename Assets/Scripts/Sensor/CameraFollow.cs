using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  

    Vector3 positionBias = new Vector3(0f, 0f, -0.030f);
    Quaternion rotationBias = Quaternion.Euler(0f, 0f, 0f);

    void Update()
    {
        if (target != null)
        {
            // Achieving Smooth Tracking Using Interpolation
            Vector3 newPosition = target.position + positionBias;
            newPosition.x = Mathf.Round(newPosition.x * 100) / 100f;
            newPosition.y = Mathf.Round(newPosition.y * 100) / 100f;
            newPosition.z = Mathf.Round(newPosition.z * 100) / 100f;

            // Setting camera's position
            transform.position = newPosition;

            // Achieving Smooth Rotation Tracking Using Interpolation
            Quaternion newRotation = target.rotation * rotationBias;

            // Setting camera's rotation
            transform.rotation = newRotation;
        }
    }
}
