using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Unity.Robotics.UrdfImporter.Control;

public class robotControl : MonoBehaviour
{
    // Start is called before the first frame update

    string topicName_subscribe = "/unity2Ros"; 
    
    private WebSocket socket;
    private string rosbridgeServerUrl = "ws://localhost:9090";

    public ArticulationBody base_link;
    public ArticulationBody link1;
    public ArticulationBody link2;
    public ArticulationBody link3;

    public ArticulationBody grap_left_1;
    public ArticulationBody grap_left_2;

    public ArticulationBody grap_right_1;
    public ArticulationBody grap_right_2;
    float grapAngle=0;
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
    void Start()
    {        
        socket = new WebSocket(rosbridgeServerUrl);
        socket.OnOpen += (sender, e) =>
        {
            SubscribeToTopic(topicName_subscribe);
        };
        socket.OnMessage += OnWebSocketMessage;
        socket.Connect();
        
    }
    private void SubscribeToTopic(string topic)
    {
        string subscribeMessage = "{\"op\":\"subscribe\",\"id\":\"1\",\"topic\":\"" + topic + "\",\"type\":\"std_msgs/msg/Float32MultiArray\"}";
        socket.Send(subscribeMessage);
    }
    // float[] data;
    float[] data = { 270f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

    private void OnWebSocketMessage(object sender, MessageEventArgs e){
        string jsonString = e.Data;
        RobotNewsMessage message = JsonUtility.FromJson<RobotNewsMessage>(jsonString);
        data = message.msg.data;
    }

    
    

    // Update is called once per frame
    void Update()
    {
        base_link.anchorRotation = Quaternion.Euler(0, 0, data[0]); 
        Debug.Log(data[0]);
        // link1.transform.rotation = Quaternion.Euler(0, 0, data[1]); 
        // link2.transform.rotation = Quaternion.Euler(0, 0, data[2]); 
        // link3.transform.rotation= Quaternion.Euler(0, data[3], 0); 
        // grapAngle = data[4];
        // grap(grapAngle, -grapAngle);
    }

    // void grap(float left, float right){
    //     grap_left_1.transform.rotation = Quaternion.Euler(0, 0, left);
    //     grap_left_2.transform.rotation = Quaternion.Euler(0, 0, left);

    //     grap_right_1.transform.rotation = Quaternion.Euler(0, 0, right);
    //     grap_right_2.transform.rotation = Quaternion.Euler(0, 0, right);
    // }

    
}
