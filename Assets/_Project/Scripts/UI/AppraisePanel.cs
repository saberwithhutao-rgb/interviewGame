using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 鉴定面板，用于显示目标的属性信息
/// </summary>
public class AppraisePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private float displayDuration = 3f;

    /// <summary>
    /// 显示鉴定信息
    /// </summary>
    /// <param name="info">要显示的鉴定信息</param>
    public void Show(string info)
    {
        infoText.text = info;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(HideAfterDelay());
    }
    
    /// <summary>
    /// 延迟隐藏鉴定面板
    /// </summary>
    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        gameObject.SetActive(false);
    }
}