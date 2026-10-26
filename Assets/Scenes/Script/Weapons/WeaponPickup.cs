using UnityEngine;
using static WeaponManager;

/// <summary>
/// 武器拾取物，玩家靠近按 F 拾取，解锁武器
/// </summary>
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("拾取配置")]
    [SerializeField] private string labelText = "拾取";
    [SerializeField] private bool destroyAfterPickup = true;

    [Tooltip("这个拾取物对应的武器类型")]
    [SerializeField] private WeaponManager.WeaponType weaponType;

    public WeaponManager.WeaponType WeaponType => weaponType;


    private void OnTriggerEnter(Collider other)
    {
        PlayerControl player = other.GetComponentInParent<PlayerControl>();
        if (player == null) return;
        if (!player.IsOwner) return;

        // 只有还没被拾取，才注册到交互提示
        if (gameObject.activeSelf)
        {
            InteractionPrompt.Instance?.Register(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerControl player = other.GetComponentInParent<PlayerControl>();
        if (player == null) return;
        if (!player.IsOwner) return;

        InteractionPrompt.Instance?.Unregister(this);
    }

    // ============================================================
    // IInteractable 实现
    // ============================================================

    public string GetLabelText()
    {
        return labelText;
    }

    public bool CanInteract()
    {
        return gameObject.activeSelf;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(GameObject interactor)
    {
        // 先注销交互提示
        InteractionPrompt.Instance?.Unregister(this);

        // 解锁武器
        WeaponManager weaponManager = interactor.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.UnlockWeaponServerRpc(weaponType, true);
            SystemMessageManager.Instance?.Success($"解锁{weaponType}!");
        }

        // 销毁或隐藏
        if (destroyAfterPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}