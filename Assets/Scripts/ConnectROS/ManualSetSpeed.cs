using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualSetSpeed : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    public string topicName = "/wheel_speed";

    public Slider leftFrontWheel;
    public Slider rightFrontWheel;
    public Slider leftBackWheel;
    public Slider rightBackWheel;

    private float[] data = new float[4];
    public bool manual;

    // Start is called before the first frame update
    void Start()
    {
        setManual(true);
    }

    // Update is called once per frame
    void Update()
    {   
        CheckMode();
        if (manual)
        {
            data[0] = leftFrontWheel.value;
            data[1] = rightFrontWheel.value;
            data[2] = leftBackWheel.value;
            data[3] = rightBackWheel.value;
            PublishFloat32MultiArray(topicName, data);
        }
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

    // will be call when switch to other mode
    public void publishZeroData()
    {
        data[0] = 0;
        data[1] = 0;
        data[2] = 0;
        data[3] = 0;
        PublishFloat32MultiArray(topicName, data);
    }

    public void setManual(bool decision)
    {
        manual = decision;
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
                manual = (textContent == "Manual");
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
