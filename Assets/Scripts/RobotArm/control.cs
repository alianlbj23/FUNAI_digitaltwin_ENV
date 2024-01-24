using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    // Start is called before the first frame update
    public ArticulationBody articulationBody;
    public float targetAngularVelocity = 5000f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Space))
        {
            SetTargetAngularVelocity(targetAngularVelocity);
            Debug.Log("yes");
        }
    }

        void SetTargetAngularVelocity(float targetVelocityDegreesPerSecond)
    {
        // 將角速度轉換為弧度/秒
        float targetVelocityRadiansPerSecond = Mathf.Deg2Rad * targetVelocityDegreesPerSecond;

        // 設定 ArticulationDrive.targetVelocity
        ArticulationDrive drive = articulationBody.xDrive;
        drive.targetVelocity = targetVelocityRadiansPerSecond;
        articulationBody.xDrive = drive;
    }
}
