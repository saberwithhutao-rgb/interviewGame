using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 让 RawImage 铺满父物体并保持宽高比（类似 CSS 的 background-size: cover）。
/// 仅在初始化和尺寸变化时更新，避免每帧开销。
/// </summary>
[RequireComponent(typeof(RawImage))]
public class RawImageCover : MonoBehaviour
{
    private RawImage rawImage;
    private RectTransform rectTransform;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Fit();
    }

    /// <summary>
    /// Unity 会在 RectTransform 的尺寸或锚点发生变化时自动调用此方法。
    /// 非常适合用来替代 Update 做自适应计算。
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        Fit();
    }

    private void Fit()
    {
        if (rawImage == null || rawImage.texture == null) return;

        RectTransform parent = transform.parent as RectTransform;
        if (parent == null) return;

        float parentWidth = parent.rect.width;
        float parentHeight = parent.rect.height;

        // 防止父物体尺寸为 0 时除以 0
        if (parentWidth <= 0 || parentHeight <= 0) return;

        float texWidth = rawImage.texture.width;
        float texHeight = rawImage.texture.height;

        if (texWidth <= 0 || texHeight <= 0) return;

        float scaleX = parentWidth / texWidth;
        float scaleY = parentHeight / texHeight;
        float scale = Mathf.Max(scaleX, scaleY);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, texWidth * scale);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, texHeight * scale);

        rectTransform.anchoredPosition = Vector2.zero;
    }
}