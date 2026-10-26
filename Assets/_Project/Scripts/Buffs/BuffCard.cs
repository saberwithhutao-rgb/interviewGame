using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buff 卡片 UI 组件，用于显示 Buff 信息并处理选择事件
/// </summary>
public class BuffCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button button;

    /// <summary>
    /// 设置 Buff 卡片的显示内容和选择回调
    /// </summary>
    /// <param name="data">要显示的 Buff 数据</param>
    /// <param name="onChosen">当 Buff 被选择时的回调</param>
    public void Setup(BuffData data, System.Action<BuffData> onChosen)
    {
        nameText.text = data.Name;
        descText.text = data.Description;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onChosen?.Invoke(data));
    }
}