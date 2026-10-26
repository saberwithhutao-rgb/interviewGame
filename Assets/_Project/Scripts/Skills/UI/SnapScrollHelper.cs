using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// SnapScrollHelper 类用于实现 ScrollRect 的自动对齐功能，当用户停止拖动时，内容会自动对齐到最近的技能项
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class SnapScrollHelper : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("对齐设置")]
    [SerializeField] private float snapDuration = 0.2f;    // 对齐动画时长
    [SerializeField] private float snapThreshold = 0.05f;  // 惯性停止速度阈值
    [SerializeField] private float itemSpacing = 10f;      // 每个技能项之间的间距

    private ScrollRect scrollRect;
    private RectTransform content;
    private float itemHeight;           // 每个技能项的高度（假设固定）
    private int itemCount;
    private bool isDragging = false;
    private bool isSnapping = false;
    private bool wasMoving = false;

    // 缓存每个技能项的世界Y坐标（相对于Content）
    private float[] itemCenterPositions;

    public Action<int> OnSnappedToIndex;

    public float ItemSpacing => itemSpacing;

    /// <summary>
    /// 初始化 SnapScrollHelper，获取 ScrollRect 和 Content 的引用
    /// </summary>
    public void Init()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    /// <summary>
    /// 刷新技能项信息，计算每个技能项的中心位置
    /// </summary>
    public void RefreshItems()
    {
        if (content == null)
        {
            Debug.LogError("SnapScrollHelper: content 为空");
            return;
        }

        if (content.childCount == 0)
        {
            Debug.LogWarning("SnapScrollHelper: content 没有子物体，可能还没有生成技能项");
            return;
        }

        // 假设所有技能项高度相同
        RectTransform firstItem = content.GetChild(0) as RectTransform;
        itemHeight = firstItem.rect.height;
        itemCount = content.childCount;

        itemCenterPositions = new float[itemCount];
        // 计算每个item的中心在content本地坐标系中的Y坐标（从上往下，中心点）
        // 注意：ScrollRect的content锚点通常为(0,1)，y从上到下减小
        for (int i = 0; i < itemCount; i++)
        {
            RectTransform item = content.GetChild(i) as RectTransform;
            // item的中心相对于content顶部的偏移（负值）
            float itemTop = -i * (itemHeight + itemSpacing);
            float itemCenterY = itemTop + itemHeight * 0.5f;
            itemCenterPositions[i] = itemCenterY;
        }
    }

    void Update()
    {
        if (isDragging || isSnapping) return;

        float velocity = Mathf.Abs(scrollRect.velocity.y);
        bool isMoving = velocity >= snapThreshold;

        // 只有当“从移动变为停止”的那一帧才触发对齐
        if (wasMoving && !isMoving)
        {
            SnapToNearest();
        }

        wasMoving = isMoving;
    }

    void SnapToNearest()
    {
        if (itemCount == 0) return;

        // 当前content的Y位置（相对于anchor）
        float contentY = content.anchoredPosition.y;

        // 找到最近的item中心
        int nearestIndex = 0;
        float minDistance = Mathf.Abs(contentY - itemCenterPositions[0]);
        for (int i = 1; i < itemCount; i++)
        {
            float distance = Mathf.Abs(contentY - itemCenterPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        float targetY = itemCenterPositions[nearestIndex];

        StartCoroutine(SmoothScrollTo(targetY, nearestIndex));
    }

    IEnumerator SmoothScrollTo(float targetY, int nearestIndex)
    {
        isSnapping = true;
        float startY = content.anchoredPosition.y;
        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            float t = elapsed / snapDuration;
            float newY = Mathf.Lerp(startY, targetY, t);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

            // 每帧都同步 normalizedPosition
            scrollRect.verticalNormalizedPosition = GetVerticalNormalizedPosition(newY);

            elapsed += Time.deltaTime;
            yield return null;
        }

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
        scrollRect.verticalNormalizedPosition = GetVerticalNormalizedPosition(targetY);
        InvokeSnappedEvent(nearestIndex);
        isSnapping = false;
        
    }

    private float GetVerticalNormalizedPosition(float anchoredY)
    {
        if (content.rect.height <= scrollRect.viewport.rect.height) return 1f;
        float maxY = content.rect.height - scrollRect.viewport.rect.height;
        return Mathf.Clamp01(1f - (anchoredY / maxY));
    }

    protected virtual void InvokeSnappedEvent(int index) // 方法改名
    {
        OnSnappedToIndex?.Invoke(index);
    }

    // ---------- 拖拽处理 ----------
    /// <summary>
    /// 当用户开始拖动时，停止所有对齐协程，并标记为正在拖动
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        StopAllCoroutines();
        isSnapping = false;
        wasMoving = false;
    }

    /// <summary>
    /// 当用户结束拖动时，标记为不再拖动，等待惯性自然停止后进行对齐
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}