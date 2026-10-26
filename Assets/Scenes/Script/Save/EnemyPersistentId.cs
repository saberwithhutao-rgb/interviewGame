using UnityEngine;

/// <summary>
/// 用于标记场景中固定存在的敌人，提供唯一 ID 用于存档
/// </summary>
public class EnemyPersistentId : MonoBehaviour
{
    [Tooltip("手动输入，必须唯一，例如 Goblin_01, Boss_Room1")]
    [SerializeField] private string uniqueId;

    public string UniqueId => uniqueId;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
            Debug.LogWarning($"{gameObject.name} 的 UniqueId 为空！", this);
    }
}