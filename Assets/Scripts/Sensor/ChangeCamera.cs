using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public Camera godCamera;
    public Camera playerCamera;
    public void useGodCamera()
    {
        playerCamera.depth = godCamera.depth - 1;
    }

    public void usePlayerCamera()
    {
        playerCamera.depth = godCamera.depth + 1;
    }
}
