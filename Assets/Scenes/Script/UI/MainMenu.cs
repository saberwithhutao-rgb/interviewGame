using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单类，负责处理主菜单的按钮点击事件
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// 开始游戏，加载地图场景Map_v1
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("Map_v1"); 
    }

    public void LoadLevelScene()
    {
        SceneManager.LoadScene("LevelScene");
    }


    /// <summary>
    /// 退出游戏，如果在编辑器中运行则停止播放模式，否则退出应用程序
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
    }
}