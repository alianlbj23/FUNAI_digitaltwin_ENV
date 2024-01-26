using UnityEngine;

public class DistanceChecker : MonoBehaviour
{
    private Vector3 startPosition;
    private bool hasWarned;

    void Start()
    {
        // 記錄起始位置
        startPosition = transform.position;
        hasWarned = false;
    }

    void Update()
    {
        // 計算當前位置與起始位置之間的距離
        float distance = Vector3.Distance(startPosition, transform.position);
        // 檢查距離是否達到一公尺，並且尚未發出警告
        if (distance >= 1.0f && !hasWarned)
        {
            // 發出警告
            Debug.Log("警告：物體已移動一公尺！");
            hasWarned = true; // 確保警告只觸發一次
        }
    }
}
