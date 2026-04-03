using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum FishType { Small, Medium, Large }
    public FishType myType; // 在Inspector中选择是大中小鱼

    public float baseSpeed = 3f;
    private float currentSpeed;
    private Transform targetPlayer;

    private bool isIlluminated = false; // 是否被光照到

    void Start()
    {
        targetPlayer = GameObject.Find("Player").transform;
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        if (targetPlayer == null) return;

        // 1. 移动逻辑
        MoveTowardsPlayer();

        // 2. 转向玩家
        Vector2 lookDir = (Vector2)targetPlayer.position - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    void MoveTowardsPlayer()
    {
        // 如果被光照到，根据类型改变速度
        if (isIlluminated)
        {
            if (myType == FishType.Small || myType == FishType.Medium)
            {
                currentSpeed = 0f; // 小中型鱼停止
            }
            else if (myType == FishType.Large)
            {
                currentSpeed = baseSpeed * 0.3f; // 大型鱼减速到30%
            }
        }
        else
        {
            currentSpeed = baseSpeed; // 黑暗中恢复正常速度
        }

        // 朝着玩家移动
        transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, currentSpeed * Time.deltaTime);
    }

    // 重点：当进入光锥区域时
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LightArea"))
        {
            isIlluminated = true;
        }
        else if (collision.CompareTag("Player"))
        {
            // 获取玩家身上的脚本并触发死亡
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    // 重点：当离开光锥区域时
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("LightArea"))
        {
            isIlluminated = false;
        }
    }
}