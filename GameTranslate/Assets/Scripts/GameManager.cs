using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static int reachedRegion = 1; 

    [Header("复活点设置")]
    public Transform[] spawnPoints; // 在面板里把4个SpawnPoint按顺序拖进来

    [Header("玩家引用")]
    public Transform playerLightCone;
    public UnityEngine.Rendering.Universal.Light2D bodyLight;
    public PlayerController playerController;

    [Header("区域状态")]
    public int currentRegion = 1;
    public bool isEndingActive = false;
    
    [Header("UI表现")]
    public CanvasGroup fadeScreen; // 拖入 FadeImage 身上的 CanvasGroup 组件
    public float fadeDuration = 1.5f; // 黑屏渐变时间
    
    private Vector3 targetConeScale = Vector3.one;
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

        // --- 每次场景重新加载时，执行复活逻辑 ---
        currentRegion = reachedRegion; // 读取记录
        Debug.Log("从区域 " + currentRegion + " 复活");

        // 1. 把玩家传送到对应的复活点 (注意数组下标从0开始)
        if (spawnPoints.Length >= currentRegion && playerController != null)
        {
            playerController.transform.position = spawnPoints[currentRegion - 1].position;
        }

        // 2. 立刻应用该区域的光照和音乐效果
        ApplyRegionEffects();

        // 3. 激活该区域的怪物 (如果你之前做了分区域激活怪物的话)
        ActivateEnemiesInRegion(currentRegion);
        
        if (fadeScreen != null) StartCoroutine(FadeInRoutine());
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
        reachedRegion = regionNumber; // 【新增】：玩家进入新区域时，更新存档记录！
        
        Debug.Log("进入区域: " + currentRegion + "，已自动存档");
        
        ActivateEnemiesInRegion(currentRegion);
        ApplyRegionEffects();
    }
    
    private void ActivateEnemiesInRegion(int regionNum)
    {
        GameObject currentRegionObj = GameObject.Find("Region_" + regionNum);
        if (currentRegionObj != null)
        {
            foreach (Transform child in currentRegionObj.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void ApplyRegionEffects()
    {
        // 这里把光照直接设为目标值，防止复活时看到光照慢慢缩小的穿帮镜头
        switch (currentRegion)
        {
            case 1:
                targetConeScale = Vector3.one;
                if(playerLightCone != null) playerLightCone.localScale = targetConeScale;
                break;
            case 2:
                targetConeScale = new Vector3(0.7f, 0.7f, 1f);
                if(playerLightCone != null) playerLightCone.localScale = targetConeScale;
                if(coneLight2D != null) coneLight2D.intensity = 0.8f;
                break;
            case 3:
                targetConeScale = new Vector3(0.5f, 0.5f, 1f);
                if(playerLightCone != null) playerLightCone.localScale = targetConeScale;
                isFlickering = true;
                if (AudioManager.instance != null) AudioManager.instance.StartHeartbeat(); 
                break;
            case 4:
                targetConeScale = new Vector3(0.3f, 0.3f, 1f);
                if(playerLightCone != null) playerLightCone.localScale = targetConeScale;
                isFlickering = false;
                if (AudioManager.instance != null) AudioManager.instance.CrossfadeBGM(2f);
                break;
        }
    }
    
    private IEnumerator FadeInRoutine()
    {
        fadeScreen.alpha = 1f; // 一开始全黑
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 0f;
    }
    
    public void TriggerDeath()
    {
        StartCoroutine(DeathFadeRoutine());
    }
    
    public void TriggerFinalEnding()
    {
        if (isEndingActive) return;
        isEndingActive = true;
        StartCoroutine(DespairEndingRoutine());
    }
    
    private IEnumerator DeathFadeRoutine()
    {
        // 1. 剥夺玩家控制，让角色停留在原地
        if (playerController != null) playerController.canMove = false;

        // 2. （可选，极度增强失落感）让背景音乐变得沉重缓慢
        if (AudioManager.instance != null && AudioManager.instance.bgmSource != null)
        {
            AudioManager.instance.bgmSource.pitch = 0.5f; 
        }

        // 3. 画面渐渐变黑
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadeScreen != null) fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 1f;

        // 4. 在无尽的黑暗中停留 1 秒钟，让失落感沉淀
        yield return new WaitForSeconds(1f);

        // 5. 重新加载场景复活
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
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