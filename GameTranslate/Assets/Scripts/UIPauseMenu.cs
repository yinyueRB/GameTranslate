using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 按ESC弹出暂停菜单：继续 / 退出到Menu
public class UIPauseMenu : MonoBehaviour
{
    [Header("暂停面板")]
    public GameObject pausePanel;

    private bool isPaused = false;

    void Start()
    {
        // 一开始隐藏暂停UI
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // 按 ESC 打开/关闭暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // 暂停游戏
    void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 冻结游戏
    }

    // 继续游戏
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // 恢复游戏
    }

    // 退出到 Menu 场景
    public void GoToMenuScene()
    {
        Time.timeScale = 1f; // 退出前必须恢复时间
        SceneManager.LoadScene("Title"); // 跳转到菜单场景
    }
}