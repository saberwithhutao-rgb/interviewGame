using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum UILayer
{
    Game,
    Pause,
    Modal,
    Overlay
}

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private DynamicCrosshair crosshair;
    [SerializeField] private GameObject _permanentGroup;
    private SkillSelectionPanel skillSelectionPanel;

    private Dictionary<string, GameObject> panelDictionary = new Dictionary<string, GameObject>();
    private Stack<GameObject> pauseStack = new Stack<GameObject>();
    private Stack<GameObject> overlayStack = new Stack<GameObject>();
    private GameObject activeModal;



    protected override void Awake()
    {
        base.Awake();  

        if (panelDictionary == null || panelDictionary.Count == 0)
        {
            LoadAllPanels();
        }
    }


    void Start()
    {
        if (crosshair == null)
        {
            PlayerControl.OnLocalPlayerSpawned += (pc) =>
            {
                crosshair = pc.GetComponentInChildren<DynamicCrosshair>();
                if (crosshair != null)
                {
                    crosshair.SetVisible(!IsInputBlocked);
                }
            };
        }
    }

    void OnEnable()
    {
        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.FindGameObjectWithTag("MainCanvas");

            if (canvasObj == null)
            {
                return;
            }
            mainCanvas = canvasObj.GetComponent<Canvas>();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
        DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeModal != null) return;

            if (pauseStack.Count > 0)
            {
                CloseTopPausePanel();
                return;
            }

            if (overlayStack.Count > 0)
            {
                CloseOverlay();
                return;
            }

            OpenPausePanel("QuitPanel");
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            ToggleOverlay("SkillSelectionPanel");
        }
    }

    

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        pauseStack.Clear();

        if (activeModal != null)
        {
            activeModal.SetActive(false);
            activeModal = null;
        }

        Time.timeScale = 1f;

        if (scene.name != "MainMenu")
        {
            ReloadAllPanels();
        }
    }

    private void ReloadAllPanels()
    {
        panelDictionary.Clear();

        GameObject canvasObj = GameObject.FindGameObjectWithTag("MainCanvas");
        if (canvasObj == null)
        {
            return;
        }

        mainCanvas = canvasObj.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            return;
        }

        GameObject[] panelPrefabs = Resources.LoadAll<GameObject>("UI/");

        foreach (var prefab in panelPrefabs)
        {
            GameObject panel = Instantiate(prefab, mainCanvas.transform);
            panel.name = prefab.name;
            panel.SetActive(false);
            panelDictionary[prefab.name] = panel;
        }

        BindAllButtons();
    }

    // 打开一个不暂停时间但阻塞输入的面板
    public void OpenOverlay(string panelName)
    {
        if (!panelDictionary.TryGetValue(panelName, out GameObject panel))
        {
            Debug.LogError($"UIManager: 未找到面板 '{panelName}'");
            return;
        }

        // 避免重复打开同一个
        if (overlayStack.Count > 0 && overlayStack.Peek() == panel)
            return;

        if (panelName == "SkillSelectionPanel")
        {
            var skillPanel = panel.GetComponent<SkillSelectionPanel>();
            if (skillPanel != null)
            {
                skillPanel.Refresh(true);
            }
        }

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        overlayStack.Push(panel);

        // 阻塞输入但不暂停时间
        ShowCursor(true);
    }

    public void CloseOverlay()
    {
        if (overlayStack.Count == 0) return;

        GameObject panel = overlayStack.Pop();
        panel.SetActive(false);

        if (!IsInputBlocked)
        {
            // 没有其他 overlay 时，恢复鼠标状态（回到游戏模式）
            Time.timeScale = 1f;
            ShowCursor(false);
        }
    }

    public void OpenPausePanel(string panelName)
    {

        if (!panelDictionary.TryGetValue(panelName, out GameObject panel))
        {
            return;
        }

        if (panel == null || panel.gameObject == null)
        {
            panelDictionary.Remove(panelName);
            return;
        }

        bool wasEmpty = pauseStack.Count == 0;

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        pauseStack.Push(panel);

        if (wasEmpty)
        {
            if (SceneManager.GetActiveScene().name != "MultiplayerScene")
            {
                Time.timeScale = 0f;
            }
            ShowCursor(true);
        }
    }

    void LoadAllPanels()
    {
        GameObject[] panelPrefabs = Resources.LoadAll<GameObject>("UI/");
        foreach (var prefab in panelPrefabs)
        {
            GameObject panel = Instantiate(prefab, mainCanvas.transform);
            panel.name = prefab.name;
            panel.SetActive(false);
            panelDictionary[prefab.name] = panel;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        BindAllButtons();
    }

    public void OpenPanel(string panelName, bool refreshData = true)
    {
        if (panelDictionary.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(true);

            // 如果是技能选择面板，额外刷新数据
            if (panelName == "SkillSelectionPanel")
            {
                var skillPanel = panel.GetComponent<SkillSelectionPanel>();
                if (skillPanel != null)
                {
                    skillPanel.Refresh(refreshData);
                }
            }
        }
        else
        {
            Debug.LogError($"UIManager: 未找到面板 '{panelName}'");
        }
    }

    public void ClosePanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(false);
        }
    }
    public void TogglePanel(string panelName, bool refreshOnOpen = true)
    {
        GameObject panel = GetPanel(panelName);
        if (panel == null) return;

        if (panel.activeSelf)
        {
            ClosePanel(panelName);
        }
        else
        {
            OpenPanel(panelName, refreshOnOpen);
        }
    }

    public void ToggleOverlay(string panelName)
    {
        if (activeModal != null) return;

        if (pauseStack.Count > 0)
        {
            return;
        }

        if (ChatManager.Instance != null && ChatManager.Instance.IsChatActive)
        {
            return;
        }

        GameObject panel = GetPanel(panelName);
        if (panel != null && panel.activeSelf)
            CloseOverlay();
        else
            OpenOverlay(panelName);
    }


    public GameObject GetPanel(string panelName)
    {
        panelDictionary.TryGetValue(panelName, out GameObject panel);
        return panel;
    }

    
    //为按钮绑定事件
    void BindAllButtons()
    {
        BindButton("QuitPanel", "Button_Yes", QuitToMainMenu);
        BindButton("QuitPanel", "Button_No", () => CloseTopPausePanel());
        BindButton("QuitPanel", "Button_Settings", () => OpenPausePanel("SettingsPanel"));

        BindButton("SettingsPanel", "Button_Back", () => CloseTopPausePanel());

        BindButton("ObjectivePanel", "Button", () => CloseModal());

        BindButton("DeathPanel", "Button_Yes", QuitToMainMenu);

        BindButton("WinPanel", "Button_Back", QuitToMainMenu);
        BindButton("WinPanel", "Button_Yes", () => WinMenu.GoToEndless());

        BindButton("WinPanel Plot", "Button_Back", QuitToMainMenu);
        BindButton("WinPanel Plot", "Button_Yes", () => CloseTopPausePanel());
    }

    void BindButton(string panelName, string buttonName, UnityEngine.Events.UnityAction action)
    {
        GameObject panel = GetPanel(panelName);
        if (panel == null)
        {
            Debug.LogError($"BindButton: 面板 '{panelName}' 不存在");
            return;
        }


        Transform button = panel.transform.Find(buttonName);
        if (button == null)
        {
            Debug.LogError($"BindButton: 面板 '{panelName}' 下找不到按钮 '{buttonName}'");
            // 打印所有子对象名称帮助调试
            Debug.Log($"面板 '{panelName}' 的子对象:");
            foreach (Transform child in panel.transform)
            {
                Debug.Log($"  - {child.name}");
            }
            return;
        }

        Button btn = button.GetComponent<Button>();
        if (btn == null)
        {
            return;
        }

        btn.onClick.AddListener(action);
    }

    public void CloseTopPausePanel()
    {
        if (pauseStack.Count == 0) return;

        // 清理栈顶无效引用
        while (pauseStack.Count > 0)
        {
            GameObject top = pauseStack.Peek();
            if (top == null || top.gameObject == null)
            {
                // 引用无效，直接弹出并跳过
                pauseStack.Pop();
                continue;
            }

            // 有效引用，关闭它
            top.SetActive(false);
            pauseStack.Pop();
            break;
        }

        // 继续清理栈中其他无效引用（可选，保持栈健康）
        CleanInvalidPauseReferences();

        if (pauseStack.Count == 0 && activeModal == null && overlayStack.Count == 0)
        {
            Time.timeScale = 1f;
            ShowCursor(false);
        }
    }

    private void CleanInvalidPauseReferences()
    {
        // 移除栈中所有已销毁的引用
        var tempStack = new Stack<GameObject>();
        while (pauseStack.Count > 0)
        {
            var obj = pauseStack.Pop();
            if (obj != null && obj.gameObject != null)
            {
                tempStack.Push(obj);
            }
        }
        while (tempStack.Count > 0)
        {
            pauseStack.Push(tempStack.Pop());
        }
    }

    public void OpenModal(string panelName)
    {
        if (!panelDictionary.TryGetValue(panelName, out GameObject panel))
        {
            Debug.LogError($"UIManager: 未找到模态面板 '{panelName}'");
            return;
        }

        // 检查面板是否已被销毁
        if (panel == null || panel.gameObject == null)
        {
            Debug.LogError($"UIManager: 面板 '{panelName}' 已被销毁");
            return;
        }

        // 🔧 修复：模态面板应该用 activeModal，而不是 pauseStack
        activeModal = panel;
        panel.SetActive(true);

        if (SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            Time.timeScale = 0f;
        }
        ShowCursor(true);

    }

    public void CloseModal()
    {

        if (activeModal == null) return;
        activeModal.SetActive(false);
        activeModal = null;

        Time.timeScale = 1f;
        ShowCursor(false);
    }

    public void ShowCursor(bool show)
    {
        if (crosshair == null)
        {
            crosshair = FindObjectOfType<DynamicCrosshair>(true);
        }

        if (crosshair != null)
        {
            crosshair.SetVisible(!show);
        }

        if (show)
        {
            // 显示鼠标，解锁光标
            if (MouseController.Instance != null)
            {
                MouseController.Instance.RequestUnlock();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            // 隐藏鼠标，锁定光标
            if (MouseController.Instance != null)
            {
                MouseController.Instance.RequestLock();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void OnDialogueStarted()
    {
        _permanentGroup.SetActive(false);
    }

    private void OnDialogueEnded()
    {
        _permanentGroup.SetActive(true);
    }


    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsInputBlocked => pauseStack.Count > 0
                            || activeModal != null
                            || overlayStack.Count > 0
        || (ChatManager.Instance && ChatManager.Instance.IsChatActive);
}