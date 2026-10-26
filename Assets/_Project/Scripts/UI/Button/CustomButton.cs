using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 自定义按钮，继承原生 Button，增加鼠标悬停和点击效果
/// </summary>
public class CustomButton : Button, IPointerEnterHandler, IPointerExitHandler
{
    [Header("悬停与点击效果")]
    [Tooltip("普通状态的缩放")]
    public float normalScale = 1f;

    [Tooltip("悬停时的缩放")]
    public float hoverScale = 1.1f;

    [Tooltip("点击按下时的缩放")]
    public float pressedScale = 0.95f;

    [Tooltip("悬停时的颜色附加")]
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private Vector3 originalScale;
    private Color originalColor;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        targetGraphic = GetComponent<Graphic>();

        if (targetGraphic != null)
        {
            originalColor = targetGraphic.color;
        }

        // 原生 Button 的点击事件绑定，用于处理按下缩放
        onClick.AddListener(OnButtonClicked);
    }

    // ==========================================
    // 鼠标悬停（进入）
    // ==========================================
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // 如果按钮不可交互，不触发悬停效果
        if (!IsInteractable()) return;

        transform.localScale = originalScale * hoverScale;

        if (targetGraphic != null)
        {
            targetGraphic.color = hoverColor;
        }
    }

    // ==========================================
    // 鼠标悬停（离开）
    // ==========================================
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // 恢复原样
        transform.localScale = originalScale;

        if (targetGraphic != null)
        {
            targetGraphic.color = originalColor;
        }
    }

    // ==========================================
    // 鼠标按下（点击）
    // ==========================================
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!IsInteractable()) return;

        // 按下时缩小
        transform.localScale = originalScale * pressedScale;
    }

    // ==========================================
    // 鼠标抬起（点击结束）
    // ==========================================
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!IsInteractable()) return;

        // 恢复为悬停的大小
        transform.localScale = originalScale * hoverScale;
    }

    private void OnButtonClicked()
    {
        // 这里可以添加点击时的音效或动画
        Debug.Log($"[CustomButton] {gameObject.name} 被点击了！");
    }
}