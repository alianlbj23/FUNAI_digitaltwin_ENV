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
    string carInputTopic = "/test";
    string carOutputTopic = "/wheel_speed";
    string outputTopic = "/arm_angle";

    public float[] jointPositions;
    private float[] data = new float[6];
    private float[] carWheelData = new float[4];
    
    public bool manual;
    void Start()
    {
        connectRos.ws.OnMessage += OnWebSocketMessage;
        SubscribeToTopic(inputTopic, "joint_trajectory_point");
        SubscribeToTopic(carInputTopic, "string");
    }

    void Update()
    {
        CheckMode();
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        string jsonString = e.Data;
        var genericMessage = JsonUtility.FromJson<GenericRosMessage>(jsonString);
        if (genericMessage.topic == inputTopic)
        {
            if(manual){
                RobotNewsMessageJointTrajectory message = JsonUtility.FromJson<RobotNewsMessageJointTrajectory>(jsonString);
                HandleJointTrajectoryMessage(message);
            }
            
        }
        else if(genericMessage.topic == carInputTopic)
        {
            RobotNewsMessageString message = JsonUtility.FromJson<RobotNewsMessageString>(jsonString);
            HandleStringMessage(message);
        }
    }
    private void HandleJointTrajectoryMessage(RobotNewsMessageJointTrajectory message)
    {
        float modifyAngle = 45.0f;
        jointPositions = message.msg.positions;
        data[0] = jointPositions[4]-modifyAngle;
        data[1] = jointPositions[4]-modifyAngle;
        data[2] = jointPositions[3]-modifyAngle;
        data[3] = jointPositions[2]-modifyAngle;
        data[4] = jointPositions[1]-modifyAngle;
        data[5] = jointPositions[0]-modifyAngle;
        PublishFloat32MultiArray(outputTopic, data);
        // Debug.Log("Received positions: " + String.Join(", ", jointPositions));
    }

    private void HandleStringMessage(RobotNewsMessageString message)
    {
        var jsonData = JObject.Parse(message.msg.data);
        var targetVel = jsonData["data"]["target_vel"];
        float speedRate = 0.1f;
        float speed = Mathf.Abs(targetVel[0].ToObject<float>()*speedRate)*360.0f;
        float rotateSpeed = speed+speed*10.0f;
        // json轉換成float
        float targetVelLeft = targetVel[0].ToObject<float>();
        float targetVelRight = targetVel[1].ToObject<float>();
        if(targetVelLeft == targetVelRight && targetVelRight > 0)
        {
            carWheelData[0] = speed;
            carWheelData[1] = speed;
            carWheelData[2] = speed;
            carWheelData[3] = speed;
        }
        else if(targetVelLeft > targetVelRight)
        {
            carWheelData[0] = rotateSpeed;
            carWheelData[1] = -rotateSpeed;
            carWheelData[2] = rotateSpeed;
            carWheelData[3] = -rotateSpeed;   
        }
        else if(targetVelLeft < targetVelRight)
        {
            carWheelData[0] = -rotateSpeed;
            carWheelData[1] = rotateSpeed;
            carWheelData[2] = -rotateSpeed;
            carWheelData[3] = rotateSpeed;   
        }
        else if(targetVelLeft == targetVelRight && targetVelRight < 0)
        {
            carWheelData[0] = -speed;
            carWheelData[1] = -speed;
            carWheelData[2] = -speed;
            carWheelData[3] = -speed;   
        }
        else
        {
            carWheelData[0] = 0.0f;
            carWheelData[1] = 0.0f;
            carWheelData[2] = 0.0f;
            carWheelData[3] = 0.0f; 
        }
        PublishFloat32MultiArray(carOutputTopic, carWheelData);
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
