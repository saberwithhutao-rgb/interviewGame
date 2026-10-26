using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 按键绑定按钮 - 点击后等待用户按键，自动识别并填入
/// </summary>
public class KeyBindingButton : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI keyDisplayText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button button;

    [SerializeField] private SkillDatabase skillDatabase;

    private SkillKeyBinding currentBinding;
    private int currentSkillId;
    private bool isWaiting = false;

    // 绑定完成回调
    public System.Action<SkillKeyBinding, int> OnBindingComplete;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(StartBinding);
    }

    private void Update()
    {
        if (!isWaiting) return;

        // ★ 检测按键
        SkillKeyBinding detected = KeyBindingDetector.DetectKeyPress();

        if (detected != null && !detected.IsEmpty())
        {
            // 检测到按键，尝试绑定
            bool conflict = SkillBindingManager.Instance.IsKeyConflicting(currentSkillId, detected);
            if (conflict)
            {
                int? conflictId = SkillBindingManager.Instance.GetConflictSkillId(currentSkillId, detected);
                if (conflictId == null) return;
                string conflictName = skillDatabase.GetNameById(conflictId.Value);
                statusText.text = $"与 [{conflictName}] 冲突!"; 
                statusText.color = Color.red;
                isWaiting = false;
                button.interactable = true;
                return;
            }

            // 无冲突，完成绑定
            SetBindingInternal(detected);
            statusText.text = "已绑定";
            statusText.color = Color.green;
            isWaiting = false;
            button.interactable = true;

            OnBindingComplete?.Invoke(currentBinding, currentSkillId);
            SkillBindingManager.Instance.SaveBinding(currentSkillId, currentBinding);
        }

        // ★ Escape 取消
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBinding();
        }
    }

    /// <summary>
    /// 绑定技能
    /// </summary>
    public void SetSkillId(int skillId)
    {
        currentSkillId = skillId;
    }

    /// <summary>
    /// 设置绑定值
    /// </summary>
    public void SetBinding(SkillKeyBinding binding)
    {
        currentBinding = binding ?? new SkillKeyBinding();
        UpdateDisplay();
        statusText.text = currentBinding.IsEmpty() ? "点击绑定" : "已绑定";
        statusText.color = currentBinding.IsEmpty() ? Color.gray : Color.green;
    }

    private void StartBinding()
    {
        isWaiting = true;
        button.interactable = false;
        keyDisplayText.text = "?";
        keyDisplayText.color = Color.yellow;
        statusText.text = "按下任意键...";
        statusText.color = Color.white;
    }

    private void CancelBinding()
    {
        isWaiting = false;
        button.interactable = true;

        UpdateDisplay();
        statusText.text = currentBinding.IsEmpty() ? "点击绑定" : "已绑定";
        statusText.color = currentBinding.IsEmpty() ? Color.gray : Color.green;
    }

    private void SetBindingInternal(SkillKeyBinding binding)
    {
        currentBinding = binding;
        UpdateDisplay();
        keyDisplayText.color = Color.white;
    }

    private void UpdateDisplay()
    {
        keyDisplayText.text = currentBinding.GetDisplayName();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 获取当前绑定
    /// </summary>
    public SkillKeyBinding GetBinding()
    {
        return currentBinding;
    }
}