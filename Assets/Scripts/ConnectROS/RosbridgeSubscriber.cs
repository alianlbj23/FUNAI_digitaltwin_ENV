using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter.Control;
using UnityEngine;
using WebSocketSharp;

public class RosbridgeSubscriber : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    public string topicName = "/wheel_speed"; 
    
    public float wA1Value = 0.0f;
    public float wA2Value = 0.0f;
    public float wA3Value = 0.0f;
    public float wA4Value = 0.0f;

    public GameObject wheel1;
    public GameObject wheel2;
    public GameObject wheel3;
    public GameObject wheel4;

    private ArticulationBody wA1;
    private ArticulationBody wA2;
    private ArticulationBody wA3;
    private ArticulationBody wA4;

    public float maxLinearSpeed = 2; //  m/s
    public float maxRotationalSpeed = 1;
    public float wheelRadius = 0.07f; //meters
    public float trackWidth = 0.288f; // meters Distance between tyres
    public float forceLimit = 100;
    public float damping = 10;
    private RotationDirection direction;
    void Start()
    {
        connectRos.ws.OnMessage += OnWebSocketMessage;

        SubscribeToTopic(topicName);

        wA1 = wheel1.GetComponent<ArticulationBody>();
        wA2 = wheel2.GetComponent<ArticulationBody>();
        wA3 = wheel3.GetComponent<ArticulationBody>();
        wA4 = wheel4.GetComponent<ArticulationBody>();
        SetParameters(wA1);
        SetParameters(wA2);
        SetParameters(wA3);
        SetParameters(wA4);
    }

    void Update()
    {
        SetSpeed(wA1, wA1Value);
        SetSpeed(wA2, wA2Value);
        SetSpeed(wA3, wA3Value);
        SetSpeed(wA4, wA4Value);
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        string jsonString = e.Data;
        RobotNewsMessage message = JsonUtility.FromJson<RobotNewsMessage>(jsonString);
        if (message.topic == topicName)
        {
            float[] data = message.msg.data;
            wA1Value = data[0];
            wA2Value = data[1];
            wA3Value = data[2];
            wA4Value = data[3];
        }
        
    }

    private void SubscribeToTopic(string topic)
    {
        string subscribeMessage = "{\"op\":\"subscribe\",\"id\":\"1\",\"topic\":\"" + topic + "\",\"type\":\"std_msgs/msg/Float32MultiArray\"}";
        connectRos.ws.Send(subscribeMessage);
    }
    private void SetParameters(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.forceLimit = forceLimit;
        drive.damping = damping;
        joint.xDrive = drive;
    }

    private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
    {
        ArticulationDrive drive = joint.xDrive;
        if (float.IsNaN(wheelSpeed))
        {
            drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
        }
        else
        {
            drive.targetVelocity = wheelSpeed;
        }
        drive.targetVelocity = wheelSpeed;
        joint.xDrive = drive;
    }


    [System.Serializable]
    public class RobotNewsMessage
    {
        public string op;
        public string topic;
        public MessageData msg;
    }

    [System.Serializable]
    public class MessageData
    {
        public LayoutData layout;
        public float[] data;
    }

    [System.Serializable]
    public class LayoutData
    {
        public int[] dim;
        public int data_offset;
    }

}


