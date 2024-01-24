using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarToROS : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    public string topicName = "/lidar_value";

    public void PublishLidar(List<float> data)
    {
        string jsonMessage = $@"{{
            ""op"": ""publish"",
            ""topic"": ""{topicName}"",
            ""msg"": {{
                ""layout"": {{
                    ""dim"": [{{""size"": {data.Count}, ""stride"": 1}}],
                    ""data_offset"": 0
                }},
                ""data"": [{string.Join(", ", data)}]
            }}
        }}";

        connectRos.ws.Send(jsonMessage);
    }
}
