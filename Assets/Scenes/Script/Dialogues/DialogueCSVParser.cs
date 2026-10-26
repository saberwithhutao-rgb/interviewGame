using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话 CSV 解析器
/// </summary>
public static class DialogueCSVParser
{
    /// <summary>
    /// 解析对话文本
    /// </summary>
    /// <param name="csvText">从 txt 读取的完整文本</param>
    /// <returns>id 到节点的字典</returns>
    public static Dictionary<int, DialogueNode> Parse(string csvText)
    {
        Dictionary<int, DialogueNode> nodes = new Dictionary<int, DialogueNode>();

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning("[DialogueCSVParser] 文本为空");
            return nodes;
        }

        const int COLUMN_COUNT = 9;

        string[] lines = csvText.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split('\t');


            // ===== 补齐到 9 列 =====
            if (cols.Length < COLUMN_COUNT)
            {
                int oldLength = cols.Length;
                System.Array.Resize(ref cols, COLUMN_COUNT);
                for (int j = oldLength; j < COLUMN_COUNT; j++)
                    cols[j] = "";
            }

            // 解析 rowType
            DialogueRowType rowType;
            switch (cols[0].Trim().ToLower())
            {
                case "line": rowType = DialogueRowType.Line; break;
                case "option": rowType = DialogueRowType.Option; break;
                case "end": rowType = DialogueRowType.End; break;
                default:
                    Debug.LogWarning($"[DialogueCSVParser] 未知 rowType: {cols[0]}，跳过");
                    continue;
            }

            // 解析 id
            if (!int.TryParse(cols[1].Trim(), out int id))
            {
                Debug.LogWarning($"[DialogueCSVParser] id 解析失败: {cols[1]}，跳过");
                continue;
            }

            // 解析 nextId
            int nextId = -1;
            if (!int.TryParse(cols[4].Trim(), out nextId))
                nextId = -1;

            // 构建节点（列数已补齐，直接取）
            DialogueNode node = new DialogueNode
            {
                rowType = rowType,
                id = id,
                speaker = cols[2].Trim(),
                content = cols[3].Trim(),
                nextId = nextId,
                conditionType = cols[5].Trim(),
                conditionValue = cols[6].Trim(),
                effectType = cols[7].Trim(),
                effectValue = cols[8].Trim()
            };

            nodes[id] = node;
        }

        Debug.Log($"[DialogueCSVParser] 解析完成，共 {nodes.Count} 个节点");
        return nodes;
    }

    /// <summary>
    /// 从 Resources 加载对话文件
    /// </summary>
    /// <param name="fileName">文件名（不含扩展名）</param>
    public static Dictionary<int, DialogueNode> LoadFromResources(string fileName)
    {
        string path = $"Dialogues/{fileName}";
        TextAsset txt = Resources.Load<TextAsset>(path);

        if (txt == null)
        {
            Debug.LogError($"[DialogueCSVParser] 找不到对话文件: {path}");
            return new Dictionary<int, DialogueNode>();
        }

        return Parse(txt.text);
    }
}