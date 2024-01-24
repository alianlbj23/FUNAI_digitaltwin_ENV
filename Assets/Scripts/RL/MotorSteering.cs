using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorSteering : MonoBehaviour
{
    ArticulationBody bd;
    const float RAD2DEG = 57.295779513f;
    // float smooth = 1.0f;
    float speed = 500;
    float stiffness = 1000f;
    float damping = 0f;
    float forceLimit = 100;

    void Awake()
    {
        bd = GetComponent<ArticulationBody>();
    }
    // Update is called once per frame
    void Update()
    {
        ArticulationDrive currentDrive = bd.xDrive;
        currentDrive.stiffness = stiffness;
        currentDrive.damping = damping;
        currentDrive.forceLimit = forceLimit;
        bd.xDrive = currentDrive;
    }
}
