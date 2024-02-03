using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IpManager : MonoBehaviour
{
    public string ip = "192.168.68.110";
    private string protocol = "ws://";
    private string port = "9090";
    public string FullUrl
    {
        get { return $"{protocol}{ip}:{port}"; }
    }
    public void SetIp(string newIp)
    {
        ip = newIp;
    }
    public string GetIp()
    {
        return FullUrl;
    }
}
