using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;

public class IpManager : MonoBehaviour
{
    // private string ip = "192.168.68.110";
    private string protocol = "ws://";
    private string port = "9090";
    public string FullUrl
    {
        get { return $"{protocol}{GetIpAddress()}:{port}"; }
    }
    private string GetIpAddress()
    {
        // 優先尋找 Wi-Fi 介面卡的 IP 地址
        string ip = GetIpByNetworkInterfaceType(NetworkInterfaceType.Wireless80211);
        if (!string.IsNullOrEmpty(ip))
        {
            return ip;
        }

        // 如果找不到 Wi-Fi IP，則尋找乙太網路卡的 IP 地址
        ip = GetIpByNetworkInterfaceType(NetworkInterfaceType.Ethernet);
        if (!string.IsNullOrEmpty(ip))
        {
            return ip;
        }

        // 如果兩者都找不到，返回預設 IP 或錯誤訊息
        return "127.0.0.1"; // 或返回適當的錯誤訊息
    }
    private string GetIpByNetworkInterfaceType(NetworkInterfaceType type)
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == type && ni.OperationalStatus == OperationalStatus.Up)
            {
                IPInterfaceProperties properties = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }

        return string.Empty; // 沒有找到符合條件的 IP 地址
    }
    // public void SetIp(string newIp)
    // {
    //     ip = newIp;
    // }
    public string GetIp()
    {
        return FullUrl;
    }
}
