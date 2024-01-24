using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowArmAngle : MonoBehaviour
{
    public GameObject arm;

    public TMP_InputField fingerText;
    public TMP_InputField wristText;
    public TMP_InputField elbowText;
    public TMP_InputField shoulderText;
    public TMP_InputField baseText;

    private ArmRosbridgeSubscriber ros;

    // Start is called before the first frame update
    void Start()
    {
        ros = arm.GetComponent<ArmRosbridgeSubscriber>();
    }

    // Update is called once per frame
    void Update()
    {
        fingerText.text = "" + (int)(ros.data[0] + 90) + "¢X";
        wristText.text = "" + (int)(ros.data[2] + 90) + "¢X";
        elbowText.text = "" + (int)(ros.data[3] + 90) + "¢X";
        shoulderText.text = "" + (int)(ros.data[4] + 90) + "¢X";
        baseText.text = "" + (int)(ros.data[5] + 90) + "¢X";
    }
}
