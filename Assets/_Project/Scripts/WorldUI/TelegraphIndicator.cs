using UnityEngine;

/// <summary>
/// 攻击预警指示器，显示在地面上的红圈
/// </summary>
public class TelegraphIndicator : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float fadeInDuration = 0.3f;

    private Material material;
    private Color baseColor;
    private float elapsed;
    private bool isShowing;

    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        // 实例化材质，避免多个红圈共享同一个材质
        material = meshRenderer.material;
        baseColor = material.GetColor(ColorID);
    }

    /// <summary>
    /// 显示红圈
    /// </summary>
    /// <param name="position">世界坐标位置</param>
    /// <param name="radius">红圈半径</param>
    /// <param name="duration">持续时间</param>
    public void Show(Vector3 position, float radius, float duration)
    {

        transform.position = position + Vector3.up * 0.05f;
        transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);

        elapsed = 0f;
        isShowing = true;

        // 初始透明，渐显
        Color c = baseColor;
        c.a = 0f;
        material.SetColor(ColorID, c);

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏红圈
    /// </summary>
    public void Hide()
    {
        isShowing = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing) return;

        elapsed += Time.deltaTime;

        // 渐显
        float t = Mathf.Clamp01(elapsed / fadeInDuration);
        Color c = baseColor;
        c.a = Mathf.Lerp(0f, baseColor.a, t);
        material.SetColor(ColorID, c);
    }
}