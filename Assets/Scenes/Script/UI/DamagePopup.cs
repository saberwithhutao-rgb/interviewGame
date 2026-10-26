using UnityEngine;
using TMPro;

/// <summary>
/// 伤害弹出提示组件，用于在游戏中显示伤害数值
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float floatSpeed = 1f;

    [Header("伤害颜色")]
    [SerializeField] private Color physicalColor;
    [SerializeField] private Color poisonColor;
    [SerializeField] private Color fireColor;
    [SerializeField] private Color iceColor;

    /// <summary>
    /// 设置伤害弹出提示的数值和类型
    /// </summary>
    /// <param name="damage">伤害数值</param>
    /// <param name="damageType">伤害类型</param>
    public void Setup(float damage, DamageType damageType = DamageType.Physical)
    {
        damageText.text = $"{damage}";

        // 根据伤害类型设置颜色
        Color color = GetColorForDamageType(damageType);
        damageText.color = color; 

        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        Destroy(gameObject, lifetime);
    }

    private Color GetColorForDamageType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return physicalColor;
            case DamageType.Poison:
                return poisonColor;
            case DamageType.Fire:
                return fireColor;
            case DamageType.Ice:
                return iceColor;
            default:
                return Color.white;
        }
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}