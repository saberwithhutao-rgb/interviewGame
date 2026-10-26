/// <summary>
/// 敌人 Buff 管理器，负责管理敌人的属性增益
/// </summary>
public static class EnemyBuffManager
{
    public static float HealthMult = 1f;
    public static float SpeedMult = 1f;
    public static float DamageMult = 1f;

    /// <summary>
    /// 设置敌人的属性增益倍率
    /// </summary>
    /// <param name="hp">生命值增益倍率</param>
    /// <param name="speed">速度增益倍率</param>
    /// <param name="damage">伤害增益倍率</param>
    public static void SetBuffs(float hp, float speed, float damage)
    {
        HealthMult = hp;
        SpeedMult = speed;
        DamageMult = damage;
    }
}