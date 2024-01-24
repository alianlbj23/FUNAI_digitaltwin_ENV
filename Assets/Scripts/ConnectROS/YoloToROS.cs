using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CJM.BBox2DToolkit;

public class YoloToROS : MonoBehaviour
{
    public ConnectRosBridge connectRos;
    public string topicName = "/yolo_bounding_box";

    public void ShowYoloInformation(BBox2DInfo[] bboxInfoArray)
    {
        string allBoundingBox = "";

        for (int i = 0; i < bboxInfoArray.Length; i++)
        {
            BBox2DInfo bboxInfo = bboxInfoArray[i];
            allBoundingBox += EachBoundingBox(bboxInfo, i == bboxInfoArray.Length - 1);
        }

        string jsonMessage = $@"{{
            ""op"": ""publish"",
            ""topic"": ""{topicName}"",
            ""msg"": {{
                ""boundingbox"": [{allBoundingBox}]
            }}
        }}";

        connectRos.ws.Send(jsonMessage);
    }

    private string EachBoundingBox(BBox2DInfo bboxInfo, bool isEnd)
    {
        string message = $@"{{
            ""x0"": {bboxInfo.bbox.x0},
            ""y0"": {bboxInfo.bbox.y0},
            ""width"": {bboxInfo.bbox.width},
            ""height"": {bboxInfo.bbox.height},
            ""confidence"": {bboxInfo.bbox.prob},
            ""label"": ""{bboxInfo.label}""
        }}";

        if (!isEnd)
            message += ",";

        return message;
    }
}
