using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 动态准心组件，用于在射击时显示准心的扩散效果
/// </summary>
public class DynamicCrosshair : MonoBehaviour
{
    [Header("默认设置")]
    public float defaultRadius = 8f;     
    public float thickness = 2f;         
    public Color defaultColor = Color.white;

    [Header("扩散设置")]
    public float maxRadius = 20f;      
    public float spreadPerShot = 5f;
    public float spreadSpeed = 10f;    
    public float recoverySpeed = 5f;

    private RawImage rawImage;
    private Texture2D texture;
    private int texSize = 128;          
    private float currentRadius;
    private float targetRadius;
    private Color currentColor;

    [SerializeField] private Gun gun;

    

    private void Awake()
    {
        GameObject circleObj = new GameObject("Crosshair");
        circleObj.transform.SetParent(transform, false);

        rawImage = circleObj.AddComponent<RawImage>();
        rawImage.rectTransform.sizeDelta = new Vector2(texSize, texSize);

        // 创建纹理
        texture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        rawImage.texture = texture;

        currentRadius = defaultRadius;
        targetRadius = defaultRadius;
        currentColor = defaultColor;

        if (gun == null)
        {
            gun = transform.root.GetComponentInChildren<Gun>();
        }

    }

    void OnEnable()
    {
        if (gun != null)
            gun.OnShoot += SpreadOnShoot;
    }

    void OnDisable()
    {
        if (gun != null)
            gun.OnShoot -= SpreadOnShoot;
    }

    void Update()
    {
        //准心恢复
        if (targetRadius > defaultRadius)
        {
            targetRadius = Mathf.MoveTowards(targetRadius, defaultRadius, recoverySpeed * Time.deltaTime);
        }

        // 平滑过渡
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * spreadSpeed);

        // 重绘
        DrawCircle(currentRadius, currentColor);
    }

    void Hide(int score, float time)
    {
        gameObject.SetActive(false);
    }

    void DrawCircle(float radius, Color color)
    {
        Color[] pixels = new Color[texSize * texSize];
        Vector2 center = new Vector2(texSize / 2f, texSize / 2f);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist > radius - thickness && dist < radius + thickness)
                {
                    pixels[y * texSize + x] = color;
                }
                else
                {
                    pixels[y * texSize + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    // ====== 公开方法 ======
    /// <summary>
    /// 在射击时增加准心的扩散半径
    /// </summary>
    public void SpreadOnShoot()
    {
        targetRadius = Mathf.Min(targetRadius + spreadPerShot, maxRadius);
    }

    /// <summary>
    /// 设置准心的扩散程度
    /// </summary>
    /// <param name="amount">准心扩散程度，范围为 0 到 1</param>
    public void SetSpread(float amount)
    {
        targetRadius = Mathf.Lerp(defaultRadius, maxRadius, Mathf.Clamp01(amount));
    }

    /// <summary>
    /// 设置准心的颜色
    /// </summary>
    /// <param name="color">准心的颜色</param>
    public void SetColor(Color color)
    {
        currentColor = color;
    }

    /// <summary>
    /// 设置准心的可见性
    /// </summary>
    /// <param name="visible">准心是否可见</param>
    public void SetVisible(bool visible)
    {
        if (rawImage != null)
        {
            rawImage.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 重置准心的半径和颜色为默认值
    /// </summary>
    public void ResetToDefault()
    {
        targetRadius = defaultRadius;
        currentColor = defaultColor;
    }
}