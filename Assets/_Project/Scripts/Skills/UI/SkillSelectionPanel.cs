// SkillSelectionPanel.cs
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectionPanel : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private GameObject panelRoot;           // 面板根物体，用于显示/隐藏
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;          // 放置技能的容器
    [SerializeField] private GameObject skillItemPrefab;     // 技能项预制体（带Text）

    [Header("详情面板")]
    [SerializeField] private GameObject detailPanel;        // 详情面板根物体
    [SerializeField] private TextMeshProUGUI detailName;
    [SerializeField] private TextMeshProUGUI detailSkillType;
    [SerializeField] private TextMeshProUGUI detailDesc;
    [SerializeField] private TextMeshProUGUI detailCost;
    [SerializeField] private Button learnButton;

    [Header("按键绑定")]
    [SerializeField] private GameObject keyBindingArea;
    [SerializeField] private KeyBindingButton keyBindingButton;

    private SkillData currentSelectedSkillData;

    [Header("技能点数")]
    [SerializeField] private TextMeshProUGUI skillPointsText;

    [Header("数据来源")]
    [SerializeField] private SkillUnlockManager unlockManager;

    [Header("显示设置")]
    [SerializeField] private float itemHeight = 60f;         // 技能项高度（需与预制体一致）

    private SkillManager skillManager;
    private SnapScrollHelper snapHelper;
    private PlayerStats playerStats;
    private List<SkillDisplayInfo> currentSkills = new List<SkillDisplayInfo>();
    private List<GameObject> itemObjects = new List<GameObject>();

    public Action<SkillDisplayInfo> OnSkillSelected;

    // 当前选中的技能索引（中心对齐的那个）
    private int currentSelectedIndex = -1;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
        // 初始关闭面板
        panelRoot.SetActive(false);

        // 获取或添加对齐助手
        snapHelper = scrollRect.GetComponent<SnapScrollHelper>();
        if (snapHelper == null)
        {
            snapHelper = scrollRect.gameObject.AddComponent<SnapScrollHelper>();
            
        }
        snapHelper.Init();

        if (unlockManager == null || skillManager == null || playerStats == null)
        {
            PlayerControl.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        learnButton.onClick.AddListener(OnLearnButtonClick);
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnSkillPointsChanged += UpdateSkillPointsUI;
            UpdateSkillPointsUI(playerStats.SkillPoints);
        }

        if (snapHelper != null)
            snapHelper.OnSnappedToIndex += OnSnappedToIndex;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnSkillPointsChanged -= UpdateSkillPointsUI;
        if (keyBindingButton != null)
            keyBindingButton.OnBindingComplete -= OnBindingChanged;
        if (snapHelper != null)
            snapHelper.OnSnappedToIndex -= OnSnappedToIndex;
    }

    void OnLocalPlayerSpawned(PlayerControl pc)
    {
        unlockManager = pc.GetComponent<SkillUnlockManager>();
        skillManager = pc.GetComponent<SkillManager>();
        playerStats = pc.GetComponent<PlayerStats>();

        playerStats.OnSkillPointsChanged += UpdateSkillPointsUI;
        //默认刷新技能点数显示
        UpdateSkillPointsUI(playerStats.SkillPoints);

        if (unlockManager != null && skillManager != null && playerStats != null)
        {
            PlayerControl.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }
    }

    private void OnSnappedToIndex(int index)
    {
        if (index >= 0 && index < currentSkills.Count)
        {
            currentSelectedIndex = index;
            OnSkillSelected?.Invoke(currentSkills[index]);
        }
    }

    // 外部调用：设置要显示的技能列表（每次打开前调用或数据变化时调用）
    public void SetSkills(List<SkillDisplayInfo> skills)
    {
        currentSkills = new List<SkillDisplayInfo>(skills);
        RefreshUI();
    }

    // 重新生成所有技能项
    private void RefreshUI()
    {
        // 清除旧项
        foreach (var obj in itemObjects)
            Destroy(obj);
        itemObjects.Clear();

        // 重新生成
        for (int i = 0; i < currentSkills.Count; i++)
        {
            var skill = currentSkills[i];
            GameObject item = Instantiate(skillItemPrefab, content);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = skill.skillName;
            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null)
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);

            Button btn = item.GetComponent<Button>();
            if (btn == null)
                btn = item.AddComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => OnSkillItemClicked(index));

            itemObjects.Add(item);
        }

        // ★ 从 SnapScrollHelper 读取间距
        float spacing = snapHelper != null ? snapHelper.ItemSpacing : 0f;

        // ★ 重新计算 content 大小（把间距算进去）
        int count = currentSkills.Count;
        float totalHeight = count * itemHeight + (count - 1) * spacing;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        // 重置滚动位置到顶部
        scrollRect.verticalNormalizedPosition = 1f;

        // 刷新对齐助手
        if (snapHelper != null)
            snapHelper.RefreshItems();
    }

    private void OnSkillItemClicked(int index)
    {
        if (index >= 0 && index < currentSkills.Count)
        {
            currentSelectedIndex = index;

            var skill = currentSkills[index];
            ShowDetailPanel(skill);

            OnSkillSelected?.Invoke(currentSkills[index]);
        }
    }

    private void ShowDetailPanel(SkillDisplayInfo skill)
    {
        // 获取对应的 SkillData
        SkillData data = unlockManager?.GetSkillDataByName(skill.skillName);
        if (data == null) return;

        // 填充 UI
        detailName.text = data.SkillName;
        detailSkillType.text = data.SkillType == SkillType.Active ? "[主动技能]" : "[被动技能]";
        detailDesc.text = data.Description;
        detailCost.text = $"消耗 {data.SkillPointCost} 技能点";

        // 更新按钮状态
        bool alreadyOwned = skillManager.HasSkill(data);
        bool enoughPoints = playerStats.SkillPoints >= data.SkillPointCost;
        learnButton.interactable = !alreadyOwned && enoughPoints;

        if (keyBindingArea != null)
        {
            keyBindingArea.SetActive(data.SkillType == SkillType.Active);
        }

        if (keyBindingButton != null)
        {
            keyBindingButton.SetSkillId(data.SkillId);
            SkillKeyBinding binding = SkillBindingManager.Instance.GetBinding(data.SkillId);
            keyBindingButton.SetBinding(binding);
            keyBindingButton.OnBindingComplete += OnBindingChanged;
        }

        // 保存当前选中的 SkillData 供学习按钮使用
        currentSelectedSkillData = data;

        // 显示详情面板
        detailPanel.SetActive(true);
    }

    private void OnBindingChanged(SkillKeyBinding binding, int skillId)
    {
        Debug.Log($"技能 ID {skillId} 绑定按键: {binding.GetDisplayName()}");
    }

    private void UpdateLearnButtonState()
    {
        // 检查是否已拥有、技能点是否足够
        bool alreadyOwned = skillManager.HasSkill(currentSelectedSkillData);
        bool enoughPoints = playerStats.SkillPoints >= currentSelectedSkillData.SkillPointCost;
        learnButton.interactable = !alreadyOwned && enoughPoints;
    }

    public void OnLearnButtonClick()
    {
        if (currentSelectedSkillData == null) return;

        // 消耗点数、学习技能、刷新列表、关闭详情面板
        playerStats.SpendSkillPoints(currentSelectedSkillData.SkillPointCost);
        skillManager.LearnSkill(currentSelectedSkillData);

        // 刷新可购买列表
        Refresh();  // 重新获取技能列表并刷新 UI

        // 隐藏详情面板
        detailPanel.SetActive(false);
    }

    /// <summary>
    /// 刷新面板数据（不控制面板显隐）
    /// </summary>
    public void Refresh(bool refreshData = true)
    {
        if (refreshData)
        {
            if (unlockManager != null)
            {
                List<SkillDisplayInfo> skills = unlockManager.GetBuyableSkills();
                Debug.Log($"[SkillSelectionPanel] 获取到 {skills.Count} 个可购买技能");
                SetSkills(skills);
            }
            else
            {
                Debug.LogWarning("SkillUnlockManager 未配置，使用演示数据");
                List<SkillDisplayInfo> demo = new List<SkillDisplayInfo>();
                for (int i = 1; i <= 10; i++)
                    demo.Add(new SkillDisplayInfo { skillName = "演示技能 " + i });
                SetSkills(demo);
            }
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 更新技能点数显示
    /// </summary>
    /// <param name="points">当前技能点数</param>
    public void UpdateSkillPointsUI(int points)
    {
        if (skillPointsText != null)
            skillPointsText.text = $"技能点数: {points}";
    }

}