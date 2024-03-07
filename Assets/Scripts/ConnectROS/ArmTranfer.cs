using UnityEngine;
using WebSocketSharp;
using System;
using Newtonsoft.Json.Linq;
using TMPro;
public class ArmTransfer : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    // inputTopic & outputTopic設成publish會有很多問題，會造成這邊改了名稱，但外面沒有導致難以找到bug
    string inputTopic = "/joint_trajectory_point";
    string carFrontInputTopic = "/test";
    string carRearInputTopic = "/car_C_control";
    string carOutputTopic = "/wheel_speed";
    string outputTopic = "/arm_angle";
    public MotorSpeedCalculator motorSpeedCalculator;
    private bool isFrontWheelDataReceived = false;
    private bool isRearWheelDataReceived = false;
    private float[] frontWheelData = new float[2];
    private float[] rearWheelData = new float[2];

    public float[] jointPositions;
    private float[] data = new float[6];
    private float[] carWheelData = new float[4];
    public float speedRate = 0.1f;
    bool manual;
    float modifyAngle = 60.0f;
    public float axis1CorrectionFactor = 90.0f;
    public float axis2CorrectionFactor = 60.0f;
    public float axis3CorrectionFactor = 60.0f;
    public float axis4CorrectionFactor = 60.0f;
    public float axis5CorrectionFactor = 60.0f;

    void Start()
    {
        connectRos.ws.OnMessage += OnWebSocketMessage;
        SubscribeToTopic(inputTopic, "joint_trajectory_point");
        SubscribeToTopic(carFrontInputTopic, "string");
        SubscribeToTopic(carRearInputTopic, "string");
    }

    void Update()
    {
        CheckMode();
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        string jsonString = e.Data;
        string carWheelType = "";
        var genericMessage = JsonUtility.FromJson<GenericRosMessage>(jsonString);
        if (genericMessage.topic == inputTopic)
        {
            if(manual){
                RobotNewsMessageJointTrajectory message = JsonUtility.FromJson<RobotNewsMessageJointTrajectory>(jsonString);
                HandleJointTrajectoryMessage(message);
            }

        }
        else if(genericMessage.topic == carFrontInputTopic)
        {
            carWheelType = "Front";
            RobotNewsMessageString message = JsonUtility.FromJson<RobotNewsMessageString>(jsonString);
            HandleStringMessage(message, carWheelType);
        }
        else if(genericMessage.topic == carRearInputTopic)
        {
            carWheelType = "Rear";
            RobotNewsMessageString message = JsonUtility.FromJson<RobotNewsMessageString>(jsonString);
            HandleStringMessage(message, carWheelType);
        }
    }
    private void HandleJointTrajectoryMessage(RobotNewsMessageJointTrajectory message)
    {
        jointPositions = message.msg.positions;
        for (int i = 0; i < jointPositions.Length; i++)
        {
            jointPositions[i] = jointPositions[i] * Mathf.Rad2Deg;
        }
        data[0] = 180.0f - jointPositions[4] - axis5CorrectionFactor;
        data[1] = 180.0f - jointPositions[4] - axis5CorrectionFactor;
        data[2] = jointPositions[3]-axis4CorrectionFactor;
        data[3] = jointPositions[2]-axis3CorrectionFactor;
        data[4] = jointPositions[1]-axis2CorrectionFactor;
        data[5] = jointPositions[0]-axis1CorrectionFactor;

        Debug.Log("Data array values: " + String.Join(", ", data));
        PublishFloat32MultiArray(outputTopic, data);

        // Debug.Log("Received positions: " + String.Join(", ", jointPositions));
    }

    private void HandleStringMessage(RobotNewsMessageString message, string carWheelType)
    {
        var jsonData = JObject.Parse(message.msg.data);
        var targetVel = jsonData["data"]["target_vel"];
        float carLeftSpeed = motorSpeedCalculator.CalculateSpeed(targetVel[0].ToObject<float>()) * speedRate;
        float carRightSpeed = motorSpeedCalculator.CalculateSpeed(targetVel[1].ToObject<float>()) * speedRate;
        // json轉換成float
        float targetVelLeft = targetVel[0].ToObject<float>();
        float targetVelRight = targetVel[1].ToObject<float>();

        if(carWheelType == "Front"){
            frontWheelData[0] = carLeftSpeed;
            frontWheelData[1] = carRightSpeed;
            isFrontWheelDataReceived = true;
        }

        else if (carWheelType == "Rear")
        {
            rearWheelData[0] = carLeftSpeed;
            rearWheelData[1] = carRightSpeed;
            isRearWheelDataReceived = true;
        }

        if (isFrontWheelDataReceived && isRearWheelDataReceived)
        {
            Debug.Log("test");
            ProcessAndPublishWheelData();
            // 重置狀態
            isFrontWheelDataReceived = false;
            isRearWheelDataReceived = false;
        }
    }

    private void ProcessAndPublishWheelData()
    {
        float[] finalWheelData = new float[4];
        finalWheelData[0] = frontWheelData[0];
        finalWheelData[1] = frontWheelData[1];
        finalWheelData[2] = rearWheelData[0];
        finalWheelData[3] = rearWheelData[1];
        PublishFloat32MultiArray(carOutputTopic, finalWheelData);
    }


    private void SubscribeToTopic(string topic, string type)
    {
        string subscribeMessage = "";
        string typeMsg = "";
        switch(type)
        {
            case "joint_trajectory_point":
                typeMsg = "trajectory_msgs/msg/JointTrajectoryPoint";
                subscribeMessage = "{\"op\":\"subscribe\",\"id\":\"1\",\"topic\":\"" + topic + "\",\"type\":\""+typeMsg+"\"}";
                break;
            case "string":
                typeMsg = "std_msgs/msg/String";
                subscribeMessage = "{\"op\":\"subscribe\",\"id\":\"1\",\"topic\":\"" + topic + "\",\"type\":\""+typeMsg+"\"}";
                break;
            default:
                break;
        }
        connectRos.ws.Send(subscribeMessage);
    }


    public void PublishFloat32MultiArray(string topic, float[] data)
    {
        string jsonMessage = $@"{{
            ""op"": ""publish"",
            ""topic"": ""{topic}"",
            ""msg"": {{
                ""layout"": {{
                    ""dim"": [{{""size"": {data.Length}, ""stride"": 1}}],
                    ""data_offset"": 0
                }},
                ""data"": [{string.Join(", ", data)}]
            }}
        }}";

        connectRos.ws.Send(jsonMessage);
    }
    //  分類topic用
    [System.Serializable]
    public class GenericRosMessage
    {
        public string op;
        public string topic;
    }
    // JointTrajectoryPoint格式
    [System.Serializable]
    public class RobotNewsMessageJointTrajectory
    {
        public string op;
        public string topic;
        public JointTrajectoryPointMessage msg;
    }

    // 收JointTrajectoryPoint
    [System.Serializable]
    public class JointTrajectoryPointMessage
    {
        public float[] positions;
        public float[] velocities;
        public float[] accelerations;
        public float[] effort;
        public TimeFromStart time_from_start;
    }
    [System.Serializable]
    public class TimeFromStart
    {
        public int sec;
        public int nanosec;
    }
    // 收string用
    [System.Serializable]
    public class RobotNewsMessageString
    {
        public string op;
        public string topic;
        public StringMessage msg;
    }
    [System.Serializable]
    public class StringMessage
    {
        public string data;
    }
    private void CheckMode()
    {
        GameObject tmpGameObject = GameObject.Find("== Canvas == /Canvas/Settings-Canvas/Car/Mode/Horizontal Selector/Main Content/Text");
        if (tmpGameObject != null)
        {
            TextMeshProUGUI tmpComponent = tmpGameObject.GetComponent<TextMeshProUGUI>();
            if (tmpComponent != null)
            {
                string textContent = tmpComponent.text;
                manual = (textContent == "AI");
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found on the object");
            }
        }
        else
        {
            Debug.LogError("GameObject not found in the scene");
        }
    }
}
