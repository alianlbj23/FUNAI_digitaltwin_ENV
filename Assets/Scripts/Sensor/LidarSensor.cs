using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class LidarSensor : MonoBehaviour
{   
    public float minRange = 0.10f;
    public float maxRange = 100;
    public int numMeasurementsPerScan = 180;
    public int lineNum = 36;
    public LineRenderer line;
    public LidarToROS lidarToRos;

    float scanAngleStartDegrees = 0; 
    float scanAngleEndDegrees = -359; 


    List<float> ranges = new List<float>();
    List<float> range_tmp;
    
    int m_NumMeasurementsTaken;
    int lineInterval;
    bool isScanning = false;

    List<Vector3> directionVectors = new List<Vector3>();
    List<Vector3> directionVectors_tmp;
    void Start()
    {
        line.positionCount = lineNum * 2;
        lineInterval = numMeasurementsPerScan / lineNum;
    }

    void BeginScan()
    {
        isScanning = true;
        m_NumMeasurementsTaken = 0;
    }

    public void EndScan()
    {
        if (ranges.Count == 0)
        {
            Debug.LogWarning($"Took {m_NumMeasurementsTaken} measurements but found no valid ranges");
        }
        else if (ranges.Count != m_NumMeasurementsTaken || ranges.Count != numMeasurementsPerScan)
        {
            Debug.LogWarning($"Expected {numMeasurementsPerScan} measurements. Actually took {m_NumMeasurementsTaken}" +
                             $"and recorded {ranges.Count} ranges.");
        }
        range_tmp = new List<float>(ranges);
        directionVectors_tmp = new List<Vector3>(directionVectors);

        // Publish lidar data to ROS //先註解掉，不然會占用頻道空間
        // lidarToRos.PublishLidar(range_tmp);

        m_NumMeasurementsTaken = 0;
        ranges.Clear();
        directionVectors.Clear();
        isScanning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isScanning)
        {
            BeginScan();
        }
        var yawBaseDegrees = transform.rotation.eulerAngles.y;

        while (m_NumMeasurementsTaken < numMeasurementsPerScan)
        {
            var t = m_NumMeasurementsTaken / (float)numMeasurementsPerScan;
            var yawSensorDegrees = Mathf.Lerp(scanAngleStartDegrees, scanAngleEndDegrees, t);
            // rotate lidar
            var yawDegrees = yawBaseDegrees + yawSensorDegrees;
            // scanning direction
            var directionVector = Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward;
            // ray scan in z axis
            var measurementStart = minRange * directionVector + transform.position;
            // Simulate laser light from the starting point in a specific direction
            Ray measurementRay = new Ray(measurementStart, directionVector);
            RaycastHit hit;

            // Returns whether an object was detected
            var foundValidMeasurement = Physics.Raycast(measurementRay, out hit, maxRange);

            // Only record measurement if it's within the sensor's operating range
            if (foundValidMeasurement)
            {
                ranges.Add(hit.distance);
                if (m_NumMeasurementsTaken % lineInterval == 0 && (m_NumMeasurementsTaken / lineInterval) < line.positionCount)
                    DrawLine(measurementRay, (m_NumMeasurementsTaken / lineInterval), hit);
            }
            else
            {
                ranges.Add(100.0f);
            }
                
            // Even if Raycast didn't find a valid hit, we still count it as a measurement
            m_NumMeasurementsTaken++;
            directionVectors.Add(directionVector);
        }

        if (m_NumMeasurementsTaken >= numMeasurementsPerScan)
        {
            if (m_NumMeasurementsTaken > numMeasurementsPerScan)
            {
                Debug.LogError($"LaserScan has {m_NumMeasurementsTaken} measurements but we expected {numMeasurementsPerScan}");
            }
            EndScan();
        }
        
    }

    public List<float> GetRange() 
    {   
        return range_tmp;
    }

    public List<Vector3> GetRangeDirection() 
    {     
        return directionVectors_tmp;
    }

    public int GetRangeSize() 
    {  
        return m_NumMeasurementsTaken;
    }

    public void lidarOn()
    {
        line.positionCount = lineNum * 2;
    }

    public void lidarOff()
    {
        line.positionCount = 0;
    }

    private void DrawLine(Ray ray, int index, RaycastHit hit)
    {
        line.SetPosition(index * 2, ray.origin);
        Vector3 rayEndPoint = hit.point;
        line.SetPosition(index * 2 + 1, rayEndPoint);
    }
}
