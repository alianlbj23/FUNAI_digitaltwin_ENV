using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorSpeedCalculator : MonoBehaviour
{
    public float maxVoltage = 12.0f; // 最大電壓
    public float maxRPM = 176.0f; // 最大RPM

    // 根據輸入電壓計算轉速
    public float CalculateSpeed(float voltage)
    {
        float rpmPerVolt = maxRPM / maxVoltage;
        return voltage * rpmPerVolt;
    }
}
