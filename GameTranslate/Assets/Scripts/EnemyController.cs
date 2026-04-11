using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum FishType { Small, Medium, Large }
    public FishType myType; // 在Inspector中选择是大中小鱼

    [Header("追击设置")]
    public float baseSpeed = 3f;
    public float chaseRadius = 8f; // 怪物多远才开始追玩家 (仇恨范围)
    
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

        // 计算怪物和玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // 如果在追击范围内，才执行移动和转向
        if (distanceToPlayer <= chaseRadius)
        {
            MoveTowardsPlayer();

            // 转向玩家
            Vector2 lookDir = (Vector2)targetPlayer.position - (Vector2)transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else
        {
            // 如果玩家跑远了，怪物停在原地休息
            currentSpeed = 0f; 
        }
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

    // 1. 用于检测【实体碰撞】（撞到玩家、撞墙等）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 注意：Collision2D 拿标签的方法是 collision.gameObject.CompareTag
        if (collision.gameObject.CompareTag("Player"))
        {
            // 获取玩家身上的脚本并触发死亡
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }

    // 2. 用于检测【区域重叠】（进入光照范围，因为光照依然是Trigger）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 注意：Collider2D 拿标签的方法是 collision.CompareTag
        if (collision.CompareTag("LightArea"))
        {
            isIlluminated = true;
        }
    }

    // 3. 离开光照范围
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("LightArea"))
        {
            isIlluminated = false;
        }
    }
}