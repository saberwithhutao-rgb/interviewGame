using UnityEngine;

public class WeaponGlow : MonoBehaviour
{
    [SerializeField] private Light glowLight;
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float pulseSpeed = 2f;

    private void Update()
    {
        if (glowLight != null)
        {
            glowLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        }
    }
}