using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 系统消息管理器
/// </summary>
public class SystemMessageManager : MonoSingleton<SystemMessageManager>
{
    [Header("UI 组件")]
    [SerializeField] private SystemMessagePanel messagePanel;

    [Header("显示设置")]
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private float minDisplayDuration = 1.5f;
    [SerializeField] private int maxQueueSize = 20;

    [Header("音效")]
    [SerializeField] private AudioClip messageSound;
    [SerializeField] private float messageSoundVolume = 0.5f;

    [Header("全局消息设置")]
    [SerializeField] private bool isGlobalMessageEnabled = true;

    private Queue<SystemMessageData> messageQueue = new Queue<SystemMessageData>();
    private Coroutine processCoroutine;

    protected override void Awake()
    {
        // 如果面板已经存在（跨场景保留了），不需要重新实例化
        if (messagePanel != null && messagePanel.gameObject.scene.isLoaded)
            return;

        // 首次创建：实例化预制体
        if (messagePanel != null)
        {
            messagePanel = Instantiate(messagePanel, transform);
            messagePanel.gameObject.SetActive(true);
        }

        base.Awake();
    }

    // ========================================
    // ★ 本地消息（仅自己看到）
    // ========================================

    public void ShowLocalMessage(string content, MessageType type = MessageType.Normal, float duration = -1f)
    {
        if (string.IsNullOrEmpty(content)) return;

        float finalDuration = duration > 0 ? duration : defaultDisplayDuration;
        finalDuration = Mathf.Max(finalDuration, minDisplayDuration);

        if (messageQueue.Count >= maxQueueSize)
            messageQueue.Dequeue();

        messageQueue.Enqueue(new SystemMessageData(content, type, finalDuration));

        if (processCoroutine == null)
            processCoroutine = StartCoroutine(ProcessQueue());
    }

    // 带格式化的重载
    public void ShowLocalMessage(string format, MessageType type, params object[] args)
    {
        ShowLocalMessage(string.Format(format, args), type);
    }

    // ========================================
    // ★ 快捷方法（本地消息）
    // ========================================

    public void Info(string content) => ShowLocalMessage(content, MessageType.Normal);
    public void Success(string content) => ShowLocalMessage(content, MessageType.Success);
    public void Warning(string content) => ShowLocalMessage(content, MessageType.Warning);
    public void Error(string content) => ShowLocalMessage(content, MessageType.Error);
    public void Achievement(string content) => ShowLocalMessage(content, MessageType.Achievement);
    public void Combat(string content) => ShowLocalMessage(content, MessageType.Combat);

    // 带格式化的快捷方法
    public void Info(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Normal);
    public void Success(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Success);
    public void Warning(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Warning);
    public void Error(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Error);
    public void Achievement(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Achievement);
    public void Combat(string format, params object[] args) => ShowLocalMessage(string.Format(format, args), MessageType.Combat);

    // ========================================
    // ★ 全局消息（所有玩家可见）
    // ========================================

    public void ShowGlobalMessage(string content, MessageType type = MessageType.Normal, float duration = -1f)
    {
        if (!isGlobalMessageEnabled) return;

        if (!NetworkManager.Singleton.IsServer)
        {
            SendGlobalMessageServerRpc(content, type, duration);
        }
        else
        {
            ReceiveGlobalMessageClientRpc(content, type, duration);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendGlobalMessageServerRpc(string content, MessageType type, float duration)
    {
        ReceiveGlobalMessageClientRpc(content, type, duration);
    }

    [ClientRpc]
    private void ReceiveGlobalMessageClientRpc(string content, MessageType type, float duration)
    {
        ShowLocalMessage(content, type, duration);
    }

    // ========================================
    // ★ 队列处理
    // ========================================

    private IEnumerator ProcessQueue()
    {
        while (messageQueue.Count > 0)
        {
            var data = messageQueue.Dequeue();
            yield return StartCoroutine(DisplayMessage(data));
        }
        processCoroutine = null;
    }

    private IEnumerator DisplayMessage(SystemMessageData data)
    {
        if (messagePanel == null)
        {
            Debug.LogError("SystemMessageManager: messagePanel 未配置");
            yield break;
        }

        messagePanel.SetText(data.content);
        messagePanel.SetMessageType(data.messageType);

        if (messageSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(messageSound, messageSoundVolume);
        }

        bool showComplete = false;
        messagePanel.Show(() => showComplete = true);
        yield return new WaitUntil(() => showComplete);

        float displayTime = data.duration > 0 ? data.duration : defaultDisplayDuration;
        yield return new WaitForSeconds(displayTime);

        bool hideComplete = false;
        messagePanel.Hide(() => hideComplete = true);
        yield return new WaitUntil(() => hideComplete);
    }

    public void ClearQueue()
    {
        messageQueue.Clear();
        if (processCoroutine != null)
        {
            StopCoroutine(processCoroutine);
            processCoroutine = null;
        }
        messagePanel.Hide();
    }

}