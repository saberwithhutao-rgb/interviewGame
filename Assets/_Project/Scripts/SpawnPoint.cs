using UnityEngine;

/// <summary>
/// SpawnPoint 类用于定义敌人生成点，继承自 MonoBehaviour
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { Melee, Ranged, Boss, Any }

    public SpawnType type = SpawnType.Any;

    void OnDrawGizmos()
    {
        Gizmos.color = type switch
        {
            SpawnType.Melee => Color.red,
            SpawnType.Ranged => Color.blue,
            SpawnType.Boss => Color.yellow,
            _ => Color.green
        };
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}