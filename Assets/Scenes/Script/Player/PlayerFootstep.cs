using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    [Header("脚步声设置")]
    public AudioClip footstepClip;

    [Tooltip("脚步声基础音量，默认0.5")]
    [Range(0f, 1f)] public float footstepVolume = 0.5f;

    private AudioSource _footstepSource;

    void Awake()
    {
        _footstepSource = gameObject.AddComponent<AudioSource>();
        _footstepSource.spatialBlend = 0f;          // 2D音效，不受距离衰减影响
        _footstepSource.volume = 1f;                // 由滑块实时控制
        _footstepSource.playOnAwake = false;
        _footstepSource.loop = false;
        _footstepSource.dopplerLevel = 0f;
    }

    void Update()
    {
        // 实时同步音效滑块音量
        if (_footstepSource != null && AudioManager.Instance != null)
            _footstepSource.volume = AudioManager.Instance.CurrentSFXVolume;
    }

    public void PlayFootstep()
    {
        if (footstepClip != null && _footstepSource != null)
        {
            _footstepSource.PlayOneShot(footstepClip, footstepVolume);
        }
    }
}