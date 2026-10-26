using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 鼠标控制器，负责管理鼠标的锁定状态和可见性
/// </summary>
public class MouseController : MonoSingleton<MouseController>
{
    private bool shouldLock = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            RequestUnlock();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt) && (UIManager.Instance == null || !UIManager.Instance.IsInputBlocked))
        {
            RequestLock();
        }
    }

    /// <summary>
    /// 请求锁定鼠标。将鼠标锁定在屏幕中心，并隐藏鼠标光标。
    /// </summary>
    public void RequestLock()
    {
        shouldLock = true;
        Apply();
    }

    /// <summary>
    /// 请求解锁鼠标。将鼠标解锁，并显示鼠标光标。
    /// </summary>
    public void RequestUnlock()
    {
        shouldLock = false;
        Apply();
    }

    /// <summary>
    /// 应用当前的鼠标锁定状态。根据 shouldLock 的值，设置鼠标的锁定状态和可见性。
    /// </summary>
    private void Apply()
    {
        if (shouldLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            RequestUnlock();
        }
        else
        {
            RequestLock();
        }
    }
}