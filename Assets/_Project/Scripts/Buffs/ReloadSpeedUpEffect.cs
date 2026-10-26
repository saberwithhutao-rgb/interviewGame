using UnityEngine;

[CreateAssetMenu(fileName = "ReloadSpeedUp", menuName = "Buff效果/换弹速度提升")]
public class ReloadSpeedUpEffect : BuffEffect
{
    public float multiplier = 1.5f;

    public override void OnApply(GameObject target)
    {
        Gun gun = target.GetComponentInChildren<Gun>();
        if (gun != null)
            gun.AddReloadSpeedMultiplier(multiplier);
    }

    public override void OnRemove(GameObject target)
    {
        Gun gun = target.GetComponentInChildren<Gun>();
        if (gun != null)
            gun.RemoveReloadSpeedMultiplier(multiplier);
    }
}