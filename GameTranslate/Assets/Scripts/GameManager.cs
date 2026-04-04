using UnityEngine;
using System.Collections; // 必须引入这个才能用协程

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("玩家引用")]
    public Transform playerLightCone; // 改成获取 LightCone 的 Transform
    public UnityEngine.Rendering.Universal.Light2D bodyLight;
    public PlayerController playerController; // 获取玩家脚本，用于大结局停止操作
    public UnityEngine.Rendering.Universal.Light2D coneLight2D;

    [Header("区域状态")]
    public int currentRegion = 1;
    
    [Header("结局状态")]
    public bool isEndingActive = false; // 是否已经触发了最终结局
    
    private Vector3 targetConeScale = Vector3.one; // 光锥的目标缩放值
    private bool isFlickering = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
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
                // 进入区域4大框：只切BGM，灯光变得极小，但不触发彻底断电
                targetConeScale = new Vector3(0.3f, 0.3f, 1f); // 光变很小
                isFlickering = false; // 区域4反而不闪了，变成微弱的死光
                if (AudioManager.instance != null) AudioManager.instance.CrossfadeBGM(2f);
                break;
        }
    }
    
    public void TriggerFinalEnding()
    {
        if (isEndingActive) return;
        isEndingActive = true;
        StartCoroutine(DespairEndingRoutine());
    }

    // 大结局演出序列
    private IEnumerator DespairEndingRoutine()
    {
        Debug.Log("到达终点，大结局演出开始！");
        
        // 1. 玩家停止移动，进入无敌被包围状态
        playerController.canMove = false;
        
        // 2. 让周围鱼的速度变慢一点，营造宿命感 (可选)
        
        // 3. 按照你的要求：灯笼闪烁几次
        for (int i = 0; i < 5; i++)
        {
            if (coneLight2D != null) coneLight2D.intensity = 0.1f;
            yield return new WaitForSeconds(0.1f);
            if (coneLight2D != null) coneLight2D.intensity = 0.8f;
            yield return new WaitForSeconds(0.15f);
        }

        // 4. 灯光彻底熄灭 (噗！)
        if (coneLight2D != null) coneLight2D.intensity = 0;
        bodyLight.intensity = 0;
        
        if (AudioManager.instance != null) AudioManager.instance.StopAllAudioForEnding();
        Debug.Log("游戏结束 - 画面纯黑，绝对安静");
    }
}