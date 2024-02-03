//  會確認目前是AI才會送資料，不然一律跳過送資料給車體的步驟
//  state回傳不管是不是在AI mod, 都會一直回傳資料給python
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using Math = System.Math;
using System.Reflection;
using WebSocketSharp;
using MiniJSON;
using UnityEngine.SceneManagement;
using TMPro;

public class TrainingManager : MonoBehaviour
{
    string Unity2AI_Topic = "/Unity_2_AI";
    string AI2Unity_Receive_Topic = "/AI_2_Unity";
    string AI2Unity_reset_Topic = "/AI_2_Unity_RESET_flag";
    public string topicName = "/wheel_speed";
    string topicName2 = "/Unity_2_AI_stop_flag";
    public ConnectRosBridge connectRos;
    public float speedRate = 0.5f;
    private WebSocket socket;
    public string rosbridgeServerUrl = "ws://192.168.68.110:9090";
    public Robot robot;
    [SerializeField]
    GameObject anchor1, anchor2, anchor3, anchor4;
    Vector3[] outerPolygonVertices;
    [SerializeField]
    GameObject target;
    Vector3 newTarget_car;
    public float maxVoltage = 12.0f; // 最大電壓
    public float maxRPM = 330.0f; // 最大RPM
    Vector3 newTarget;
    public System.Random random = new System.Random();
    private float[] wheel_data = new float[4];
    Transform baselink;
    // Transform baselink;
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
    string mode;
    public ModeManager modeManager;
    float target_change_flag = 0;
    bool manual; //  偵測目前使否為AI模式
    void Awake()
    {
        baselink = robot.transform.Find("base_link");
    }

    Vector3 carPos;
    
    float key = 0;
    float delayInSeconds = 0f;
    void Start()
    {
        StartCoroutine(DelayedExecution());
    }

    IEnumerator DelayedExecution()
    {
        mode = modeManager.GetMode();
        delayInSeconds = 0.001f;
        yield return new WaitForSeconds(delayInSeconds);
        
        
        newTarget = GetTargetPosition(target, newTarget);

        socket = new WebSocket(rosbridgeServerUrl);
        

        socket.OnOpen += (sender, e) =>
        {
            SubscribeToTopic(AI2Unity_Receive_Topic);
            SubscribeToTopic(AI2Unity_reset_Topic);
        };
        socket.OnMessage += OnWebSocketMessage;

        socket.Connect();

        // change_target();
        get_position_test();
    }

    float target_x;
    float target_y;
    float target_x_car;
    float target_y_car;

    
    void FixedUpdate ()
    {
        CheckMode(); // 確認目前模式
        if (mode == "inference")
        {
            if (key == 1)
            {
                key = 0;
                StartCoroutine(DelayedSend(0.2f, newTarget));                
            }

            if (target_change_flag == 1)
            {
                ReloadCurrentScene();
            } 
        }
        else if (mode == "data")
        {
            CarMove();
        }
        else
        {
            Debug.Log("please set the correct mode.");
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
    }

    void CarMove()
    {
        float[] data = new float[] {0.0f, 0.0f, 0.0f, 0.0f};
        float speed = 5.0f;
        if (Input.GetKey(KeyCode.W))
        {
            data[0] = speed;
            data[1] = speed;
            data[2] = speed;
            data[3] = speed;
            ExecuteRobotAction(data, robot);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            data[0] = speed;
            data[1] = -speed;
            data[2] = speed;
            data[3] = -speed;
            ExecuteRobotAction(data, robot);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            data[0] = -speed;
            data[1] = speed;
            data[2] = -speed;
            data[3] = speed;
            ExecuteRobotAction(data, robot);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            data[0] = -speed;
            data[1] = -speed;
            data[2] = -speed;
            data[3] = -speed;
            ExecuteRobotAction(data, robot);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            exitUnityAndStoreData();
        }
        State state = robot.GetState(newTarget);
        Send(state);
    }
    void exitUnityAndStoreData()
    {
        Dictionary<string, object> message1 = new Dictionary<string, object>
        {
            { "op", "publish" },
            { "id", "1" },
            { "topic", topicName2 },
            { "msg", new Dictionary<string, string>
                {
                    { "data", "0"}
                }
            }
        };
        string jsonMessage1 = MiniJSON.Json.Serialize(message1);
        try
        {
            socket.Send(jsonMessage1);
        }
        catch
        {
            Debug.Log("error-send");
        }
        UnityEditor.EditorApplication.isPlaying = false;
    }
    void WheelSpeed(float leftWheel, float rightWheel)
    {
        Robot.Action action = new Robot.Action();
        action.voltage = new List<float>();

        action.voltage.Add(leftWheel);
        action.voltage.Add(rightWheel);
        robot.DoAction(action);

        State state = robot.GetState(newTarget);
        Send(state);
    }
    IEnumerator DelayedSend(float delayTime, Vector3 newTarget)
    {
        yield return new WaitForSeconds(delayTime);
        State state = robot.GetState(newTarget);
        Send(state);
        key = 0;
    }

    void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    
    void MoveGameObject(GameObject obj, Vector3 pos)
    {
        obj.transform.position = pos;
    }

    void MoveRobot(Vector3 pos)
    {
        baselink.GetComponent<ArticulationBody>().TeleportRoot(pos, Quaternion.identity);
    }


    Vector3 GetTargetPosition(GameObject obj, Vector3 pos)  //  取得target position
    {
        Transform objTransform = obj.transform;
        pos = objTransform.position;
        return pos;
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        string jsonString = e.Data;
        RobotNewsMessage message = JsonUtility.FromJson<RobotNewsMessage>(jsonString);
        // float[] data = message.msg.data;
        
        switch (message.topic)
        {
            case "/AI_2_Unity":
                HandleAI2UnityReceiveTopic(message);
                break;
            case "/AI_2_Unity_RESET_flag":
                HandleAI2UnityResetTopic(message);
                break;
            default:
                Debug.Log("Received message on unknown topic: " + message.topic);
                break;
        }   
    }


    public void ExecuteRobotAction(float[] data, Robot robot)
    {
        if (data == null || robot == null || data.Length < 1)
        {
            return;
        }
        //  若校正好 可用speed和rotateSpeed固定以下四輪數值
        float speed = 0.0f;
        float rotateSpeed = 0.0f;

        wheel_data[0] = data[0] * 360.0f * speedRate; // LF
        wheel_data[2] = data[2] * 360.0f * speedRate; // RF
        wheel_data[1] = data[1] * 360.0f * speedRate; // LB
        wheel_data[3] = data[3] * 360.0f * speedRate; // RB
        
        if(manual)
        {
            PublishFloat32MultiArray(topicName, wheel_data);
        }
        key = 1;    
    }
    public void PublishFloat32MultiArray(string topic, float[] wheel_data)
    {
        string jsonMessage = $@"{{
            ""op"": ""publish"",
            ""topic"": ""{topic}"",
            ""msg"": {{
                ""layout"": {{
                    ""dim"": [{{""size"": {wheel_data.Length}, ""stride"": 1}}],
                    ""data_offset"": 0
                }},
                ""data"": [{string.Join(", ", wheel_data)}]
            }}
        }}";

        connectRos.ws.Send(jsonMessage);
    }
    private void HandleAI2UnityReceiveTopic(RobotNewsMessage message)
    {   
        float[] data = message.msg.data;
        ExecuteRobotAction(data, robot);
    }

    private void HandleAI2UnityResetTopic(RobotNewsMessage message)
    {        
        target_change_flag = 1;
    }

    State updateState(Vector3 newTarget)
    {
        State state = robot.GetState(newTarget);
        return state;
    }
    private float randomFloat(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    private List<Vector3> findPeak(Vector3 start, Vector3 end)
    {
        Vector3 mid = (start + end) / 2;
        float minX = Mathf.Min(start[0], end[0]);
        float maxX = Mathf.Max(start[0], end[0]);
        float minY = Mathf.Min(start[2], end[2]);
        float maxY = Mathf.Max(start[2], end[2]);

        Vector3 c0 = new Vector3(randomFloat(minX, maxX), start[1], randomFloat(minY, minY + Mathf.Abs(maxY - minY) * 0.7f));
        Vector3 c1 = new Vector3(randomFloat(minX, maxX), start[1], randomFloat(maxY - Mathf.Abs(maxY - minY) * 0.7f, maxY));

        List<Vector3> result = new List<Vector3> { c0, c1 };
        return result;
    }
    void Send(object data) //send data to AI
    {
        var properties = typeof(State).GetProperties();
        Dictionary<string, object> stateDict = new Dictionary<string, object>();

        foreach (var property in properties)
        {
            string propertyName = property.Name;
            var value = property.GetValue(data);
            stateDict[propertyName] = value;
        }

        string dictData = MiniJSON.Json.Serialize(stateDict);

        Dictionary<string, object> message = new Dictionary<string, object>
        {
            { "op", "publish" },
            { "id", "1" },
            { "topic", Unity2AI_Topic },
            { "msg", new Dictionary<string, object>
                {
                    { "data", dictData}
                }
           }
        };

        string jsonMessage = MiniJSON.Json.Serialize(message);

        try
        {
            socket.Send(jsonMessage);
        }
        catch
        {
            Debug.Log("error-send");
        }
    }
    private void SubscribeToTopic(string topic)
    {
        string subscribeMessage = "{\"op\":\"subscribe\",\"id\":\"1\",\"topic\":\"" + topic + "\",\"type\":\"std_msgs/msg/Float32MultiArray\"}";
        socket.Send(subscribeMessage);
    }

    bool IsPointInsidePolygon(Vector3 point, Vector3[] polygonVertices)
    {
        int polygonSides = polygonVertices.Length;
        bool isInside = false;

        for (int i = 0, j = polygonSides - 1; i < polygonSides; j = i++)
        {
            if (((polygonVertices[i].z <= point.z && point.z < polygonVertices[j].z) ||
                (polygonVertices[j].z <= point.z && point.z < polygonVertices[i].z)) &&
                (point.x < (polygonVertices[j].x - polygonVertices[i].x) * (point.z - polygonVertices[i].z) / (polygonVertices[j].z - polygonVertices[i].z) + polygonVertices[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    void change_target() 
    // 新的一輪 換car、target position
    //  這邊會更新newtarget值
    {
        carPos = baselink.GetComponent<ArticulationBody>().transform.position;
        outerPolygonVertices = new Vector3[]{
            anchor1.transform.position,
            anchor2.transform.position,
            anchor3.transform.position,
            anchor4.transform.position
        };
        // -------------------------------------
         do {
        target_x_car = Random.Range(-3.0f, 3.0f);
        target_x_car = abs_biggerthan1(target_x_car);
        target_y_car = Random.Range(-3.0f, 3.0f);
        target_y_car = abs_biggerthan1(target_y_car);
        newTarget_car = new Vector3(carPos[0] + target_x_car, -1.608f, carPos[2] + target_y_car);
    } while (!IsPointInsidePolygon(newTarget_car, outerPolygonVertices));
        MoveRobot(newTarget_car);
        do {
        target_x = Random.Range(-3.0f, 3.0f);
        target_x = abs_biggerthan1(target_x);
        target_y = Random.Range(-3.0f, 3.0f);
        target_y = abs_biggerthan1(target_y);
        newTarget = new Vector3(newTarget_car[0] + target_x, -1.608f, newTarget_car[2] + target_y);
    } while (!IsPointInsidePolygon(newTarget, outerPolygonVertices) || Vector3.Distance(newTarget, newTarget_car) < 3);
        MoveGameObject(target, newTarget);
        State state = updateState(newTarget);
        Send(state);
    }

    void get_position_test()
    {
       Vector3 targetPosition = target.transform.position;
       State state = updateState(newTarget);
       Send(state);
    }

    private float abs_biggerthan1(float random)
    {
        if (random <= 1 && random >= -1)
        {
            if (random > 0)
            {
                random += 1;
            }
            else
            {
                random -= 1;
            }
        }
        return random;
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