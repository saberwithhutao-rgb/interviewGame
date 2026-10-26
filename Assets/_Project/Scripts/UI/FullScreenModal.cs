using UnityEngine;

/// <summary>
/// FullScreenModal类用于管理全屏模态窗口的显示和隐藏。它可以切换游戏视图和模态窗口之间的显示状态。
/// </summary>
public class FullScreenModal : MonoBehaviour
{
    [SerializeField] private GameObject fullScreenGroup;
    [SerializeField] private GameObject disappearUI;
    private Canvas playerCanvas;
    private bool isViewingGame; // 当前是否正在查看游戏视图

    private void Awake()
    {
        if (playerCanvas == null)
        {
            // 尝试获取本地玩家的Canvas组件
            playerCanvas = PlayerControl.LocalInstance.transform.Find("Canvas")?.GetComponent<Canvas>();
        }
        playerCanvas.gameObject.SetActive(false);

        if (disappearUI == null)
        {
            disappearUI = GameObject.Find("SkillShop");
        }
        if (disappearUI != null)
            disappearUI.SetActive(false);

        fullScreenGroup.SetActive(true);
        isViewingGame = false;
    }

    void OnEnable()
    {
        fullScreenGroup.SetActive(true);
        isViewingGame = false;
    }

    /// <summary>
    /// 切换游戏视图的显示状态。如果当前正在查看游戏视图，则隐藏游戏视图并显示全屏模态窗口；如果当前不在查看游戏视图，则显示游戏视图并隐藏全屏模态窗口。
    /// </summary>
    public void ToggleGameView()
    {
        isViewingGame = !isViewingGame;
        fullScreenGroup.SetActive(!isViewingGame);
        playerCanvas.gameObject.SetActive(isViewingGame);
        if (disappearUI != null)
            disappearUI.SetActive(isViewingGame);
    }

    /// <summary>
    /// 设置游戏视图的显示状态。如果传入的参数为true，则显示游戏视图并隐藏全屏模态窗口；如果传入的参数为false，则隐藏游戏视图并显示全屏模态窗口。
    /// </summary>
    /// <param name="show">是否显示游戏视图</param>
    public void IsShowGameView(bool show)
    {
        isViewingGame = show;
        fullScreenGroup.SetActive(!show);
        playerCanvas.gameObject.SetActive(show);
        if (disappearUI != null)
            disappearUI.SetActive(show);
    }
}