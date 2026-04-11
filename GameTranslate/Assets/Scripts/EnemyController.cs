using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum FishType { Small, Medium, Large }
    public FishType myType; // 在Inspector中选择是大中小鱼

    [Header("追击设置")]
    public float baseSpeed = 3f;
    public float chaseRadius = 8f; // 怪物多远才开始追玩家 (仇恨范围)
    
    [Header("避障设置")]
    public LayerMask obstacleLayer;  // 告诉怪物哪些东西是障碍物
    public float detectDistance = 1.5f; // 前方探路触须的长度
    public float avoidStrength = 2f;    // 避开障碍物的转向力度
    
    private float currentSpeed;
    private Transform targetPlayer;

    private bool isIlluminated = false; // 是否被光照到
    
    private Rigidbody2D rb;

    void Start()
    {
        targetPlayer = GameObject.Find("Player").transform;
        currentSpeed = baseSpeed;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (targetPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer <= chaseRadius)
        {
            MoveTowardsPlayer();
        }
        else
        {
            currentSpeed = 0f; 
            rb.velocity = Vector2.zero; // 跑远了就停下
        }
    }

    void MoveTowardsPlayer()
    {
        // 如果被光照到，调整速度 (保留你之前的光照逻辑)
        if (isIlluminated)
        {
            if (myType == FishType.Small || myType == FishType.Medium) currentSpeed = 0f;
            else if (myType == FishType.Large) currentSpeed = baseSpeed * 0.3f;
        }
        else
        {
            currentSpeed = baseSpeed;
        }

        if (currentSpeed <= 0f) 
        {
            rb.velocity = Vector2.zero;
            return; // 如果速度为0（被照停了），直接退出，不移动不转向
        }

        // 1. 基础方向：直直地指向玩家
        Vector2 dirToPlayer = ((Vector2)targetPlayer.position - (Vector2)transform.position).normalized;
        Vector2 finalMoveDir = dirToPlayer;

        // 2. 发射射线检测前方是否有障碍物（触须检测）
        // transform.up 是因为2DSprite默认顶部朝上
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, detectDistance, obstacleLayer);

        // 如果前方检测到了障碍物
        if (hit.collider != null)
        {
            // 获取障碍物表面的法线（垂直于墙面的力）
            Vector2 hitNormal = hit.normal;

            // 计算出沿着墙壁滑行的切线方向
            Vector2 avoidDir = Vector2.Perpendicular(hitNormal);

            // 智能判断：向左滑还是向右滑离玩家更近？
            if (Vector2.Dot(avoidDir, dirToPlayer) < 0)
            {
                avoidDir = -avoidDir; // 反转方向
            }

            // 将避障方向叠加到基础方向上
            finalMoveDir = (dirToPlayer + avoidDir * avoidStrength).normalized;
            
            // 可选：画出射线方便你在 Scene 窗口调试 (仅在Editor中可见)
            Debug.DrawLine(transform.position, hit.point, Color.red);
            Debug.DrawRay(hit.point, avoidDir, Color.green);
        }

        // 3. 应用物理移动 (使用 rigidbody 控制比直接修改 position 更平滑且不易穿模)
        rb.velocity = finalMoveDir * currentSpeed;

        // 4. 平滑转向最终的移动方向 (让鱼看起来是游过去的，而不是平移)
        float angle = Mathf.Atan2(finalMoveDir.y, finalMoveDir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
        // 使用 Lerp 让转向稍微带点阻尼感，像真鱼一样
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
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