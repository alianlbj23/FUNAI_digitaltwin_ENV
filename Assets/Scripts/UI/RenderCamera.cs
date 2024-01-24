using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderCamera : MonoBehaviour
{
    [SerializeField] InferenceController inferenceController;
    [SerializeField] Camera carCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RenderCameraImage();
    }

    void RenderCameraImage()
    {
        // Create a RenderTexture to store the rendering result
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        carCamera.targetTexture = renderTexture;
        carCamera.Render();
        // Set it back to null so that the camera can continue rendering to the screen
        carCamera.targetTexture = null;
        inferenceController.UpdateCameraTexture(renderTexture);
        Object.Destroy(renderTexture);
    }
}
