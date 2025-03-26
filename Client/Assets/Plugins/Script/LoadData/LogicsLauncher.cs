using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MainMenu : MonoBehaviour
{
    // 主界面UI元素
    public GameObject mainMenuUI;
    public GameObject startButton;
    public GameObject quitButton;

    void Start()
    {
        // 绑定按钮事件
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
        quitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(QuitGame);
    }

    // 进入游戏
    void StartGame()
    {
        Debug.Log("进入游戏");
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

