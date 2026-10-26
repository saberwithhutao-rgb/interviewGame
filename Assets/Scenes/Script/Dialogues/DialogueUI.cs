using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对话面板 UI，负责显示对话内容、打字机效果、选项按钮
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("对话框")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("选项")]
    [SerializeField] private GameObject optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("打字机")]
    [SerializeField] private float typingInterval = 0.1f;  // 每个字的间隔

    // 打字状态
    private bool isTyping = false;
    private string fullText = "";
    private Coroutine typingCoroutine;

    // 当前是否处于选项阶段
    private bool isInOptionPhase = false;

    // 当前显示的选项列表
    private List<DialogueNode> currentOptions = new List<DialogueNode>();

    // ============================================================
    // 生命周期
    // ============================================================

    private void Awake()
    {
        // 初始隐藏选项容器
        if (optionsContainer != null)
            optionsContainer.SetActive(false);

        // 给对话框挂点击事件
        if (dialogueBox != null)
        {
            Button btn = dialogueBox.GetComponent<Button>();
            if (btn == null)
                btn = dialogueBox.AddComponent<Button>();
            btn.onClick.AddListener(OnDialogueBoxClicked);
        }
    }

    private void OnEnable()
    {
        // 订阅 DialogueManager 事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnLineReceived += OnLineReceived;
            DialogueManager.Instance.OnOptionsReceived += OnOptionsReceived;
            DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnLineReceived -= OnLineReceived;
            DialogueManager.Instance.OnOptionsReceived -= OnOptionsReceived;
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }
    }

    // ============================================================
    // 事件处理
    // ============================================================

    /// <summary>
    /// 收到一句对话
    /// </summary>
    private void OnLineReceived(DialogueNode node)
    {
        Debug.Log($"OnLineReceived: {node.speaker}: {node.content}");
        isInOptionPhase = false;
        ClearOptions();
        optionsContainer?.SetActive(false);

        // 说话人名字（为空就留空）
        if (speakerNameText != null)
            speakerNameText.text = node.speaker ?? "";

        // 开始打字
        StartTyping(node.content);
    }

    /// <summary>
    /// 收到一组选项
    /// </summary>
    private void OnOptionsReceived(List<DialogueNode> options)
    {
        // 清空旧选项
        ClearOptions();

        isInOptionPhase = true;
        currentOptions = options;

        // 显示选项容器
        optionsContainer?.SetActive(true);

        // 动态生成选项按钮
        for (int i = 0; i < options.Count; i++)
        {
            CreateOptionButton(options[i], i);
        }
    }

    /// <summary>
    /// 对话结束
    /// </summary>
    private void OnDialogueEnded()
    {
        StopTyping();
        ClearOptions();
        optionsContainer?.SetActive(false);
        currentOptions.Clear();
        // 关闭面板
        UIManager.Instance?.CloseOverlay();
    }

    // ============================================================
    // 打字机
    // ============================================================

    private void StartTyping(string text)
    {
        StopTyping();

        fullText = text ?? "";
        contentText.text = "";
        isTyping = true;
        typingCoroutine = StartCoroutine(TypingRoutine());
    }

    private IEnumerator TypingRoutine()
    {
        contentText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            contentText.text += fullText[i];
            yield return new WaitForSeconds(typingInterval);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        contentText.text = fullText;
        isTyping = false;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    // ============================================================
    // 点击对话框
    // ============================================================

    private void OnDialogueBoxClicked()
    {
        // 选项阶段：点击对话框无效
        if (isInOptionPhase) return;

        if (isTyping)
        {
            // 打字中：跳过打字
            SkipTyping();
        }
        else
        {
            // 打字完成：进入下一句
            DialogueManager.Instance?.OnNextClicked();
        }
    }

    // ============================================================
    // 选项按钮
    // ============================================================

    private void CreateOptionButton(DialogueNode option, int index)
    {
        
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer.transform);

        // 设置文字
        TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = option.content;

        // 绑定点击
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            DialogueNode captured = option;  // 闭包捕获
            btn.onClick.AddListener(() => OnOptionClicked(captured));
        }
    }

    private void OnOptionClicked(DialogueNode option)
    {
        if (!isInOptionPhase) return;

        // 通知 Manager 处理选择
        DialogueManager.Instance?.OnOptionSelected(option);
    }

    private void ClearOptions()
    {
        if (optionsContainer == null) return;

        foreach (Transform child in optionsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
    }
}