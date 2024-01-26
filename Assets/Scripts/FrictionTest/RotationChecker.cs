using UnityEngine;

public class RotationChecker : MonoBehaviour
{
    private Quaternion startRotation;
    private bool hasRotated;

    void Start()
    {
        // 記錄起始旋轉
        startRotation = transform.rotation;
        hasRotated = false;
    }

    void Update()
    {
        // 計算旋轉差異
        Quaternion currentRotation = transform.rotation;
        float angleDiff = Quaternion.Angle(startRotation, currentRotation);

        // 檢查是否轉動了90度
        if (angleDiff >= 90f && !hasRotated)
        {
            Debug.Log("物體已轉動90度！");
            hasRotated = true; // 確保只觸發一次
        }
    }
}
