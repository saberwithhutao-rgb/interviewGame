using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 死亡菜单组件，用于在玩家死亡后显示死亡面板
/// </summary>
public class DeathMenu : MonoBehaviour
{
    [SerializeField] private float delay = 3.8f;
    private PlayerHealth playerHealth;

    void OnEnable()
    {
        PlayerControl.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    void OnDisable()
    {
        PlayerControl.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied -= OnPlayerDied;
        }
    }

    void OnLocalPlayerSpawned(PlayerControl pc)
    {
        playerHealth = pc.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnPlayerDied += OnPlayerDied;
    }

    void OnPlayerDied(GameObject player)
    {
        if (SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            StartCoroutine(ShowAfterDelay());
        }
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.OpenModal("DeathPanel");
    }

}