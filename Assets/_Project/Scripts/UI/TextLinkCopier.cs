using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextLinkCopier : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    // 实现 IPointerClickHandler 接口来处理点击事件[citation:1][citation:9]
    public void OnPointerClick(PointerEventData eventData)
    {
        // 获取当前点击位置对应的链接索引[citation:4]
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            // 通过索引获取链接信息
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            // 拿到链接ID，也就是我们要复制的内容[citation:1][citation:4]
            string linkId = linkInfo.GetLinkID();

            // 将内容复制到系统剪贴板[citation:6][citation:8]
            GUIUtility.systemCopyBuffer = linkId;

            Debug.Log($"已复制到剪贴板: {linkId}");
            // 可选：在这里弹出一个提示，比如“大厅ID已复制”
        }
    }
}