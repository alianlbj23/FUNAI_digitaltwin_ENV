using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LidarToROS : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    string topicName = "/scan";

    public void PublishLidar(List<float> data)
    {
        // string jsonMessage = $@"{{
        //     ""op"": ""publish"",
        //     ""topic"": ""{topicName}"",
        //     ""msg"": {{
        //         ""layout"": {{
        //             ""dim"": [{{""size"": {data.Count}, ""stride"": 1}}],
        //             ""data_offset"": 0
        //         }},
        //         ""data"": [{string.Join(", ", data)}]
        //     }}
        // }}";
        DateTime now = DateTime.UtcNow;
        long secs = ((DateTimeOffset)now).ToUnixTimeSeconds();
        long nsecs = now.Ticks % TimeSpan.TicksPerSecond * 100;
        float angleMin = -Mathf.PI;
        float angleMax = Mathf.PI;
        float angleIncrement = 2 * Mathf.PI / data.Count;
        float timeIncrement = 0.0f;
        float scanTime = 0.0f;
        float rangeMin = 0.0f;
        float rangeMax = 100.0f;
        string ranges = string.Join(", ", data);

        string jsonMessage = $@"{{
            ""op"": ""publish"",
            ""topic"": ""{topicName}"",
            ""msg"": {{
                ""header"": {{
                    ""stamp"": {{
                        ""secs"": {secs},
                        ""nsecs"": {nsecs}
                    }},
                    ""frame_id"": ""laser""
                }},
                ""angle_min"": {angleMin},
                ""angle_max"": {angleMax},
                ""angle_increment"": {angleIncrement},
                ""time_increment"": {timeIncrement},
                ""scan_time"": {scanTime},
                ""range_min"": {rangeMin},
                ""range_max"": {rangeMax},
                ""ranges"": [{ranges}],
                ""intensities"": []
            }}
        }}";

        connectRos.ws.Send(jsonMessage);
    }
}
