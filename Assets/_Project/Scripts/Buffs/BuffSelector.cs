using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff 选择器，用于在游戏中显示 Buff 选项并让玩家选择
/// </summary>
public class BuffSelector : MonoBehaviour
{
    [SerializeField] private BuffCard[] cards;
    [SerializeField] private FullScreenModal fullScreenModal;

    /// <summary>
    /// 显示 Buff 选择界面
    /// </summary>
    /// <param name="pool">可供选择的 Buff 数据池</param>
    /// <param name="count">要显示的 Buff 数量</param>
    /// <param name="callback">当玩家选择 Buff 后的回调</param>
    public void Show(BuffData[] pool, int count, System.Action<BuffData> callback)
    {
        UIManager.Instance.OpenModal("BuffPanel");
        fullScreenModal.IsShowGameView(false);

        List<BuffData> poolList = new List<BuffData>(pool);
        int actualCount = Mathf.Min(count, poolList.Count);

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < actualCount)
            {
                int randomIndex = Random.Range(0, poolList.Count);
                BuffData chosen = poolList[randomIndex];
                poolList.RemoveAt(randomIndex);

                cards[i].Setup(chosen, (buff) =>
                {
                    UIManager.Instance.CloseModal();
                    fullScreenModal.IsShowGameView(true);
                    callback?.Invoke(buff);       
                });

                cards[i].gameObject.SetActive(true);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }
}