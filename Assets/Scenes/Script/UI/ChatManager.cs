using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 聊天管理器，负责处理聊天消息的发送、接收和显示
/// </summary>
public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    [Header("UI 引用")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TMP_Text chatDisplayText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Canvas canvas;

    [Header("设置")]
    [SerializeField] private KeyCode toggleChatKey = KeyCode.T;
    [SerializeField] private int maxMessages = 100;

    [Header("滞留效果")]
    [SerializeField] private float fadeDuration = 1f;    // 淡入淡出时长
    [SerializeField] private float stayDuration = 3f;
    [SerializeField] private Transform keepVisible;
    public string lobbyId;

    private List<string> chatMessages = new List<string>();

    /// <summary>
    /// 获取聊天输入框是否处于激活状态
    /// </summary>
    public bool IsChatActive => chatInputField != null && chatInputField.gameObject.activeInHierarchy;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (canvas == null)
        {
            PlayerControl.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        chatPanel.SetActive(false);
        chatInputField.onSubmit.AddListener(SendChatMessage);
        lobbyId = SteamManager.Instance.mLobby.Id.ToString();
        AddSystemMessage($"当前大厅ID: <u><link={lobbyId}>{lobbyId}</link></u>");
    }

    void Update()
    {
        if (!chatInputField.isFocused && Input.GetKeyDown(toggleChatKey))
        {
            ToggleChat();
        }
    }

    void OnDisable()
    {
        PlayerControl.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    void OnLocalPlayerSpawned(PlayerControl pc)
    {
        canvas = pc.transform.Find("Canvas")?.GetComponent<Canvas>();
    }

    void ToggleChat()
    {
        bool isActive = !IsChatActive;
        chatPanel.SetActive(isActive);

        if (isActive)
        {
            foreach (Transform child in chatPanel.transform)
            {
                child.gameObject.SetActive(true);
            }

            UIManager.Instance.ShowCursor(true);
            canvas.gameObject.SetActive(false);
            chatInputField.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(chatInputField.gameObject); // 设置输入框为当前选中的 UI 元素
            chatInputField.MoveTextEnd(false); // 将光标移动到文本末尾
            StopAllCoroutines();
        }
        else
        {
            CloseChat();
        }
    }

    /// <summary>
    /// 关闭聊天面板并显示滞留效果
    /// </summary>
    public void CloseChat()
    {
        foreach (Transform child in chatPanel.transform)
        {
            child.gameObject.SetActive(child == keepVisible);
        }
        canvas.gameObject.SetActive(true);
        UIManager.Instance.ShowCursor(false);
        EventSystem.current.SetSelectedGameObject(null);

        StopAllCoroutines();
        StartCoroutine(HideKeepVisibleAfterDelay());
    }

    IEnumerator HideKeepVisibleAfterDelay()
    {
        // 等待 stayDuration 秒
        yield return new WaitForSeconds(stayDuration);

        // 隐藏 keepVisible
        keepVisible.gameObject.SetActive(false);
    }

    /// <summary>
    /// 发送聊天消息到服务器
    /// </summary>
    /// <param name="message">要发送的聊天消息</param>
    public void SendChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        SendChatMessageServerRpc(message, SteamClient.SteamId);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message, ulong senderSteamId)
    {
        ReceiveChatMessageClientRpc(message, senderSteamId);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message, ulong senderSteamId)
    {
        string senderName = new Friend(senderSteamId).Name;
        chatMessages.Add($"<b>{senderName}:</b> {message}");

        if (chatMessages.Count > maxMessages)
            chatMessages.RemoveAt(0);

        UpdateChatDisplay();

        if (!IsChatActive)
        {
            StopAllCoroutines();
            StartCoroutine(HideKeepVisibleAfterDelay());
        }
    }

    /// <summary>
    /// 处理发送按钮点击事件，将输入框中的消息发送出去
    /// </summary>
    public void OnSendButtonClicked()
    {
        SendChatMessage(chatInputField.text);
    }

    
    void UpdateChatDisplay()
    {
        chatDisplayText.text = string.Join("\n", chatMessages);
        Canvas.ForceUpdateCanvases(); // 强制更新 Canvas，以确保 ScrollRect 的内容尺寸正确计算
        scrollRect.verticalNormalizedPosition = 0f; // 将滚动条位置设置为底部
    }

    /// <summary>
    /// 添加系统消息到聊天显示中，并在聊天面板未激活时显示滞留效果
    /// </summary>
    /// <param name="message">要添加的系统消息</param>
    public void AddSystemMessage(string message)
    {
        chatMessages.Add($"<color=#FFD700>[系统]</color> {message}");
        if (chatMessages.Count > maxMessages)
            chatMessages.RemoveAt(0);

        UpdateChatDisplay();

        if (!IsChatActive)
        {
            keepVisible.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideKeepVisibleAfterDelay());
        }
    }
}