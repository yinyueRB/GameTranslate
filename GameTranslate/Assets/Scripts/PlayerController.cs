using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    [HideInInspector] public float moveSpeed; // 实际速度，隐藏起来防止在Inspector调错
    public bool canMove = true; // 大结局剥夺控制权

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCam;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        moveSpeed = baseSpeed; // 初始化速度
    }

    void Update()
    {
        if (!canMove) return; // 如果不能动，直接退出

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f; 
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ... (之前如果有写碰到敌人死亡的代码，保留) ...

        // 检查是否碰到了区域边界
        if (collision.CompareTag("RegionTrigger"))
        {
            // 假设我们把区域的数字写在它的名字里，比如 "Region_2"
            string regionName = collision.gameObject.name;
            string[] parts = regionName.Split('_'); // 分割字符串获取数字
            
            if (parts.Length > 1)
            {
                if (int.TryParse(parts[1], out int regionNum))
                {
                    // 告诉 GameManager 我们到了新区域
                    GameManager.instance.EnterNewRegion(regionNum);
                }
            }
        }
        
        if (collision.CompareTag("EndingTrigger"))
        {
            GameManager.instance.TriggerFinalEnding();
        }
    }
    
    public void Die()
    {
        // 如果正在播放大结局，玩家处于"被鱼群淹没但游戏不重置"的状态，直接忽略死亡
        if (GameManager.instance.isEndingActive) 
        {
            return; 
        }

        // 正常的死亡读档逻辑
        Debug.Log("被吃掉了！重新开始！");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}