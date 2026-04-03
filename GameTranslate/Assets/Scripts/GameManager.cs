using UnityEngine;
using System.Collections; // 必须引入这个才能用协程

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("玩家引用")]
    public Transform playerLightCone; // 改成获取 LightCone 的 Transform
    public UnityEngine.Rendering.Universal.Light2D bodyLight;
    public PlayerController playerController; // 获取玩家脚本，用于大结局停止操作

    [Header("区域状态")]
    public int currentRegion = 1;
    
    private Vector3 targetConeScale = Vector3.one; // 光锥的目标缩放值
    private bool isFlickering = false;
    private UnityEngine.Rendering.Universal.Light2D coneLight2D;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerLightCone != null)
        {
            coneLight2D = playerLightCone.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        }
    }

    private void Update()
    {
        // 1. 平滑缩放整个光锥 (视觉+物理碰撞同时缩小)
        if (playerLightCone != null)
        {
            playerLightCone.localScale = Vector3.Lerp(playerLightCone.localScale, targetConeScale, Time.deltaTime * 2f);
        }

        // 2. 处理闪烁
        if (isFlickering && coneLight2D != null)
        {
            if (Random.Range(0, 100) < 5) coneLight2D.intensity = Random.Range(0.2f, 1f);
        }
    }

    public void EnterNewRegion(int regionNumber)
    {
        if (regionNumber <= currentRegion) return;
        currentRegion = regionNumber;
        Debug.Log("进入区域: " + currentRegion);
        
        // 激活当前区域名字对应的子物体(即怪物)
        GameObject currentRegionObj = GameObject.Find("Region_" + currentRegion);
        if (currentRegionObj != null)
        {
            // 遍历该区域下的所有怪物并激活它们
            foreach (Transform child in currentRegionObj.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        ApplyRegionEffects();
    }

    private void ApplyRegionEffects()
    {
        switch (currentRegion)
        {
            case 2:
                targetConeScale = new Vector3(0.7f, 0.7f, 1f);
                if(coneLight2D != null) coneLight2D.intensity = 0.8f;
                break;
            case 3:
                targetConeScale = new Vector3(0.5f, 0.5f, 1f);
                isFlickering = true;
                // 【新增】：进入区域3，开启心跳声！
                if (AudioManager.instance != null) AudioManager.instance.StartHeartbeat(); 
                break;
            case 4:
                // 【新增】：进入区域4，BGM用 2秒 的时间淡出再淡入新BGM！
                if (AudioManager.instance != null) AudioManager.instance.CrossfadeBGM(2f);
                
                StartCoroutine(DespairEndingRoutine());
                break;
        }
    }

    // 大结局演出序列
    private IEnumerator DespairEndingRoutine()
    {
        Debug.Log("大结局开始！");
        isFlickering = false; 

        targetConeScale = new Vector3(0.1f, 0.1f, 1f); 
        playerController.moveSpeed = playerController.baseSpeed * 0.3f;

        // 【新增】：绝望感飙升，心跳变得缓慢而沉重 (Pitch变小)
        if (AudioManager.instance != null) AudioManager.instance.SetHeartbeatPitch(0.6f);

        yield return new WaitForSeconds(3f);

        if(coneLight2D != null) coneLight2D.intensity = 0;
        bodyLight.intensity = 0;
        playerController.canMove = false;

        // 【新增】：灯光熄灭瞬间，切断所有声音，陷入纯粹的死寂！
        if (AudioManager.instance != null) AudioManager.instance.StopAllAudioForEnding();
        // 如果你找到了一个清脆的“噗/断电”音效，可以在上面这行之后单独播放一次

        Debug.Log("游戏结束 - 画面纯黑，绝对安静");
    }
}