using UnityEngine;

public class WeaponIKController : MonoBehaviour
{
    [Header("IK 目标（空物体，挂在玩家身上）")]
    [SerializeField] private Transform gripTarget;   // TwoBoneIKConstraint.target 引用这个
    [SerializeField] private Transform hintTarget;   // TwoBoneIKConstraint.hint 引用这个

    private GameObject currentWeapon;

    private void OnEnable()
    {
        GameEvents.OnWeaponChanged += OnWeaponChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(GameObject weapon)
    {
        currentWeapon = weapon;
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        // ★ 查找武器上的握把点
        Transform grip = currentWeapon.transform.Find("WeaponGrip_R");
        Transform hint = currentWeapon.transform.Find("WeaponHint_R");

        if (grip != null)
            gripTarget.position = grip.position;

        if (hint != null)
            hintTarget.position = hint.position;
    }
}