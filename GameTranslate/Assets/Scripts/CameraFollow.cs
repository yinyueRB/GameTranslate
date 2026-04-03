using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 玩家
    public float smoothTime = 0.2f; // 平滑时间，值越大相机越软
    public Vector3 offset = new Vector3(0, 0, -10f); // 偏移量，2D相机Z轴必须在负数

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // 自动寻找场景中Tag为"Player"的物体
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void LateUpdate() // 摄像机跟随一定要用 LateUpdate，防止画面抖动
    {
        if (target != null)
        {
            // 目标位置 = 玩家位置 + 偏移量
            Vector3 targetPosition = target.position + offset;
            
            // 使用 SmoothDamp 平滑移动到目标位置
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}