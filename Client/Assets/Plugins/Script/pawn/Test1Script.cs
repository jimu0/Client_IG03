using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LogicsLauncher : MonoBehaviour
{
    // 主界面UI元素
    public GameObject mainMenuUI;
    public GameObject startButton;
    public GameObject quitButton;

    void Start()
    {
        // 初始化资源加载
        StartCoroutine(LoadInitialResources());
    }

    IEnumerator LoadInitialResources()
    {
        Debug.Log("开始加载初始资源...");
        // 模拟资源加载
        yield return new WaitForSeconds(2.0f);
        Debug.Log("初始资源加载完毕");

        // 显示主界面UI
        mainMenuUI.SetActive(true);

        // 绑定按钮事件
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
        quitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(QuitGame);
    }

    // 进入游戏
    void StartGame()
    {
        Debug.Log("进入游戏");
        // 隐藏主界面UI
        mainMenuUI.SetActive(false);
        // 在这里添加进入游戏的逻辑
        // 例如加载游戏场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    // 退出游戏
    void QuitGame()
    {
        Debug.Log("退出游戏");
        // 在编辑器中退出播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建版本中退出应用程序
        Application.Quit();
#endif
    }
}


