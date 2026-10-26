using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家控制类，负责处理玩家的移动、旋转、动画和其他相关逻辑
/// </summary>
public class PlayerControl : NetworkBehaviour
{
    public static PlayerControl LocalInstance { get; private set; }
    private CharacterController player;
    public Transform cameraTransform;
    public float rotationSpeed = 10f;

    private float gravity = -9.81f;
    private Vector3 velocity;

    private bool isAttackInputLocked = false;

    [SerializeField] private Gun gun;
    private PlayerHealth playerHealth;

    [Header("动画")]
    [SerializeField] private Animator animator;

    [SerializeField] private PlayerStats playerStats;


    private float currentMoveSpeed;
    public static event System.Action<PlayerControl> OnLocalPlayerSpawned;

    /// <summary>
    /// 当网络对象生成时调用，初始化玩家位置和UI
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        else
        {
            Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                canvas.gameObject.SetActive(true);
            }
            OnLocalPlayerSpawned.Invoke(this);
            LocalInstance = this;
        }
        DynamicGI.UpdateEnvironment();
    }

    void OnEnable()
    {
        gun.OnShoot += FaceForward;
        if (playerHealth != null)
            playerHealth.OnPlayerDied += OnDied;
    }

    void OnDisable()
    {
        gun.OnShoot -= FaceForward;
        if (playerHealth != null)
            playerHealth.OnPlayerDied -= OnDied;
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        player = GetComponent<CharacterController>();
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
    }

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!IsOwner) return;
        // ===== 1. 获取输入 =====
        float horizontal, vertical;

        if (playerStats == null)
            return;

        if (UIManager.Instance.IsInputBlocked || isAttackInputLocked)
        {
            horizontal = 0f;
            vertical = 0f;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }

        // ===== 2. 计算移动方向 =====
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * vertical + right * horizontal;
        moveDirection.Normalize();
        bool isMoving = moveDirection.magnitude > 0.1f;
        bool wantsToSprint = isMoving && Input.GetMouseButton(1);
        bool canSprint = wantsToSprint && playerStats.CurrentBurstSP > 0;

        // ===== 3. SP 消耗（只有移动时才扣） =====
        if (isMoving)
        {
            float spCost = Time.deltaTime;

            // ★ 只有真正能冲刺时，才消耗更多
            if (canSprint)
                spCost = Time.deltaTime * 2f;

            playerStats.ConsumeSP(spCost);
        }

        // ===== 4. 速度计算 =====
        float speed;
        float animSpeed;

        if (playerStats.CurrentBurstSP <= 0)
        {
            // 黄色耗尽：移动速度惩罚
            speed = playerStats.Speed * 0.5f;
            animSpeed = isMoving ? 0.5f : 0f;
        }
        else if (canSprint)
        {
            speed = playerStats.SprintSpeed;
            animSpeed = 2f;
        }
        else
        {
            speed = playerStats.Speed;
            animSpeed = isMoving ? 1f : 0f;
        }

        // ===== 5. 重力 =====
        if (player.isGrounded && velocity.y < 0)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;

        // ===== 6. 移动 =====
        player.Move((moveDirection * speed + velocity) * Time.deltaTime);

        // ===== 7. 动画 =====
        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, animSpeed, Time.deltaTime * 10f);
        animator?.SetFloat("MoveSpeed", currentMoveSpeed);

        // ===== 8. 摔落移至世界上层 =====
        if (transform.position.y < -200f)
            transform.position = new Vector3(transform.position.x, 800f, transform.position.z);

        // ===== 9. 角色朝向 =====
        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// 让玩家面向摄像机的前方方向，通常在攻击时调用
    /// </summary>
    /// <param name="player">玩家对象</param>
    public void FaceForward()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();
        transform.rotation = Quaternion.LookRotation(forward);
    }

    void OnDied(GameObject player)
    {
        this.enabled = false;
    }

    public void SetAttackInputLock(bool locked)
    {
        isAttackInputLocked = locked;
    }

    public void MoveToSpawnPoint()
    {
        GameObject spawnPoint = GameObject.Find("playerSpawnPoint");
        if (spawnPoint != null)
        {
            TeleportTo(spawnPoint.transform.position);
        }
        else
        {
            Vector3 pos = new Vector3(-20.42627f, -0.01100016f, 7.17627f);
            TeleportTo(pos);
        }
    }

    /// <summary>
    /// 将玩家传送到指定位置（用于读档、复活等）
    /// </summary>
    public void TeleportTo(Vector3 position, float rotationY = 0f)
    {
        // 1. 禁用 CharacterController，防止物理系统把玩家拉回原位
        if (player != null) player.enabled = false;

        // 2. 设置位置和朝向
        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // 3. 清空重力速度，防止传送后瞬间下坠
        velocity = Vector3.zero;

        // 4. 重新启用 CharacterController
        if (player != null) player.enabled = true;

        // 5. 刷新摄像机（如果你有 ThirdPersonCamera，确保它的角度跟着更新）
        // 例如：ThirdPersonCamera.Instance?.SnapToPlayer();
    }
}