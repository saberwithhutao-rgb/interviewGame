using UnityEngine;

public class GunReloadAudio : MonoBehaviour
{
    [Header("换弹音效")]
    public AudioClip reloadSound1;
    public float reloadVolume1 = 0.8f;
    public AudioClip reloadSound2;
    public float reloadVolume2 = 0.8f;

    // 给动画帧事件调用的方法
    public void PlayReloadSound1()
    {
        if (reloadSound1 != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(reloadSound1, reloadVolume1);
        }
    }

    public void PlayReloadSound2()
    {
        if (reloadSound2 != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(reloadSound2, reloadVolume2);
        }
    }
}