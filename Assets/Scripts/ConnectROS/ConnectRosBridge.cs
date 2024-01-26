using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
public class ConnectRosBridge : MonoBehaviour
{
    string rosbridgeServerAddress = "ws://localhost:9090";
    // private string rosbridgeServerAddress = "ws://192.168.200.34:9090";
    public WebSocket ws;

    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket(rosbridgeServerAddress);
        ws.Connect();
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}
