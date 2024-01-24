
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowWheelSpeed : MonoBehaviour
{
    public GameObject car;

    public TMP_InputField leftFrontWheel;
    public TMP_InputField rightFrontWheel;
    public TMP_InputField leftBackWheel;
    public TMP_InputField rightBackWheel;

    private RosbridgeSubscriber ros;
    // Start is called before the first frame update
    void Start()
    {
        ros = car.GetComponent<RosbridgeSubscriber>();
    }

    // Update is called once per frame
    void Update()
    {
        leftFrontWheel.text = "" + (int)ros.wA1Value;
        rightFrontWheel.text = "" + (int)ros.wA2Value;
        leftBackWheel.text = "" + (int)ros.wA3Value;
        rightBackWheel.text = "" + (int)ros.wA4Value;
    }
}
