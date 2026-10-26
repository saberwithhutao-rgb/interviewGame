using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 交互提示管理器，屏幕右侧显示所有可交互对象
/// </summary>
public class InteractionPrompt : MonoSingleton<InteractionPrompt>
{
    [Header("UI 组件")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject promptItemPrefab;

    // 当前所有可交互对象
    private List<IInteractable> interactables = new List<IInteractable>();
    private Dictionary<IInteractable, GameObject> itemObjects = new Dictionary<IInteractable, GameObject>();

    // 当前按 F 的目标
    private IInteractable currentTarget;

    protected override void Awake()
    {
        base.Awake();
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (interactables.Count == 0) return;

        // 每帧刷新排序和当前目标
        SortByDistance();
        UpdateCurrentTarget();

        // 按 F 触发
        if (Input.GetKeyDown(KeyCode.F) && currentTarget != null)
        {
            if (currentTarget.CanInteract())
            {
                // 隐藏提示
                ForceHideAll();

                // 执行交互
                GameObject interactor = PlayerControl.LocalInstance?.gameObject;
                currentTarget.Interact(interactor);
            }
        }
    }

    /// <summary>
    /// 注册一个可交互对象
    /// </summary>
    public void Register(IInteractable interactable)
    {
        Debug.Log($"[InteractionPrompt] Register: {interactable.GetLabelText()}");
        if (interactables.Contains(interactable)) return;

        interactables.Add(interactable);
        CreatePromptItem(interactable);
        RefreshVisibility();
    }

    /// <summary>
    /// 取消注册
    /// </summary>
    public void Unregister(IInteractable interactable)
    {
        if (!interactables.Contains(interactable)) return;

        interactables.Remove(interactable);

        if (itemObjects.TryGetValue(interactable, out GameObject obj))
        {
            Destroy(obj);
            itemObjects.Remove(interactable);
        }

        if (interactable == currentTarget)
            currentTarget = null;

        RefreshVisibility();
    }

    /// <summary>
    /// 强制隐藏全部（对话开始时调用）
    /// </summary>
    public void ForceHideAll()
    {
        foreach (var obj in itemObjects.Values)
        {
            obj.SetActive(false);
        }
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    /// <summary>
    /// 恢复显示
    /// </summary>
    public void RestoreAll()
    {
        RefreshVisibility();
    }

    // ============================================================
    // 内部逻辑
    // ============================================================

    private void SortByDistance()
    {
        GameObject player = PlayerControl.LocalInstance?.gameObject;
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        interactables.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerPos, a.GetTransform().position);
            float distB = Vector3.Distance(playerPos, b.GetTransform().position);
            return distA.CompareTo(distB);
        });

        // 按新顺序重新排列 UI
        for (int i = 0; i < interactables.Count; i++)
        {
            if (itemObjects.TryGetValue(interactables[i], out GameObject obj))
            {
                obj.transform.SetSiblingIndex(i);
            }
        }
    }

    private void UpdateCurrentTarget()
    {
        currentTarget = null;

        foreach (var interactable in interactables)
        {
            if (interactable is UnityEngine.Object obj && obj == null)
            {
                Unregister(interactable);
                continue;
            }

            if (interactable.CanInteract())
            {
                currentTarget = interactable;
                break;  // 已按距离排序，第一个就是最近的
            }
        }
    }

    private void CreatePromptItem(IInteractable interactable)
    {
        if (promptItemPrefab == null || container == null) return;

        GameObject item = Instantiate(promptItemPrefab, container);
        itemObjects[interactable] = item;

        // 按名字查找子物体
        Transform keyTransform = item.transform.Find("KeyText");
        Transform labelTransform = item.transform.Find("LabelText");
        if (keyTransform != null)
        {
            TextMeshProUGUI keyText = keyTransform.GetComponent<TextMeshProUGUI>();
            if (keyText != null)
                keyText.text = "[F]";
        }

        if (labelTransform != null)
        {
            TextMeshProUGUI labelText = labelTransform.GetComponent<TextMeshProUGUI>();
            if (labelText != null)
                labelText.text = interactable.GetLabelText();
        }
    }

    private void RefreshVisibility()
    {
        if (panelRoot == null) return;

        bool hasAny = interactables.Count > 0;
        panelRoot.SetActive(hasAny);
    }
}