using UnityEngine;

/// <summary>
/// 对话触发器，挂在 NPC 上，玩家靠近按 F 触发对话
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("对话配置")]
    [SerializeField] private string dialogueFileName;

    [Header("对话后行为")]
    [SerializeField] private DialogueEndAction endAction = DialogueEndAction.None;

    [Header("可交互")]
    [SerializeField] private bool canInteract = true;

    private bool playerInRange = false;
    private bool isInDialogue = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DialogueTrigger] InteractionPrompt.Instance = {InteractionPrompt.Instance}");
        if (!canInteract) return;

        PlayerControl player = other.GetComponentInParent<PlayerControl>();
        if (player == null) return;
        if (!player.IsOwner) return;

        playerInRange = true;
        InteractionPrompt.Instance?.Register(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerControl player = other.GetComponentInParent<PlayerControl>();
        if (player == null) return;
        if (!player.IsOwner) return;

        playerInRange = false;
        InteractionPrompt.Instance?.Unregister(this);
    }

    private void OnDestroy()
    {
        if (playerInRange)
        {
            InteractionPrompt.Instance?.Unregister(this);
        }
    }

    // ============================================================
    // IInteractable 实现
    // ============================================================

    public string GetLabelText()
    {
        return "对话";
    }

    public bool CanInteract()
    {
        return canInteract && playerInRange && !isInDialogue;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(GameObject interactor)
    {
        StartDialogue();
    }

    // ============================================================
    // 对话逻辑
    // ============================================================

    private void StartDialogue()
    {
        if (string.IsNullOrEmpty(dialogueFileName))
        {
            Debug.LogWarning($"[DialogueTrigger] {gameObject.name} 未配置对话文件名");
            return;
        }

        isInDialogue = true;

        // 打开对话面板
        UIManager.Instance?.OpenOverlay("DialoguePanel");

        // 订阅对话结束事件
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;

        // 开始对话
        DialogueManager.Instance.StartDialogue(dialogueFileName);
    }

    private void OnDialogueEnded()
    {
        DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;

        isInDialogue = false;

        // 执行对话后行为
        ExecuteEndAction();

        if (!gameObject.activeSelf) return;

        // 如果玩家还在范围内，且还能交互，才重新注册
        if (playerInRange && canInteract && this != null)
        {
            InteractionPrompt.Instance?.Register(this);
            InteractionPrompt.Instance?.RestoreAll();
        }
    }

    private void ExecuteEndAction()
    {
        switch (endAction)
        {
            case DialogueEndAction.Disappear:
                gameObject.SetActive(false);
                break;

            case DialogueEndAction.Destroy:
                Destroy(gameObject);
                break;

            case DialogueEndAction.None:
            default:
                break;
        }
    }
}

enum DialogueEndAction
{
    None,
    Disappear,
    Destroy
}