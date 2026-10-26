using Unity.Netcode;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;

    public Vector3 shoulderOffset = new Vector3(0.6f, 1.5f, 0f);

    public float distance = 4.5f;

    public float sensitivity;

    public Vector2 pitchLimit = new Vector2(-89f, 89f);

    [Header("碰撞检测")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.3f;
    public float smoothSpeed = 10f;

    [Header("后坐力")]
    public float recoilAmount = 1.5f;
    public float recoilSpeed = 15f;
    public float recoilRecoverySpeed = 8f;

    private float recoilOffset;
    private float recoilVelocity;

    private float yaw;
    private float pitch;
    private float currentDistance;

    private void Awake()
    {
        recoilOffset = 0f;
    }

    void Start()
    {
        PlayerControl.OnLocalPlayerSpawned += (pc) =>
        {
            player = pc.transform;
            Vector3 initialDir = player.forward;
            yaw = Mathf.Atan2(initialDir.x, initialDir.z) * Mathf.Rad2Deg;
        };

        // 如果本地玩家已经存在，直接使用
        if (PlayerControl.LocalInstance != null)
        {
            player = PlayerControl.LocalInstance.transform;
            Vector3 initialDir = player.forward;
            yaw = Mathf.Atan2(initialDir.x, initialDir.z) * Mathf.Rad2Deg;
        }

        pitch = 10f;
        currentDistance = distance;
        sensitivity = SettingsManager.Instance != null
            ? SettingsManager.Instance.MouseSensitivity : 100f;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSensitivityChanged += RefreshSensitivity;
        }
        collisionMask = LayerMask.GetMask("Environment");

    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSensitivityChanged -= RefreshSensitivity;
        }
    }

    void LateUpdate()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsInputBlocked) return;
        if (player == null) return;

        //读取鼠标输入
        yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchLimit.x, pitchLimit.y);

        if (recoilOffset > 0.01f)
        {
            // 冲上去
            recoilVelocity += recoilSpeed * Time.deltaTime;
            recoilOffset = Mathf.Max(0, recoilOffset - recoilVelocity * Time.deltaTime);

            // 冲过目标了，开始回弹
            if (recoilOffset <= 0.01f)
            {
                recoilOffset = 0f;
                recoilVelocity = 0f;
            }
        }
        else
        {
            // 回落（往回收一点，制造阻尼感）
            recoilOffset = Mathf.Lerp(recoilOffset, -0.5f, Time.deltaTime * recoilRecoverySpeed);
            recoilOffset = Mathf.Lerp(recoilOffset, 0f, Time.deltaTime * recoilRecoverySpeed * 0.5f);
            recoilVelocity = 0f;
        }

        float finalPitch = pitch - recoilOffset;
        finalPitch = Mathf.Clamp(finalPitch, pitchLimit.x, pitchLimit.y);

        //计算肩部偏移点
        Quaternion rotation = Quaternion.Euler(finalPitch, yaw, 0f);
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;

        //偏移点
        Vector3 rotatedOffset = right * shoulderOffset.x + up * shoulderOffset.y;
        Vector3 pivot = player.position + rotatedOffset;

        //碰撞检测
        float targetDistance = distance;
        if (Physics.SphereCast(pivot, collisionRadius, -forward, out RaycastHit hit, distance + collisionRadius, collisionMask))
        {
            targetDistance = hit.distance - collisionRadius;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        //相机位置
        transform.position = pivot - forward * currentDistance;
        transform.rotation = Quaternion.LookRotation(forward);
    }

    public void ApplyRecoil()
    {
        recoilOffset += recoilAmount;
        recoilVelocity = 0f;
    }

    public void RefreshSensitivity()
    {
        if (SettingsManager.Instance != null)
        {
            sensitivity = SettingsManager.Instance.MouseSensitivity;
        }
    }

    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }
}