using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Reflection;

public class PlayerDeathAudio : NetworkBehaviour
{
    public AudioClip gameOverClip;
    [Range(0f, 1f)] public float gameOverVolume = 0.4f;

    public AudioClip deathLoopBGM;
    [Range(0f, 1f)] public float deathBGMVolume = 0.5f;

    private AudioSource gameOverAudioSource;
    private AudioSource loopBGMAudioSource;
    private PlayerHealth playerHealth;
    private FieldInfo musicSourceField;

    void Awake()
    {
        // ��ǰ���淴���ֶ�
        musicSourceField = typeof(AudioManager).GetField("musicSource", BindingFlags.Instance | BindingFlags.NonPublic);

        gameOverAudioSource = gameObject.AddComponent<AudioSource>();
        gameOverAudioSource.playOnAwake = false;
        gameOverAudioSource.spatialBlend = 0f;

        loopBGMAudioSource = gameObject.AddComponent<AudioSource>();
        loopBGMAudioSource.playOnAwake = false;
        loopBGMAudioSource.spatialBlend = 0f;
        loopBGMAudioSource.loop = true;
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnPlayerDied += (_) => OnPlayerDead();
    }

    private void OnPlayerDead()
    {
        PlayDeathSequenceClientRpc();
    }

    [ClientRpc]
    private void PlayDeathSequenceClientRpc()
    {
        // �����жϣ��ž��ձ���
        if (AudioManager.Instance != null && musicSourceField != null)
        {
            AudioSource musicSource = musicSourceField.GetValue(AudioManager.Instance) as AudioSource;
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
        StartCoroutine(DeathAudioFlow());
    }

    IEnumerator DeathAudioFlow()
    {
        yield return new WaitForSeconds(2.2f);

        if (gameOverClip != null)
        {
            // gameOverClip 为一次性提示音，受音效/SFX滑块控制
            float sfxVol = AudioManager.Instance != null
                ? AudioManager.Instance.CurrentSFXVolume
                : 1f;
            gameOverAudioSource.Stop();
            gameOverAudioSource.PlayOneShot(gameOverClip, gameOverVolume * sfxVol);
            yield return new WaitForSeconds(gameOverClip.length);
        }
        PlayLoopBGM();
    }

    void PlayLoopBGM()
    {
        if (loopBGMAudioSource != null && deathLoopBGM != null)
        {
            // 死亡循环BGM受音量/音乐滑块控制
            float musicVol = AudioManager.Instance != null
                ? AudioManager.Instance.CurrentMusicVolume
                : 1f;
            loopBGMAudioSource.clip = deathLoopBGM;
            loopBGMAudioSource.volume = deathBGMVolume * musicVol;
            loopBGMAudioSource.Play();
        }
    }

    void Update()
    {
        // 死亡BGM播放期间，实时同步音乐音量（用户可能拖动滑块）
        if (loopBGMAudioSource != null && loopBGMAudioSource.isPlaying
            && AudioManager.Instance != null)
        {
            loopBGMAudioSource.volume = deathBGMVolume * AudioManager.Instance.CurrentMusicVolume;
        }
    }

    public void StopDeathBGM()
    {
        if (loopBGMAudioSource != null)
        {
            loopBGMAudioSource.Stop();
            loopBGMAudioSource.clip = null;
        }

        // ����У�飬ÿһ�㶼�жϲ�Ϊ�ղ�ִ��
        if (AudioManager.Instance != null && musicSourceField != null)
        {
            AudioSource musicSource = musicSourceField.GetValue(AudioManager.Instance) as AudioSource;
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }
    }

    public override void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDied -= (_) => OnPlayerDead();
        base.OnDestroy();
    }
}